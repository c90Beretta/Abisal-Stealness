using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class WorldPickup : MonoBehaviour
    {
        public string id;
        public string label = "Suministros";
        public int ammo, doses, medkits, bandages;
        // -1 = calibre del arma equipada; 0..5 = calibre especifico
        public int ammoCaliber = -1;
        public bool Collected { get; private set; }
        public bool CanReach(Transform player)
        {
            Vector3 from = player.position + Vector3.up * 0.8f;
            return !Physics.Linecast(from, transform.position, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
                hit.transform == transform || hit.transform.IsChildOf(transform);
        }
        public void Collect(SandboxSession session)
        {
            if (Collected || !session.Running) return;
            session.SyncActiveWeaponToPool();
            int caliber = ammoCaliber >= 0 && ammoCaliber < WeaponAmmoPool.WeaponCount ? ammoCaliber : session.ActiveWeaponIndex;
            int currentRes = session.AmmoPool.GetReserve(caliber);
            int maxRes = WeaponAmmoPool.GetMaxReserve(caliber);

            if (session.Night != null && (session.Night.Inventory.Weight(session) + ammo * .018f + doses * .2f + medkits * .8f + bandages * .15f > session.Night.Inventory.MaxCapacity(session) ||
                currentRes + ammo > maxRes || session.State.medkits + medkits > 8 || session.State.bandages + bandages > 12 || session.State.doses + doses > 8))
            { session.SetMessage("Mochila o reserva de calibre llena. Usa suministros antes de recogerlos."); return; }

            session.Telemetry?.Loot(ammo, doses);
            session.AddAmmoForWeapon(caliber, ammo);
            session.State.doses += doses;
            session.State.medkits += medkits;
            session.State.bandages += bandages;
            string ammoSuffix = ammo > 0 ? $" (+{ammo} {WeaponAmmoPool.GetCaliberShort(caliber)})" : string.Empty;
            session.SetMessage(label + " recogido" + ammoSuffix + ".");
            Restore(true);
        }
        public void Restore(bool collected) { Collected = collected; gameObject.SetActive(!collected); }
    }
}
