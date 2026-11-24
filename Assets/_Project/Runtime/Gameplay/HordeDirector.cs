using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Zombtoy.Gameplay
{
    public sealed class HordeDirector : MonoBehaviour
    {
        public SandboxSession session;
        public SandboxEnemy[] reserves;
        public bool[] released;
        public float pressure;
        public int waves;
        public const int ActiveLimit = 24;
        private const int MaxDynamicPool = 16;
        private readonly List<SandboxEnemy> dynamicPool = new List<SandboxEnemy>();
        private float nextWave;
        private float autoWaveCountdown = 6f;
        private bool waveCombatActive;
        private int clearedWaves;

        public SandboxEnemy ActiveBoss { get; private set; }
        public int Remaining => released == null ? 0 : released.Count(value => !value);
        public int ClearedWaves => clearedWaves;
        public float NextWaveSeconds => Mathf.Max(0f, autoWaveCountdown);
        public int ActiveAlertedCount => session == null || session.Enemies == null ? 0 :
            session.Enemies.Count(e => e != null && e.gameObject.activeInHierarchy && !e.IsDead && e.Alerted);
        public bool IsHordeActive => waveCombatActive && (ActiveAlertedCount > 0 || (ActiveBoss != null && !ActiveBoss.IsDead && ActiveBoss.gameObject.activeInHierarchy));

        public event System.Action<int, bool> OnHordeStarted;
        public event System.Action<int> OnHordeCleared;

        public static int GetWaveSpawnCount(int waveNumber)
        {
            int w = Mathf.Max(1, waveNumber);
            // Round 1 starts with 8 enemies, scaling +2 per round up to 16 per wave
            return Mathf.Min(16, 8 + (w - 1) * 2);
        }

        public static float GetDifficultyMultiplier(int waveNumber)
        {
            int w = Mathf.Max(1, waveNumber);
            return 1.15f + Mathf.Min(2.0f, (w - 1) * 0.22f);
        }

        public string StatusHudText
        {
            get
            {
                if (ActiveBoss != null && !ActiveBoss.IsDead && ActiveBoss.gameObject.activeInHierarchy)
                    return $"JEFE RONDA #{waves}: {ActiveBoss.definition.displayName.ToUpperInvariant()} ({Mathf.CeilToInt(ActiveBoss.health)} HP)";
                int alerted = ActiveAlertedCount;
                float mult = GetDifficultyMultiplier(Mathf.Max(1, waves));
                if (waveCombatActive && alerted > 0)
                    return $"RONDA #{waves} (x{mult:0.00}) · {alerted} ACTIVOS · PRESION {pressure:0}%";
                return $"RONDA {waves} · PROX: {Mathf.CeilToInt(autoWaveCountdown)}s [N] · SIGUIENTE: {GetWaveSpawnCount(waves + 1)} ZOMBIES";
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

            pressure = Mathf.MoveTowards(pressure, 0f, Time.deltaTime * 0.45f);
            autoWaveCountdown -= Time.deltaTime * (1f + pressure * 0.02f);

            if ((pressure >= 16f || autoWaveCountdown <= 0f) && Time.time >= nextWave)
            {
                autoWaveCountdown = NextAutoCountdown(waves + 1);
                Release(session.player.transform.position);
            }

            if (waveCombatActive && Time.time > nextWave - 10f && ActiveAlertedCount == 0)
            {
                waveCombatActive = false;
                clearedWaves++;
                int added = session.AddAmmoToCurrentWeapon(10);
                session.State.bandages = Mathf.Min(12, session.State.bandages + 1);
                var expander = session.GetComponent<PortMapExpander>();
                if (expander != null) expander.DeliverHordeSupplyDrop();
                TacticalAudio.Play(session.player.transform.position, TacticalCue.Swing, 1.3f);
                OnHordeCleared?.Invoke(waves);
                session.SetMessage($"¡Ronda #{waves} repelida! +{added} {session.CurrentCaliberShort}, +1 venda y Suministros en Helipuerto Sur.");
            }
        }

        private static float NextAutoCountdown(int completedWaves)
        {
            return Mathf.Max(20f, 40f - completedWaves * 3f);
        }

        public void Noise(Vector3 position, float radius)
        {
            if (!enabled || !session.Running || radius < 7f) return;
            pressure = Mathf.Min(100f, pressure + Mathf.Min(radius * 0.45f, 18f));
            if (radius >= 18f && Time.time >= nextWave) Release(position);
        }

        public void CallWaveEarly()
        {
            if (!enabled || session == null || !session.Running) return;
            nextWave = 0f;
            autoWaveCountdown = NextAutoCountdown(waves + 1);
            int spawned = Release(session.player.transform.position);
            if (spawned > 0)
            {
                int bonus = session.AddAmmoToCurrentWeapon(12);
                if (session.Night != null) session.Night.Inventory.stamina = 100f;
                session.SetMessage($"¡Ronda #{waves} adelantada ({spawned} enemigos)! Bonificacion: +{bonus} {session.CurrentCaliberShort}.");
            }
        }

        public void TriggerAlarmHorde(string reason, Vector3 origin, bool spawnBoss = false)
        {
            if (!enabled || session == null || !session.Running || reserves == null) return;
            nextWave = 0f;
            pressure = Mathf.Min(100f, pressure + 40f);
            int count = Release(origin, spawnBoss);
            if (count == 0 && spawnBoss) PromoteWaveBoss();
            TacticalAudio.Play(origin, TacticalCue.Growl, 1.35f);
            session.SetMessage(reason);
        }

        public int Release(Vector3 target) => Release(target, false);

        public int Release(Vector3 target, bool forceBoss)
        {
            if (!enabled || !session.Running || Time.time < nextWave || reserves == null) return 0;
            int alive = session.Enemies.Count(e => e != null && e.gameObject.activeInHierarchy && !e.IsDead);
            int nextWaveNumber = waves + 1;
            int desiredCount = GetWaveSpawnCount(nextWaveNumber);
            var spawnedList = new List<SandboxEnemy>(desiredCount);

            // 1. Wake authored reserves first (keep minimum safe distance from player)
            foreach (var i in Enumerable.Range(0, reserves.Length).OrderBy(i => (reserves[i].transform.position - target).sqrMagnitude))
            {
                if (released[i] || alive + spawnedList.Count >= ActiveLimit || spawnedList.Count >= desiredCount) continue;
                if (reserves[i] == null) continue;
                if (Vector3.Distance(reserves[i].transform.position, session.player.transform.position) < 8f) continue;

                released[i] = true;
                var enemy = reserves[i];
                enemy.gameObject.SetActive(true);
                ApplyWaveScaling(enemy, nextWaveNumber);
                enemy.Hear(target, 140f);
                spawnedList.Add(enemy);
            }

            // 2. If wave still needs more enemies, recycle dead reserves / dynamic pool on NavMesh spawn points
            if (spawnedList.Count < desiredCount && alive + spawnedList.Count < ActiveLimit)
            {
                RecycleIntoWave(target, nextWaveNumber, desiredCount, alive, spawnedList);
            }

            // 3. If reserves are all still alive in combat, expand with runtime clones up to ActiveLimit
            if (spawnedList.Count < desiredCount && alive + spawnedList.Count < ActiveLimit)
            {
                SpawnDynamicReinforcements(target, nextWaveNumber, desiredCount, alive, spawnedList);
            }

            if (spawnedList.Count > 0)
            {
                waves = nextWaveNumber;
                waveCombatActive = true;
                nextWave = Time.time + 15f;
                pressure = Mathf.Max(0f, pressure - 18f);
                autoWaveCountdown = NextAutoCountdown(waves);

                bool isBossWave = forceBoss || (waves >= 3 && waves % 2 == 1);
                PromoteWaveSpecials(spawnedList, waves, isBossWave);

                OnHordeStarted?.Invoke(waves, isBossWave);
                session.SetMessage(isBossWave
                    ? $"¡RONDA DE ELITE #{waves} ({spawnedList.Count} infectados)! Un Coloso Abisal lidera el asalto."
                    : $"¡RONDA #{waves} EN CURSO! {spawnedList.Count} infectados avanzan hacia tu posicion.");
            }
            return spawnedList.Count;
        }

        private void RecycleIntoWave(Vector3 target, int waveNumber, int desiredCount, int initialAlive, List<SandboxEnemy> spawnedList)
        {
            var points = PortMapExpander.ExpandedSpawnPoints;
            int pointOffset = waveNumber * 3;

            for (int i = 0; i < reserves.Length && spawnedList.Count < desiredCount && initialAlive + spawnedList.Count < ActiveLimit; i++)
            {
                var enemy = reserves[i];
                if (enemy == null || !enemy.IsDead) continue;
                if (!TryPickSpawnPoint(points, pointOffset + i, out Vector3 spawnPos)) continue;

                ReviveAndScale(enemy, spawnPos, target, waveNumber);
                spawnedList.Add(enemy);
            }

            for (int i = 0; i < dynamicPool.Count && spawnedList.Count < desiredCount && initialAlive + spawnedList.Count < ActiveLimit; i++)
            {
                var enemy = dynamicPool[i];
                if (enemy == null || !enemy.IsDead) continue;
                if (!TryPickSpawnPoint(points, pointOffset + reserves.Length + i, out Vector3 spawnPos)) continue;

                ReviveAndScale(enemy, spawnPos, target, waveNumber);
                spawnedList.Add(enemy);
            }
        }

        private void SpawnDynamicReinforcements(Vector3 target, int waveNumber, int desiredCount, int initialAlive, List<SandboxEnemy> spawnedList)
        {
            if (reserves == null || reserves.Length == 0 || dynamicPool.Count >= MaxDynamicPool) return;
            var points = PortMapExpander.ExpandedSpawnPoints;
            int attempt = 0;

            while (spawnedList.Count < desiredCount
                && initialAlive + spawnedList.Count < ActiveLimit
                && dynamicPool.Count < MaxDynamicPool
                && attempt < 24)
            {
                attempt++;
                if (!TryPickSpawnPoint(points, waveNumber * 5 + dynamicPool.Count + attempt, out Vector3 spawnPos)) continue;

                var template = reserves[(waveNumber + dynamicPool.Count) % reserves.Length];
                if (template == null) continue;

                bool wasActive = template.gameObject.activeSelf;
                template.gameObject.SetActive(false);
                var cloneObj = Instantiate(template.gameObject, spawnPos, Quaternion.identity, template.transform.parent);
                template.gameObject.SetActive(wasActive);

                cloneObj.name = $"WaveReinforcement_{dynamicPool.Count:00}";
                var clone = cloneObj.GetComponent<SandboxEnemy>();
                if (clone == null)
                {
                    Destroy(cloneObj);
                    continue;
                }

                clone.session = session;
                var existingSpecial = cloneObj.GetComponent<SpecialInfected>();
                if (existingSpecial != null) Destroy(existingSpecial);
                if (cloneObj.GetComponent<AttackPose>() == null) cloneObj.AddComponent<AttackPose>();
                var loot = cloneObj.GetComponent<LootContainer>();
                if (loot != null) loot.id = "corpse-dynamic-" + dynamicPool.Count;

                dynamicPool.Add(clone);
                session.RegisterDynamicEnemy(clone);

                cloneObj.SetActive(true);
                var gore = clone.GetComponent<Dismemberment>();
                if (gore != null) gore.Restore(Severed.None);
                clone.ResetWaveMultipliers();
                clone.Restore(clone.MaxHealth, spawnPos);
                ApplyWaveScaling(clone, waveNumber);
                clone.Hear(target, 140f);
                spawnedList.Add(clone);
            }
        }

        private void ReviveAndScale(SandboxEnemy enemy, Vector3 spawnPos, Vector3 target, int waveNumber)
        {
            var gore = enemy.GetComponent<Dismemberment>();
            if (gore != null) gore.Restore(Severed.None);
            enemy.gameObject.SetActive(true);
            enemy.ResetWaveMultipliers();
            enemy.Restore(enemy.MaxHealth, spawnPos);
            ApplyWaveScaling(enemy, waveNumber);
            enemy.Hear(target, 140f);
        }

        private bool TryPickSpawnPoint(Vector3[] points, int seed, out Vector3 result)
        {
            result = Vector3.zero;
            if (points == null || points.Length == 0 || session == null || session.player == null) return false;
            Vector3 playerPos = session.player.transform.position;

            for (int offset = 0; offset < points.Length; offset++)
            {
                Vector3 candidate = points[(seed + offset) % points.Length];
                if (Vector3.Distance(candidate, playerPos) < 11f) continue;
                if (NavMesh.SamplePosition(candidate, out var navHit, 6f, NavMesh.AllAreas))
                {
                    result = navHit.position + Vector3.up * 0.1f;
                    return true;
                }
            }

            if (reserves != null)
            {
                for (int i = 0; i < reserves.Length; i++)
                {
                    var r = reserves[(seed + i) % reserves.Length];
                    if (r == null) continue;
                    Vector3 candidate = r.transform.position;
                    if (Vector3.Distance(candidate, playerPos) < 10f) continue;
                    if (NavMesh.SamplePosition(candidate, out var navHit, 4f, NavMesh.AllAreas))
                    {
                        result = navHit.position + Vector3.up * 0.1f;
                        return true;
                    }
                }
            }
            return false;
        }

        private void ApplyWaveScaling(SandboxEnemy enemy, int waveNumber)
        {
            if (enemy == null) return;
            int w = Mathf.Max(1, waveNumber);
            float hpMult = GetDifficultyMultiplier(w);
            float spdMult = 1.06f + Mathf.Min(0.45f, (w - 1) * 0.06f);
            float dmgMult = 1.15f + Mathf.Min(1.25f, (w - 1) * 0.16f);
            float sightBonus = Mathf.Min(6f, 2f + (w - 1) * 0.6f);
            enemy.ApplyWaveMultipliers(hpMult, spdMult, dmgMult, sightBonus);
        }

        private void PromoteWaveSpecials(List<SandboxEnemy> spawnedList, int waveNumber, bool bossWave)
        {
            if (spawnedList == null || spawnedList.Count == 0) return;

            SandboxEnemy bossTarget = null;
            if (bossWave)
            {
                bossTarget = spawnedList[spawnedList.Count - 1];
                PromoteEnemyToSpecial(bossTarget, SpecialSpecies.Colossus, true, waveNumber);
            }

            int eliteQuota = waveNumber >= 3 ? 2 : 1;
            int promoted = 0;
            for (int i = 0; i < spawnedList.Count && promoted < eliteQuota; i++)
            {
                var candidate = spawnedList[i];
                if (candidate == null || candidate == bossTarget || candidate.IsDead) continue;
                if (candidate.GetComponent<SpecialInfected>() != null) continue;

                SpecialSpecies species = (waveNumber + promoted) % 2 == 0
                    ? SpecialSpecies.Colossus
                    : SpecialSpecies.Bloater;
                PromoteEnemyToSpecial(candidate, species, false, waveNumber);
                promoted++;
            }
        }

        private void PromoteWaveBoss()
        {
            var candidate = session.Enemies.FirstOrDefault(e => e != null && e.gameObject.activeInHierarchy && !e.IsDead && e.GetComponent<SpecialInfected>() == null);
            if (candidate != null) PromoteEnemyToSpecial(candidate, SpecialSpecies.Colossus, true, Mathf.Max(1, waves));
        }

        private void PromoteEnemyToSpecial(SandboxEnemy enemy, SpecialSpecies species, bool boss, int waveNumber)
        {
            if (enemy == null) return;
            var special = enemy.GetComponent<SpecialInfected>();
            if (special == null) special = enemy.gameObject.AddComponent<SpecialInfected>();
            special.Initialize(session, species, boss);
            if (waveNumber > 1)
            {
                float hpMult = 1f + Mathf.Min(1.2f, (waveNumber - 1) * 0.15f);
                float spdMult = 1f + Mathf.Min(0.30f, (waveNumber - 1) * 0.04f);
                float dmgMult = 1f + Mathf.Min(0.80f, (waveNumber - 1) * 0.12f);
                enemy.ApplyWaveMultipliers(hpMult, spdMult, dmgMult, 3f);
            }
            if (boss) ActiveBoss = enemy;
        }

        public void Restore(bool[] flags, float value, int waveCount)
        {
            released = flags.ToArray(); pressure = value; waves = waveCount; nextWave = Time.time + 8f;
            autoWaveCountdown = NextAutoCountdown(waves);
            waveCombatActive = false;
            for (int i = 0; i < reserves.Length; i++) reserves[i].gameObject.SetActive(released[i]);
        }
    }
}
