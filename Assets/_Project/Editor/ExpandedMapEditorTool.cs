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
                expander.BuildMap(GameMapId.MassiveSubmarine, false);
            else if (scene.name.Contains("Level03_AbyssalCave"))
                expander.BuildMap(GameMapId.AbyssalWaterCave, false);
            else if (GameObject.Find("Port_Expanded_Sectors") == null && GameObject.Find("Port_MainDeck_DenseProps") == null)
                expander.Initialize(session);
        }

        [MenuItem("Zombtoy/Mapas/01 - Ver Mapa 1: Puerto Carguero Denso")]
        public static void PreviewPortMap()
        {
            OpenPortSceneIfNeeded();
            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null) return;
            var expander = session.GetComponent<PortMapExpander>() ?? session.gameObject.AddComponent<PortMapExpander>();
            expander.BuildMap(GameMapId.PortCargo, false);
            FocusSceneView(new Vector3(0f, 0f, -5f), 46f);
        }

        [MenuItem("Zombtoy/Mapas/02 - Ver Mapa 2: Submarino Masivo B-07")]
        public static void PreviewSubmarineMap()
        {
            OpenPortSceneIfNeeded();
            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null) return;
            var expander = session.GetComponent<PortMapExpander>() ?? session.gameObject.AddComponent<PortMapExpander>();
            expander.BuildMap(GameMapId.MassiveSubmarine, false);
            FocusSceneView(new Vector3(0f, 1.5f, 0f), 38f);
        }

        [MenuItem("Zombtoy/Mapas/03 - Ver Mapa 3: Cueva Submarina Abisal")]
        public static void PreviewWaterCaveMap()
        {
            OpenPortSceneIfNeeded();
            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null) return;
            var expander = session.GetComponent<PortMapExpander>() ?? session.gameObject.AddComponent<PortMapExpander>();
            expander.BuildMap(GameMapId.AbyssalWaterCave, false);
            FocusSceneView(new Vector3(0f, 1.5f, 0f), 42f);
        }

        [MenuItem("Zombtoy/Mapas/Guardar Escenas Level02_Submarine y Level03_AbyssalCave")]
        public static void SaveDedicatedMapScenes()
        {
            if (Application.isPlaying) return;

            OpenPortSceneIfNeeded();
            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null) return;
            var expander = session.GetComponent<PortMapExpander>() ?? session.gameObject.AddComponent<PortMapExpander>();

            expander.BuildMap(GameMapId.MassiveSubmarine, false);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/_Project/Scenes/Level02_Submarine.unity", true);

            expander.BuildMap(GameMapId.AbyssalWaterCave, false);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/_Project/Scenes/Level03_AbyssalCave.unity", true);

            expander.BuildMap(GameMapId.PortCargo, false);
            EditorSceneManager.OpenScene(PortMapBuilder.ScenePath);
            PreviewPortMap();
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
