using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Zombtoy.Gameplay;

namespace Zombtoy.Tests
{
    public sealed class CharacterAndReticleTests
    {
        [UnityTest]
        public IEnumerator SurvivorsHaveDistinctModelsAndReticleReflectsWeaponAccuracy()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync("Level01_Port");
            yield return null;

            var session = Object.FindAnyObjectByType<SandboxSession>();
            Assert.That(session, Is.Not.Null);
            session.BeginMission();
            yield return null;

            // Reticle exists on HUD and uses zero RawImage components
            Assert.That(session.Hud.Reticle, Is.Not.Null);
            Assert.That(Object.FindObjectsByType<RawImage>().Length, Is.Zero);

            // Weapon accuracy spread differs clearly between Sniper (3) and Shotgun (2)
            var loadout = session.player.GetComponent<WeaponLoadout>();
            Assert.That(loadout, Is.Not.Null);

            loadout.Equip(3); // Fusil de precision (0.12 deg)
            yield return null;
            float sniperSpread = session.player.CurrentEffectiveSpreadDegrees;
            float sniperAcc = session.player.CurrentAccuracyNormalized;

            loadout.Equip(2); // Escopeta S12 (6.2 deg)
            yield return null;
            float shotgunSpread = session.player.CurrentEffectiveSpreadDegrees;
            float shotgunAcc = session.player.CurrentAccuracyNormalized;

            Assert.That(shotgunSpread, Is.GreaterThan(sniperSpread * 5f), "Shotgun spread must be significantly wider than sniper");
            Assert.That(sniperAcc, Is.GreaterThan(shotgunAcc), "Sniper precision percentage must exceed shotgun");

            // Firing increases dynamic bloom temporarily and only consumes THAT weapon's caliber (non-shared)
            int pistolMagBefore = session.AmmoPool.GetMag(0);
            int shotgunMagBefore = session.rounds;
            float preShotSpread = session.player.CurrentEffectiveSpreadDegrees;
            session.player.SendMessage("Shoot");
            Assert.That(session.player.CurrentEffectiveSpreadDegrees, Is.GreaterThan(preShotSpread), "Firing must add dynamic bloom");
            Assert.That(session.rounds, Is.EqualTo(shotgunMagBefore - 1), "Shotgun shot must consume 12G round");
            Assert.That(session.AmmoPool.GetMag(0), Is.EqualTo(pistolMagBefore), "Shotgun shot must NOT consume pistol 9mm ammo");

            // Crouching tightens effective spread
            float standingSpread = session.player.CurrentEffectiveSpreadDegrees;
            session.player.SetCrouching(true);
            Assert.That(session.player.CurrentEffectiveSpreadDegrees, Is.LessThan(standingSpread), "Crouching must improve weapon accuracy");
            session.player.SetCrouching(false);

            // If integrated catalog exists, verify all 4 characters spawn distinct models with Humanoid Animators
            var catalog = Resources.Load<SurvivorVisualCatalog>(SurvivorVisualCatalog.ResourcePath);
            if (catalog != null && catalog.IsComplete())
            {
                var roster = session.player.Roster;
                Assert.That(roster, Is.Not.Null);
                var distinctPrefabs = new HashSet<GameObject>();
                for (int i = 0; i < 4; i++)
                {
                    roster.ApplyCharacter(i, true);
                    yield return null;
                    var entry = catalog.GetEntry((SurvivorId)i);
                    Assert.That(entry, Is.Not.Null);
                    Assert.That(entry.modelPrefab, Is.Not.Null);
                    Assert.That(distinctPrefabs.Add(entry.modelPrefab), Is.True, $"Character {i} must use a distinct model prefab");
                    Assert.That(roster.ActiveModelInstance, Is.Not.Null);
                    Assert.That(roster.ActiveAnimator, Is.Not.Null);
                    Assert.That(roster.ActiveAnimator.isHuman, Is.True, $"Character {i} animator must be Humanoid");
                    Assert.That(roster.ActiveAnimator.runtimeAnimatorController, Is.Not.Null);
                }

                // Verify enemy 3D models & animator controller are complete and wired on active enemies
                Assert.That(catalog.enemyAnimatorController, Is.Not.Null);
                Assert.That(catalog.enemyEntries.Length, Is.EqualTo(4));
                var walker = System.Array.Find(session.Enemies, e => e != null && e.gameObject.activeInHierarchy && e.GetComponent<SpecialInfected>() == null);
                Assert.That(walker, Is.Not.Null);
                Assert.That(walker.VisualAdapter, Is.Not.Null);
                Assert.That(walker.VisualAdapter.ModelInstance, Is.Not.Null);
                Assert.That(walker.VisualAdapter.EnemyAnimator, Is.Not.Null);
                Assert.That(walker.VisualAdapter.EnemyAnimator.isHuman, Is.True);
            }
        }
    }
}
