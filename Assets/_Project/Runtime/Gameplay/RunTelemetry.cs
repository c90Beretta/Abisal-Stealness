using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    [Serializable]
    public sealed class RunMetrics
    {
        public string id = Guid.NewGuid().ToString("N");
        public string source;
        public int initialAmmo, initialDoses, ammoFound, dosesFound, dosesUsed, maniaEpisodes, loads;
        public float[] phaseSeconds = new float[4];
        public float activeSeconds, serumWasted;
        public bool baselineKnown = true;
        public bool Valid => !string.IsNullOrEmpty(id) && id.Length == 32 &&
            System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-f0-9]+$") &&
            initialAmmo >= 0 && initialDoses >= 0 && ammoFound >= 0 && dosesFound >= 0 && dosesUsed >= 0 && maniaEpisodes >= 0 && loads >= 0 &&
            float.IsFinite(activeSeconds) && activeSeconds >= 0 && float.IsFinite(serumWasted) && serumWasted >= 0 &&
            phaseSeconds != null && phaseSeconds.Length == 4 && Array.TrueForAll(phaseSeconds, v => float.IsFinite(v) && v >= 0);
    }

    // Anonymous, local-only evidence. Editor fixtures and synthetic runs are labelled explicitly.
    public sealed class RunTelemetry : MonoBehaviour
    {
        public RunMetrics Data { get; private set; }
        private SandboxSession session;
        private bool manic, exported;
        [Serializable] private sealed class BuildIdentity { public string sourceCommit; }
        private string BuildSha { get { var item = Resources.Load<TextAsset>("AbisalBuildIdentity"); return item != null ? JsonUtility.FromJson<BuildIdentity>(item.text).sourceCommit : "editor-working-tree"; } }
        public void Initialize(SandboxSession value)
        {
            session = value;
            Data = new RunMetrics
            {
                source = Application.isEditor ? "editor-fixture" : "local-playtest",
                initialAmmo = value.rounds + value.reserve,
                initialDoses = value.State.doses
            };
        }
        private void Update()
        {
            if (session == null || Data == null) return;
            if (session.Running)
            {
                Data.activeSeconds += Time.unscaledDeltaTime;
                Data.phaseSeconds[session.State.Phase] += Time.deltaTime;
                if (session.State.IsManic && !manic) Data.maniaEpisodes++;
                manic = session.State.IsManic;
            }
            else if (!exported && (session.Mode == SessionMode.Complete || session.Mode == SessionMode.Dead)) Export(session.Mode.ToString());
        }
        public void Loot(int ammo, int doses) { Data.ammoFound += Mathf.Max(0, ammo); Data.dosesFound += Mathf.Max(0, doses); }
        public void Injection(float before) { Data.dosesUsed++; Data.serumWasted += Mathf.Max(0, before + session.config.injectionAmount - 100); }
        public void Restore(RunMetrics saved)
        {
            if (saved != null && saved.Valid) Data = JsonUtility.FromJson<RunMetrics>(JsonUtility.ToJson(saved));
            else { Initialize(session); Data.baselineKnown = false; }
            Data.loads++; exported = false; manic = session.State.IsManic;
        }
        public string Export(string outcome)
        {
            if (Data == null || !Data.Valid || Data.activeSeconds < 1) return null;
            try
            {
                string folder = Path.Combine(Application.persistentDataPath, "Playtests"); Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, Data.id + ".csv");
                var c = CultureInfo.InvariantCulture;
                string[] headers = { "build_sha", "run_id", "source", "version", "outcome", "baseline_known", "active_seconds", "simulation_seconds", "ammo_initial", "ammo_found", "shots", "hits", "ammo_remaining", "doses_initial", "doses_found", "doses_used", "doses_remaining", "serum_wasted", "phase0_seconds", "phase1_seconds", "phase2_seconds", "phase3_seconds", "mania_episodes", "checkpoint_loads", "gpu", "width", "height" };
                string[] values = { BuildSha, Data.id, Data.source, Application.version, outcome, Data.baselineKnown.ToString(), Data.activeSeconds.ToString("F2", c), session.State.elapsed.ToString("F2", c), Data.initialAmmo.ToString(), Data.ammoFound.ToString(), session.Shots.ToString(), session.Hits.ToString(), (session.reserve + session.rounds).ToString(), Data.initialDoses.ToString(), Data.dosesFound.ToString(), Data.dosesUsed.ToString(), session.State.doses.ToString(), Data.serumWasted.ToString("F2", c), Data.phaseSeconds[0].ToString("F2", c), Data.phaseSeconds[1].ToString("F2", c), Data.phaseSeconds[2].ToString("F2", c), Data.phaseSeconds[3].ToString("F2", c), Data.maniaEpisodes.ToString(), Data.loads.ToString(), SystemInfo.graphicsDeviceName, Screen.width.ToString(), Screen.height.ToString() };
                for (int i = 0; i < values.Length; i++) values[i] = "\"" + values[i].Replace("\"", "\"\"") + "\"";
                File.WriteAllText(path, string.Join(",", headers) + "\n" + string.Join(",", values) + "\n"); exported = true; return path;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { Debug.LogWarning("No se pudo escribir el registro local de partida."); return null; }
        }
        private void OnApplicationQuit() => Export("Quit");
    }
}
