using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zombtoy.Core;

namespace Zombtoy.Gameplay
{
    public enum SessionMode { Title, Playing, Paused, Dead, Complete }

    public sealed class SandboxSession : MonoBehaviour
    {
        public SurvivalConfig config;
        public SandboxPlayer player;
        public Transform terminal;
        public Transform extraction;
        public Material tracerMaterial;
        public Font hudFont, displayFont;
        public CombatArt combatArt;
        public Material damageVignetteMaterial;
        public PortMission portMission;
        public string missionTitle = "ABISAL / PUERTO CARGUERO";
        public SurvivalState State = new SurvivalState();
        public int rounds = 12, reserve = 36;
        public int Kills { get; set; }
        public int Shots { get; private set; }
        public int Hits { get; private set; }
        public bool Injecting => action == "suero" && actionLeft > 0;
        public bool Busy => actionLeft > 0 || reloadLeft > 0;
        public bool Reloading => reloadLeft > 0;
        public float ActionProgress => actionLeft > 0 ? 1 - actionLeft / actionDuration : reloadLeft > 0 ? 1 - reloadLeft / 1.5f : 0;
        public string ActionLabel => actionLeft > 0 ? "Aplicando " + action : Reloading ? "Recargando" : string.Empty;
        public bool Running => Mode == SessionMode.Playing && State.health > 0;
        public SessionMode Mode { get; private set; } = SessionMode.Playing;
        public string Message { get; private set; } = "";
        public float MessageUntil { get; private set; }
        public float DamageFlash { get; private set; }
        public float HitFlash { get; private set; }
        public GameHud Hud { get; private set; }
        public NightSurvival Night { get; private set; }
        public RunTelemetry Telemetry { get; private set; }
        public SandboxEnemy[] Enemies { get; private set; }
        public WorldPickup[] Pickups { get; private set; }
        public string CheckpointPathOverride { get; set; }
        public bool HasCheckpoint => CheckpointStore.TryRead(SavePath, out _);
        private string SavePath => string.IsNullOrEmpty(CheckpointPathOverride) ? CheckpointStore.DefaultPath : CheckpointPathOverride;
        private bool terminalActivated;
        private string action;
        private float actionLeft, actionDuration, reloadLeft, immuneUntil;

        private void Start()
        {
            Night = GetComponent<NightSurvival>();
            CombatEffects.Configure(combatArt);
            Telemetry = gameObject.AddComponent<RunTelemetry>(); Telemetry.Initialize(this);
            Enemies = FindObjectsByType<SandboxEnemy>(FindObjectsInactive.Include);
            Pickups = FindObjectsByType<WorldPickup>(FindObjectsInactive.Include);
            Application.targetFrameRate = 60;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Hud = gameObject.AddComponent<GameHud>();
            Hud.Setup(this);
            Mode = portMission == null ? SessionMode.Playing : SessionMode.Title;
            ApplyTime();
        }

        private void Update()
        {
            DamageFlash = Mathf.MoveTowards(DamageFlash, 0, Time.unscaledDeltaTime * 2);
            HitFlash = Mathf.MoveTowards(HitFlash, 0, Time.unscaledDeltaTime * 6);
            var keys = Keyboard.current;
            if (keys != null)
            {
                if (keys.escapeKey.wasPressedThisFrame) TogglePause();
                if (keys.enterKey.wasPressedThisFrame && Mode == SessionMode.Title) BeginMission();
                else if (keys.enterKey.wasPressedThisFrame && (Mode == SessionMode.Complete || Mode == SessionMode.Dead)) Restart();
                if (keys.f1Key.wasPressedThisFrame) Hud.ToggleHelp();
                if (Running && keys.tabKey.wasPressedThisFrame) Hud.ToggleEquipment();
                if (Running && keys.f5Key.wasPressedThisFrame) SaveCheckpoint();
                if (Running && keys.f9Key.wasPressedThisFrame) LoadCheckpoint();
            }
            if (State.health <= 0 && Mode == SessionMode.Playing)
            {
                actionLeft = reloadLeft = 0;
                Mode = SessionMode.Dead;
                ApplyTime();
            }
            if (!Running) return;
            State.Tick(Time.deltaTime, config);
            if (actionLeft > 0)
            {
                actionLeft -= Time.deltaTime;
                if (actionLeft <= 0)
                {
                    float serumBefore = State.serum;
                    bool applied = action == "suero" ? State.Inject(config) : action == "botiquin" ? State.Heal() : State.Bandage();
                    if (applied && action == "suero") Telemetry.Injection(serumBefore);
                    if (applied) SetMessage(action == "suero" ? "Suero administrado." : action == "botiquin" ? "Vida recuperada: +40." : "Hemorragia detenida.");
                }
            }
            if (reloadLeft > 0)
            {
                reloadLeft -= Time.deltaTime;
                if (reloadLeft <= 0)
                {
                    int count = Mathf.Min(12 - rounds, reserve);
                    rounds += count; reserve -= count;
                }
            }
        }

        public void BeginMission()
        {
            if (Mode != SessionMode.Title) return;
            Hud.ClosePanels();
            Mode = SessionMode.Playing; ApplyTime();
            SetMessage("WASD para moverte. Busca el repuesto del almacen.");
        }
        public void TogglePause()
        {
            if (Mode != SessionMode.Playing && Mode != SessionMode.Paused) return;
            Mode = Mode == SessionMode.Playing ? SessionMode.Paused : SessionMode.Playing;
            Hud.ClosePanels(); ApplyTime();
        }
        public void Resume()
        {
            if (Mode != SessionMode.Paused) return;
            Mode = SessionMode.Playing; Hud.ClosePanels(); ApplyTime();
        }
        public void Restart() { Telemetry?.Export("Restart"); Time.timeScale = 1; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        public void ReturnToTitle() => Restart();
        public void Quit()
        {
            Time.timeScale = 1;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        private void ApplyTime() { Time.timeScale = Running ? 1 : 0; AudioListener.pause = !Running; }
        public bool UseRound()
        {
            if (!Running || rounds <= 0 || Busy) return false;
            rounds--; Shots++; return true;
        }
        public void RegisterHit() { Hits++; HitFlash = 1; }
        public void Reload()
        {
            if (!Running || rounds >= 12 || reserve <= 0 || Busy) return;
            reloadLeft = 1.5f; EmitNoise(player.transform.position, 4);
        }
        public void BeginInjection() => BeginUse("suero", config.injectionSeconds);
        public void BeginHeal() => BeginUse("botiquin", 1.6f);
        public void BeginBandage() => BeginUse("venda", 1.2f);
        private void BeginUse(string item, float duration)
        {
            if (!Running || Busy) return;
            if (State.InventoryLocked) { SetMessage("Mania: equipo bloqueado temporalmente."); return; }
            bool usable = item == "suero" ? State.doses > 0 : item == "botiquin" ? State.medkits > 0 && State.health < 100 : State.bandages > 0 && State.bleeding > 0;
            if (!usable) { SetMessage("No puedes usar " + item + " ahora."); return; }
            action = item; actionDuration = actionLeft = duration;
            EmitNoise(player.transform.position, item == "suero" ? 7 : 3);
        }
        public void TakeDamage(float amount, bool wound = true)
        {
            if (!Running || amount <= 0 || Time.time < immuneUntil) return;
            immuneUntil = Time.time + 0.6f;
            if (wound && Night != null) amount = Night.Absorb(amount);
            State.health = Mathf.Max(0, State.health - amount);
            if (wound) State.bleeding = Mathf.Min(1, State.bleeding + 0.2f);
            actionLeft = 0; DamageFlash = 1;
            SetMessage(wound ? "Herida abierta. Usa B para vendarte." : "Has recibido dano.");
        }
        public WorldPickup NearestPickup()
        {
            if (Pickups == null) return null;
            WorldPickup best = null; float distance = 2.2f;
            foreach (var item in Pickups)
            {
                if (item == null || !item.gameObject.activeInHierarchy || item.Collected) continue;
                float d = Vector3.Distance(player.transform.position + Vector3.up * 0.6f, item.transform.position);
                if (d < distance && item.CanReach(player.transform)) { best = item; distance = d; }
            }
            return best;
        }
        public void Interact()
        {
            if (!Running || Busy) return;
            var loot = Night != null ? Night.NearestLoot() : null;
            if (loot != null) { Hud.OpenLoot(loot); return; }
            var pickup = NearestPickup();
            if (pickup != null) { pickup.Collect(this); return; }
            if (portMission != null) { portMission.Interact(); return; }
            if (Vector3.Distance(player.transform.position, terminal.position) < 2.5f)
            {
                terminalActivated = true; SetMessage("Terminal activada. Busca EXTRACCION.");
                EmitNoise(terminal.position, 12);
            }
            else if (Vector3.Distance(player.transform.position, extraction.position) < 2.5f)
            {
                if (terminalActivated) CompleteRoute("Ruta completada.");
                else SetMessage("Activa primero la terminal.");
            }
        }
        public void ClearMessage() {Message="";MessageUntil=0;}
        public void SetMessage(string value) { Message = value; MessageUntil = Time.unscaledTime + 5; }
        public void CompleteRoute(string value) { Mode = SessionMode.Complete; SetMessage(value); ApplyTime(); }
        public void EmitNoise(Vector3 position, float radius)
        {
            if (Night != null) Night.Noise(position, radius);
            if (Enemies == null) return;
            foreach (var enemy in Enemies)
                if (enemy != null && !enemy.IsDead && enemy.gameObject.activeSelf) enemy.Hear(position, radius);
        }
        public bool SaveCheckpoint(bool automatic = false)
        {
            if (!Running || portMission == null || Busy) return false;
            if (!automatic && Array.Exists(Enemies, e => e != null && e.gameObject.activeInHierarchy && !e.IsDead && Vector3.Distance(e.transform.position, player.transform.position) < 7))
            { SetMessage("Alejate de los infectados para guardar."); return false; }
            bool saved = CheckpointStore.Save(SavePath, this);
            SetMessage(saved ? "Checkpoint guardado." : "No se pudo guardar. Comprueba el espacio en disco.");
            return saved;
        }
        public bool LoadCheckpoint()
        {
            if (portMission == null || !CheckpointStore.TryRead(SavePath, out var save))
            { SetMessage("No hay un checkpoint valido para este puerto."); return false; }
            CheckpointStore.Restore(this, save);
            actionLeft = reloadLeft = 0; immuneUntil = Time.time + 1;
            Mode = SessionMode.Playing; Hud.ClosePanels(); ApplyTime(); SetMessage("Checkpoint recuperado."); return true;
        }
        public void RestoreCounters(int kills, int shots, int hits) { Kills = kills; Shots = shots; Hits = hits; }
        private void OnDestroy() { Time.timeScale = 1; AudioListener.pause = false; }
    }
}
