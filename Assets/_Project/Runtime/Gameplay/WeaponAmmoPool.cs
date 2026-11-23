using System;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    public enum WeaponAmmoKind
    {
        Pistol9mm = 0,
        Rifle556 = 1,
        Shotgun12G = 2,
        Sniper338 = 3,
        Smg46 = 4,
        Heavy762 = 5
    }

    /// <summary>
    /// Administra cargadores y reservas independientes por cada tipo de arma (sin balas compartidas).
    /// </summary>
    [Serializable]
    public sealed class WeaponAmmoPool
    {
        public const int WeaponCount = 6;

        public static readonly string[] CaliberNames =
        {
            "9mm Pistola",
            "5.56mm Fusil",
            "Cal.12 Escopeta",
            ".338 Precision",
            "4.6mm Subfusil",
            "7.62mm Pesada"
        };

        public static readonly string[] CaliberShort =
        {
            "9mm",
            "5.56",
            "12G",
            ".338",
            "SMG",
            "7.62"
        };

        public static readonly int[] MagCapacities = { 15, 30, 8, 5, 32, 60 };
        public static readonly int[] StartingReserves = { 45, 60, 24, 15, 64, 120 };
        public static readonly int[] MaxReserves = { 150, 210, 64, 45, 224, 240 };
        public static readonly float[] WeightPerRound = { 0.012f, 0.016f, 0.032f, 0.038f, 0.011f, 0.020f };

        public int[] magazines = new int[WeaponCount];
        public int[] reserves = new int[WeaponCount];

        public WeaponAmmoPool()
        {
            ResetDefaults();
        }

        public void ResetDefaults()
        {
            EnsureArrays();
            for (int i = 0; i < WeaponCount; i++)
            {
                magazines[i] = MagCapacities[i];
                reserves[i] = StartingReserves[i];
            }
        }

        public void EnsureArrays()
        {
            if (magazines == null || magazines.Length != WeaponCount)
            {
                var next = new int[WeaponCount];
                for (int i = 0; i < WeaponCount; i++)
                    next[i] = magazines != null && i < magazines.Length ? magazines[i] : MagCapacities[i];
                magazines = next;
            }
            if (reserves == null || reserves.Length != WeaponCount)
            {
                var next = new int[WeaponCount];
                for (int i = 0; i < WeaponCount; i++)
                    next[i] = reserves != null && i < reserves.Length ? reserves[i] : StartingReserves[i];
                reserves = next;
            }
        }

        public static int ClampIndex(int index) => Mathf.Clamp(index, 0, WeaponCount - 1);
        public static int GetMagCapacity(int index) => MagCapacities[ClampIndex(index)];
        public static int GetMaxReserve(int index) => MaxReserves[ClampIndex(index)];
        public static string GetCaliberName(int index) => CaliberNames[ClampIndex(index)];
        public static string GetCaliberShort(int index) => CaliberShort[ClampIndex(index)];
        public static float GetRoundWeight(int index) => WeightPerRound[ClampIndex(index)];

        public int GetMag(int index)
        {
            EnsureArrays();
            return magazines[ClampIndex(index)];
        }

        public void SetMag(int index, int value)
        {
            EnsureArrays();
            int i = ClampIndex(index);
            magazines[i] = Mathf.Clamp(value, 0, MagCapacities[i]);
        }

        public int GetReserve(int index)
        {
            EnsureArrays();
            return reserves[ClampIndex(index)];
        }

        public void SetReserve(int index, int value)
        {
            EnsureArrays();
            int i = ClampIndex(index);
            reserves[i] = Mathf.Clamp(value, 0, MaxReserves[i]);
        }

        public int AddReserve(int index, int amount)
        {
            if (amount <= 0) return 0;
            EnsureArrays();
            int i = ClampIndex(index);
            int space = Mathf.Max(0, MaxReserves[i] - reserves[i]);
            int added = Mathf.Min(amount, space);
            reserves[i] += added;
            return added;
        }

        public int Reload(int index)
        {
            EnsureArrays();
            int i = ClampIndex(index);
            int needed = Mathf.Max(0, MagCapacities[i] - magazines[i]);
            int moved = Mathf.Min(needed, reserves[i]);
            magazines[i] += moved;
            reserves[i] -= moved;
            return moved;
        }

        public int TotalAllRounds()
        {
            EnsureArrays();
            int total = 0;
            for (int i = 0; i < WeaponCount; i++)
                total += magazines[i] + reserves[i];
            return total;
        }

        public int TotalAllReserves()
        {
            EnsureArrays();
            int total = 0;
            for (int i = 0; i < WeaponCount; i++)
                total += reserves[i];
            return total;
        }

        public string BuildCompactSummary()
        {
            EnsureArrays();
            return $"9mm:{reserves[0]} · 5.56:{reserves[1]} · 12G:{reserves[2]}\n.338:{reserves[3]} · SMG:{reserves[4]} · 7.62:{reserves[5]}";
        }

        public WeaponAmmoPool Copy()
        {
            EnsureArrays();
            var clone = new WeaponAmmoPool();
            Array.Copy(magazines, clone.magazines, WeaponCount);
            Array.Copy(reserves, clone.reserves, WeaponCount);
            return clone;
        }

        public bool Valid()
        {
            if (magazines == null || reserves == null || magazines.Length != WeaponCount || reserves.Length != WeaponCount)
                return false;
            for (int i = 0; i < WeaponCount; i++)
            {
                if (magazines[i] < 0 || magazines[i] > MagCapacities[i] || reserves[i] < 0 || reserves[i] > 999)
                    return false;
            }
            return true;
        }
    }
}
