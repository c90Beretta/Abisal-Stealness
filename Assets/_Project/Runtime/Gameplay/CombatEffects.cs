using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    public static class CombatEffects
    {
        private static readonly Queue<GameObject> objects = new Queue<GameObject>();
        public static int LiveCount { get { Prune(); return objects.Count; } }
        private static void Prune() { while (objects.Count > 0 && objects.Peek() == null) objects.Dequeue(); }
        private static GameObject Track(GameObject go, float seconds)
        {
            Prune(); while (objects.Count >= 90) { var old = objects.Dequeue(); if (old != null) Object.Destroy(old); }
            objects.Enqueue(go); Object.Destroy(go, seconds); return go;
        }
        private static GameObject Piece(string name, Vector3 position, Vector3 size, Color color, Material material, float life, Vector3 velocity)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.GetComponent<Collider>().enabled = false; Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = position; go.transform.localScale = size; go.transform.rotation = Random.rotation;
            var renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", color); block.SetColor("_Color", color); renderer.SetPropertyBlock(block);
            var effect = go.AddComponent<CombatEffectLifetime>(); effect.velocity = velocity; effect.gravity = velocity.sqrMagnitude > 0;
            Track(go, life); return go;
        }
        public static void Shot(Vector3 point, Transform player, Material material)
        {
            Piece("Muzzle flash", point, new Vector3(.12f, .12f, .32f), new Color(4, 2.5f, .5f), material, .045f, Vector3.zero).transform.rotation = player.rotation;
            var go = Track(new GameObject("Muzzle light"), .055f); go.transform.position = point;
            var light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = new Color(1, .65f, .25f); light.intensity = 4; light.range = 4;
            Piece("Ejected casing", player.position + Vector3.up * 1.2f + player.right * .3f, new Vector3(.035f, .035f, .08f), new Color(.7f, .5f, .16f), material, 3,
                player.right * 1.7f + Vector3.up * 1.4f);
            Piece("Muzzle smoke", point, Vector3.one * .08f, new Color(.26f, .3f, .28f), material, .25f, Vector3.up * .8f).GetComponent<CombatEffectLifetime>().expand = true;
            player.GetComponent<SandboxPlayer>().view.GetComponent<CameraTrauma>()?.Add(.08f);
        }
        public static void Blood(Vector3 point, Material material, int drops = 5)
        {
            for (int i = 0; i < drops; i++) Piece("Blood spray", point, Vector3.one * Random.Range(.035f, .08f), new Color(.35f, .008f, .014f), material, .45f, Random.insideUnitSphere * 2 + Vector3.up);
            var pool = Piece("Blood pool", new Vector3(point.x, .045f, point.z), new Vector3(.4f, .008f, .5f), new Color(.16f, .006f, .012f), material, 80, Vector3.zero);
            pool.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }
        public static void Impact(Vector3 point, Vector3 normal, Material material)
        {
            for (int i = 0; i < 4; i++) Piece("Impact spark", point + normal * .03f, Vector3.one * .035f, new Color(2, 1.1f, .3f), material, .18f, normal * 1.4f + Random.insideUnitSphere);
        }
        public static void Cry(Vector3 point, Material material)
        {
            for (int i = 0; i < 16; i++) { float a = i * Mathf.PI / 8; Piece("Cry pulse", point, Vector3.one * .035f, new Color(.7f, .3f, .15f), material, .45f, new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * 5); }
        }
        public static void Swing(Transform player, Material material)
        {
            var line = Track(new GameObject("Melee arc"), .16f).AddComponent<LineRenderer>(); line.sharedMaterial = material; line.startWidth = .04f; line.endWidth = .01f;
            line.startColor = line.endColor = new Color(.55f, .8f, .7f); line.positionCount = 13;
            for (int i = 0; i < 13; i++) line.SetPosition(i, player.position + Vector3.up * .95f + Quaternion.AngleAxis(-55 + i * 9, Vector3.up) * player.forward * 1.4f);
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
        public void Add(float amount) { trauma = Mathf.Min(.28f, trauma + amount); }
        private void LateUpdate()
        {
            if (Time.timeScale <= 0) return;
            transform.position += transform.right * Mathf.Sin(Time.time * 130) * trauma + transform.up * Mathf.Sin(Time.time * 87) * trauma * .55f;
            trauma = Mathf.MoveTowards(trauma, 0, Time.deltaTime * .65f);
        }
    }
}
