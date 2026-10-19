using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Zombtoy.Core;

namespace Zombtoy.Gameplay
{
    [Serializable] public sealed class EnemySave { public string id; public float health; public Vector3 position; }
    [Serializable] public sealed class PickupSave { public string id; public bool collected; }
    [Serializable] public sealed class PortSave
    {
        public int version = 1;
        public string level = "Level01_Port";
        public SurvivalState state;
        public Vector3 position;
        public int rounds, reserve, kills, shots, hits;
        public bool fuse, power, boarding, supplies;
        public EnemySave[] enemies;
        public PickupSave[] pickups;
    }
    public static class CheckpointStore
    {
        public static string DefaultPath => Path.Combine(Application.persistentDataPath, "port01-checkpoint.json");
        public static bool Save(string path, SandboxSession session)
        {
            try
            {
                var m = session.portMission;
                var save = new PortSave { state = session.State, position = session.player.transform.position,
                    rounds = session.rounds, reserve = session.reserve, kills = session.Kills, shots = session.Shots, hits = session.Hits,
                    fuse = m.HasFuse, power = m.PowerOn, boarding = m.BoardingAuthorized, supplies = m.SuppliesTaken,
                    enemies = session.Enemies.Select(e => new EnemySave { id = e.name, health = e.health, position = e.transform.position }).ToArray(),
                    pickups = session.Pickups.Select(p => new PickupSave { id = p.id, collected = p.Collected }).ToArray() };
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
                if (s == null || s.version != 1 || s.level != "Level01_Port" || s.state == null || s.enemies == null || s.pickups == null ||
                    !float.IsFinite(s.state.health) || s.state.health <= 0 || s.state.health > 100 || !Finite(s.position) ||
                    !Bounded(s.state.serum, 0, 100) || !Bounded(s.state.infection, 0, 100) || !Bounded(s.state.bleeding, 0, 1) ||
                    !Bounded(s.state.elapsed, 0, 10000000) || !float.IsFinite(s.state.lastInjection) || !float.IsFinite(s.state.maniaUntil) ||
                    !float.IsFinite(s.state.inventoryLockedUntil) || !float.IsFinite(s.state.tremorUntil) ||
                    (s.power && !s.fuse) || (s.boarding && !s.power) || s.pickups.Any(p => p == null || string.IsNullOrEmpty(p.id)) ||
                    s.rounds < 0 || s.rounds > 12 || s.reserve < 0 || s.state.doses < 0 || s.state.medkits < 0 || s.state.bandages < 0 ||
                    s.kills < 0 || s.shots < 0 || s.hits < 0 ||
                    s.enemies.Any(e => e == null || string.IsNullOrEmpty(e.id) || !Finite(e.position) || !Bounded(e.health, 0, 1000))) return false;
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
            var cc = session.player.GetComponent<CharacterController>();
            cc.enabled = false; session.player.transform.position = save.position; cc.enabled = true;
            session.portMission.Restore(save.fuse, save.power, save.boarding, save.supplies);
            foreach (var enemy in session.Enemies)
            {
                var data = Array.Find(save.enemies, e => e.id == enemy.name);
                if (data != null) enemy.Restore(data.health, data.position);
            }
            foreach (var pickup in session.Pickups)
            {
                var data = Array.Find(save.pickups, p => p.id == pickup.id);
                pickup.Restore(data != null && data.collected);
            }
            Physics.SyncTransforms();
        }
    }
}
