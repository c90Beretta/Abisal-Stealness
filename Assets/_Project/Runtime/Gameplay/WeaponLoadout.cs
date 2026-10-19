using UnityEngine;
using Zombtoy.Core;

namespace Zombtoy.Gameplay
{
    public sealed class WeaponLoadout : MonoBehaviour
    {
        public WeaponDefinition[] weapons;
        public Transform socket;
        public int Index { get; private set; }
        public WeaponDefinition Current => weapons != null && weapons.Length > 0 ? weapons[Index] : null;
        public Transform Muzzle { get; private set; }
        private GameObject model;
        private SandboxPlayer player;

        private void Awake() { player = GetComponent<SandboxPlayer>(); Equip(0, true); }
        public bool Equip(int index, bool restoring = false)
        {
            if (weapons == null || index < 0 || index >= weapons.Length || weapons[index] == null || socket == null) return false;
            if (!restoring && (player.session == null || !player.session.Running || player.session.Busy || player.session.State.InventoryLocked)) return false;
            Index = index;
            if (model != null) { model.SetActive(false); Destroy(model); }
            model = Instantiate(weapons[index].visualPrefab, socket, false);
            model.name = weapons[index].displayName;
            Muzzle = model.transform.Find("Muzzle");
            if (!restoring) player.session.SetMessage("Arma: " + weapons[index].displayName + ". Municion compartida.");
            return true;
        }
        public void Recoil()
        {
            if (model != null) model.transform.localPosition = Vector3.back * 0.075f;
        }
        private void LateUpdate()
        {
            if (model != null) model.transform.localPosition = Vector3.Lerp(model.transform.localPosition, Vector3.zero, Time.deltaTime * 18);
        }
    }
}
