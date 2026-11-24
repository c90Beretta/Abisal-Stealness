using System;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    [Serializable]
    public sealed class SurvivorVisualEntry
    {
        public SurvivorId id;
        public string label;
        public GameObject modelPrefab;
        public Avatar avatar;
        public float targetHeight = 1.82f;
        public Vector3 weaponHandOffset = new Vector3(0.01f, 0.02f, 0.06f);
        public Vector3 weaponEulerOffset = Vector3.zero;
        public string rightHandBoneName;
        public string leftHandBoneName;
        public string[] hiddenChildNames;
    }

    [Serializable]
    public sealed class EnemyVisualEntry
    {
        public InfectedKind kind;
        public string label;
        public GameObject modelPrefab;
        public Avatar avatar;
        public float targetHeight = 1.80f;
    }

    public sealed class SurvivorVisualCatalog : ScriptableObject
    {
        public const string ResourcePath = "SurvivorVisualCatalog";
        public const int CurrentCatalogVersion = 8;

        public int version;
        public RuntimeAnimatorController animatorController;
        public SurvivorVisualEntry[] entries = Array.Empty<SurvivorVisualEntry>();
        public GameObject[] weaponPrefabs = Array.Empty<GameObject>();

        public RuntimeAnimatorController enemyAnimatorController;
        public EnemyVisualEntry[] enemyEntries = Array.Empty<EnemyVisualEntry>();

        public SurvivorVisualEntry GetEntry(SurvivorId id)
        {
            if (entries == null) return null;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null && entries[i].id == id)
                    return entries[i];
            }
            return null;
        }

        public GameObject GetWeaponPrefab(int index)
        {
            if (weaponPrefabs == null || index < 0 || index >= weaponPrefabs.Length) return null;
            return weaponPrefabs[index];
        }

        public EnemyVisualEntry GetEnemyEntry(InfectedKind kind)
        {
            if (enemyEntries == null) return null;
            for (int i = 0; i < enemyEntries.Length; i++)
            {
                if (enemyEntries[i] != null && enemyEntries[i].kind == kind)
                    return enemyEntries[i];
            }
            return enemyEntries.Length > 0 ? enemyEntries[0] : null;
        }

        public bool IsComplete()
        {
            if (version != CurrentCatalogVersion) return false;
            if (animatorController == null || entries == null || entries.Length < 4) return false;
            for (int i = 0; i < 4; i++)
            {
                var entry = GetEntry((SurvivorId)i);
                if (entry == null || entry.modelPrefab == null || entry.avatar == null || !entry.avatar.isValid || !entry.avatar.isHuman)
                    return false;
            }
            if (weaponPrefabs == null || weaponPrefabs.Length < 6) return false;
            for (int i = 0; i < 6; i++)
            {
                if (weaponPrefabs[i] == null) return false;
            }
            if (enemyAnimatorController == null || enemyEntries == null || enemyEntries.Length < 4) return false;
            for (int i = 0; i < 4; i++)
            {
                var e = GetEnemyEntry((InfectedKind)i);
                if (e == null || e.modelPrefab == null || e.avatar == null || !e.avatar.isValid || !e.avatar.isHuman)
                    return false;
            }
            return true;
        }
    }
}
