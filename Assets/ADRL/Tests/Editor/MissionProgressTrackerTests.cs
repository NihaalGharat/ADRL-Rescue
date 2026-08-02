namespace ADRL.Tests.Editor.Mission
{
    using NUnit.Framework;
    using ADRL.Core.Events;
    using ADRL.Environment.Core;
    using ADRL.Environment.Events;
    using ADRL.Tests.Editor;
    using UnityEngine;

    /// <summary>
    /// Verifies <see cref="MissionProgressTracker"/> counts registered and rescued
    /// victims and publishes <see cref="MissionCompletedEvent"/> exactly once when
    /// all are rescued. The tracker must never control rewards, episodes, or
    /// simulation state.
    /// </summary>
    [TestFixture]
    internal sealed class MissionProgressTrackerTests
    {
        [Test]
        public void NoCompletion_UntilAllVictimsRescued()
        {
            using var fixture = CreateFixture();

            fixture.Bus.Publish(new VictimRegisteredEvent(1));
            fixture.Bus.Publish(new VictimRegisteredEvent(2));

            fixture.Bus.Publish(new VictimRescuedEvent(1));

            Assert.AreEqual(2, fixture.Tracker.TotalVictims);
            Assert.AreEqual(1, fixture.Tracker.RescuedVictims);
            Assert.IsFalse(fixture.Tracker.IsMissionCompleted);
            Assert.AreEqual(0, fixture.CompletedCount);
        }

        [Test]
        public void PublishesCompletion_WhenAllRescued()
        {
            using var fixture = CreateFixture();

            fixture.Bus.Publish(new VictimRegisteredEvent(1));
            fixture.Bus.Publish(new VictimRegisteredEvent(2));

            fixture.Bus.Publish(new VictimRescuedEvent(1));
            fixture.Bus.Publish(new VictimRescuedEvent(2));

            Assert.AreEqual(1, fixture.CompletedCount);
            Assert.AreEqual(2, fixture.Received.RescuedCount);
            Assert.AreEqual(2, fixture.Received.TotalVictimCount);
            Assert.IsTrue(fixture.Tracker.IsMissionCompleted);
        }

        [Test]
        public void PublishesCompletion_ExactlyOnce()
        {
            using var fixture = CreateFixture();

            fixture.Bus.Publish(new VictimRegisteredEvent(1));
            fixture.Bus.Publish(new VictimRescuedEvent(1));
            fixture.Bus.Publish(new VictimRescuedEvent(1)); // late duplicate rescue event

            Assert.AreEqual(1, fixture.CompletedCount);
            Assert.IsTrue(fixture.Tracker.IsMissionCompleted);
        }

        [Test]
        public void NoCompletion_WhenNoVictimsRegistered()
        {
            using var fixture = CreateFixture();

            fixture.Bus.Publish(new VictimRescuedEvent(1));

            Assert.AreEqual(0, fixture.CompletedCount);
            Assert.IsFalse(fixture.Tracker.IsMissionCompleted);
        }

        [Test]
        public void Reset_RestartsProgress_KeepsRegisteredTotal()
        {
            using var fixture = CreateFixture();

            fixture.Bus.Publish(new VictimRegisteredEvent(1));
            fixture.Bus.Publish(new VictimRescuedEvent(1));
            Assert.AreEqual(1, fixture.CompletedCount);

            fixture.Bus.Publish(new EnvironmentResetEvent());

            Assert.AreEqual(1, fixture.Tracker.TotalVictims);
            Assert.AreEqual(0, fixture.Tracker.RescuedVictims);
            Assert.IsFalse(fixture.Tracker.IsMissionCompleted);

            fixture.Bus.Publish(new VictimRescuedEvent(1));
            Assert.AreEqual(2, fixture.CompletedCount);
        }

        private static Fixture CreateFixture()
        {
            var bus = new EventBus();
            bus.Initialize();
            var trackerObject = new GameObject("[TestMissionProgressTracker]");
            var tracker = trackerObject.AddComponent<MissionProgressTracker>();
            tracker.Initialize(bus);
            return new Fixture(bus, tracker, trackerObject);
        }

        private sealed class Fixture : System.IDisposable
        {
            public readonly EventBus Bus;
            public readonly MissionProgressTracker Tracker;
            public readonly GameObject TrackerObject;
            public int CompletedCount;
            public MissionCompletedEvent Received;

            public Fixture(EventBus bus, MissionProgressTracker tracker, GameObject trackerObject)
            {
                Bus = bus;
                Tracker = tracker;
                TrackerObject = trackerObject;
                bus.Subscribe<MissionCompletedEvent>(e => { CompletedCount++; Received = e; });
            }

            public void Dispose()
            {
                TestUtilities.DestroySafe(TrackerObject);
                Bus.Shutdown();
            }
        }
    }
}
