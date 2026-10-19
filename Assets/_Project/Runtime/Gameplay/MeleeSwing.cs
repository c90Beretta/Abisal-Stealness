using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class MeleeSwing : MonoBehaviour
    {
        private GameObject model;
        private Transform firearm;
        private float until;
        public bool Active => model != null && Time.time < until;
        public void Play(bool axe, Material material)
        {
            if (model != null) Destroy(model);
            firearm = GetComponent<WeaponLoadout>()?.socket;
            if (firearm != null) firearm.gameObject.SetActive(false);
            model = new GameObject(axe ? "Boarding axe" : "Combat knife"); model.transform.SetParent(transform, false);
            Part("Grip", new Vector3(0, -.12f, 0), new Vector3(.045f, axe ? .55f : .18f, .045f), new Color(.035f, .04f, .04f), material);
            Part("Steel blade", new Vector3(axe ? .08f : 0, axe ? .2f : .14f, 0), new Vector3(axe ? .3f : .045f, axe ? .16f : .28f, .025f), new Color(.55f, .65f, .65f), material);
            until = Time.time + .24f;
        }
        private void Part(string label, Vector3 pos, Vector3 size, Color color, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube); part.name = label; part.GetComponent<Collider>().enabled = false; Destroy(part.GetComponent<Collider>());
            part.transform.SetParent(model.transform, false); part.transform.localPosition = pos; part.transform.localScale = size;
            var r = part.GetComponent<Renderer>(); r.sharedMaterial = material; var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", color); block.SetColor("_Color", color); r.SetPropertyBlock(block);
        }
        private void Update()
        {
            if (model == null) return;
            if (Time.time >= until) { Destroy(model); if (firearm != null) firearm.gameObject.SetActive(true); return; }
            float t = 1 - (until - Time.time) / .24f; model.transform.localPosition = new Vector3(.25f, 1, .35f);
            model.transform.localRotation = Quaternion.Euler(55, -70 + 140 * t, -20);
        }
        private void OnDisable() { if (model != null) Destroy(model); if (firearm != null) firearm.gameObject.SetActive(true); }
    }
}
