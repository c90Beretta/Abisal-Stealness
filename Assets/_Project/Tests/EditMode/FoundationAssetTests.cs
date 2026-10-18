using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zombtoy.Gameplay;

namespace Zombtoy.Tests
{
    public sealed class FoundationAssetTests
    {
        [TestCase("Assets/_Project/Scenes/MVP_Sandbox.unity")]
        [TestCase("Assets/_Project/Scenes/Level01_Port.unity")]
        public void SceneHasNoMissingScriptsAndHasRequiredBindings(string path)
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(path), Is.Not.Null);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            try
            {
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var item in root.GetComponentsInChildren<Transform>(true))
                        Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject), Is.Zero, item.name);
                SandboxSession session = null;
                foreach (var root in scene.GetRootGameObjects())
                    if (root.TryGetComponent(out SandboxSession found)) session = found;
                Assert.That(session, Is.Not.Null);
                Assert.That(session.config, Is.Not.Null);
                Assert.That(session.player, Is.Not.Null);
                Assert.That(session.player.view, Is.Not.Null);
                Assert.That(session.player.stepSound, Is.Not.Null);
                Assert.That(session.player.shotSound, Is.Not.Null);
                Assert.That(session.terminal, Is.Not.Null);
                Assert.That(session.extraction, Is.Not.Null);
                if (path.Contains("Level01_Port"))
                {
                    Assert.That(session.portMission, Is.Not.Null);
                    Assert.That(session.portMission.arrival, Is.Not.Null);
                    Assert.That(session.portMission.fuse, Is.Not.Null);
                    Assert.That(session.portMission.generator, Is.Not.Null);
                    Assert.That(session.portMission.controlDoor, Is.Not.Null);
                    Assert.That(session.portMission.boardingGate, Is.Not.Null);
                }
            }
            finally { EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); }
        }
    }
}
