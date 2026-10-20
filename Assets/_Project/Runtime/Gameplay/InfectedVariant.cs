using UnityEngine;

namespace Zombtoy.Gameplay
{
    public enum InfectedKind { Walker, Runner, Brute, Screamer }
    public sealed class InfectedVariant : MonoBehaviour
    {
        public InfectedKind kind;
        public float nextCry;
        public AudioClip groan, scream;
        private AudioSource voice;
        private float nextGroan;
        public float MoveMultiplier => LegsLost > 0 ? .28f : 1;
        public int LegsLost { get; set; }
        public float DamageMultiplier => (kind == InfectedKind.Brute ? 1.5f : 1) * (ArmsLost > 0 ? .55f : 1);
        public int ArmsLost { get; set; }
        public string Label => kind == InfectedKind.Runner ? "Corredor" : kind == InfectedKind.Brute ? "Acorazado" : kind == InfectedKind.Screamer ? "Aullador" : "Errante";
        private SandboxEnemy enemy;
        private void Awake()
        {
            enemy = GetComponent<SandboxEnemy>(); voice = gameObject.AddComponent<AudioSource>(); voice.spatialBlend = 1;
            voice.minDistance = 2; voice.maxDistance = 12; voice.rolloffMode = AudioRolloffMode.Linear; voice.pitch = kind == InfectedKind.Brute ? .75f : kind == InfectedKind.Runner ? 1.2f : 1;
        }
        private void Update()
        {
            if (enemy.session == null || !enemy.session.Running || enemy.IsDead) return;
            if (groan != null && Time.time >= nextGroan && Vector3.Distance(transform.position, enemy.session.player.transform.position) < 12)
            { nextGroan = Time.time + Random.Range(5f, 9f); voice.PlayOneShot(groan, .3f); }
            if (kind != InfectedKind.Screamer || enemy.Mode != EnemyMode.Chase || Time.time < nextCry) return;
            if (scream != null) voice.PlayOneShot(scream, .65f);
            nextCry = Time.time + 18; enemy.session.EmitNoise(transform.position, 32);
            CombatEffects.Cry(transform.position + Vector3.up, enemy.session.tracerMaterial);
            enemy.session.SetMessage("Aullador: su grito atrae a otros infectados.");
        }
    }
}
