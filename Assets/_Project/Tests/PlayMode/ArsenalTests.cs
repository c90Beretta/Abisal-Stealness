using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Zombtoy.Gameplay;
namespace Zombtoy.Tests
{
    public sealed class ArsenalTests
    {
        [UnityTest] public IEnumerator DeploymentSelectsHeavyWeaponAndGrantsKitOnlyOnce()
        {
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("Level01_Port");yield return null;
            var session=Object.FindAnyObjectByType<SandboxSession>();
            var arsenal=session.GetComponent<ArsenalExpansion>();Assert.That(arsenal,Is.Not.Null);
            var buttons=arsenal.GetComponentsInChildren<Button>();
            buttons.Single(b=>b.name=="Explorador").onClick.Invoke();
            buttons.Single(b=>b.name=="6  Arma pesada").onClick.Invoke();
            buttons.Single(b=>b.name=="Reserva de municion").onClick.Invoke();
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(buttons.Single(b=>b.name=="1  Arma corta").gameObject);
            yield return null;
            int reserve=session.reserve;
            yield return FirstLevelTests.Capture(session,"arsenal-menu.png");
            session.BeginMission();yield return null;
            Assert.That(session.player.GetComponent<WeaponLoadout>().Index,Is.EqualTo(5));
            Assert.That(session.reserve,Is.EqualTo(reserve+24));
            session.TogglePause();session.Resume();yield return null;
            Assert.That(session.reserve,Is.EqualTo(reserve+24),"Pause cannot duplicate the deployment grant");
            var specials=session.Enemies.Select(e=>e.GetComponent<SpecialInfected>()).Where(e=>e!=null).ToArray();
            Assert.That(specials.Select(e=>e.Species).Distinct().Count(),Is.EqualTo(3));
            Assert.That(session.player.GetComponent<WeaponLoadout>().weapons[3].range,Is.GreaterThan(50));
            Time.timeScale=1;
        }
    }
}
