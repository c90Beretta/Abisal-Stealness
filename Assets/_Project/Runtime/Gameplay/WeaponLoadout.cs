using UnityEngine;
using Zombtoy.Core;

namespace Zombtoy.Gameplay
{
    public enum AmmoGrade
    {
        StandardFMJ = 0,
        HollowPoint = 1,
        ArmorPiercingIncendiary = 2
    }

    public sealed class WeaponLoadout : MonoBehaviour
    {
        public static readonly string[] AmmoNames = { "FMJ Estandar", "Punta Hueca (HP)", "Perforante (AP-I)" };
        public static readonly string[] AmmoShortNames = { "FMJ", "HP+", "AP-I" };

        public WeaponDefinition[] weapons;
        public Transform socket;
        public int Index { get; private set; }
        public AmmoGrade AmmoType { get; private set; } = AmmoGrade.StandardFMJ;
        public string AmmoLabel => AmmoShortNames[(int)AmmoType];
        public WeaponDefinition Current => weapons != null && weapons.Length > 0 ? weapons[Index] : null;
        public Transform Muzzle { get; private set; }
        private GameObject model;
        private SandboxPlayer player;
        private float climbPitch;

        private void Awake()
        {
            player = GetComponent<SandboxPlayer>();
            Equip(0, true);
        }

        public bool Equip(int index, bool restoring = false)
        {
            if (weapons == null || index < 0 || index >= weapons.Length || weapons[index] == null || socket == null) return false;
            if (!restoring && (player.session == null || !player.session.Running || player.session.Busy || player.session.State.InventoryLocked)) return false;
            Index = index;
            if (model != null) { model.SetActive(false); Destroy(model); }
            model = Instantiate(weapons[index].visualPrefab, socket, false);
            model.name = weapons[index].displayName;
            model.SetActive(true);
            Muzzle = model.transform.Find("Muzzle");
            climbPitch = 0f;
            if (!restoring) player.session.SetMessage("Arma: " + weapons[index].displayName + " [" + AmmoNames[(int)AmmoType] + "].");
            return true;
        }

        public void RestoreAmmo(int index) { AmmoType=(AmmoGrade)Mathf.Clamp(index,0,2); }

        public void CycleAmmoType()
        {
            if (player == null || player.session == null || !player.session.Running || player.session.Busy) return;
            AmmoType = (AmmoGrade)(((int)AmmoType + 1) % 3);
            TacticalAudio.Play(transform.position, TacticalCue.Swing, 1.35f);
            string desc = AmmoType == AmmoGrade.StandardFMJ ? "Equilibrada" :
                AmmoType == AmmoGrade.HollowPoint ? "+25% dano a carne y desmembramiento" :
                "Perfora armadura, atraviesa objetivos e incendia";
            player.session.SetMessage($"Municion equipada: {AmmoNames[(int)AmmoType]} ({desc}).");
        }

        public void Recoil()
        {
            if (model == null) return;
            float stability = player != null && player.Roster != null && player.Roster.Current.Id == SurvivorId.KaelAssault ? 0.72f : 1f;
            float kick = (Index == 3 ? 0.12f : Index == 2 ? 0.105f : Index == 5 ? 0.085f : 0.075f) * stability;
            model.transform.localPosition = Vector3.back * kick;
            climbPitch = Mathf.Min(18f, climbPitch + (Index == 3 ? 9f : Index == 2 ? 7.5f : 4.2f) * stability);
        }

        private void LateUpdate()
        {
            if (model == null) return;
            model.transform.localPosition = Vector3.Lerp(model.transform.localPosition, Vector3.zero, Time.deltaTime * 18f);
            climbPitch = Mathf.Lerp(climbPitch, 0f, Time.deltaTime * 16f);
            model.transform.localRotation = Quaternion.Euler(-climbPitch, 0f, 0f);
        }
    }
}
