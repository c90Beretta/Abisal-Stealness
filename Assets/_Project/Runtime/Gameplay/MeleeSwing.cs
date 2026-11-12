using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class MeleeSwing : MonoBehaviour
    {
        public static readonly string[] WeaponNames =
        {
            "Cuchillo KM-9",
            "Hacha de abordaje",
            "Machete sierra",
            "Mazo neumatico"
        };

        private GameObject model;
        private Transform firearm;
        private float until, duration = .4f;
        private int comboStep;
        private bool heavySwing;
        private bool blocking;

        public bool Active => model != null && Time.time < until;
        public bool Blocking => blocking;

        public void Play(bool axe, Material material, GameObject prefab = null)
        {
            PlayWeapon(axe ? 1 : 0, 0, false, material, prefab);
        }

        public void PlayWeapon(int meleeKind, int combo, bool heavy, Material material, GameObject prefab = null)
        {
            blocking = false;
            if (model != null) Destroy(model);
            firearm = GetComponent<WeaponLoadout>()?.socket;
            if (firearm != null) firearm.gameObject.SetActive(false);

            comboStep = Mathf.Clamp(combo, 0, 2);
            heavySwing = heavy;
            duration = heavy ? .52f : meleeKind == 3 ? .48f : meleeKind == 1 ? .44f : .38f;
            until = Time.time + duration;

            if (prefab != null && meleeKind <= 1)
            {
                model = Instantiate(prefab, transform, false);
                model.transform.SetParent(transform, false);
                return;
            }
            model = ModelUpgradeBuilder.CreateMeleeModel(meleeKind, transform, material);
        }

        public void SetBlocking(bool active, int meleeKind, Material material)
        {
            if (Active) return;
            if (active == blocking) return;
            blocking = active;
            firearm = GetComponent<WeaponLoadout>()?.socket;
            if (!blocking)
            {
                if (model != null) Destroy(model);
                if (firearm != null) firearm.gameObject.SetActive(true);
                return;
            }
            if (model != null) Destroy(model);
            if (firearm != null) firearm.gameObject.SetActive(false);
            model = ModelUpgradeBuilder.CreateMeleeModel(meleeKind, transform, material);
            model.transform.localPosition = new Vector3(.12f, 1.18f, .42f);
            model.transform.localRotation = Quaternion.Euler(18, -55, 52);
        }

        private void Update()
        {
            if (model == null) return;
            if (blocking && Time.time >= until)
            {
                model.transform.localPosition = new Vector3(.12f, 1.18f + Mathf.Sin(Time.time * 6f) * .015f, .42f);
                model.transform.localRotation = Quaternion.Euler(18, -55, 52);
                return;
            }
            if (Time.time >= until)
            {
                Destroy(model);
                if (firearm != null) firearm.gameObject.SetActive(true);
                return;
            }
            float t = 1f - (until - Time.time) / Mathf.Max(.1f, duration);
            float swingProgress = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - .12f) / .62f));

            if (heavySwing)
            {
                model.transform.localPosition = new Vector3(Mathf.Lerp(.32f, -.28f, swingProgress), 1.05f, .44f + Mathf.Sin(t * Mathf.PI) * .18f);
                model.transform.localRotation = Quaternion.Euler(55 + 25 * Mathf.Sin(t * Mathf.PI), Mathf.Lerp(-110, 110, swingProgress), -28);
            }
            else if (comboStep == 0)
            {
                model.transform.localPosition = new Vector3(.25f, 1f, .35f);
                model.transform.localRotation = Quaternion.Euler(35 + 30 * Mathf.Sin(t * Mathf.PI), Mathf.Lerp(-85, 90, swingProgress), -20);
            }
            else if (comboStep == 1)
            {
                model.transform.localPosition = new Vector3(Mathf.Lerp(-.24f, .26f, swingProgress), 1.02f, .38f);
                model.transform.localRotation = Quaternion.Euler(32 + 28 * Mathf.Sin(t * Mathf.PI), Mathf.Lerp(90, -85, swingProgress), 22);
            }
            else
            {
                // Combo finisher: forward lunge cleave
                model.transform.localPosition = new Vector3(0f, 1.05f, .35f + Mathf.Sin(t * Mathf.PI) * .32f);
                model.transform.localRotation = Quaternion.Euler(65 * Mathf.Sin(t * Mathf.PI), Mathf.Lerp(-45, 45, swingProgress), 0);
            }
        }

        private void OnDisable()
        {
            blocking = false;
            if (model != null) Destroy(model);
            if (firearm != null) firearm.gameObject.SetActive(true);
        }
    }
}
