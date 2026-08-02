namespace ADRL.Tests.Editor.Interaction
{
    using NUnit.Framework;
    using ADRL.AI.Interaction;
    using ADRL.Core.Events;
    using ADRL.Environment.Victims;
    using ADRL.Sensors.Interfaces;
    using ADRL.Tests.Editor;
    using UnityEngine;

    /// <summary>
    /// Verifies the Phase 8.1.1 victim pipeline: the victim is the single owner of
    /// its lifecycle and publishes <see cref="VictimFoundEvent"/> and
    /// <see cref="VictimRescuedEvent"/> exactly once per guarded transition, and
    /// <see cref="DroneVictimInteraction"/> drives that lifecycle purely through
    /// <see cref="IVictimDetectable"/>.
    /// </summary>
    [TestFixture]
    internal sealed class VictimInteractionTests
    {
        [Test]
        public void MarkDetected_PublishesFound_ExactlyOnce()
        {
            var bus = CreateBus();
            var (victim, victimObject) = CreateVictim(bus);

            var count = 0;
            VictimFoundEvent received = default;
            bus.Subscribe<VictimFoundEvent>(e => { count++; received = e; });

            var detectable = (IVictimDetectable)victim;
            detectable.MarkDetected();
            detectable.MarkDetected(); // duplicate call is a no-op

            Assert.AreEqual(1, count);
            Assert.AreEqual(victim.Id, received.VictimId);
            Assert.AreEqual(VictimState.Detected, victim.State);
            Cleanup(bus, victimObject);
        }

        [Test]
        public void MarkRescued_RequiresDetectedState()
        {
            var bus = CreateBus();
            var (victim, victimObject) = CreateVictim(bus);

            var count = 0;
            bus.Subscribe<VictimRescuedEvent>(_ => count++);

            ((IVictimDetectable)victim).MarkRescued(); // Waiting -> no-op
            Assert.AreEqual(0, count);
            Assert.AreEqual(VictimState.Waiting, victim.State);

            ((IVictimDetectable)victim).MarkDetected();
            ((IVictimDetectable)victim).MarkRescued();

            Assert.AreEqual(1, count);
            Assert.AreEqual(VictimState.Rescued, victim.State);
            Cleanup(bus, victimObject);
        }

        [Test]
        public void FoundAndRescued_EachFireExactlyOnce_EndToEnd()
        {
            var bus = CreateBus();
            var (victim, victimObject) = CreateVictim(bus);
            var found = 0;
            var rescued = 0;
            bus.Subscribe<VictimFoundEvent>(_ => found++);
            bus.Subscribe<VictimRescuedEvent>(_ => rescued++);

            var detectable = (IVictimDetectable)victim;
            detectable.MarkDetected();
            detectable.MarkRescued();
            detectable.MarkRescued(); // already rescued -> no-op

            Assert.AreEqual(1, found);
            Assert.AreEqual(1, rescued);
            Cleanup(bus, victimObject);
        }

        [Test]
        public void IsAlive_TurnsFalse_AfterRescue()
        {
            var bus = CreateBus();
            var (victim, victimObject) = CreateVictim(bus);
            var detectable = (IVictimDetectable)victim;

            Assert.IsTrue(detectable.IsAlive);
            detectable.MarkDetected();
            Assert.IsTrue(detectable.IsAlive);
            detectable.MarkRescued();
            Assert.IsFalse(detectable.IsAlive);
            Cleanup(bus, victimObject);
        }

        [Test]
        public void Interaction_MarksDetected_InsideDetectionRange()
        {
            var bus = CreateBus();
            var (victim, victimObject) = CreateVictim(bus, position: Vector3.forward * 10f);
            var interaction = CreateInteraction();

            var found = 0;
            bus.Subscribe<VictimFoundEvent>(_ => found++);

            interaction.Scan();

            Assert.AreEqual(1, found);
            Assert.AreEqual(VictimState.Detected, victim.State);
            Cleanup(bus, victimObject, interaction.gameObject);
        }

        [Test]
        public void Interaction_MarksRescued_InsideRescueRange()
        {
            var bus = CreateBus();
            var (victim, victimObject) = CreateVictim(bus, position: Vector3.forward * 1f);
            var interaction = CreateInteraction(detectionRange: 30f, rescueRange: 2f);

            var found = 0;
            var rescued = 0;
            bus.Subscribe<VictimFoundEvent>(_ => found++);
            bus.Subscribe<VictimRescuedEvent>(_ => rescued++);

            interaction.Scan();

            Assert.AreEqual(1, found);
            Assert.AreEqual(1, rescued);
            Assert.AreEqual(VictimState.Rescued, victim.State);
            Cleanup(bus, victimObject, interaction.gameObject);
        }

        [Test]
        public void Interaction_IgnoresOutOfRangeVictims()
        {
            var bus = CreateBus();
            var (victim, victimObject) = CreateVictim(bus, position: Vector3.forward * 100f);
            var interaction = CreateInteraction(detectionRange: 30f, rescueRange: 2f);

            var found = 0;
            bus.Subscribe<VictimFoundEvent>(_ => found++);

            interaction.Scan();

            Assert.AreEqual(0, found);
            Assert.AreEqual(VictimState.Waiting, victim.State);
            Cleanup(bus, victimObject, interaction.gameObject);
        }

        private static EventBus CreateBus()
        {
            var bus = new EventBus();
            bus.Initialize();
            return bus;
        }

        private static (Victim, GameObject) CreateVictim(EventBus bus, Vector3? position = null)
        {
            var victimObject = new GameObject("Victim");
            if (position.HasValue)
                victimObject.transform.position = position.Value;

            var victim = victimObject.AddComponent<Victim>();
            victim.SetId(1);
            victim.Initialize(bus);
            victimObject.AddComponent<SphereCollider>(); // solid collider required by sensors/interaction
            return (victim, victimObject);
        }

        private static DroneVictimInteraction CreateInteraction(
            float detectionRange = 30f, float rescueRange = 2f)
        {
            var interactionObject = new GameObject("Drone");
            var interaction = interactionObject.AddComponent<DroneVictimInteraction>();
            interaction.Initialize(detectionRange, rescueRange);
            return interaction;
        }

        private static void Cleanup(EventBus bus, params Object[] objects)
        {
            foreach (var obj in objects)
                TestUtilities.DestroySafe(obj);
            bus.Shutdown();
        }
    }
}
