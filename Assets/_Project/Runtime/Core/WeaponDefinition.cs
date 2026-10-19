using UnityEngine;

namespace Zombtoy.Core
{
    [CreateAssetMenu(menuName = "Abisal/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string displayName;
        public float damage = 35, fireInterval = 0.22f, spread, noiseRadius = 18;
        public int pellets = 1;
        public GameObject visualPrefab;
        public AudioClip shotClip;
    }
}
