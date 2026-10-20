using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    public static class CombatEffects
    {
        private static readonly Queue<GameObject> objects = new Queue<GameObject>();
        private static CombatArt art;
        public static void Configure(CombatArt value) { art = value; }
        public static int LiveCount { get { Prune(); return objects.Count; } }
        private static void Prune() { while (objects.Count > 0 && objects.Peek() == null) objects.Dequeue(); }
        private static GameObject Track(GameObject go, float seconds)
        {
            Prune(); while (objects.Count >= 90) { var old = objects.Dequeue(); if (old != null) Object.Destroy(old); }
            objects.Enqueue(go); Object.Destroy(go, seconds); return go;
        }
        public static void Shot(Vector3 point, Transform player, Material material)
        {
            Burst("Muzzle flash", point, player.forward, new Color(4, 2.2f, .65f, .9f), 3, .19f, 1, .055f, 0);
            var go = Track(new GameObject("Muzzle light"), .055f); go.transform.position = point;
            var light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = new Color(1, .65f, .25f); light.intensity = 4; light.range = 4;
            if (art != null && art.casing != null)
            {
                var casing = new GameObject("Ejected brass", typeof(MeshFilter), typeof(MeshRenderer)); casing.transform.position = player.position + Vector3.up * 1.3f + player.right * .25f;
                casing.GetComponent<MeshFilter>().sharedMesh = art.casing; casing.GetComponent<MeshRenderer>().sharedMaterial = art.brass;
                var motion = casing.AddComponent<CombatEffectLifetime>(); motion.velocity = player.right * 1.6f + Vector3.up * 1.3f; motion.gravity = true; Track(casing, 4);
            }
            Burst("Muzzle smoke", point, Vector3.up, new Color(.45f, .43f, .37f, .18f), 3, .12f, .45f, .65f, -.025f);
            player.GetComponent<SandboxPlayer>().view.GetComponent<CameraTrauma>()?.Add(.08f);
        }
        public static void Blood(Vector3 point, Material material, int drops = 5)
        {
            Burst("Blood spray", point, Vector3.up, new Color(.28f, .008f, .01f, .9f), Mathf.Clamp(drops, 1, 16), .065f, 1.8f, .4f, .7f);
            if (art == null || art.bloodPool == null) return;
            var pool = new GameObject("Blood stain", typeof(MeshFilter), typeof(MeshRenderer)); pool.transform.position = new Vector3(point.x, .066f + Random.value * .002f, point.z);
            pool.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0); pool.transform.localScale = Vector3.one * Random.Range(.55f, 1.05f);
            pool.GetComponent<MeshFilter>().sharedMesh = art.bloodPool; pool.GetComponent<MeshRenderer>().sharedMaterial = art.blood;
            pool.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; Track(pool, 80);
        }
        public static void Impact(Vector3 point, Vector3 normal, Material material)
        {
            Burst("Impact sparks", point + normal * .025f, normal, new Color(3, 1.6f, .5f, .9f), 5, .028f, 2.8f, .18f, .2f);
        }
        public static void Cry(Vector3 point, Material material)
        {
            Burst("Infected breath", point, Vector3.forward, new Color(.42f, .3f, .25f, .18f), 4, .15f, .5f, .5f, -.02f);
        }
        public static void Swing(Transform player, Material material)
        {
            var line = Track(new GameObject("Melee arc"), .16f).AddComponent<LineRenderer>(); line.sharedMaterial = material; line.startWidth = .04f; line.endWidth = .01f;
            line.startColor = line.endColor = new Color(.8f, .75f, .61f, .35f); line.positionCount = 13;
            for (int i = 0; i < 13; i++) line.SetPosition(i, player.position + Vector3.up * .95f + Quaternion.AngleAxis(-55 + i * 9, Vector3.up) * player.forward * 1.4f);
        }
        private static void Burst(string label, Vector3 point, Vector3 direction, Color color, int count, float size, float speed, float lifetime, float gravity)
        {
            if (art == null || art.particles == null) return;
            var go = new GameObject(label); go.transform.position = point; var particles = go.AddComponent<ParticleSystem>(); particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main; main.loop = false; main.playOnAwake = false; main.startLifetime = lifetime; main.startSize = size; main.startSpeed = 0; main.gravityModifier = gravity; main.maxParticles = 32; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission; emission.enabled = false; var shape = particles.shape; shape.enabled = false;
            var fade = particles.colorOverLifetime; fade.enabled = true; var gradient = new Gradient(); gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) }); fade.color = gradient;
            var scale = particles.sizeOverLifetime; scale.enabled = true; scale.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .6f), new Keyframe(.3f, 1), new Keyframe(1, .15f)));
            var renderer = particles.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = art.particles; renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            particles.Play(); for (int i = 0; i < count; i++) particles.Emit(new ParticleSystem.EmitParams { velocity = direction * speed + Random.insideUnitSphere * speed * .55f, startColor = color, startSize = size * Random.Range(.7f, 1.2f), startLifetime = lifetime * Random.Range(.7f, 1.1f) }, 1);
            Track(go, lifetime * 1.2f + .1f);
        }
    }
    public sealed class CombatEffectLifetime : MonoBehaviour
    {
        public Vector3 velocity; public bool gravity, expand;
        private void Update()
        {
            if (gravity) velocity += Vector3.down * 8 * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            if (transform.position.y < .055f) { var p = transform.position; p.y = .055f; transform.position = p; velocity = Vector3.zero; gravity = false; }
            if (expand) transform.localScale += Vector3.one * Time.deltaTime * .5f;
        }
    }
    [DefaultExecutionOrder(200)]
    public sealed class CameraTrauma : MonoBehaviour
    {
        private float trauma;
        public void Add(float amount) { if (PresentationOptions.ReducedEffects) return; trauma = Mathf.Min(.28f, trauma + amount); }
        private void LateUpdate()
        {
            if (Time.timeScale <= 0) return;
            transform.position += transform.right * Mathf.Sin(Time.time * 130) * trauma + transform.up * Mathf.Sin(Time.time * 87) * trauma * .55f;
            trauma = Mathf.MoveTowards(trauma, 0, Time.deltaTime * .65f);
        }
    }
}
