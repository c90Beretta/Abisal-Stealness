using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zombtoy.Gameplay;

namespace Zombtoy.Tests
{
    public sealed class IndustrialLevelTests
    {
        private SandboxSession session;
        private string checkpoint;
        private IEnumerator Load()
        {
            Time.timeScale = 1; yield return SceneManager.LoadSceneAsync("Level01_Port"); yield return null;
            session = Object.FindAnyObjectByType<SandboxSession>(); checkpoint = Path.GetFullPath("TestResults/industrial-checkpoint-" + System.Guid.NewGuid() + ".json"); session.CheckpointPathOverride = checkpoint;
            foreach (var e in session.Enemies) e.gameObject.SetActive(false); session.Night.hordes.enabled = false; session.BeginMission(); session.player.enabled = false;
        }
        private static void Move(Transform actor, Vector3 position)
        { var cc = actor.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false; actor.position = position; if (cc != null) cc.enabled = true; Physics.SyncTransforms(); }
        private SandboxEnemy Victim()
        {
            var e = session.Enemies.First(v => v.GetComponent<InfectedVariant>().kind == InfectedKind.Walker); e.gameObject.SetActive(true); e.enabled = false;
            Move(e.transform, new Vector3(-17, .1f, -15)); return e;
        }
        [TearDown]
        public void Cleanup()
        {
            Time.timeScale = 1; if (checkpoint == null) return; foreach (string suffix in new[] { "", ".tmp", ".bak" }) if (File.Exists(checkpoint + suffix)) File.Delete(checkpoint + suffix);
        }
        [UnityTest]
        public IEnumerator AdultBodiesHaveWeightedFacesAndThirteenJointedBodies()
        {
            yield return Load(); var e = Victim(); var rag = e.GetComponent<RagdollController>(); var skin = e.GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.That(rag.bodies.Length, Is.EqualTo(13)); Assert.That(rag.poseBones.Length, Is.EqualTo(19)); Assert.That(skin.sharedMesh.isReadable, Is.True);
            Assert.That(skin.sharedMesh.bounds.size.y, Is.InRange(1.75f, 1.95f));
            Assert.That(skin.sharedMaterials.Length, Is.EqualTo(6)); Assert.That(skin.sharedMaterials[5].GetTexture("_BaseMap"), Is.Not.Null);
            Assert.That(rag.bodies.All(b => b.body.isKinematic && !b.collider.enabled), Is.True);
            Assert.That(rag.bodies.Count(b => b.bone.GetComponent<ConfigurableJoint>() != null), Is.EqualTo(12));
            Assert.That(session.Enemies.Select(v => v.definition).Distinct().Count(), Is.EqualTo(8), "Two data variants per archetype must occur in the level");
            Assert.That(session.hudFont, Is.Not.Null); Assert.That(session.displayFont, Is.Not.Null);
        }
        [UnityTest]
        public IEnumerator RagdollFallsFromImpactAndSettlesWithoutExploding()
        {
            yield return Load(); var e = Victim(); var rag = e.GetComponent<RagdollController>(); float height = rag.pelvis.position.y;
            e.Hit(1000, e.transform.position + Vector3.up * 1.2f, false); Assert.That(e.IsDead, Is.True); Assert.That(rag.Active, Is.True);
            Assert.That(rag.bodies.Any(b => !b.body.isKinematic && b.collider.enabled), Is.True);
            yield return new WaitForSeconds(2.2f);
            Assert.That(rag.pelvis.position.y, Is.LessThan(height - .2f));
            Assert.That(rag.bodies.All(b => float.IsFinite(b.bone.position.y) && Vector3.Distance(b.bone.position, e.transform.position) < 4), Is.True);
            yield return new WaitForSeconds(3.2f); Assert.That(rag.Settled, Is.True); Assert.That(rag.bodies.All(b => b.body.isKinematic && !b.collider.enabled), Is.True);
            Assert.That(session.Kills, Is.EqualTo(1));
        }
        [UnityTest]
        public IEnumerator MissingLimbsHaveNoRagdollCollisionAndCanBeLooted()
        {
            yield return Load(); var e = Victim(); var gore = e.GetComponent<Dismemberment>(); gore.Sever(Severed.LeftLeg); e.Damage(1000);
            var rag = e.GetComponent<RagdollController>(); Assert.That(rag.bodies.Where(b => b.region == Severed.LeftLeg).All(b => !b.collider.enabled), Is.True);
            yield return new WaitForSeconds(5.2f); Move(session.player.transform, e.CorpseAnchor + Vector3.back * .9f);
            var loot = e.GetComponent<LootContainer>(); Assert.That(loot.Reachable(session), Is.True); Assert.That(loot.Take(session), Is.True); Assert.That(loot.Take(session), Is.False);
        }
        [UnityTest]
        public IEnumerator TenCheckpointCyclesPreserveRagdollPoseLootAndResources()
        {
            yield return Load(); var e = Victim(); var gore = e.GetComponent<Dismemberment>(); gore.Sever(Severed.LeftArm);
            e.transform.rotation = Quaternion.Euler(0, 73, 0); e.Damage(1000); yield return new WaitForSeconds(5.2f);
            var rag = e.GetComponent<RagdollController>(); var pose = rag.Capture(); Assert.That(pose.Valid, Is.True);
            var box = session.Night.Containers.Single(c => c.id == "dock-locker"); Move(session.player.transform, box.Point + Vector3.back);
            box.Take(session); session.State.health = 71; session.State.bleeding = 0; session.Night.Inventory.battery = 52; session.Night.Inventory.air = 63;
            session.State.elapsed = 123; session.State.lastInjection = 105; session.State.maniaUntil = 150; session.State.inventoryLockedUntil = 133; session.State.tremorUntil = 143;
            session.portMission.Restore(true, true, false, false); int reserve = session.reserve; int cells = session.Night.Inventory.cells;
            var rows = new System.Collections.Generic.List<string> { "cycle,save_ok,load_ok,health,reserve,cells,battery,air,pose_max_error,loot_empty" };
            for (int i = 0; i < 10; i++)
            {
                Assert.That(session.SaveCheckpoint(true), Is.True); var before = rag.Capture();
                session.State.health = 20; session.reserve = 0; session.Night.Inventory.cells = 0; box.stock.ammo = 99; gore.Restore(Severed.None); e.Restore(e.MaxHealth, e.transform.position);
                Assert.That(session.LoadCheckpoint(), Is.True); var after = rag.Capture(); Assert.That(after, Is.Not.Null); Assert.That(session.Telemetry.Data.loads, Is.EqualTo(i + 1));
                float error = 0, rotationError = 0; for (int b = 0; b < before.positions.Length; b++) { error = Mathf.Max(error, Vector3.Distance(before.positions[b], after.positions[b])); rotationError = Mathf.Max(rotationError, Quaternion.Angle(before.rotations[b], after.rotations[b])); }
                Assert.That(rotationError, Is.LessThan(.1f)); Assert.That(session.State.elapsed, Is.EqualTo(123)); Assert.That(session.State.lastInjection, Is.EqualTo(105)); Assert.That(session.State.maniaUntil, Is.EqualTo(150)); Assert.That(session.State.inventoryLockedUntil, Is.EqualTo(133)); Assert.That(session.State.tremorUntil, Is.EqualTo(143));
                Assert.That(error, Is.LessThan(.001f)); Assert.That(Quaternion.Angle(e.transform.rotation, Quaternion.Euler(0, 73, 0)), Is.LessThan(.1f));
                Assert.That(session.State.health, Is.EqualTo(71)); Assert.That(session.reserve, Is.EqualTo(reserve)); Assert.That(session.Night.Inventory.cells, Is.EqualTo(cells));
                Assert.That(session.Night.Inventory.battery, Is.EqualTo(52)); Assert.That(session.Night.Inventory.air, Is.EqualTo(63)); Assert.That(gore.Lost, Is.EqualTo(Severed.LeftArm)); Assert.That(box.stock.Empty, Is.True);
                rows.Add($"{i + 1},true,true,71,{reserve},{cells},52,63,{error.ToString(System.Globalization.CultureInfo.InvariantCulture)},true");
            }
            Directory.CreateDirectory("TestResults"); File.WriteAllLines("TestResults/checkpoint-ten-cycles.csv", rows);
        }
        [UnityTest]
        public IEnumerator CorruptRagdollQuaternionIsRejectedBeforeChangingSession()
        {
            yield return Load(); var e = Victim(); e.Damage(1000); yield return new WaitForSeconds(.1f); session.SaveCheckpoint(true);
            var save = JsonUtility.FromJson<PortSave>(File.ReadAllText(checkpoint)); var item = save.enemies.Single(v => v.id == e.name); item.ragdoll.rotations[0] = new Quaternion(3, 0, 0, 0);
            File.WriteAllText(checkpoint, JsonUtility.ToJson(save)); float health = session.State.health; Assert.That(session.LoadCheckpoint(), Is.False); Assert.That(session.State.health, Is.EqualTo(health));
        }
        [UnityTest]
        public IEnumerator CaptureIndustrialPortAndRagdollAtNativeResolution()
        {
            yield return Load(); Screen.SetResolution(1280, 720, FullScreenMode.Windowed); session.player.transform.rotation = Quaternion.Euler(0, 0, 0);
            yield return new WaitForSecondsRealtime(.3f); yield return FirstLevelTests.Capture(session, "industrial-arrival.png");
            var canvas = session.Hud.GetComponentInChildren<Canvas>(); canvas.gameObject.SetActive(false); session.player.view.orthographicSize = 2.65f;
            session.player.transform.rotation=Quaternion.Euler(0,225,0);
            yield return null; yield return FirstLevelTests.Capture(session, "industrial-soldier.png"); canvas.gameObject.SetActive(true); session.player.view.orthographicSize = 2.85f;
            Move(session.player.transform, new Vector3(-22, .1f, 15.2f)); session.player.transform.forward = Vector3.forward;
            yield return null; yield return FirstLevelTests.Capture(session, "industrial-warehouse.png");
            session.portMission.Restore(true, true, true, false); Move(session.player.transform, new Vector3(20, .1f, 16));
            yield return null; yield return FirstLevelTests.Capture(session, "industrial-control.png");
            Move(session.player.transform, new Vector3(-10, .1f, -17)); session.player.transform.forward = Vector3.forward; session.SetMessage(""); session.player.view.orthographicSize = 3.5f;
            var e = Victim(); Move(e.transform, new Vector3(-10, .1f, -15)); e.transform.forward = Vector3.back;
            yield return null; yield return FirstLevelTests.Capture(session, "industrial-fall-00.png");
            e.Damage(1000); float fallStart = Time.time;
            var fallRows = new System.Collections.Generic.List<string> { "frame,seconds,pelvis_x,pelvis_y,pelvis_z,dynamic_bodies" };
            for (int frame = 1; frame <= 4; frame++)
            {
                yield return new WaitForSeconds(frame == 1 ? .16f : frame == 2 ? .35f : frame == 3 ? .65f : 1.4f);
                var rag = e.GetComponent<RagdollController>(); var p = rag.pelvis.position;
                fallRows.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1:F3},{2:F4},{3:F4},{4:F4},{5}", frame, Time.time - fallStart, p.x, p.y, p.z, rag.bodies.Count(b => !b.body.isKinematic)));
                yield return FirstLevelTests.Capture(session, "industrial-fall-0" + frame + ".png");
            }
            Assert.That(Physics.Linecast(session.player.view.transform.position, e.CorpseAnchor, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore), Is.False, "Corpse evidence must have a clear camera sight line");
            File.WriteAllLines("TestResults/industrial-fall-sequence.csv", fallRows);
            yield return FirstLevelTests.Capture(session, "industrial-ragdoll.png");
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.2f);
            yield return FirstLevelTests.Capture(session, "industrial-ragdoll-1080.png", 1920, 1080);
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }
    }
}
