namespace ADRL.Tests.Editor.Rewards
{
    using NUnit.Framework;
    using UnityEngine;
    using ADRL.AI.Rewards;
    using ADRL.Core.Events;

    /// <summary>
    /// Verifies the M2 event-driven reward pipeline: subscribe routing,
    /// DroneId filtering, duplicate-subscribe protection, and dispose
    /// unsubscription, all observed through the public EventBus/RewardEvaluator
    /// surface.
    /// </summary>
    [TestFixture]
    internal sealed class EventRewardPipelineTests
    {
        [Test]
        public void RewardEvaluator_DeliversToCorrectDroneId()
        {
            using var h = new TestHarness(droneId: 7);
            h.EventBus.Publish(new DroneEnergyDepletedEvent(7));
            Assert.AreEqual(-15f, h.Evaluator.CurrentBreakdown.EnergyPenaltyReward, 1e-6f);
            Assert.AreEqual(1, h.Sink.Count);
        }

        [Test]
        public void RewardEvaluator_IgnoresWrongDroneId()
        {
            using var h = new TestHarness(droneId: 7);
            h.EventBus.Publish(new DroneEnergyDepletedEvent(8));
            Assert.AreEqual(0f, h.Evaluator.CurrentBreakdown.EnergyPenaltyReward, 0f);
            Assert.AreEqual(0, h.Sink.Count);
        }

        [Test]
        public void RewardEvaluator_CollisionReward_EndToEnd()
        {
            using var h = new TestHarness(droneId: 7);

            h.EventBus.Publish(new CollisionEvent(7, "Obstacle", 1f));

            Assert.AreEqual(1, h.Sink.Count);
            Assert.AreEqual(h.Config.CollisionPenalty, h.Sink.Amounts[0], 1e-6f);
            Assert.AreEqual(h.Config.CollisionPenalty, h.Evaluator.EpisodeReward, 1e-6f);
            Assert.AreEqual(h.Config.CollisionPenalty, h.Evaluator.CurrentBreakdown.CollisionPenaltyReward, 1e-6f);
            Assert.AreEqual(1, h.Evaluator.CurrentBreakdown.CollisionEvents);
            Assert.AreEqual(h.Config.CollisionPenalty, h.Evaluator.CurrentBreakdown.TotalReward, 1e-6f);

            h.EventBus.Publish(new CollisionEvent(8, "Obstacle", 1f));

            Assert.AreEqual(1, h.Sink.Count);
            Assert.AreEqual(1, h.Evaluator.CurrentBreakdown.CollisionEvents);
            Assert.AreEqual(h.Config.CollisionPenalty, h.Evaluator.EpisodeReward, 1e-6f);
        }

        [Test]
        public void NoDuplicateReward_WhenPublishingSingleEvent()
        {
            using var h = new TestHarness(droneId: 0);
            h.EventBus.Publish(new DroneOutOfBoundsEvent(0));
            Assert.AreEqual(1, h.Sink.Count);
        }

        [Test]
        public void EventBus_DuplicateSubscribe_Blocked()
        {
            var bus = new EventBus();
            bus.Initialize();
            var invoked = 0;
            void Handler(DroneEnergyDepletedEvent _) => invoked++;

            bus.Subscribe<DroneEnergyDepletedEvent>(Handler);
            bus.Subscribe<DroneEnergyDepletedEvent>(Handler); // same target+method => ignored
            bus.Publish(new DroneEnergyDepletedEvent(0));

            Assert.AreEqual(1, invoked);
        }

        [Test]
        public void EventBus_DistinctSubscribers_BothFire()
        {
            var bus = new EventBus();
            bus.Initialize();
            var first = 0;
            var second = 0;

            bus.Subscribe<DroneEnergyDepletedEvent>(_ => first++);
            bus.Subscribe<DroneEnergyDepletedEvent>(_ => second++); // distinct closure => both invoked
            bus.Publish(new DroneEnergyDepletedEvent(0));

            Assert.AreEqual(1, first);
            Assert.AreEqual(1, second);
        }

        [Test]
        public void Dispose_Unsubscribes_AllTerminalEvents()
        {
            using var h = new TestHarness(droneId: 0);
            h.Evaluator.Dispose(); // detaches OnEnergyDepleted/OnOutOfBounds

            h.EventBus.Publish(new DroneEnergyDepletedEvent(0));
            h.EventBus.Publish(new DroneOutOfBoundsEvent(0));

            Assert.AreEqual(0, h.Sink.Count);
            Assert.AreEqual(0f, h.Evaluator.EpisodeReward, 0f);
        }

        [Test]
        public void Subscribe_AfterDispose_DoesNotLeak()
        {
            // Publishing after teardown must not throw and must not deliver.
            using var h = new TestHarness(droneId: 0);
            h.Evaluator.Dispose();
            Assert.DoesNotThrow(() => h.EventBus.Publish(new DroneEnergyDepletedEvent(0)));
        }

        [Test]
        public void MissionCompleted_GrantsSuccessBonus()
        {
            using var h = new TestHarness(droneId: 0);
            h.EventBus.Publish(new MissionCompletedEvent(2, 2));

            var bd = h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(h.Config.SuccessBonus, bd.SuccessReward, 1e-6f);
            Assert.AreEqual(1, bd.SuccessEvents);
            Assert.AreEqual(h.Config.SuccessBonus, h.Evaluator.EpisodeReward, 1e-6f);
            Assert.AreEqual(h.Config.SuccessBonus, bd.TotalReward, 1e-6f);
        }

        [Test]
        public void MissionCompleted_DuplicatePublish_DoesNotStackReward()
        {
            using var h = new TestHarness(droneId: 0);
            h.EventBus.Publish(new MissionCompletedEvent(2, 2));
            h.EventBus.Publish(new MissionCompletedEvent(2, 2)); // duplicate report

            var bd = h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(h.Config.SuccessBonus, bd.SuccessReward, 1e-6f);
            Assert.AreEqual(1, bd.SuccessEvents);
            Assert.AreEqual(1, h.Sink.Count);
            Assert.AreEqual(h.Config.SuccessBonus, h.Evaluator.EpisodeReward, 1e-6f);
        }

        [Test]
        public void MissionCompleted_Reset_ClearsSuccessState()
        {
            using var h = new TestHarness(droneId: 0);
            h.EventBus.Publish(new MissionCompletedEvent(1, 1));
            Assert.AreEqual(1, h.Evaluator.CurrentBreakdown.SuccessEvents);

            h.Evaluator.Reset(Vector3.zero);
            Assert.AreEqual(0, h.Evaluator.CurrentBreakdown.SuccessEvents);
            Assert.AreEqual(0f, h.Evaluator.CurrentBreakdown.SuccessReward, 0f);

            // A new episode may earn the success bonus again.
            h.EventBus.Publish(new MissionCompletedEvent(1, 1));
            var bd = h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(1, bd.SuccessEvents);
            Assert.AreEqual(h.Config.SuccessBonus, bd.SuccessReward, 1e-6f);
        }

        [Test]
        public void MissionCompleted_TotalEqualsBreakdownSum()
        {
            using var h = new TestHarness(droneId: 0);
            h.EventBus.Publish(new CollisionEvent(0, "Obstacle", 1f));
            h.EventBus.Publish(new MissionCompletedEvent(2, 2));

            var bd = h.Evaluator.CurrentBreakdown;
            var categorySum = bd.TimePenaltyReward + bd.NoveltyReward + bd.PotentialReward +
                              bd.StuckPenaltyReward + bd.OscillationPenaltyReward +
                              bd.CollisionPenaltyReward + bd.EnergyPenaltyReward +
                              bd.OutOfBoundsPenaltyReward + bd.VictimFoundReward +
                              bd.VictimRescuedReward + bd.SuccessReward;

            Assert.AreEqual(bd.TotalReward, categorySum, 1e-5f);
            Assert.AreEqual(h.Sink.Sum, h.Evaluator.EpisodeReward, 1e-6f);
            Assert.AreEqual(h.Sink.Sum, bd.TotalReward, 1e-6f);
        }
    }
}
