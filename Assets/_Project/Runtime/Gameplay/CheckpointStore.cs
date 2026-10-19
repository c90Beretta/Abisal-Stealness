using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Zombtoy.Core;

namespace Zombtoy.Gameplay
{
    [Serializable] public sealed class EnemySave { public string id; public float health; public Vector3 position; public bool active = true; public Severed severed; public RagdollPose ragdoll; public Quaternion rotation = Quaternion.identity; }
    [Serializable] public sealed class PickupSave { public string id; public bool collected; }
    [Serializable] public sealed class LootSave { public string id; public LootStock stock; }
    [Serializable]
    public sealed class PortSave
    {
        public int version = 3;
        public string level = "Level01_Port";
        public SurvivalState state;
        public Vector3 position;
        public int rounds, reserve, kills, shots, hits, weapon;
        public bool fuse, power, boarding, supplies;
        public EnemySave[] enemies;
        public PickupSave[] pickups;
        public FieldInventory inventory;
        public LootSave[] loot;
        public bool[] released;
        public float pressure;
        public int waves;
        public RunMetrics metrics;
    }
    public static class CheckpointStore
    {
        public static string DefaultPath => Path.Combine(Application.persistentDataPath, "port01-checkpoint.json");
        public static bool Save(string path, SandboxSession session)
        {
            try
            {
                var m = session.portMission;
                var save = new PortSave
                {
                    state = session.State,
                    position = session.player.transform.position,
                    rounds = session.rounds,
                    reserve = session.reserve,
                    kills = session.Kills,
                    shots = session.Shots,
                    hits = session.Hits,
                    metrics = session.Telemetry?.Data,
                    weapon = session.player.GetComponent<WeaponLoadout>()?.Index ?? 0,
                    fuse = m.HasFuse,
                    power = m.PowerOn,
                    boarding = m.BoardingAuthorized,
                    supplies = m.SuppliesTaken,
                    enemies = session.Enemies.Select(e => new EnemySave { id = e.name, health = e.health, position = e.transform.position, rotation = e.transform.rotation, active = e.gameObject.activeSelf, severed = e.GetComponent<Dismemberment>()?.Lost ?? Severed.None, ragdoll = e.GetComponent<RagdollController>()?.Capture() }).ToArray(),
                    pickups = session.Pickups.Select(p => new PickupSave { id = p.id, collected = p.Collected }).ToArray()
                };
                if (session.Night != null)
                {
                    var n = session.Night; save.inventory = n.Inventory;
                    save.loot = n.Containers.Select(c => new LootSave { id = c.id, stock = c.stock }).ToArray();
                    save.released = n.hordes.released; save.pressure = n.hordes.pressure; save.waves = n.hordes.waves;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path + ".tmp", JsonUtility.ToJson(save, true));
                if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
                else File.Move(path + ".tmp", path);
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException) { return false; }
        }
        public static bool TryRead(string path, out PortSave save)
        {
            save = null;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 1024 * 1024) return false;
                var s = JsonUtility.FromJson<PortSave>(File.ReadAllText(path));
                if (s == null || (s.version != 1 && s.version != 2 && s.version != 3) || s.level != "Level01_Port" || s.state == null || s.enemies == null || s.pickups == null ||
                    !float.IsFinite(s.state.health) || s.state.health <= 0 || s.state.health > 100 || !Finite(s.position) ||
                    !Bounded(s.state.serum, 0, 100) || !Bounded(s.state.infection, 0, 100) || !Bounded(s.state.bleeding, 0, 1) ||
                    !Bounded(s.state.elapsed, 0, 10000000) || !float.IsFinite(s.state.lastInjection) || !float.IsFinite(s.state.maniaUntil) ||
                    !float.IsFinite(s.state.inventoryLockedUntil) || !float.IsFinite(s.state.tremorUntil) ||
                    (s.power && !s.fuse) || (s.boarding && !s.power) || s.pickups.Any(p => p == null || string.IsNullOrEmpty(p.id)) ||
                    s.rounds < 0 || s.rounds > 12 || s.reserve < 0 || s.state.doses < 0 || s.state.medkits < 0 || s.state.bandages < 0 ||
                    s.kills < 0 || s.shots < 0 || s.hits < 0 || s.weapon < 0 || s.weapon > 2 ||
                    s.enemies.Any(e => e == null || string.IsNullOrEmpty(e.id) || !Finite(e.position) || !Bounded(e.health, 0, 1000))) return false;
                if (s.version >= 2 && (s.inventory == null || !s.inventory.Valid || s.loot == null || s.released == null || s.released.Length > 64 || s.waves < 0 || !Bounded(s.pressure, 0, 100) ||
                    s.loot.Any(l => l == null || string.IsNullOrEmpty(l.id) || l.stock == null || !l.stock.Valid) || s.loot.Select(l => l.id).Distinct().Count() != s.loot.Length ||
                    s.enemies.Any(e => (s.version >= 3 && (!float.IsFinite(e.rotation.x) || !float.IsFinite(e.rotation.y) || !float.IsFinite(e.rotation.z) || !float.IsFinite(e.rotation.w) || Mathf.Abs(Quaternion.Dot(e.rotation, e.rotation) - 1) > .05f)) || ((int)e.severed & ~31) != 0 || (e.ragdoll != null && e.ragdoll.present && !e.ragdoll.Valid)))) return false;
                if (s.metrics != null && !s.metrics.Valid) return false;
                save = s; return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException) { return false; }
        }
        private static bool Finite(Vector3 p) => float.IsFinite(p.x) && float.IsFinite(p.y) && float.IsFinite(p.z);
        private static bool Bounded(float value, float min, float max) => float.IsFinite(value) && value >= min && value <= max;
        public static void Restore(SandboxSession session, PortSave save)
        {
            session.State = JsonUtility.FromJson<SurvivalState>(JsonUtility.ToJson(save.state));
            session.rounds = save.rounds; session.reserve = save.reserve;
            session.RestoreCounters(save.kills, save.shots, save.hits);
            var loadout = session.player.GetComponent<WeaponLoadout>();
            if (loadout != null) loadout.Equip(save.weapon, true);
            var cc = session.player.GetComponent<CharacterController>();
            cc.enabled = false; session.player.transform.position = save.position; cc.enabled = true;
            session.portMission.Restore(save.fuse, save.power, save.boarding, save.supplies);
            if (session.Night != null && save.inventory != null)
            {
                var n = session.Night;
                n.Inventory = JsonUtility.FromJson<FieldInventory>(JsonUtility.ToJson(save.inventory));
                if (save.released.Length == n.hordes.reserves.Length) n.hordes.Restore(save.released, save.pressure, save.waves);
                foreach (var c in n.Containers) { var entry = Array.Find(save.loot, l => l.id == c.id); if (entry != null) c.stock = entry.stock.Copy(); }
            }
            foreach (var enemy in session.Enemies)
            {
                var data = Array.Find(save.enemies, e => e.id == enemy.name);
                if (data != null)
                {
                    if (save.version >= 2) enemy.gameObject.SetActive(data.active);
                    if (save.version >= 3) enemy.transform.rotation = data.rotation;
                    enemy.Restore(data.health, data.position);
                    enemy.GetComponent<Dismemberment>()?.Restore(data.severed);
                    var ragdoll = enemy.GetComponent<RagdollController>();
                    if (enemy.IsDead && ragdoll != null)
                    {
                        if (!ragdoll.RestorePose(data.ragdoll)) ragdoll.Activate(Vector3.zero, enemy.transform.position + Vector3.up, data.severed);
                    }
                }
            }
            foreach (var pickup in session.Pickups)
            {
                var data = Array.Find(save.pickups, p => p.id == pickup.id);
                pickup.Restore(data != null && data.collected);
            }
            session.Telemetry?.Restore(save.metrics);
            Physics.SyncTransforms();
        }
    }
}
