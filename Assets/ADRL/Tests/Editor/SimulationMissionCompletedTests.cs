namespace ADRL.Tests.Editor.Simulation
{
    using NUnit.Framework;
    using UnityEngine;
    using ADRL.Tests.Editor;
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Core.Simulation;

    /// <summary>
    /// Verifies Phase 8.1.1 Task 4: <see cref="SimulationManager"/> remains the sole
    /// owner of the episode lifecycle and finalizes the current episode when
    /// <see cref="MissionCompletedEvent"/> is published, mirroring the
    /// AgentEpisodeEndedEvent completion path.
    /// </summary>
    [TestFixture]
    internal sealed class SimulationMissionCompletedTests
    {
        private SimulationManager _manager;
        private EventBus _bus;
        private SimulationConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = TestUtilities.CreateSimulationConfig(autoStart: false);
            _bus = new EventBus();
            _bus.Initialize();
            _manager = SimulationManager.CreateInstance(_config);
        }

        [TearDown]
        public void TearDown()
        {
            _bus.Shutdown();
            _bus = null;
            if (_manager != null)
                Object.DestroyImmediate(_manager.gameObject);
            _manager = null;
            TestUtilities.DestroySafe(_config);
        }

        [Test]
        public void MissionCompleted_PublishesSingleEpisodeCompleted()
        {
            var completed = 0;
            var stopped = 0;
            EpisodeCompletedEvent received = default;
            _bus.Subscribe<EpisodeCompletedEvent>(e => { completed++; received = e; });
            _bus.Subscribe<SimulationStoppedEvent>(_ => stopped++);

            _manager.Initialize(_bus);
            _manager.StartSimulation();
            _bus.Publish(new MissionCompletedEvent(2, 2));

            Assert.AreEqual(1, completed);
            Assert.AreEqual(1, received.EpisodeNumber);
            Assert.AreEqual(1, stopped);
            Assert.AreEqual(SimulationState.Completed, _manager.CurrentState);
        }

        [Test]
        public void MissionCompleted_OutsideRunningEpisode_IsIgnored()
        {
            var completed = 0;
            _bus.Subscribe<EpisodeCompletedEvent>(_ => completed++);

            _manager.Initialize(_bus); // state Ready, never started
            _bus.Publish(new MissionCompletedEvent(1, 1));

            Assert.AreEqual(0, completed);
            Assert.AreEqual(SimulationState.Ready, _manager.CurrentState);
        }

        [Test]
        public void MissionCompleted_OnlyCompletesOncePerEpisode()
        {
            var completed = 0;
            _bus.Subscribe<EpisodeCompletedEvent>(_ => completed++);

            _manager.Initialize(_bus);
            _manager.StartSimulation();
            _bus.Publish(new MissionCompletedEvent(1, 1));
            _bus.Publish(new MissionCompletedEvent(1, 1)); // late/duplicate report

            Assert.AreEqual(1, completed);
        }
    }
}
