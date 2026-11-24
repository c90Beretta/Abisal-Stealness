using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    /// <summary>
    /// Controla la musica ambiental aleatoria de fondo y realiza crossfade dinamico
    /// hacia musica de horda segun la vida del jugador:
    /// - Vida > 80: pista de DILLOM ((Irreversible)).
    /// - Vida <= 80: temas de horda de Left 4 Dead 2 (aleatorios).
    /// </summary>
    public sealed class SoundtrackDirector : MonoBehaviour
    {
        public static readonly string[] DefaultAmbientPaths =
        {
            "Assets/Soundtrack/stage_1/05. Over - Akira Yamaoka (Silent Hill Soundtrack).mp3",
            "Assets/Soundtrack/stage_1/Ambient Horror Music - Abandoned Underwater Facility.mp3",
            "Assets/Soundtrack/stage_1/J1 - Beyond these quiet days lies the universe you called home.mp3",
            "Assets/Soundtrack/stage_1/Milwaukee Protocol - Day 3_ E3 - Intervention.mp3",
            "Assets/Soundtrack/stage_1/Milwaukee Protocol - Day 3_ F1 - HYPERSALIVATIAGOLERIUM (Various).mp3",
            "Assets/Soundtrack/stage_1/What Lies Beneath - Underwater horror ambiance.mp3"
        };

        public static readonly string[] DefaultHordeHighHealthPaths =
        {
            "Assets/Soundtrack/stage_1/(Irreversible) - DILLOM (Video) - KAT1AM.mp3"
        };

        public static readonly string[] DefaultHordeLowHealthPaths =
        {
            "Assets/Soundtrack/stage_1/Left 4 Dead 2 - Dead Center Horde Theme.mp3",
            "Assets/Soundtrack/stage_1/Left 4 Dead 2 - Swamp Fever Horde Theme.mp3"
        };

        [Header("Clips de Audio")]
        [Tooltip("Musica ambiental de fondo que se reproduce aleatoriamente durante la exploracion.")]
        public AudioClip[] ambientClips;

        [Tooltip("Pista de horda cuando la vida del personaje es mayor a 80 (DILLOM - Irreversible).")]
        public AudioClip[] hordeHighHealthClips;

        [Tooltip("Pistas de horda cuando la vida del personaje es 80 o menor (Left 4 Dead 2).")]
        public AudioClip[] hordeLowHealthClips;

        [Header("Umbral de Vida en Horda")]
        [Tooltip("Si la vida es mayor a este valor suenan las pistas de vida alta (DILLOM); si es menor o igual suenan las de peligro (L4D2).")]
        [Range(1f, 100f)] public float hordeHighHealthThreshold = 80f;

        [Header("Volumenes")]
        [Range(0f, 1f)] public float ambientVolume = 0.28f;
        [Range(0f, 1f)] public float ambientVolumeDuringHorde = 0.04f;
        [Range(0f, 1f)] public float ambientVolumeMenuOrPause = 0.14f;
        [Range(0f, 1f)] public float hordeVolume = 0.52f;

        [Header("Transiciones (segundos)")]
        public float hordeFadeInSeconds = 1.25f;
        public float hordeFadeOutSeconds = 3.0f;
        public float tierCrossfadeSeconds = 1.5f;
        public float hordeReleaseDelay = 2.5f;

        private SandboxSession session;
        private HordeDirector hordes;
        private AudioSource ambientSource;
        private AudioSource hordeHighSource;
        private AudioSource hordeLowSource;

        private int lastAmbientIndex = -1;
        private int lastHighHordeIndex = -1;
        private int lastLowHordeIndex = -1;
        private bool hordeMusicActive;
        private float hordeReleaseTimer;
        private bool initialized;

        public bool IsHordeMusicPlaying => hordeMusicActive;
        private bool IsPlayerHighHealth => session == null || session.State == null || session.State.health > hordeHighHealthThreshold;

        public void Initialize(SandboxSession targetSession, HordeDirector targetHordes)
        {
            session = targetSession;
            BindHordeDirector(targetHordes);

            EnsureDefaultClipsInEditor();
            EnsureAudioSources();

            if (!initialized)
            {
                initialized = true;
                StartRandomAmbientClip();
            }
        }

        private void Awake()
        {
            EnsureAudioSources();
        }

        private void Start()
        {
            if (!initialized)
            {
                var foundSession = session != null ? session : GetComponent<SandboxSession>();
                var foundHordes = hordes != null ? hordes : GetComponent<HordeDirector>();
                Initialize(foundSession, foundHordes);
            }
        }

        private void Update()
        {
            if (ambientSource == null || hordeHighSource == null || hordeLowSource == null) return;

            bool playing = session == null || session.Running;
            bool menuOrPause = session != null && (session.Mode == SessionMode.Title || session.Mode == SessionMode.Paused);
            bool directorHordeActive = hordes != null && hordes.IsHordeActive;

            if (playing && directorHordeActive)
            {
                hordeReleaseTimer = hordeReleaseDelay;
                if (!hordeMusicActive)
                    BeginHordeMusic();
            }
            else if (hordeMusicActive)
            {
                if (!playing && session != null && (session.Mode == SessionMode.Dead || session.Mode == SessionMode.Complete))
                {
                    hordeMusicActive = false;
                    hordeReleaseTimer = 0f;
                }
                else if (playing)
                {
                    hordeReleaseTimer -= Time.unscaledDeltaTime;
                    if (hordeReleaseTimer <= 0f)
                        hordeMusicActive = false;
                }
            }

            // Rotacion aleatoria de musica ambiental de fondo
            if (ambientClips != null && ambientClips.Length > 0 && !ambientSource.isPlaying && ambientSource.clip != null)
                StartRandomAmbientClip();

            // Determinar que capa de horda corresponde segun la vida actual (> 80 DILLOM, <= 80 L4D2)
            bool wantHighHorde = hordeMusicActive && playing && IsPlayerHighHealth && HasAnyClip(hordeHighHealthClips);
            bool wantLowHorde = hordeMusicActive && playing && !wantHighHorde && HasAnyClip(hordeLowHealthClips);

            // Si falto un grupo de clips, usar el otro como respaldo
            if (hordeMusicActive && playing && !wantHighHorde && !wantLowHorde)
            {
                if (HasAnyClip(hordeHighHealthClips)) wantHighHorde = true;
                else if (HasAnyClip(hordeLowHealthClips)) wantLowHorde = true;
            }

            if (wantHighHorde && !hordeHighSource.isPlaying)
                StartRandomClip(hordeHighSource, hordeHighHealthClips, ref lastHighHordeIndex);

            if (wantLowHorde && !hordeLowSource.isPlaying)
                StartRandomClip(hordeLowSource, hordeLowHealthClips, ref lastLowHordeIndex);

            float dt = Time.unscaledDeltaTime;

            // Fade de ambiente
            float targetAmbient = hordeMusicActive
                ? ambientVolumeDuringHorde
                : (menuOrPause ? ambientVolumeMenuOrPause : ambientVolume);
            float ambientDuration = hordeMusicActive ? hordeFadeInSeconds : hordeFadeOutSeconds;
            ambientSource.volume = Mathf.MoveTowards(ambientSource.volume, targetAmbient, dt / Mathf.Max(0.15f, ambientDuration));

            // Fade de capa DILLOM (vida > 80)
            float targetHigh = wantHighHorde ? hordeVolume : 0f;
            float highDuration = wantHighHorde
                ? (hordeLowSource.volume > 0.01f ? tierCrossfadeSeconds : hordeFadeInSeconds)
                : (wantLowHorde ? tierCrossfadeSeconds : hordeFadeOutSeconds);
            hordeHighSource.volume = Mathf.MoveTowards(
                hordeHighSource.volume,
                targetHigh,
                (dt / Mathf.Max(0.15f, highDuration)) * Mathf.Max(0.1f, hordeVolume));

            // Fade de capa Left 4 Dead 2 (vida <= 80)
            float targetLow = wantLowHorde ? hordeVolume : 0f;
            float lowDuration = wantLowHorde
                ? (hordeHighSource.volume > 0.01f ? tierCrossfadeSeconds : hordeFadeInSeconds)
                : (wantHighHorde ? tierCrossfadeSeconds : hordeFadeOutSeconds);
            hordeLowSource.volume = Mathf.MoveTowards(
                hordeLowSource.volume,
                targetLow,
                (dt / Mathf.Max(0.15f, lowDuration)) * Mathf.Max(0.1f, hordeVolume));

            // Detener capas silenciosas para que la proxima horda/cambio elija otra pista aleatoria
            if (!wantHighHorde && hordeHighSource.isPlaying && hordeHighSource.volume <= 0.001f)
                hordeHighSource.Stop();

            if (!wantLowHorde && hordeLowSource.isPlaying && hordeLowSource.volume <= 0.001f)
                hordeLowSource.Stop();
        }

        public void TriggerHordeMusic()
        {
            hordeReleaseTimer = hordeReleaseDelay;
            if (!hordeMusicActive)
                BeginHordeMusic();
        }

        public void StopHordeMusic()
        {
            hordeReleaseTimer = 0f;
            hordeMusicActive = false;
        }

        private void HandleHordeStarted(int waveNumber, bool isBossWave)
        {
            TriggerHordeMusic();
        }

        private void HandleHordeCleared(int waveNumber)
        {
            hordeReleaseTimer = Mathf.Min(hordeReleaseTimer, 1.0f);
        }

        private void BeginHordeMusic()
        {
            if (!HasAnyClip(hordeHighHealthClips) && !HasAnyClip(hordeLowHealthClips)) return;
            hordeMusicActive = true;

            if (IsPlayerHighHealth && HasAnyClip(hordeHighHealthClips))
                StartRandomClip(hordeHighSource, hordeHighHealthClips, ref lastHighHordeIndex);
            else if (HasAnyClip(hordeLowHealthClips))
                StartRandomClip(hordeLowSource, hordeLowHealthClips, ref lastLowHordeIndex);
            else if (HasAnyClip(hordeHighHealthClips))
                StartRandomClip(hordeHighSource, hordeHighHealthClips, ref lastHighHordeIndex);
        }

        private void StartRandomAmbientClip()
        {
            StartRandomClip(ambientSource, ambientClips, ref lastAmbientIndex);
        }

        private static bool HasAnyClip(AudioClip[] clips)
        {
            if (clips == null) return false;
            for (int i = 0; i < clips.Length; i++)
                if (clips[i] != null) return true;
            return false;
        }

        private static void StartRandomClip(AudioSource source, AudioClip[] clips, ref int lastIndex)
        {
            if (source == null || clips == null || clips.Length == 0) return;

            var validIndices = new List<int>();
            for (int i = 0; i < clips.Length; i++)
                if (clips[i] != null) validIndices.Add(i);
            if (validIndices.Count == 0) return;

            int chosen;
            if (validIndices.Count == 1)
            {
                chosen = validIndices[0];
            }
            else
            {
                var candidates = new List<int>(validIndices.Count);
                foreach (int idx in validIndices)
                    if (idx != lastIndex) candidates.Add(idx);

                chosen = candidates.Count > 0
                    ? candidates[Random.Range(0, candidates.Count)]
                    : validIndices[Random.Range(0, validIndices.Count)];
            }

            lastIndex = chosen;
            source.clip = clips[chosen];
            source.loop = validIndices.Count == 1;
            source.Play();
        }

        private void EnsureAudioSources()
        {
            ambientSource = EnsureChildSource(ambientSource, "AmbientMusicSource", true);
            hordeHighSource = EnsureChildSource(hordeHighSource, "HordeHighHealthSource", false);
            hordeLowSource = EnsureChildSource(hordeLowSource, "HordeLowHealthSource", false);
        }

        private AudioSource EnsureChildSource(AudioSource current, string childName, bool ignorePause)
        {
            if (current != null) return current;

            var child = transform.Find(childName);
            if (child == null)
            {
                child = new GameObject(childName).transform;
                child.SetParent(transform, false);
            }

            var source = child.GetComponent<AudioSource>();
            if (source == null)
                source = child.gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = ignorePause;
            source.volume = 0f;
            return source;
        }

        private void BindHordeDirector(HordeDirector target)
        {
            if (hordes != null)
            {
                hordes.OnHordeStarted -= HandleHordeStarted;
                hordes.OnHordeCleared -= HandleHordeCleared;
            }

            hordes = target;
            if (hordes != null)
            {
                hordes.OnHordeStarted += HandleHordeStarted;
                hordes.OnHordeCleared += HandleHordeCleared;
            }
        }

        private void EnsureDefaultClipsInEditor()
        {
            ambientClips = FilterExcludedClips(ambientClips);
            hordeHighHealthClips = FilterExcludedClips(hordeHighHealthClips);
            hordeLowHealthClips = FilterExcludedClips(hordeLowHealthClips);
#if UNITY_EDITOR
            if (!HasAnyClip(ambientClips))
                ambientClips = LoadClipsFromPaths(DefaultAmbientPaths);

            if (!HasAnyClip(hordeHighHealthClips))
                hordeHighHealthClips = LoadClipsFromPaths(DefaultHordeHighHealthPaths);

            if (!HasAnyClip(hordeLowHealthClips))
                hordeLowHealthClips = LoadClipsFromPaths(DefaultHordeLowHealthPaths);
#endif
        }

        private static AudioClip[] FilterExcludedClips(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return clips;
            var filtered = new List<AudioClip>(clips.Length);
            for (int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                if (clip == null) continue;
                if (clip.name.IndexOf("Coyote", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
                filtered.Add(clip);
            }
            return filtered.ToArray();
        }

#if UNITY_EDITOR
        public static AudioClip[] LoadClipsFromPaths(string[] paths)
        {
            var loaded = new List<AudioClip>();
            foreach (var path in paths)
            {
                var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) loaded.Add(clip);
            }
            return loaded.ToArray();
        }
#endif

        private void OnDestroy()
        {
            BindHordeDirector(null);
        }
    }
}
