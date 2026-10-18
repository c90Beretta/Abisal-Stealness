using UnityEngine;

namespace Zombtoy.Core
{
    [CreateAssetMenu(menuName = "Zombtoy/Survival config")]
    public sealed class SurvivalConfig : ScriptableObject
    {
        [Min(0.1f)] public float secondsPerSerumPoint = 6f;
        [Range(0f, 100f)] public float lowSerumThreshold = 40f;
        [Range(0f, 100f)] public float injectionAmount = 35f;
        [Min(0.1f)] public float injectionSeconds = 2f;
        [Min(0.1f)] public float infectionPerSecond = 0.5f;
        [Min(0.1f)] public float maniaWindowSeconds = 60f;
        [Min(0.1f)] public float maniaSeconds = 30f;
        [Min(0.1f)] public float inventoryLockSeconds = 10f;
        [Min(0.1f)] public float tremorSeconds = 20f;
    }
}
