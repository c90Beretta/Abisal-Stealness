using UnityEngine;

namespace Zombtoy.Gameplay
{
    [RequireComponent(typeof(Light))]
    public sealed class IndustrialLight : MonoBehaviour
    {
        public float strength = 1, variation = .025f, seed;
        private Light lamp;
        private void Awake() { lamp = GetComponent<Light>(); }
        private void Update()
        {
            if (lamp != null && lamp.enabled) lamp.intensity = strength * (1 - variation + Mathf.PerlinNoise(seed, Time.time * 13) * variation);
        }
    }
}
