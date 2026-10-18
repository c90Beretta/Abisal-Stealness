using NUnit.Framework;
using UnityEngine;
using Zombtoy.Core;

namespace Zombtoy.Tests
{
    public sealed class SurvivalStateTests
    {
        private SurvivalConfig config;

        [SetUp] public void SetUp() => config = ScriptableObject.CreateInstance<SurvivalConfig>();
        [TearDown] public void TearDown() => Object.DestroyImmediate(config);

        [Test] public void SixSecondsConsumeOneSerumPoint()
        {
            var state = new SurvivalState();
            state.Tick(6f, config);
            Assert.That(state.serum, Is.EqualTo(99f).Within(0.001f));
        }

        [Test] public void OnlyLowSerumIncreasesInfection()
        {
            var state = new SurvivalState { serum = 50 };
            state.Tick(1, config);
            Assert.That(state.infection, Is.Zero);
            state.serum = 39;
            state.Tick(2, config);
            Assert.That(state.infection, Is.EqualTo(1));
        }

        [TestCase(0, 0)] [TestCase(29, 0)] [TestCase(30, 1)]
        [TestCase(64, 1)] [TestCase(65, 2)] [TestCase(99, 2)] [TestCase(100, 3)]
        public void PhaseBoundaries(float infection, int phase)
        {
            Assert.That(new SurvivalState { infection = infection }.Phase, Is.EqualTo(phase));
        }

        [Test] public void InjectionClampsAndConsumesOneDose()
        {
            var state = new SurvivalState { serum = 90, doses = 1 };
            Assert.That(state.Inject(config), Is.True);
            Assert.That(state.serum, Is.EqualTo(100));
            Assert.That(state.doses, Is.Zero);
            Assert.That(state.Inject(config), Is.False);
        }

        [Test] public void TwoInjectionsAtWindowBoundaryTriggerManiaAndLock()
        {
            var state = new SurvivalState();
            state.Inject(config);
            state.Tick(60, config);
            state.Inject(config);
            Assert.That(state.IsManic, Is.True);
            Assert.That(state.InventoryLocked, Is.True);
            state.Tick(10, config);
            Assert.That(state.InventoryLocked, Is.False);
            Assert.That(state.IsManic, Is.True);
            state.Tick(20, config);
            Assert.That(state.IsManic, Is.False);
        }

        [Test] public void InjectionsOutsideWindowDoNotTriggerMania()
        {
            var state = new SurvivalState();
            state.Inject(config);
            state.Tick(60.1f, config);
            state.Inject(config);
            Assert.That(state.IsManic, Is.False);
        }

        [Test] public void MaximumInfectionTriggersOnceAndCanExpire()
        {
            var state = new SurvivalState { infection = 99.9f, serum = 0 };
            state.Tick(1, config);
            Assert.That(state.IsManic, Is.True);
            state.Tick(31, config);
            Assert.That(state.IsManic, Is.False);
        }

        [Test] public void DeadAndPausedSimulationDoNotConsumeResources()
        {
            var state = new SurvivalState();
            state.Tick(0, config);
            Assert.That(state.serum, Is.EqualTo(100));
            state.health = 0;
            state.Tick(10, config);
            Assert.That(state.serum, Is.EqualTo(100));
            Assert.That(state.Inject(config), Is.False);
        }
    }
}
