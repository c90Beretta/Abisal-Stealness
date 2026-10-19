using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zombtoy.Gameplay;

namespace Zombtoy.Tests
{
    public sealed class FirstLevelTests
    {
        private SandboxSession session;
        private string path;
        private IEnumerator Load(bool begin = true)
        {
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync("Level01_Port"); yield return null;
            session = Object.FindAnyObjectByType<SandboxSession>();
            path = Path.GetFullPath("TestResults/checkpoint-" + System.Guid.NewGuid() + ".json");
            session.CheckpointPathOverride = path;
            if (session.Night != null) { session.Night.hordes.enabled = false; session.Night.Inventory.armorOn = false; }
            foreach (var enemy in session.Enemies) enemy.gameObject.SetActive(false);
            if (begin) session.BeginMission();
        }
        [TearDown] public void Cleanup()
        {
            Time.timeScale = 1;
            if (path == null) return;
            foreach (string suffix in new[] { "", ".tmp", ".bak" }) if (File.Exists(path + suffix)) File.Delete(path + suffix);
        }
        private void Move(Transform actor, Vector3 position)
        {
            var cc = actor.GetComponent<CharacterController>(); cc.enabled = false; actor.position = position; cc.enabled = true; Physics.SyncTransforms();
        }
        [UnityTest] public IEnumerator MenuButtonsStartAndPauseSimulation()
        {
            yield return Load(false);
            Assert.That(session.Mode, Is.EqualTo(SessionMode.Title)); Assert.That(Time.timeScale, Is.Zero);
            Assert.That(session.UseRound(), Is.False);
            var button = session.Hud.GetComponentsInChildren<Button>(true).Single(b => b.name == "Comenzar nivel");
            button.onClick.Invoke(); yield return null;
            Assert.That(session.Running, Is.True);
            // Enter and a selected UI button can both submit in one frame.
            // A repeated start callback must not restart an already running level.
            button.onClick.Invoke(); yield return null;
            Assert.That(Object.FindAnyObjectByType<SandboxSession>(), Is.SameAs(session));
            float serum = session.State.serum; session.TogglePause();
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(session.State.serum, Is.EqualTo(serum));
            Assert.That(session.UseRound(), Is.False);
            session.Resume(); Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(session.player.GetComponent<CharacterMotion>().rig["idle"].clip.legacy, Is.True);
        }
        [UnityTest] public IEnumerator TreatmentTakesTimeCanBeInterruptedAndDeathFreezesPlay()
        {
            yield return Load(); session.State.health = 40;
            session.BeginHeal(); Assert.That(session.Busy, Is.True); Assert.That(session.UseRound(), Is.False);
            session.TakeDamage(10); Assert.That(session.Busy, Is.False);
            Assert.That(session.State.medkits, Is.EqualTo(2));
            yield return new WaitForSeconds(0.7f);
            session.BeginBandage(); yield return new WaitForSeconds(1.3f);
            Assert.That(session.State.bleeding, Is.Zero);
            session.BeginHeal(); yield return new WaitForSeconds(1.7f);
            Assert.That(session.State.health, Is.InRange(69, 70)); Assert.That(session.State.medkits, Is.EqualTo(1));
            session.TakeDamage(200); yield return null; yield return null;
            Assert.That(session.Mode, Is.EqualTo(SessionMode.Dead)); Assert.That(Time.timeScale, Is.Zero);
        }
        [UnityTest] public IEnumerator CheckpointRestoresMissionHealthEnemiesAndCollectedSupplies()
        {
            yield return Load();
            var mission = session.portMission; mission.Restore(true, true, false, false);
            var enemy = session.Enemies[0]; enemy.Damage(1000); enemy.Damage(1000);
            Assert.That(session.Kills, Is.EqualTo(1));
            var pickup = session.Pickups[0]; pickup.Collect(session);
            session.State.health = 64; session.reserve = 19;
            session.player.GetComponent<WeaponLoadout>().Equip(2);
            Assert.That(session.SaveCheckpoint(), Is.True);
            Vector3 position = session.player.transform.position;
            session.State.health = 10; session.reserve = 0; mission.Restore(false, false, false, false); pickup.Restore(false); enemy.Restore(70, enemy.transform.position);
            Assert.That(session.LoadCheckpoint(), Is.True);
            Assert.That(session.State.health, Is.EqualTo(64)); Assert.That(session.reserve, Is.EqualTo(19));
            Assert.That(mission.PowerOn, Is.True); Assert.That(mission.controlDoor.activeSelf, Is.False);
            Assert.That(pickup.Collected, Is.True); Assert.That(enemy.IsDead, Is.True);
            Assert.That(session.Kills, Is.EqualTo(1)); Assert.That(session.player.transform.position, Is.EqualTo(position));
            Assert.That(session.player.GetComponent<WeaponLoadout>().Index, Is.EqualTo(2));
            File.WriteAllText(path, "{malformed"); Assert.That(session.LoadCheckpoint(), Is.False);
        }
        [UnityTest] public IEnumerator InfectedAttackHasWindupCanMissAndWallsBlockSight()
        {
            yield return Load();
            var enemy = session.Enemies[0]; enemy.gameObject.SetActive(true); enemy.useNavigation = false;
            Vector3 start = session.portMission.arrival.position + Vector3.right * 2;
            Move(enemy.transform, start); Move(session.player.transform, start + Vector3.forward * 1.2f);
            enemy.transform.forward = Vector3.forward; yield return null;
            Assert.That(enemy.Mode, Is.EqualTo(EnemyMode.Windup)); Assert.That(session.State.health, Is.EqualTo(100));
            Move(session.player.transform, start + Vector3.forward * 4);
            yield return new WaitForSeconds(0.8f); Assert.That(session.State.health, Is.EqualTo(100), "Dodge must avoid committed attack");
            Move(session.player.transform, enemy.transform.position + Vector3.forward * 1.2f);
            yield return new WaitForSeconds(2.4f); Assert.That(session.State.health, Is.LessThan(100));
            session.State.health = 100; session.State.bleeding = 0;
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.transform.position = enemy.transform.position + Vector3.forward * 0.6f;
            wall.transform.localScale = new Vector3(4, 3, 0.15f); Physics.SyncTransforms();
            yield return new WaitForSeconds(2.3f); Assert.That(session.State.health, Is.EqualTo(100), "Walls must stop attacks");
            Object.Destroy(wall);
        }
        [UnityTest] public IEnumerator CaptureCompositedMenuHudAndHelp()
        {
            yield return Load(false); Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(0.4f); yield return Capture(session, "first-level-menu.png");
            session.BeginMission(); session.State.health = 72; session.State.bleeding = 0.2f;
            yield return new WaitForSecondsRealtime(0.3f); yield return Capture(session, "first-level-hud.png");
            session.Hud.ToggleHelp(); yield return null; yield return Capture(session, "first-level-help.png");
            session.Hud.ClosePanels();
            var hudCanvas = session.Hud.GetComponentInChildren<Canvas>(); hudCanvas.gameObject.SetActive(false);
            float size = session.player.view.orthographicSize; session.player.view.orthographicSize = 2.4f;
            for (int i = 0; i < 3; i++)
            {
                session.player.GetComponent<WeaponLoadout>().Equip(i); yield return null;
                yield return Capture(session, "weapon-" + i + ".png");
            }
            hudCanvas.gameObject.SetActive(true); session.player.view.orthographicSize = size;
        }
        [UnityTest] public IEnumerator Native3DWeaponsSwitchWithoutCreatingAmmoAndBlockDuringActions()
        {
            yield return Load();
            Assert.That(session.player.view.targetTexture, Is.Null);
            Assert.That(Object.FindObjectsByType<RawImage>().Length, Is.Zero, "No low-res world presentation layer");
            var loadout = session.player.GetComponent<WeaponLoadout>();
            Assert.That(loadout.weapons.Length, Is.EqualTo(3));
            int total = session.rounds + session.reserve;
            for (int i = 0; i < 3; i++)
            {
                Assert.That(loadout.Equip(i), Is.True); yield return null;
                Assert.That(loadout.Current.visualPrefab, Is.Not.Null); Assert.That(loadout.Muzzle, Is.Not.Null);
                Assert.That(loadout.socket.GetComponentsInChildren<MeshRenderer>().Length, Is.GreaterThan(0));
                Assert.That(session.rounds + session.reserve, Is.EqualTo(total));
            }
            session.State.health = 50; session.BeginHeal();
            Assert.That(loadout.Equip(0), Is.False, "Switching must not bypass treatment");
            session.TogglePause(); Assert.That(loadout.Equip(0), Is.False);
        }
        [UnityTest] public IEnumerator AllWeaponModelsShootFromTheirMuzzlesAndCannotBypassWalls()
        {
            yield return Load();
            var loadout = session.player.GetComponent<WeaponLoadout>();
            var enemy = session.Enemies[0]; enemy.gameObject.SetActive(true); enemy.useNavigation = false;
            Vector3 position = session.portMission.arrival.position;
            Move(session.player.transform, position);
            var debris = GameObject.CreatePrimitive(PrimitiveType.Sphere); debris.name = "Cosmetic fragment"; debris.layer = 2;
            debris.transform.position = position + Vector3.up * 1.1f + Vector3.forward; debris.transform.localScale = Vector3.one * .5f; Physics.SyncTransforms();
            for (int i = 0; i < 3; i++)
            {
                loadout.Equip(i); session.player.transform.forward = Vector3.forward;
                enemy.Restore(enemy.MaxHealth, position + Vector3.forward * 1.6f); Physics.SyncTransforms();
                int rounds = session.rounds;
                session.player.SendMessage("Shoot");
                Assert.That(enemy.health, Is.LessThan(enemy.MaxHealth), loadout.Current.displayName + " must hit from its actual muzzle");
                Assert.That(session.rounds, Is.EqualTo(rounds - 1), "Pellets cost one round per shot");
            }
            Object.Destroy(debris);
            enemy.Restore(enemy.MaxHealth, position + Vector3.forward * 1.6f);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = position + Vector3.forward * 0.55f + Vector3.up;
            wall.transform.localScale = new Vector3(3, 3, 0.12f); Physics.SyncTransforms();
            loadout.Equip(0); session.player.SendMessage("Shoot");
            Assert.That(enemy.health, Is.EqualTo(enemy.MaxHealth), "A barrel crossing a wall must not allow firing through it");
            Object.Destroy(wall); yield return null;
        }
        internal static IEnumerator Capture(SandboxSession session, string file, int width = 1280, int height = 720)
        {
            Directory.CreateDirectory("TestResults"); string full = Path.GetFullPath("TestResults/" + file);
            // Native world capture at 1280x720, then the same live HUD canvases.
            var main = session.player.view;
            var worldTexture = new RenderTexture(width, height, 24);
            main.targetTexture = worldTexture;
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = worldTexture };
            RenderPipeline.SubmitRenderRequest(main, request); RenderPipeline.SubmitRenderRequest(main, request);
            main.targetTexture = null;
            var backdrop = new GameObject("Native screenshot backdrop", typeof(Canvas));
            backdrop.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            backdrop.GetComponent<Canvas>().sortingOrder = -100;
            var worldImage = new GameObject("Native frame", typeof(RectTransform), typeof(RawImage));
            worldImage.transform.SetParent(backdrop.transform, false);
            var worldRect = worldImage.GetComponent<RectTransform>(); worldRect.anchorMin = Vector2.zero; worldRect.anchorMax = Vector2.one; worldRect.offsetMin = worldRect.offsetMax = Vector2.zero;
            worldImage.GetComponent<RawImage>().texture = worldTexture;
            var camera = new GameObject("UI capture camera").AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 5; camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black; camera.cullingMask = 1 << 5;
            var texture = new RenderTexture(width, height, 24); camera.targetTexture = texture;
            var canvases = Object.FindObjectsByType<Canvas>();
            var layers = new System.Collections.Generic.Dictionary<GameObject, int>();
            foreach (var canvas in canvases)
            {
                foreach (var node in canvas.GetComponentsInChildren<Transform>(true))
                    if (!layers.ContainsKey(node.gameObject)) { layers[node.gameObject] = node.gameObject.layer; node.gameObject.layer = 5; }
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 2;
            }
            yield return null;
            Canvas.ForceUpdateCanvases(); camera.Render(); camera.Render();
            var previous = RenderTexture.active;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = texture; image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                File.WriteAllBytes(full, image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
                foreach (var node in layers) node.Key.layer = node.Value;
                camera.targetTexture = null; Object.Destroy(camera.gameObject); Object.Destroy(image); texture.Release(); Object.Destroy(texture);
                Object.Destroy(backdrop); worldTexture.Release(); Object.Destroy(worldTexture);
            }
            Assert.That(new FileInfo(full).Length, Is.GreaterThan(1000));
            yield return null;
        }
    }
}
