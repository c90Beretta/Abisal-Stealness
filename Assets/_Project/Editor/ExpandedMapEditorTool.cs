using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zombtoy.Gameplay;

namespace Zombtoy.Editor
{
    [InitializeOnLoad]
    public static class ExpandedMapEditorTool
    {
        public const string SubmarineScenePath = "Assets/_Project/Scenes/Level02_Submarine.unity";
        public const string CaveScenePath = "Assets/_Project/Scenes/Level03_AbyssalCave.unity";

        static ExpandedMapEditorTool()
        {
            EditorApplication.delayCall += EnsureExpandedInOpenScene;
            EditorSceneManager.sceneOpened += (_, __) => EnsureExpandedInOpenScene();
        }

        private static void EnsureExpandedInOpenScene()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) return;
            if (!scene.name.Contains("Level01_Port") && !scene.name.Contains("Level02_Submarine") && !scene.name.Contains("Level03_AbyssalCave")) return;

            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null || session.portMission == null) return;

            var expander = session.GetComponent<PortMapExpander>() ?? session.gameObject.AddComponent<PortMapExpander>();
            if (scene.name.Contains("Level02_Submarine"))
                expander.Initialize(session, GameMapId.MassiveSubmarine);
            else if (scene.name.Contains("Level03_AbyssalCave"))
                expander.Initialize(session, GameMapId.AbyssalWaterCave);
            else if (GameObject.Find(PortMapExpander.MapRootName) == null)
                expander.Initialize(session, GameMapId.PortCargo);
        }

        [MenuItem("Zombtoy/Mapas/01 - Ver Mapa 1: Puerto Carguero Denso", priority = 10)]
        public static void PreviewPortMap()
        {
            OpenPortSceneIfNeeded();
            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null) return;
            var expander = session.GetComponent<PortMapExpander>() ?? session.gameObject.AddComponent<PortMapExpander>();
            expander.Initialize(session, GameMapId.PortCargo);
            FocusSceneView(new Vector3(0f, 0f, -5f), 46f);
        }

        [MenuItem("Zombtoy/Mapas/02 - Ver Mapa 2: Submarino Masivo B-07", priority = 11)]
        public static void PreviewSubmarineMap()
        {
            OpenPortSceneIfNeeded();
            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null) return;
            var expander = session.GetComponent<PortMapExpander>() ?? session.gameObject.AddComponent<PortMapExpander>();
            expander.Initialize(session, GameMapId.MassiveSubmarine);
            FocusSceneView(new Vector3(0f, 1.5f, 0f), 38f);
        }

        [MenuItem("Zombtoy/Mapas/03 - Ver Mapa 3: Cueva Submarina Abisal", priority = 12)]
        public static void PreviewWaterCaveMap()
        {
            OpenPortSceneIfNeeded();
            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null) return;
            var expander = session.GetComponent<PortMapExpander>() ?? session.gameObject.AddComponent<PortMapExpander>();
            expander.Initialize(session, GameMapId.AbyssalWaterCave);
            FocusSceneView(new Vector3(0f, 1.5f, 0f), 42f);
        }

        [MenuItem("Zombtoy/Mapas/Guardar Escenas Level02_Submarine y Level03_AbyssalCave", priority = 25)]
        public static void SaveDedicatedMapScenes()
        {
            if (Application.isPlaying) return;

            // Always generate dedicated scenes cleanly from Level01_Port so base references are intact
            EditorSceneManager.OpenScene(PortMapBuilder.ScenePath, OpenSceneMode.Single);
            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null) return;
            var expander = session.GetComponent<PortMapExpander>() ?? session.gameObject.AddComponent<PortMapExpander>();

            // 1. Build & save Level02_Submarine.unity with clean colliders + NavMesh
            expander.Initialize(session, GameMapId.MassiveSubmarine);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SubmarineScenePath, true);

            // 2. Build & save Level03_AbyssalCave.unity with clean colliders + NavMesh
            expander.Initialize(session, GameMapId.AbyssalWaterCave);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), CaveScenePath, true);

            // 3. Register all 3 map scenes in Build Settings without duplicates
            var buildScenes = EditorBuildSettings.scenes.ToList();
            foreach (string path in new[] { PortMapBuilder.ScenePath, SubmarineScenePath, CaveScenePath })
            {
                if (!buildScenes.Any(s => s.path == path))
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();

            // 4. Re-open Level01_Port cleanly in Map 1 mode
            EditorSceneManager.OpenScene(PortMapBuilder.ScenePath, OpenSceneMode.Single);
            PreviewPortMap();
            Debug.Log("[Zombtoy] Escenas regeneradas y registradas en Build Settings: Level02_Submarine.unity y Level03_AbyssalCave.unity.");
        }

        [MenuItem("Zombtoy/Mapas/Auditar Colisiones Fantasma en Escena Actual", priority = 40)]
        public static void AuditGhostCollidersInActiveScene()
        {
            var ghosts = new List<string>();
            foreach (var col in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude))
            {
                if (col == null || !col.enabled || col.isTrigger) continue;
                if (col.GetComponent<CharacterController>() != null) continue;
                var renderers = col.GetComponentsInChildren<Renderer>(false);
                bool hasVisibleRenderer = renderers.Any(r => r != null && r.enabled && !r.forceRenderingOff);
                if (!hasVisibleRenderer)
                    ghosts.Add($"{col.gameObject.name} @ {col.transform.position} (parent: {(col.transform.parent != null ? col.transform.parent.name : "<root>")})");
            }

            if (ghosts.Count == 0)
                Debug.Log("[Zombtoy] Auditoria OK: 0 colisiones fantasma (sin renderer visible) en la escena actual.");
            else
                Debug.LogWarning($"[Zombtoy] Se encontraron {ghosts.Count} colisiones sin renderer visible:\n - " + string.Join("\n - ", ghosts));
        }

        [MenuItem("Zombtoy/Mapa Expandido/Generar y Enfocar Mapa Completo")]
        public static void BuildAndFocusFullMap() => PreviewPortMap();

        [MenuItem("Zombtoy/Mapa Expandido/Enfocar Sector 07 (Sur - Helipuerto H-01)")]
        public static void FocusSouthSector()
        {
            PreviewPortMap();
            FocusSceneView(new Vector3(8f, 1f, -33f), 18f);
        }

        [MenuItem("Zombtoy/Mapa Expandido/Enfocar Sector 08 (Oeste - Bio-Cuarentena)")]
        public static void FocusWestSector()
        {
            PreviewPortMap();
            FocusSceneView(new Vector3(-38f, 1f, 0f), 15f);
        }

        [MenuItem("Zombtoy/Mapa Expandido/Enfocar Sector 09 (Norte - Subestacion Tesla)")]
        public static void FocusNorthSector()
        {
            PreviewPortMap();
            FocusSceneView(new Vector3(-1f, 1f, 33f), 15f);
        }

        private static void OpenPortSceneIfNeeded()
        {
            if (Application.isPlaying) return;
            var scene = SceneManager.GetActiveScene();
            if (!scene.name.Contains("Level01_Port") && !scene.name.Contains("Level02_Submarine") && !scene.name.Contains("Level03_AbyssalCave"))
                EditorSceneManager.OpenScene(PortMapBuilder.ScenePath);
        }

        private static void FocusSceneView(Vector3 point, float size)
        {
            var view = SceneView.lastActiveSceneView;
            if (view == null) return;
            view.LookAt(point, Quaternion.Euler(40f, 45f, 0f), size);
            view.Repaint();
        }
    }
}
