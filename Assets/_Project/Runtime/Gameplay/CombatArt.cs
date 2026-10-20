using UnityEngine;

namespace Zombtoy.Gameplay
{
    [CreateAssetMenu(menuName = "Abisal/Combat art")]
    public sealed class CombatArt : ScriptableObject
    {
        public Material particles, blood, brass;
        public Mesh bloodPool, casing;
        public GameObject knife, axe;
    }
}
