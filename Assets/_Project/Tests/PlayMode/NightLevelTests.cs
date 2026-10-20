using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Zombtoy.Gameplay;

namespace Zombtoy.Tests
{
    public sealed class NightLevelTests
    {
        private SandboxSession session;
        private string path;
        private IEnumerator Load()
        {
            Time.timeScale = 1; yield return SceneManager.LoadSceneAsync("Level01_Port"); yield return null;
            session = Object.FindAnyObjectByType<SandboxSession>();
            path = Path.GetFullPath("TestResults/night-checkpoint-" + System.Guid.NewGuid() + ".json"); session.CheckpointPathOverride = path;
            foreach (var e in session.Enemies) e.gameObject.SetActive(false);
            session.BeginMission();
        }
        private static void Move(Transform actor, Vector3 position)
        {
            var cc = actor.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false;
            actor.position = position; if (cc != null) cc.enabled = true; Physics.SyncTransforms();
        }
        [TearDown] public void Cleanup()
        {
            Time.timeScale = 1;
            if (path != null) foreach (string suffix in new[] { "", ".tmp", ".bak" }) if (File.Exists(path + suffix)) File.Delete(path + suffix);
        }
        [UnityTest] public IEnumerator DarknessHasFlashlightAwarenessAndPausedResources()
        {
            yield return Load(); var n = session.Night; Assert.That(n, Is.Not.Null);
            Assert.That(RenderSettings.ambientSkyColor.maxColorComponent, Is.LessThan(.1f));
            Assert.That(n.flashlight.shadows, Is.EqualTo(LightShadows.Soft));
            // Open quay lane: the new quarantine outer bulkhead occupies x=-18.
            Move(session.player.transform, new Vector3(-10, .1f, -17));
            session.player.transform.forward = Vector3.forward;
            Vector3 ahead = session.player.transform.position + Vector3.forward * 5 + Vector3.up * .9f;
            Assert.That(n.Perceives(ahead), Is.True); n.ToggleLamp(); Assert.That(n.Perceives(ahead), Is.False);
            Assert.That(n.Perceives(session.player.transform.position + Vector3.up), Is.True);
            n.ToggleLamp(); float battery = n.Inventory.battery;
            yield return new WaitForSeconds(.3f); Assert.That(n.Inventory.battery, Is.LessThan(battery));
            session.TogglePause(); battery = n.Inventory.battery; float air = n.Inventory.air;
            yield return new WaitForSecondsRealtime(.2f); Assert.That(n.Inventory.battery, Is.EqualTo(battery)); Assert.That(n.Inventory.air, Is.EqualTo(air));
            session.Resume(); n.Inventory.battery = 0; Assert.That(n.LampLit, Is.False);
            Assert.That(n.UseSupply(0), Is.True); Assert.That(n.Inventory.battery, Is.EqualTo(100));
            Assert.That(n.UseSupply(0), Is.False, "Cannot waste a fresh battery");
        }
        [UnityTest] public IEnumerator LootRespectsCapacityAndCheckpointDoesNotDuplicateContents()
        {
            yield return Load(); var n = session.Night; n.hordes.enabled = false;
            var box = n.Containers.Single(c => c.id == "dock-locker"); Move(session.player.transform, box.transform.position + Vector3.back);
            Assert.That(box.Take(session), Is.True); int ammo = session.reserve; Assert.That(box.stock.Empty, Is.True);
            Assert.That(box.Take(session), Is.False); Assert.That(session.reserve, Is.EqualTo(ammo));
            n.Inventory.battery = 43; n.Inventory.air = 65;
            Assert.That(session.SaveCheckpoint(), Is.True);
            box.stock.ammo = 100; n.Inventory.cells = 0; session.reserve = 0;
            Assert.That(session.LoadCheckpoint(), Is.True); Assert.That(session.reserve, Is.EqualTo(ammo)); Assert.That(box.stock.Empty, Is.True);
            Assert.That(n.Inventory.battery, Is.EqualTo(43)); Assert.That(n.Inventory.air, Is.EqualTo(65));
            var large = new LootStock { ammo = 999, medkits = 20, plates = 20, oxygen = 20 };
            n.Inventory.Transfer(session, large);
            Assert.That(n.Inventory.Weight(session), Is.LessThanOrEqualTo(FieldInventory.Capacity + .001f));
            Assert.That(large.Empty, Is.False); Assert.That(session.reserve, Is.LessThanOrEqualTo(240));
        }
        [UnityTest] public IEnumerator SeveredLegsSlowEnemiesAndHeadKillsOnlyOnce()
        {
            yield return Load(); session.Night.hordes.enabled = false;
            var enemy = session.Enemies.First(e => e.GetComponent<InfectedVariant>().kind == InfectedKind.Walker);
            enemy.gameObject.SetActive(true); Move(enemy.transform, session.player.transform.position + Vector3.forward * 3);
            var gore = enemy.GetComponent<Dismemberment>(); var skin = enemy.GetComponentInChildren<SkinnedMeshRenderer>(); int triangles = skin.sharedMesh.triangles.Length;
            Assert.That(gore.Sever(Severed.LeftLeg), Is.True); Assert.That(gore.Sever(Severed.LeftLeg), Is.False);
            Assert.That(skin.sharedMesh.triangles.Length, Is.LessThan(triangles)); Assert.That(enemy.GetComponent<InfectedVariant>().MoveMultiplier, Is.LessThan(.4f));
            gore.Restore(Severed.None); Assert.That(skin.sharedMesh.triangles.Length, Is.EqualTo(triangles));
            enemy.Hit(40, enemy.transform.position + Vector3.up * 1.65f, false);
            Assert.That(enemy.IsDead, Is.True); Assert.That(gore.Lost & Severed.Head, Is.EqualTo(Severed.Head));
            enemy.Hit(500, enemy.transform.position, true); enemy.Damage(500); Assert.That(session.Kills, Is.EqualTo(1));
            Assert.That(enemy.GetComponent<LootContainer>().Available, Is.True);
            Assert.That(session.SaveCheckpoint(true), Is.True); gore.Restore(Severed.None); enemy.Restore(enemy.MaxHealth, enemy.transform.position);
            Assert.That(session.LoadCheckpoint(), Is.True); Assert.That(enemy.IsDead, Is.True); Assert.That(gore.Lost, Is.EqualTo(Severed.Head));
            yield return null;
        }
        [UnityTest] public IEnumerator HordesAreFiniteCappedAndPreserveReleasedEnemies()
        {
            yield return Load(); var director = session.Night.hordes;
            Assert.That(director.reserves.Length, Is.EqualTo(16)); Assert.That(director.Remaining, Is.EqualTo(16));
            int count = director.Release(session.player.transform.position);
            Assert.That(count, Is.EqualTo(4)); Assert.That(director.Remaining, Is.EqualTo(12));
            Assert.That(director.Release(session.player.transform.position), Is.Zero, "Cooldown prevents wave floods");
            Assert.That(session.Enemies.Count(e => e.gameObject.activeSelf && !e.IsDead), Is.LessThanOrEqualTo(HordeDirector.ActiveLimit));
            Assert.That(session.SaveCheckpoint(true), Is.True);
            director.Restore(new bool[16], 0, 0); Assert.That(session.LoadCheckpoint(), Is.True);
            Assert.That(director.Remaining, Is.EqualTo(12)); Assert.That(director.waves, Is.EqualTo(1));
            yield return null;
        }
        [UnityTest] public IEnumerator MeleeCostsStaminaAndWallsBlockTheSwing()
        {
            yield return Load(); session.Night.hordes.enabled = false;
            var enemy = session.Enemies.First(e => e.GetComponent<InfectedVariant>().kind == InfectedKind.Walker); enemy.gameObject.SetActive(true); enemy.useNavigation = false;
            var player = session.player; player.transform.forward = Vector3.forward;
            Move(enemy.transform, player.transform.position + Vector3.forward * 1.3f); float health = enemy.health;
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.transform.position = player.transform.position + Vector3.forward * .65f + Vector3.up;
            wall.transform.localScale = new Vector3(2, 3, .1f); Physics.SyncTransforms();
            Assert.That(player.TryMelee(), Is.True); Assert.That(enemy.health, Is.EqualTo(health)); Assert.That(session.Night.Inventory.stamina, Is.EqualTo(84).Within(.1f));
            Object.Destroy(wall); yield return new WaitForSeconds(.7f); player.transform.forward = Vector3.forward;
            Move(enemy.transform, player.transform.position + Vector3.forward * 1.3f); Assert.That(player.TryMelee(), Is.True); Assert.That(enemy.health, Is.LessThan(health));
            Assert.That(player.TryMelee(), Is.False, "Cannot bypass swing cooldown");
        }
        [UnityTest] public IEnumerator ArmorAndOxygenHaveGameplayConsequences()
        {
            yield return Load(); var bag = session.Night.Inventory;
            session.TakeDamage(20); Assert.That(session.State.health, Is.EqualTo(87)); Assert.That(bag.armor, Is.EqualTo(93));
            session.State.bleeding = 0; yield return new WaitForSeconds(.7f);
            bag.armorOn = false; session.TakeDamage(20); Assert.That(session.State.health, Is.EqualTo(67));
            session.State.bleeding = 0; yield return new WaitForSeconds(.7f); Move(session.player.transform, new Vector3(5, .1f, 14));
            bag.maskOn = true; bag.air = 50; float health = session.State.health;
            yield return new WaitForSeconds(.3f); Assert.That(session.State.health, Is.EqualTo(health)); Assert.That(bag.air, Is.LessThan(50));
            bag.maskOn = false; yield return new WaitForSeconds(.15f); Assert.That(session.State.health, Is.LessThan(health));
        }
        [UnityTest] public IEnumerator EquipmentDisablesUnusableSuppliesAndShowsLootFeedback()
        {
            yield return Load(); var n = session.Night; n.hordes.enabled = false;
            session.Hud.ToggleEquipment(); yield return null;
            var button = session.Hud.GetComponentsInChildren<Button>().Single(b => b.name == "Cambiar pila");
            Assert.That(button.interactable, Is.False); n.Inventory.battery = 30; yield return null;
            Assert.That(button.interactable, Is.True); int cells = n.Inventory.cells; button.onClick.Invoke(); yield return null;
            Assert.That(n.Inventory.battery, Is.EqualTo(100).Within(.1f)); Assert.That(n.Inventory.cells, Is.EqualTo(cells - 1)); Assert.That(button.interactable, Is.False);
            var box = n.Containers.Single(c => c.id == "dock-locker"); Move(session.player.transform, box.transform.position + Vector3.back);
            session.reserve = 240; n.Inventory.cells = 8; n.Inventory.oxygen = 4; session.Hud.OpenLoot(box); yield return null;
            var take = session.Hud.GetComponentsInChildren<Button>().Single(b => b.name == "Recoger lo que quepa"); take.onClick.Invoke(); yield return null;
            Assert.That(box.stock.Empty, Is.False); Assert.That(session.Hud.GetComponentsInChildren<Text>().Any(t => t.text.StartsWith("Mochila o pila llena")), Is.True);
        }
        [UnityTest] public IEnumerator CaptureNightWorldInventoryAndInfectedModels()
        {
            yield return Load(); session.Night.hordes.enabled = false; session.player.enabled = false; Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            session.player.transform.forward = Vector3.forward;
            yield return new WaitForSecondsRealtime(.3f); yield return FirstLevelTests.Capture(session, "night-hud.png");
            var box = session.Night.Containers.Single(c => c.id == "dock-locker"); Move(session.player.transform, box.transform.position + Vector3.back);
            session.Hud.OpenLoot(box); yield return null; yield return FirstLevelTests.Capture(session, "night-inventory.png"); session.Hud.ClosePanels();
            session.reserve = 240; session.Night.Inventory.cells = 8; session.Night.Inventory.oxygen = 4;
            session.Hud.OpenLoot(box); box.Take(session); yield return null; yield return FirstLevelTests.Capture(session, "night-inventory-feedback.png");
            session.reserve = 24; session.Night.Inventory.cells = 2; session.Night.Inventory.oxygen = 1; session.SetMessage("Repuestos: carga <=70% y uno disponible."); session.Hud.ClosePanels();
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.2f);
            session.Hud.OpenLoot(box); yield return null; yield return FirstLevelTests.Capture(session, "night-inventory-1080.png", 1920, 1080); session.Hud.ClosePanels();
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.2f);
            session.player.view.orthographicSize = 2.4f; session.player.transform.rotation = Quaternion.Euler(0, 150, 0);
            yield return null; yield return FirstLevelTests.Capture(session, "night-soldier.png"); session.player.view.orthographicSize = 3.5f;
            Move(session.player.transform, new Vector3(-10, .1f, -17)); session.player.transform.forward = Vector3.forward;session.ClearMessage();
            int i = 0;
            foreach (var kind in new[] { InfectedKind.Walker, InfectedKind.Runner, InfectedKind.Brute, InfectedKind.Screamer })
            {
                var enemy = session.Enemies.First(e => e.GetComponent<InfectedVariant>().kind == kind); enemy.gameObject.SetActive(true); enemy.enabled = false;
                Move(enemy.transform, session.player.transform.position + new Vector3(-1.65f + i * 1.1f, 0, 1.7f)); enemy.transform.rotation = Quaternion.Euler(0, 180, 0); i++;
            }
            yield return null; yield return FirstLevelTests.Capture(session, "night-infected.png");
        }
    }
}
