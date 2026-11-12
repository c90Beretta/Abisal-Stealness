using System.Linq;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class HordeDirector : MonoBehaviour
    {
        public SandboxSession session;
        public SandboxEnemy[] reserves;
        public bool[] released;
        public float pressure;
        public int waves;
        public const int ActiveLimit = 14;
        private float nextWave;
        private float autoWaveCountdown = 65f;
        private bool waveCombatActive;
        private int clearedWaves;

        public SandboxEnemy ActiveBoss { get; private set; }
        public int Remaining => released == null ? 0 : released.Count(value => !value);
        public int ClearedWaves => clearedWaves;
        public float NextWaveSeconds => Mathf.Max(0f, autoWaveCountdown);
        public int ActiveAlertedCount => session == null || session.Enemies == null ? 0 :
            session.Enemies.Count(e => e != null && e.gameObject.activeInHierarchy && !e.IsDead && e.Alerted);

        public string StatusHudText
        {
            get
            {
                if (ActiveBoss != null && !ActiveBoss.IsDead && ActiveBoss.gameObject.activeInHierarchy)
                    return $"JEFE: {ActiveBoss.definition.displayName.ToUpperInvariant()} ({Mathf.CeilToInt(ActiveBoss.health)} HP)";
                int alerted = ActiveAlertedCount;
                if (waveCombatActive && alerted > 0)
                    return $"HORDA #{waves} EN CURSO · {alerted} ACTIVOS · PRESION {pressure:0}%";
                return $"OLEADA {waves} · PROX: {Mathf.CeilToInt(autoWaveCountdown)}s [N] · RESERVA {Remaining}";
            }
        }

        private void Awake()
        {
            if (released == null || (reserves != null && released.Length != reserves.Length))
                released = new bool[reserves != null ? reserves.Length : 16];
        }

        private void Update()
        {
            if (session == null || !session.Running) return;

            pressure = Mathf.MoveTowards(pressure, 0, Time.deltaTime * .8f);
            autoWaveCountdown -= Time.deltaTime * (1f + pressure * 0.015f);

            if ((pressure >= 20f || autoWaveCountdown <= 0f) && Time.time >= nextWave)
            {
                autoWaveCountdown = Mathf.Max(38f, 68f - waves * 4f);
                Release(session.player.transform.position);
            }

            if (waveCombatActive && Time.time > nextWave - 18f && ActiveAlertedCount == 0)
            {
                waveCombatActive = false;
                clearedWaves++;
                session.reserve = Mathf.Min(240, session.reserve + 10);
                session.State.bandages = Mathf.Min(12, session.State.bandages + 1);
                session.GetComponent<PortMapExpander>()?.DeliverHordeSupplyDrop();
                TacticalAudio.Play(session.player.transform.position, TacticalCue.Swing, 1.3f);
                session.SetMessage($"¡Horda #{waves} repelida! +10 municion, +1 venda y Suministros en Helipuerto Sur.");
            }
        }

        public void Noise(Vector3 position, float radius)
        {
            if (!enabled || !session.Running || radius < 7) return;
            pressure = Mathf.Min(100, pressure + Mathf.Min(radius * .35f, 14));
            if (radius >= 20 && Time.time >= nextWave) Release(position);
        }

        public void CallWaveEarly()
        {
            if (!enabled || session == null || !session.Running) return;
            nextWave = 0f;
            autoWaveCountdown = Mathf.Max(38f, 65f - waves * 4f);
            int spawned = Release(session.player.transform.position);
            if (spawned == 0)
                spawned = RecycleExpandedWave(session.player.transform.position, false);
            if (spawned > 0)
            {
                session.reserve = Mathf.Min(240, session.reserve + 12);
                if (session.Night != null) session.Night.Inventory.stamina = 100f;
                session.SetMessage($"¡Oleada #{waves} adelantada! Bonificacion tactica: +12 cartuchos.");
            }
        }

        public void TriggerAlarmHorde(string reason, Vector3 origin, bool spawnBoss = false)
        {
            if (!enabled || session == null || !session.Running || reserves == null) return;
            nextWave = 0f;
            pressure = Mathf.Min(100f, pressure + 35f);
            int count = Release(origin);
            if (count == 0) count = RecycleExpandedWave(origin, spawnBoss);
            else if (spawnBoss) PromoteWaveBoss();
            TacticalAudio.Play(origin, TacticalCue.Growl, 1.35f);
            session.SetMessage(reason);
        }

        public int Release(Vector3 target)
        {
            if (!enabled || !session.Running || Time.time < nextWave || reserves == null) return 0;
            int alive = session.Enemies.Count(e => e != null && e.gameObject.activeInHierarchy && !e.IsDead);
            int count = 0;
            SandboxEnemy lastAwakened = null;
            // Authored reserves wake in place; nothing materializes inside the player's light.
            foreach (var i in Enumerable.Range(0, reserves.Length).OrderBy(i => (reserves[i].transform.position - target).sqrMagnitude))
            {
                if (released[i] || alive + count >= ActiveLimit || count >= 4) continue;
                if (Vector3.Distance(reserves[i].transform.position, session.player.transform.position) < 9) continue;
                released[i] = true;
                var enemy = reserves[i];
                enemy.gameObject.SetActive(true);
                ApplyWaveScaling(enemy, waves + 1);
                enemy.Hear(target, 100);
                lastAwakened = enemy;
                count++;
            }
            if (count == 0 && Remaining == 0 && waves >= 3)
            {
                count = RecycleExpandedWave(target, (waves + 1) % 3 == 0);
                return count;
            }
            if (count > 0)
            {
                waves++;
                waveCombatActive = true;
                nextWave = Time.time + 24;
                pressure = Mathf.Max(0, pressure - 20);
                autoWaveCountdown = Mathf.Max(38f, 68f - waves * 4f);
                if (waves % 3 == 0 && lastAwakened != null)
                    PromoteEnemyToBoss(lastAwakened);
                session.SetMessage(waves % 3 == 0
                    ? $"¡HORDA DE ELITE #{waves}! Un Coloso Abisal lidera el asalto."
                    : "Ruido de horda en el puerto. Apaga la linterna y rompe el contacto.");
            }
            return count;
        }

        private int RecycleExpandedWave(Vector3 target, bool bossWave)
        {
            if (reserves == null || session == null) return 0;
            int alive = session.Enemies.Count(e => e != null && e.gameObject.activeInHierarchy && !e.IsDead);
            int count = 0;
            SandboxEnemy lastRecycled = null;
            var points = PortMapExpander.ExpandedSpawnPoints;

            for (int i = 0; i < reserves.Length && count < 4 && alive + count < ActiveLimit; i++)
            {
                var enemy = reserves[i];
                if (enemy == null || !enemy.IsDead) continue;
                Vector3 spawnPos = points[(waves + i) % points.Length];
                if (Vector3.Distance(spawnPos, session.player.transform.position) < 12f) continue;

                enemy.GetComponent<Dismemberment>()?.Restore(Severed.None);
                enemy.gameObject.SetActive(true);
                enemy.Restore(enemy.MaxHealth, spawnPos);
                ApplyWaveScaling(enemy, waves + 1);
                enemy.Hear(target, 120f);
                lastRecycled = enemy;
                count++;
            }

            if (count > 0)
            {
                waves++;
                waveCombatActive = true;
                nextWave = Time.time + 24f;
                pressure = Mathf.Max(0f, pressure - 20f);
                if ((bossWave || waves % 3 == 0) && lastRecycled != null)
                    PromoteEnemyToBoss(lastRecycled);
            }
            return count;
        }

        private void ApplyWaveScaling(SandboxEnemy enemy, int waveNumber)
        {
            if (enemy == null || waveNumber <= 1) return;
            float hpMult = 1f + Mathf.Min(1.2f, (waveNumber - 1) * 0.12f);
            float spdMult = 1f + Mathf.Min(0.35f, (waveNumber - 1) * 0.04f);
            enemy.health = enemy.MaxHealth * hpMult;
            enemy.speed = (enemy.definition != null ? enemy.definition.speed : 1.2f) * spdMult;
        }

        private void PromoteWaveBoss()
        {
            var candidate = session.Enemies.FirstOrDefault(e => e != null && e.gameObject.activeInHierarchy && !e.IsDead && e.GetComponent<SpecialInfected>() == null);
            if (candidate != null) PromoteEnemyToBoss(candidate);
        }

        private void PromoteEnemyToBoss(SandboxEnemy enemy)
        {
            if (enemy == null) return;
            var special = enemy.GetComponent<SpecialInfected>() ?? enemy.gameObject.AddComponent<SpecialInfected>();
            special.Initialize(session, SpecialSpecies.Colossus, true);
            ActiveBoss = enemy;
        }

        public void Restore(bool[] flags, float value, int waveCount)
        {
            released = flags.ToArray(); pressure = value; waves = waveCount; nextWave = Time.time + 8;
            waveCombatActive = false;
            for (int i = 0; i < reserves.Length; i++) reserves[i].gameObject.SetActive(released[i]);
        }
    }
}
