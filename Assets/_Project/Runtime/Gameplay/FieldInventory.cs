using System;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    [Serializable]
    public sealed class LootStock
    {
        public int ammo, medkits, bandages, doses, cells, oxygen, plates;
        public bool Empty => ammo + medkits + bandages + doses + cells + oxygen + plates == 0;
        public LootStock Copy() => (LootStock)MemberwiseClone();
        public bool Valid => ammo >= 0 && ammo <= 999 && medkits >= 0 && medkits <= 20 && bandages >= 0 && bandages <= 20 &&
            doses >= 0 && doses <= 20 && cells >= 0 && cells <= 20 && oxygen >= 0 && oxygen <= 20 && plates >= 0 && plates <= 20;
        public string Description => $"Cartuchos {ammo}   Botiquines {medkits}   Vendas {bandages}\nSuero {doses}   Pilas {cells}   O2 {oxygen}   Placas {plates}";
    }

    [Serializable]
    public sealed class FieldInventory
    {
        public int cells = 2, oxygen = 1, plates = 1, melee;
        public float battery = 100, air = 100, armor = 100, stamina = 100;
        public bool lampOn = true, maskOn = true, armorOn = true;
        public const float Capacity = 24;
        public float Weight(SandboxSession s) => s.reserve * .018f + s.State.medkits * .8f + s.State.bandages * .15f + s.State.doses * .2f + cells * .25f + oxygen * 1.2f + plates * 1.5f + 7;
        public bool Valid => cells >= 0 && cells <= 20 && oxygen >= 0 && oxygen <= 20 && plates >= 0 && plates <= 20 && melee >= 0 && melee <= 1 &&
            Bounded(battery) && Bounded(air) && Bounded(armor) && Bounded(stamina);
        private static bool Bounded(float f) => float.IsFinite(f) && f >= 0 && f <= 100;
        // Transfer each stack only up to weight and stack limits. Leftovers stay in the container.
        public int Transfer(SandboxSession s, LootStock stock)
        {
            if (!stock.Valid) return 0;
            int count = 0; int ammoBefore = s.reserve, dosesBefore = s.State.doses;
            count += Move(s, ref stock.ammo, ref s.reserve, .018f, 240);
            count += Move(s, ref stock.medkits, ref s.State.medkits, .8f, 8);
            count += Move(s, ref stock.bandages, ref s.State.bandages, .15f, 12);
            count += Move(s, ref stock.doses, ref s.State.doses, .2f, 8);
            count += Move(s, ref stock.cells, ref cells, .25f, 8);
            count += Move(s, ref stock.oxygen, ref oxygen, 1.2f, 4);
            count += Move(s, ref stock.plates, ref plates, 1.5f, 4);
            s.Telemetry?.Loot(s.reserve - ammoBefore, s.State.doses - dosesBefore);
            return count;
        }
        private int Move(SandboxSession s, ref int source, ref int target, float weight, int limit)
        {
            int n = Mathf.Max(0, Mathf.Min(source, limit - target, Mathf.FloorToInt((Capacity - Weight(s) + .0001f) / weight)));
            source -= n; target += n; return n;
        }
    }
}
