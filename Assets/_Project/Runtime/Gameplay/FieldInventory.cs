using System;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    [Serializable]
    public sealed class LootStock
    {
        public int ammo, medkits, bandages, doses, cells, oxygen, plates;
        // -1 = municion para el arma equipada; 0..5 = calibre especifico (WeaponAmmoKind)
        public int ammoCaliber = -1;
        public bool Empty => ammo + medkits + bandages + doses + cells + oxygen + plates == 0;
        public LootStock Copy() => (LootStock)MemberwiseClone();
        public bool Valid => ammo >= 0 && ammo <= 999 && medkits >= 0 && medkits <= 20 && bandages >= 0 && bandages <= 20 &&
            doses >= 0 && doses <= 20 && cells >= 0 && cells <= 20 && oxygen >= 0 && oxygen <= 20 && plates >= 0 && plates <= 20 &&
            ammoCaliber >= -1 && ammoCaliber < WeaponAmmoPool.WeaponCount;
        public string AmmoLabel => ammoCaliber >= 0 && ammoCaliber < WeaponAmmoPool.WeaponCount
            ? WeaponAmmoPool.GetCaliberShort(ammoCaliber)
            : "Calibre activo";
        public string Description => $"Municion ({AmmoLabel}) {ammo}   Botiquines {medkits}   Vendas {bandages}\nSuero {doses}   Pilas {cells}   O2 {oxygen}   Placas {plates}";
    }

    [Serializable]
    public sealed class FieldInventory
    {
        public int cells = 2, oxygen = 1, plates = 1, melee;
        public float battery = 100, air = 100, armor = 100, stamina = 100;
        public bool lampOn = true, maskOn = true, armorOn = true;
        public const float Capacity = 24;
        public float MaxCapacity(SandboxSession s) => Capacity + (s != null && s.player != null && s.player.Roster != null ? s.player.Roster.Current.ExtraCapacityKg : 0f);
        public float Weight(SandboxSession s)
        {
            s.SyncActiveWeaponToPool();
            float ammoWeight = s.reserve * .018f;
            return ammoWeight + s.State.medkits * .8f + s.State.bandages * .15f + s.State.doses * .2f + cells * .25f + oxygen * 1.2f + plates * 1.5f + 7;
        }
        public bool Valid => cells >= 0 && cells <= 20 && oxygen >= 0 && oxygen <= 20 && plates >= 0 && plates <= 20 && melee >= 0 && melee <= 3 &&
            Bounded(battery) && Bounded(air) && Bounded(armor) && Bounded(stamina);
        private static bool Bounded(float f) => float.IsFinite(f) && f >= 0 && f <= 100;
        // Transfer each stack only up to weight and per-caliber stack limits. Leftovers stay in the container.
        public int Transfer(SandboxSession s, LootStock stock)
        {
            if (!stock.Valid) return 0;
            s.SyncActiveWeaponToPool();
            int targetCaliber = stock.ammoCaliber >= 0 && stock.ammoCaliber < WeaponAmmoPool.WeaponCount
                ? stock.ammoCaliber
                : s.ActiveWeaponIndex;

            int count = 0;
            int ammoBefore = s.AmmoPool.GetReserve(targetCaliber);
            int dosesBefore = s.State.doses;

            int targetReserve = s.AmmoPool.GetReserve(targetCaliber);
            int maxReserve = WeaponAmmoPool.GetMaxReserve(targetCaliber);
            int movedAmmo = Move(s, ref stock.ammo, ref targetReserve, .018f, maxReserve);
            if (movedAmmo > 0)
            {
                s.AmmoPool.SetReserve(targetCaliber, targetReserve);
                s.reserve = s.AmmoPool.GetReserve(s.ActiveWeaponIndex);
                count += movedAmmo;
            }

            count += Move(s, ref stock.medkits, ref s.State.medkits, .8f, 8);
            count += Move(s, ref stock.bandages, ref s.State.bandages, .15f, 12);
            count += Move(s, ref stock.doses, ref s.State.doses, .2f, 8);
            count += Move(s, ref stock.cells, ref cells, .25f, 8);
            count += Move(s, ref stock.oxygen, ref oxygen, 1.2f, 4);
            count += Move(s, ref stock.plates, ref plates, 1.5f, 4);
            s.Telemetry?.Loot(s.AmmoPool.GetReserve(targetCaliber) - ammoBefore, s.State.doses - dosesBefore);
            return count;
        }
        private int Move(SandboxSession s, ref int source, ref int target, float weight, int limit)
        {
            float maxCap = MaxCapacity(s);
            int n = Mathf.Max(0, Mathf.Min(source, limit - target, Mathf.FloorToInt((maxCap - Weight(s) + .0001f) / weight)));
            source -= n; target += n; return n;
        }
    }
}
