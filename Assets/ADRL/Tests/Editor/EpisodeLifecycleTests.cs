namespace ADRL.Tests.Editor.Simulation
{
    using NUnit.Framework;
    using UnityEngine;
    using ADRL.Tests.Editor;
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Core.Simulation;

    /// <summary>
    /// Verifies the M1 episode lifecycle: SimulationManager owns episode start/
    /// completion, emits EpisodeStartedEvent with the correct number, and
    /// converts a single AgentEpisodeEndedEvent into a single
    /// EpisodeCompletedEvent. Events arriving outside a running episode are
    /// ignored.
    /// </summary>
    [TestFixture]
    internal sealed class EpisodeLifecycleTests
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
        public void Initialize_SetsState_Ready()
        {
            _manager.Initialize(_bus);
            Assert.AreEqual(SimulationState.Ready, _manager.CurrentState);
        }

        [Test]
        public void StartSimulation_PublishesEpisodeStarted_WithIncrementedNumber()
        {
            var started = -1;
            _bus.Subscribe<EpisodeStartedEvent>(e => started = e.EpisodeNumber);

            _manager.Initialize(_bus);
            _manager.StartSimulation();

            Assert.AreEqual(1, started);
            Assert.AreEqual(1, _manager.CurrentEpisode);
            Assert.AreEqual(SimulationState.Running, _manager.CurrentState);
        }

        [Test]
        public void AgentEpisodeEnded_PublishesSingleEpisodeCompleted()
        {
            var completed = 0;
            EpisodeCompletedEvent received = default;
            _bus.Subscribe<EpisodeCompletedEvent>(e => { completed++; received = e; });

            _manager.Initialize(_bus);
            _manager.StartSimulation();
            _bus.Publish(new AgentEpisodeEndedEvent(0, 0.5f, 42));

            Assert.AreEqual(1, completed);
            Assert.AreEqual(1, received.EpisodeNumber);
            Assert.AreEqual(0.5f, received.TotalReward, 1e-6f);
            Assert.AreEqual(42, received.StepsCompleted);
            Assert.AreEqual(SimulationState.Completed, _manager.CurrentState);
        }

        [Test]
        public void AgentEpisodeEnded_OnlyOneCompletionPerEpisode()
        {
            var completed = 0;
            _bus.Subscribe<EpisodeCompletedEvent>(_ => completed++);

            _manager.Initialize(_bus);
            _manager.StartSimulation();
            _bus.Publish(new AgentEpisodeEndedEvent(0, 0.1f, 10));
            _bus.Publish(new AgentEpisodeEndedEvent(0, 0.2f, 20)); // late/duplicate report

            Assert.AreEqual(1, completed);
        }

        [Test]
        public void EventsOutsideRunningEpisode_AreIgnored()
        {
            var completed = 0;
            _bus.Subscribe<EpisodeCompletedEvent>(_ => completed++);

            _manager.Initialize(_bus); // state Ready, never started
            _bus.Publish(new AgentEpisodeEndedEvent(0, 0.5f, 42));

            Assert.AreEqual(0, completed);
        }
    }
}
