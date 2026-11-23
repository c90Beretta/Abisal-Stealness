using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zombtoy.Gameplay;

namespace Zombtoy.Editor
{
    public static class SoundtrackSetupTool
    {
        [MenuItem("Zombtoy/Audio/Configurar Soundtrack (Ambiente + Horda)")]
        public static void ConfigureActiveSceneSoundtrack()
        {
            var session = Object.FindAnyObjectByType<SandboxSession>();
            if (session == null)
            {
                Debug.LogWarning("[Zombtoy Audio] No se encontro SandboxSession en la escena abierta.");
                return;
            }

            var soundtrack = session.GetComponent<SoundtrackDirector>();
            if (soundtrack == null) soundtrack = session.gameObject.AddComponent<SoundtrackDirector>();
            soundtrack.ambientClips = SoundtrackDirector.LoadClipsFromPaths(SoundtrackDirector.DefaultAmbientPaths);
            soundtrack.hordeHighHealthClips = SoundtrackDirector.LoadClipsFromPaths(SoundtrackDirector.DefaultHordeHighHealthPaths);
            soundtrack.hordeLowHealthClips = SoundtrackDirector.LoadClipsFromPaths(SoundtrackDirector.DefaultHordeLowHealthPaths);

            EditorUtility.SetDirty(soundtrack);
            EditorSceneManager.MarkSceneDirty(session.gameObject.scene);
            EditorSceneManager.SaveScene(session.gameObject.scene);

            Selection.activeGameObject = session.gameObject;
            Debug.Log($"[Zombtoy Audio] SoundtrackDirector configurado en '{session.gameObject.name}': {soundtrack.ambientClips.Length} pistas ambientales, {soundtrack.hordeHighHealthClips.Length} de horda (vida > 80 / DILLOM), {soundtrack.hordeLowHealthClips.Length} de horda (vida <= 80 / L4D2).");
        }
    }
}
