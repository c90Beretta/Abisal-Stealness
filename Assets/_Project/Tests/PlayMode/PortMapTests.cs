using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zombtoy.Gameplay;

namespace Zombtoy.Tests
{
    public sealed class PortMapTests
    {
        private PortMission mission;
        private SandboxSession session;

        private IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Level01_Port");
            yield return null;
            mission = Object.FindAnyObjectByType<PortMission>();
            Assert.That(mission, Is.Not.Null);
            session = mission.session;
            session.CheckpointPathOverride = Path.GetFullPath("TestResults/test-port-checkpoint.json");
            session.BeginMission();
            foreach (var enemy in Object.FindObjectsByType<SandboxEnemy>()) enemy.gameObject.SetActive(false);
        }

        private void StandNear(Transform point)
        {
            var controller = session.player.GetComponent<CharacterController>();
            controller.enabled = false;
            session.player.transform.position = new Vector3(point.position.x, 0.1f, point.position.z - 1.8f);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void HasRoute(Vector3 start, Vector3 end)
        {
            Assert.That(NavMesh.SamplePosition(start, out NavMeshHit a, 1.5f, NavMesh.AllAreas), Is.True, "Start must be on NavMesh: " + start);
            Assert.That(NavMesh.SamplePosition(end, out NavMeshHit b, 1.5f, NavMesh.AllAreas), Is.True, "End must be on NavMesh: " + end);
            var path = new NavMeshPath();
            Assert.That(NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path), Is.True);
            Assert.That(path.status, Is.EqualTo(NavMeshPathStatus.PathComplete), "Disconnected route: " + start + " -> " + end);
        }

        [UnityTest]
        public IEnumerator ObjectivesRequireFuseAndPowerBeforeBoarding()
        {
            yield return Load();
            StandNear(mission.generator); session.Interact();
            Assert.That(mission.PowerOn, Is.False);
            StandNear(session.terminal); session.Interact();
            Assert.That(mission.BoardingAuthorized, Is.False);
            StandNear(session.extraction); session.Interact();
            Assert.That(session.Running, Is.True);
            HasRoute(mission.arrival.position, new Vector3(-22, 0.1f, 15.2f));
            StandNear(mission.fuse); session.Interact();
            Assert.That(mission.HasFuse, Is.True);
            HasRoute(session.player.transform.position, new Vector3(5, 0.1f, 13.2f));
            StandNear(mission.generator); session.Interact();
            Assert.That(mission.PowerOn, Is.True);
            Assert.That(mission.controlDoor.activeSelf, Is.False);
            yield return new WaitForSeconds(0.5f);
            HasRoute(session.player.transform.position, new Vector3(22, 0.1f, 16.2f));
            StandNear(session.terminal); session.Interact();
            Assert.That(mission.BoardingAuthorized, Is.True);
            Assert.That(mission.boardingGate.activeSelf, Is.False);
            yield return new WaitForSeconds(0.5f);
            HasRoute(session.player.transform.position, new Vector3(39, 0.1f, -9.8f));
            StandNear(session.extraction); session.Interact();
            Assert.That(session.Running, Is.False);
            Time.timeScale = 1;
        }

        [UnityTest]
        public IEnumerator SuppliesCannotBeDuplicatedAndWallsBlockInteractions()
        {
            yield return Load();
            int ammo = session.reserve;
            int doses = session.State.doses;
            StandNear(mission.supplies);
            session.Interact(); session.Interact();
            Assert.That(session.reserve, Is.EqualTo(ammo + 12));
            Assert.That(session.State.doses, Is.EqualTo(doses + 1));
            Assert.That(mission.SuppliesTaken, Is.True);
            var controller = session.player.GetComponent<CharacterController>();
            controller.enabled = false;
            session.player.transform.position = mission.fuse.position + Vector3.back * 1.8f;
            controller.enabled = true;
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = mission.fuse.position + Vector3.back * 0.9f;
            wall.transform.localScale = new Vector3(3, 3, 0.2f);
            Physics.SyncTransforms();
            session.Interact();
            Assert.That(mission.HasFuse, Is.False, "Interaction must not pass through walls");
            Object.Destroy(wall);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ArrivalHasFloorAndWaterFallReturnsPlayerSafely()
        {
            yield return Load();
            yield return new WaitForSeconds(0.3f);
            Assert.That(session.player.transform.position.y, Is.InRange(-0.1f, 0.3f));
            Assert.That(session.State.health, Is.EqualTo(100));
            CaptureView(session.player.view.targetTexture);
            var controller = session.player.GetComponent<CharacterController>();
            controller.enabled = false;
            session.player.transform.position = new Vector3(50, -4, 0);
            controller.enabled = true;
            yield return null;
            yield return null;
            Assert.That(Vector3.Distance(session.player.transform.position, mission.arrival.position), Is.LessThan(0.5f));
            Assert.That(session.State.health, Is.EqualTo(90));
        }

        private static void CaptureView(RenderTexture texture)
        {
            var previous = RenderTexture.active;
            var image = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                image.Apply();
                Directory.CreateDirectory("TestResults");
                File.WriteAllBytes("TestResults/port-runtime.png", image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                Object.Destroy(image);
            }
        }
    }
}
