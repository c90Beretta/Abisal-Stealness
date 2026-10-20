using System;
using UnityEngine;

namespace Zombtoy.Core
{
    // Uses simulation time so pause does not consume serum or mania timers.
    [Serializable]
    public sealed class SurvivalState
    {
        public float health = 100f;
        public float serum = 100f;
        public float infection;
        public float elapsed;
        public float lastInjection = -10000f;
        public float maniaUntil;
        public float inventoryLockedUntil;
        public float tremorUntil;
        public int doses = 3;
        public int medkits = 1;
        public int bandages = 2;
        public float bleeding;

        public int Phase => infection >= 100f ? 3 : infection >= 65f ? 2 : infection >= 30f ? 1 : 0;
        public bool IsManic => elapsed < maniaUntil;
        public bool InventoryLocked => elapsed < inventoryLockedUntil;

        public void Tick(float dt, SurvivalConfig config)
        {
            if (dt <= 0f || health <= 0f) return;
            elapsed += dt;
            serum = Mathf.Max(0f, serum - dt / config.secondsPerSerumPoint);
            health = Mathf.Max(0f, health - bleeding * dt);
            float previous = infection;
            if (serum < config.lowSerumThreshold)
                infection = Mathf.Min(100f, infection + dt * config.infectionPerSecond);
            // Crossing the maximum triggers once; it cannot perpetually retrigger at 100.
            if (previous < 100f && infection >= 100f) BeginMania(config);
        }

        public bool Heal()
        {
            if (medkits <= 0 || health <= 0 || health >= 100) return false;
            medkits--;
            health = Mathf.Min(100, health + 40);
            return true;
        }

        public bool Bandage()
        {
            if (bandages <= 0 || health <= 0 || bleeding <= 0) return false;
            bandages--;
            bleeding = 0;
            return true;
        }

        public bool Inject(SurvivalConfig config)
        {
            if (doses <= 0 || health <= 0f) return false;
            doses--;
            serum = Mathf.Min(100f, serum + config.injectionAmount);
            tremorUntil = elapsed + config.tremorSeconds;
            if (elapsed - lastInjection <= config.maniaWindowSeconds) BeginMania(config);
            lastInjection = elapsed;
            return true;
        }

        private void BeginMania(SurvivalConfig config)
        {
            maniaUntil = elapsed + config.maniaSeconds;
            inventoryLockedUntil = elapsed + config.inventoryLockSeconds;
        }
    }
}
