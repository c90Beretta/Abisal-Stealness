using UnityEngine;

namespace Zombtoy.Core
{
    [CreateAssetMenu(menuName = "Zombtoy/Enemy definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string displayName = "Infectado";
        public float health = 70;
        public float speed = 1.15f;
        public float sightRange = 15;
        public float fieldOfView = 120;
        public float hearingMultiplier = 1;
        public float damage = 26;
        public float attackWindup = 0.44f;
        public float attackRecovery = 1.2f;
    }
}
