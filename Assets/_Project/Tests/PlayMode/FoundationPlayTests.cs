using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zombtoy.Gameplay;

namespace Zombtoy.Tests
{
    public sealed class FoundationPlayTests
    {
        [UnityTest]
        public IEnumerator SandboxLoadsAndObjectiveCanBeCompleted()
        {
            yield return SceneManager.LoadSceneAsync("MVP_Sandbox");
            yield return null;
            var session = Object.FindAnyObjectByType<SandboxSession>();
            Assert.That(session, Is.Not.Null);
            Assert.That(session.Running, Is.True);
            Assert.That(session.player.view.orthographic, Is.True);
            Assert.That(session.player.view.targetTexture.width, Is.EqualTo(480));
            Assert.That(session.player.view.targetTexture.height, Is.EqualTo(270));
            Assert.That(session.player.view.targetTexture.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(Object.FindObjectsByType<SandboxEnemy>().Length, Is.EqualTo(4));
            var character = session.player.GetComponent<CharacterController>();
            character.enabled = false;
            session.player.transform.position = session.extraction.position;
            session.Interact();
            Assert.That(session.Running, Is.True, "Exit must reject a player before terminal activation");
            session.player.transform.position = session.terminal.position;
            session.Interact();
            session.player.transform.position = session.extraction.position;
            session.Interact();
            Assert.That(session.Running, Is.False, "Terminal followed by extraction should complete route");
            Time.timeScale = 1;
        }

        [UnityTest]
        public IEnumerator InjectionsWaitForAnimationAndDoNotCreateAmmo()
        {
            yield return SceneManager.LoadSceneAsync("MVP_Sandbox");
            yield return null;
            var session = Object.FindAnyObjectByType<SandboxSession>();
            foreach (var enemy in Object.FindObjectsByType<SandboxEnemy>())
                enemy.gameObject.SetActive(false);
            session.State.serum = 30;
            session.BeginInjection();
            Assert.That(session.Injecting, Is.True);
            Assert.That(session.State.doses, Is.EqualTo(3));
            yield return new WaitForSeconds(2.2f);
            Assert.That(session.Injecting, Is.False);
            Assert.That(session.State.doses, Is.EqualTo(2));
            Assert.That(session.State.serum, Is.InRange(64f, 65f));
            int total = session.rounds + session.reserve;
            Assert.That(session.UseRound(), Is.True);
            session.Reload();
            yield return new WaitForSeconds(1.7f);
            Assert.That(session.rounds, Is.EqualTo(12));
            Assert.That(session.rounds + session.reserve, Is.EqualTo(total - 1));
            Time.timeScale = 1;
        }
    }
}
