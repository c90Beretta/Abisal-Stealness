using UnityEngine;

namespace Zombtoy.Gameplay
{
    public enum TacticalCue
    {
        Swing,
        Impact,
        Gunshot,
        Growl,
        Burst,
        EnemyAlert,
        EnemyAttack,
        EnemyDeath,
        PlayerDamage,
        PlayerDeath,
        Interact,
        UiClick,
        UiPause
    }

    public static class TacticalAudio
    {
        private const int CueCount = 13;
        private static readonly AudioClip[] synthFallbacks = new AudioClip[CueCount];
        private static readonly float[] lastPlayedAt = new float[CueCount];
        private static TacticalAudioCatalog cachedCatalog;
        private static bool triedLoadCatalog;

        private static TacticalAudioCatalog Catalog
        {
            get
            {
                if (!triedLoadCatalog || cachedCatalog == null)
                {
                    triedLoadCatalog = true;
                    cachedCatalog = Resources.Load<TacticalAudioCatalog>(TacticalAudioCatalog.ResourcePath);
                }
                return cachedCatalog;
            }
        }

        public static void Ambience(GameObject owner)
        {
            if (owner == null) return;
            // Preserves SoundtrackDirector's existing ambient music; replaces only the old synthetic sine hum
            // with a very subtle low-level air-ventilation bed.
            AudioClip clip = ResolveClip(Catalog != null ? Catalog.airVentLoop : null, TacticalAudioCatalog.AirVentPath);
            float volume = 0.055f;
            if (clip == null)
            {
                const int rate = 22050;
                var data = new float[rate * 4];
                for (int i = 0; i < data.Length; i++)
                {
                    float t = (float)i / rate;
                    data[i] = (0.5f * Mathf.Sin(t * 50f * Mathf.PI * 2f) + 0.2f * Mathf.Sin(t * 75f * Mathf.PI * 2f)) * 0.08f;
                }
                clip = AudioClip.Create("Original ventilation loop", data.Length, 1, rate, false);
                clip.SetData(data, 0);
                volume = 0.14f;
            }

            var source = owner.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.volume = volume;
            source.spatialBlend = 0f;
            source.Play();
        }

        public static AudioClip GetFootstepClip(bool sprinting, bool limping)
        {
            var cat = Catalog;
            if (limping)
                return ResolveClip(cat != null ? cat.footstepsLimping : null, TacticalAudioCatalog.FootstepsLimpingPath);
            if (sprinting)
                return ResolveClip(cat != null ? cat.footstepsRunning : null, TacticalAudioCatalog.FootstepsRunningPath);
            return ResolveClip(cat != null ? cat.footstepsConcrete : null, TacticalAudioCatalog.FootstepsConcretePath);
        }

        public static void Play(Vector3 position, TacticalCue cue, float tone = 1f)
        {
            int index = (int)cue;
            if (index < 0 || index >= CueCount) return;

            float cooldown = GetCooldown(cue);
            if (cooldown > 0f && Time.unscaledTime - lastPlayedAt[index] < cooldown)
                return;
            lastPlayedAt[index] = Time.unscaledTime;

            AudioClip clip = ResolveCueClip(cue);
            bool isSynthesized = false;
            if (clip == null)
            {
                clip = GetSynthFallback(cue);
                isSynthesized = true;
            }

            var obj = new GameObject("Audio_" + cue);
            obj.transform.position = position;
            var source = obj.AddComponent<AudioSource>();
            source.clip = clip;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;

            bool is2D = IsNonSpatial(cue);
            bool ignorePause = IgnoreListenerPause(cue);
            source.spatialBlend = is2D ? 0f : 0.85f;
            source.minDistance = 2.5f;
            source.maxDistance = cue == TacticalCue.Gunshot || cue == TacticalCue.EnemyAlert ? 30f : 22f;
            source.ignoreListenerPause = ignorePause;

            float baseVolume = GetBaseVolume(cue, isSynthesized);
            if (!is2D)
            {
                var listener = Object.FindAnyObjectByType<AudioListener>();
                if (listener != null &&
                    Physics.Linecast(position + Vector3.up * 0.5f, listener.transform.position, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) &&
                    hit.collider.GetComponentInParent<SandboxPlayer>() == null)
                {
                    baseVolume *= 0.45f;
                }
            }

            source.volume = baseVolume;
            source.pitch = ComputePitch(cue, tone, isSynthesized);

            float maxDuration = GetMaxDuration(cue, clip.length);
            float startOffset = ComputeStartOffset(cue, clip.length, maxDuration);
            if (startOffset > 0.01f && startOffset < clip.length - 0.1f)
                source.time = startOffset;

            source.Play();

            if (maxDuration < clip.length - 0.05f)
            {
                var segment = obj.AddComponent<AudioSegmentFade>();
                segment.Configure(source, maxDuration, GetFadeOut(cue));
            }
            else
            {
                Object.Destroy(obj, Mathf.Max(0.25f, (clip.length - startOffset) / Mathf.Max(0.2f, source.pitch) + 0.1f));
            }
        }

        private static AudioClip ResolveCueClip(TacticalCue cue)
        {
            var cat = Catalog;
            switch (cue)
            {
                case TacticalCue.Swing:
                    return ResolveClip(cat != null ? cat.playerAttackWhoosh : null, TacticalAudioCatalog.PlayerAttackWhooshPath);
                case TacticalCue.Impact:
                case TacticalCue.Burst:
                    return ResolveClip(cat != null ? cat.enemyHitImpactFlesh : null, TacticalAudioCatalog.EnemyHitImpactFleshPath);
                case TacticalCue.Gunshot:
                    return ResolveClip(cat != null ? cat.weaponGunshot : null, TacticalAudioCatalog.WeaponGunshotPath);
                case TacticalCue.Growl:
                    return ResolveClip(cat != null ? cat.aggressiveGrowlLoop : null, TacticalAudioCatalog.AggressiveGrowlPath);
                case TacticalCue.EnemyAlert:
                    return ResolveClip(cat != null ? cat.enemyAlertMonster : null, TacticalAudioCatalog.EnemyAlertMonsterPath);
                case TacticalCue.EnemyAttack:
                    return ResolveClip(cat != null ? cat.enemyAttackSwipe : null, TacticalAudioCatalog.EnemyAttackSwipePath);
                case TacticalCue.EnemyDeath:
                    return ResolveClip(cat != null ? cat.enemyDeathGurgle : null, TacticalAudioCatalog.EnemyDeathGurglePath);
                case TacticalCue.PlayerDamage:
                    return ResolveClip(cat != null ? cat.playerTakeDamage : null, TacticalAudioCatalog.PlayerTakeDamagePath);
                case TacticalCue.PlayerDeath:
                    return ResolveClip(cat != null ? cat.playerDeathBodyFall : null, TacticalAudioCatalog.PlayerDeathBodyFallPath);
                case TacticalCue.Interact:
                    return ResolveClip(cat != null ? cat.playerInteract : null, TacticalAudioCatalog.PlayerInteractPath);
                case TacticalCue.UiClick:
                    return ResolveClip(cat != null ? cat.uiMenuClick : null, TacticalAudioCatalog.UiMenuClickPath);
                case TacticalCue.UiPause:
                    return ResolveClip(cat != null ? cat.uiPauseSwitch : null, TacticalAudioCatalog.UiPauseSwitchPath);
                default:
                    return null;
            }
        }

        private static AudioClip ResolveClip(AudioClip catalogClip, string editorAssetPath)
        {
            if (catalogClip != null) return catalogClip;
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(editorAssetPath);
#else
            return null;
#endif
        }

        private static bool IsNonSpatial(TacticalCue cue)
        {
            return cue == TacticalCue.PlayerDamage
                || cue == TacticalCue.PlayerDeath
                || cue == TacticalCue.Interact
                || cue == TacticalCue.UiClick
                || cue == TacticalCue.UiPause;
        }

        private static bool IgnoreListenerPause(TacticalCue cue)
        {
            return cue == TacticalCue.PlayerDeath
                || cue == TacticalCue.UiClick
                || cue == TacticalCue.UiPause;
        }

        private static float GetCooldown(TacticalCue cue)
        {
            switch (cue)
            {
                case TacticalCue.Growl: return 0.55f;
                case TacticalCue.EnemyAlert: return 0.85f;
                case TacticalCue.Impact: return 0.04f;
                case TacticalCue.EnemyAttack: return 0.12f;
                case TacticalCue.EnemyDeath: return 0.08f;
                case TacticalCue.PlayerDamage: return 0.15f;
                case TacticalCue.UiClick: return 0.04f;
                case TacticalCue.UiPause: return 0.08f;
                default: return 0f;
            }
        }

        private static float GetBaseVolume(TacticalCue cue, bool synthesized)
        {
            if (synthesized) return 0.45f;
            switch (cue)
            {
                case TacticalCue.Swing: return 0.42f;
                case TacticalCue.Impact: return 0.52f;
                case TacticalCue.Burst: return 0.65f;
                case TacticalCue.Gunshot: return 0.34f;
                case TacticalCue.Growl: return 0.44f;
                case TacticalCue.EnemyAlert: return 0.50f;
                case TacticalCue.EnemyAttack: return 0.48f;
                case TacticalCue.EnemyDeath: return 0.52f;
                case TacticalCue.PlayerDamage: return 0.55f;
                case TacticalCue.PlayerDeath: return 0.65f;
                case TacticalCue.Interact: return 0.42f;
                case TacticalCue.UiClick: return 0.32f;
                case TacticalCue.UiPause: return 0.38f;
                default: return 0.45f;
            }
        }

        private static float ComputePitch(TacticalCue cue, float tone, bool synthesized)
        {
            if (synthesized) return Mathf.Clamp(1f / Mathf.Max(0.25f, tone), 0.65f, 1.5f);
            switch (cue)
            {
                case TacticalCue.Gunshot:
                    // Weapon fireInterval is passed as tone (0.082 SMG -> faster/higher, 1.15 sniper -> deeper)
                    return Mathf.Clamp(1.18f - tone * 0.32f, 0.72f, 1.24f) * Random.Range(0.96f, 1.04f);
                case TacticalCue.Burst:
                    return Random.Range(0.70f, 0.82f);
                case TacticalCue.Growl:
                case TacticalCue.EnemyAlert:
                case TacticalCue.EnemyAttack:
                case TacticalCue.EnemyDeath:
                case TacticalCue.Impact:
                case TacticalCue.Swing:
                    return Mathf.Clamp(1f / Mathf.Max(0.4f, tone), 0.72f, 1.32f) * Random.Range(0.95f, 1.05f);
                case TacticalCue.Interact:
                    return Mathf.Clamp(tone, 0.85f, 1.2f);
                default:
                    return 1f;
            }
        }

        private static float GetMaxDuration(TacticalCue cue, float clipLength)
        {
            switch (cue)
            {
                case TacticalCue.Growl: return Mathf.Min(clipLength, 1.65f);
                case TacticalCue.EnemyAlert: return Mathf.Min(clipLength, 2.20f);
                case TacticalCue.Swing: return Mathf.Min(clipLength, 0.62f);
                case TacticalCue.Gunshot: return Mathf.Min(clipLength, 1.15f);
                case TacticalCue.UiClick: return Mathf.Min(clipLength, 0.38f);
                case TacticalCue.UiPause: return Mathf.Min(clipLength, 0.70f);
                default: return clipLength;
            }
        }

        private static float GetFadeOut(TacticalCue cue)
        {
            switch (cue)
            {
                case TacticalCue.Growl: return 0.45f;
                case TacticalCue.EnemyAlert: return 0.55f;
                case TacticalCue.Gunshot: return 0.25f;
                case TacticalCue.Swing: return 0.16f;
                case TacticalCue.UiClick: return 0.12f;
                case TacticalCue.UiPause: return 0.20f;
                default: return 0.15f;
            }
        }

        private static float ComputeStartOffset(TacticalCue cue, float clipLength, float maxDuration)
        {
            if (cue == TacticalCue.Growl && clipLength > maxDuration + 2f)
                return Random.Range(0.25f, Mathf.Max(0.3f, clipLength - maxDuration - 1f));
            return 0f;
        }

        private static AudioClip GetSynthFallback(TacticalCue cue)
        {
            int index = (int)cue;
            if (synthFallbacks[index] != null) return synthFallbacks[index];

            const int rate = 22050;
            bool lowVoice = cue == TacticalCue.Growl || cue == TacticalCue.EnemyAlert || cue == TacticalCue.EnemyDeath;
            float duration = lowVoice ? 0.55f : 0.20f;
            var samples = new float[(int)(rate * duration)];
            var random = new System.Random(790 + index);
            float filtered = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                filtered = Mathf.Lerp(filtered, (float)random.NextDouble() * 2f - 1f, lowVoice ? 0.08f : 0.65f);
                float freq = lowVoice ? 85f : cue == TacticalCue.Impact ? 150f : 65f;
                float wave = Mathf.Sin(t * freq * Mathf.PI * 2f);
                samples[i] = (filtered * 0.7f + wave * 0.3f) * Mathf.Exp(-t * (lowVoice ? 5f : 25f)) * 0.55f;
            }
            synthFallbacks[index] = AudioClip.Create("Original synthesized " + cue, samples.Length, 1, rate, false);
            synthFallbacks[index].SetData(samples, 0);
            return synthFallbacks[index];
        }

        private sealed class AudioSegmentFade : MonoBehaviour
        {
            private AudioSource source;
            private float duration;
            private float fadeOut;
            private float initialVolume;
            private float elapsed;

            public void Configure(AudioSource target, float maxDuration, float fadeSeconds)
            {
                source = target;
                duration = Mathf.Max(0.05f, maxDuration);
                fadeOut = Mathf.Clamp(fadeSeconds, 0.02f, duration);
                initialVolume = source != null ? source.volume : 0.4f;
            }

            private void Update()
            {
                elapsed += Time.unscaledDeltaTime;
                if (source == null || !source.isPlaying || elapsed >= duration)
                {
                    if (source != null) source.Stop();
                    Destroy(gameObject);
                    return;
                }

                float remaining = duration - elapsed;
                if (remaining < fadeOut)
                    source.volume = initialVolume * Mathf.Clamp01(remaining / fadeOut);
            }
        }
    }
}
