using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Zombtoy.Editor
{
    [InitializeOnLoad]
    public static class AbisalEntry
    {
        static AbisalEntry()
        {
            if (Application.isBatchMode) return;
            // Play always starts the real level, even while SampleScene is open.
            EditorApplication.delayCall += () => EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(PortMapBuilder.ScenePath);
        }

        [MenuItem("Abisal/Abrir puerto y jugar", priority = 0)]
        public static void PlayPort()
        {
            if (EditorApplication.isPlaying) return;
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(PortMapBuilder.ScenePath);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Abisal/Abrir escena del puerto", priority = 1)]
        public static void OpenPort()
        {
            if (EditorApplication.isPlaying) return;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(PortMapBuilder.ScenePath);
        }

        [MenuItem("Abisal/Mostrar ejecutable de Windows", priority = 2)]
        public static void ShowBuild() => EditorUtility.RevealInFinder("Builds/Windows/AbisalStealness.exe");
    }
}
