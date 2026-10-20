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
        public int Remaining => released == null ? 0 : released.Count(value => !value);
        private void Awake() { if (released == null || released.Length != reserves.Length) released = new bool[reserves.Length]; }
        private void Update()
        {
            if (!session.Running) return;
            pressure = Mathf.MoveTowards(pressure, 0, Time.deltaTime * .8f);
            if (pressure >= 20 && Time.time >= nextWave) Release(session.player.transform.position);
        }
        public void Noise(Vector3 position, float radius)
        {
            if (!enabled || !session.Running || radius < 7) return;
            pressure = Mathf.Min(100, pressure + Mathf.Min(radius * .35f, 14));
            if (radius >= 20 && Time.time >= nextWave) Release(position);
        }
        public int Release(Vector3 target)
        {
            if (!enabled || !session.Running || Time.time < nextWave || reserves == null) return 0;
            int alive = session.Enemies.Count(e => e != null && e.gameObject.activeInHierarchy && !e.IsDead);
            int count = 0;
            // Authored reserves wake in place; nothing materializes inside the player's light.
            foreach (var i in Enumerable.Range(0, reserves.Length).OrderBy(i => (reserves[i].transform.position - target).sqrMagnitude))
            {
                if (released[i] || alive + count >= ActiveLimit || count >= 4) continue;
                if (Vector3.Distance(reserves[i].transform.position, session.player.transform.position) < 9) continue;
                released[i] = true; reserves[i].gameObject.SetActive(true); reserves[i].Hear(target, 100); count++;
            }
            if (count > 0) { waves++; nextWave = Time.time + 24; pressure = Mathf.Max(0, pressure - 20); session.SetMessage("Ruido de horda en el puerto. Apaga la linterna y rompe el contacto."); }
            return count;
        }
        public void Restore(bool[] flags, float value, int waveCount)
        {
            released = flags.ToArray(); pressure = value; waves = waveCount; nextWave = Time.time + 8;
            for (int i = 0; i < reserves.Length; i++) reserves[i].gameObject.SetActive(released[i]);
        }
    }
}
