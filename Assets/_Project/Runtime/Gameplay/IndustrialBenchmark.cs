using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Zombtoy.Gameplay
{
    // Explicit opt-in synthetic renderer/AI/physics stress. Never runs during normal play.
    public sealed class IndustrialBenchmark : MonoBehaviour
    {
        [Serializable]
        private sealed class Result
        {
            public string kind = "synthetic-industrial-stress", version, buildIdentity, gpu, cpu, os, graphicsApi, utc,presentation;
            public int width, height, gpuMemoryMB, frames, gpuTimingSamples, peakAlive, peakDynamicBodies, peakEffects;
            public float warmupSeconds, measuredSeconds, frameP50ms, frameP95ms, frameP99ms, cpuP50ms, cpuP95ms, gpuP50ms, gpuP95ms, framesOver16_67Percent;
            public bool vSyncDisabled, uncapped, graphicsEnabled, validResolution;
        }
        private SandboxSession session;
        private readonly List<float> frames = new List<float>(250000), gpu = new List<float>(250000), cpu = new List<float>(250000);
        private readonly FrameTiming[] timing = new FrameTiming[1];
        private readonly List<string> seconds = new List<string>();
        private float start, measureStart, duration, nextShot, nextPulse, nextSecond; private int sector = -1;
        private int peakAlive, peakBodies, peakEffects; private bool measuring, done;
        private static readonly Vector3[] Route = { new Vector3(-24, .1f, -15), new Vector3(-24, .1f, -2), new Vector3(-22, .1f, 14), new Vector3(7, .1f, 17), new Vector3(20, .1f, 16), new Vector3(25, .1f, -7) };
        private string folder;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AllowBackground() { if(Array.IndexOf(Environment.GetCommandLineArgs(),"-abisal-benchmark")>=0){Application.runInBackground=true;Debug.Log("Benchmark background rendering enabled before scene load.");} }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            string[] args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, "-abisal-benchmark");
            if (i < 0) return;Debug.Log("Benchmark command-line bootstrap recognized.");
            var runner = new GameObject("Explicit synthetic benchmark").AddComponent<IndustrialBenchmark>();
            runner.duration = i + 1 < args.Length && float.TryParse(args[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out float s) ? Mathf.Clamp(s, 10, 1800) : 600;
            int output = Array.IndexOf(args, "-abisal-output"); runner.folder = output >= 0 && output + 1 < args.Length ? Path.GetFullPath(args[output + 1]) : Path.Combine(Application.persistentDataPath, "Benchmark");
        }
        private IEnumerator Start()
        {
            yield return null; session = FindAnyObjectByType<SandboxSession>();
            if (session == null) { Debug.LogError("Benchmark requires Level01_Port."); Application.Quit(2); yield break; }
            Application.runInBackground = true; QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1;
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            Debug.Log("Explicit industrial benchmark initialized with graphics: "+SystemInfo.graphicsDeviceType);
            session.GetComponent<PresentationOptions>().SetEffects(Array.IndexOf(Environment.GetCommandLineArgs(),"-abisal-native")<0);
            session.BeginMission(); session.player.enabled = false; session.Night.hordes.enabled = false; session.Telemetry.Data.source = "synthetic-benchmark";
            session.portMission.Restore(true, true, true, true); start = Time.realtimeSinceStartup; nextSecond = start + 1;
            seconds.Add("elapsed_seconds,sector,alive,dynamic_bodies,effects,last_frame_ms");
        }
        private void Update()
        {
            if (session == null || done) return;
            float now = Time.realtimeSinceStartup, elapsed = now - start;
            session.State.health = 100; session.State.bleeding = 0; session.State.serum = 65; session.Night.Inventory.battery = 100; session.Night.Inventory.air = 100;
            int nextSector = (int)(elapsed / 20) % Route.Length;
            if (nextSector != sector) { sector = nextSector; SetSector(); }
            if (now >= nextShot) { nextShot = now + .12f; CombatEffects.Shot(session.player.transform.position + Vector3.up * 1.3f + session.player.transform.forward * .7f, session.player.transform, session.tracerMaterial); }
            if (now >= nextPulse) { nextPulse = now + 12; PulseCorpses(); session.EmitNoise(session.player.transform.position, 20); }
            FrameTimingManager.CaptureFrameTimings();
            if (elapsed >= 60 && !measuring) { measuring = true; measureStart = now; }
            if (measuring)
            {
                frames.Add(Time.unscaledDeltaTime * 1000);
                if (FrameTimingManager.GetLatestTimings(1, timing) > 0) { if (timing[0].gpuFrameTime > 0) gpu.Add((float)timing[0].gpuFrameTime); if (timing[0].cpuFrameTime > 0) cpu.Add((float)timing[0].cpuFrameTime); }
            }
            if (now >= nextSecond)
            {
                nextSecond = now + 1; int alive = 0, bodies = 0;
                foreach (var enemy in session.Enemies) if (enemy.gameObject.activeInHierarchy)
                    { if (!enemy.IsDead) alive++; foreach (var b in enemy.GetComponent<RagdollController>().bodies) if (!b.body.isKinematic) bodies++; }
                peakAlive = Mathf.Max(peakAlive, alive); peakBodies = Mathf.Max(peakBodies, bodies); peakEffects = Mathf.Max(peakEffects, CombatEffects.LiveCount);
                seconds.Add(string.Format(CultureInfo.InvariantCulture, "{0:F2},{1},{2},{3},{4},{5:F3}", elapsed, sector, alive, bodies, CombatEffects.LiveCount, Time.unscaledDeltaTime * 1000));
            }
            if (measuring && now - measureStart >= duration) Finish(now);
        }
        private static void Place(Transform actor, Vector3 p)
        { var cc = actor.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false; actor.position = p; if (cc != null) cc.enabled = true; }
        private void SetSector()
        {
            Vector3 center = Route[sector]; Place(session.player.transform, center); session.player.transform.rotation = Quaternion.Euler(0, sector * 55, 0);
            for (int i = 0; i < session.Enemies.Length; i++)
            {
                var e = session.Enemies[i]; e.gameObject.SetActive(i < 18); if (i >= 18) continue;
                float angle = i * Mathf.PI * 2 / 14; Vector3 candidate = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (4 + i % 3);
                if (NavMesh.SamplePosition(candidate, out var hit, 4, NavMesh.AllAreas)) candidate = hit.position;
                e.GetComponent<Dismemberment>().Restore(Severed.None); e.Restore(e.MaxHealth, candidate); e.Hear(center, 100);
            }
            PulseCorpses(); Physics.SyncTransforms();
        }
        private void PulseCorpses()
        {
            for (int i = 14; i < Mathf.Min(18, session.Enemies.Length); i++)
            {
                var e = session.Enemies[i]; e.GetComponent<Dismemberment>().Restore(Severed.None); e.Restore(e.MaxHealth, e.transform.position);
                if (i % 2 == 0) e.GetComponent<Dismemberment>().Sever(Severed.LeftArm);
                e.Hit(1000, e.transform.position + Vector3.up * 1.2f, false);
            }
        }
        private static float Percentile(List<float> sorted, float p) => sorted.Count == 0 ? 0 : sorted[Mathf.Min(sorted.Count - 1, Mathf.FloorToInt((sorted.Count - 1) * p))];
        private void Finish(float now)
        {
            done = true; frames.Sort(); gpu.Sort(); cpu.Sort();
            var identity = Resources.Load<TextAsset>("AbisalBuildIdentity");
            var r = new Result { presentation=PresentationOptions.ReducedEffects?"native":"retro-480x270",version = Application.version, buildIdentity = identity != null ? identity.text : "unavailable", gpu = SystemInfo.graphicsDeviceName, cpu = SystemInfo.processorType, os = SystemInfo.operatingSystem, graphicsApi = SystemInfo.graphicsDeviceType.ToString(), utc = DateTime.UtcNow.ToString("O"), width = Screen.width, height = Screen.height, gpuMemoryMB = SystemInfo.graphicsMemorySize, frames = frames.Count, gpuTimingSamples = gpu.Count, peakAlive = peakAlive, peakDynamicBodies = peakBodies, peakEffects = peakEffects, warmupSeconds = 60, measuredSeconds = now - measureStart, frameP50ms = Percentile(frames, .5f), frameP95ms = Percentile(frames, .95f), frameP99ms = Percentile(frames, .99f), cpuP50ms = Percentile(cpu, .5f), cpuP95ms = Percentile(cpu, .95f), gpuP50ms = Percentile(gpu, .5f), gpuP95ms = Percentile(gpu, .95f), framesOver16_67Percent = frames.Count == 0 ? 100 : frames.Count(f => f > 16.6667f) * 100f / frames.Count, vSyncDisabled = QualitySettings.vSyncCount == 0, uncapped = Application.targetFrameRate < 0, graphicsEnabled = SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null, validResolution = Screen.width == 1920 && Screen.height == 1080 };
            Directory.CreateDirectory(folder); File.WriteAllText(Path.Combine(folder, "benchmark.json"), JsonUtility.ToJson(r, true)); File.WriteAllLines(Path.Combine(folder, "benchmark-seconds.csv"), seconds);
            session.Telemetry.Export("SyntheticStress"); Debug.Log("Industrial benchmark finished: " + folder); Application.Quit(r.graphicsEnabled && r.validResolution && gpu.Count > 0 ? 0 : 3);
        }
    }
}
