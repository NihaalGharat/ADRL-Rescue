namespace ADRL.Tests.Editor.Rewards
{
    using NUnit.Framework;
    using UnityEngine;
    using ADRL.Tests.Editor;
    using ADRL.AI.DecisionMaking;
    using ADRL.AI.Rewards;
    using ADRL.Core.Events;

    /// <summary>
    /// Verifies reproducibility: identical inputs must yield identical reward
    /// totals, per-category accounting, and event counters across separate
    /// evaluators (no hidden random divergence).
    /// </summary>
    [TestFixture]
    internal sealed class DeterminismTests
    {
        private static (RewardBreakdown breakdown, float episodeReward) RunSequence(TestHarness h)
        {
            h.Evaluator.Reset(Vector3.zero);
            h.Evaluator.UpdateStep(0.5f, Vector3.zero, DroneCommand.Idle);               // cell(0,0) new
            h.Evaluator.UpdateStep(0.5f, new Vector3(2.5f, 0f, 0f), DroneCommand.Idle); // cell(1,0) new
            h.Evaluator.UpdateStep(0.5f, Vector3.zero, DroneCommand.Idle);             // repeat cell(0,0)
            h.Evaluator.UpdateStep(0.5f, new Vector3(5f, 0f, 0f), DroneCommand.Idle);   // cell(2,0) new
            h.EventBus.Publish(new DroneOutOfBoundsEvent(0));
            return (h.Evaluator.CurrentBreakdown, h.Evaluator.EpisodeReward);
        }

        [Test]
        public void IdenticalInputs_ProduceIdenticalRewardResults()
        {
            using var a = new TestHarness(0);
            using var b = new TestHarness(0);

            var (bdA, epA) = RunSequence(a);
            var (bdB, epB) = RunSequence(b);

            Assert.AreEqual(epA, epB, 1e-7f);
            AssertBreakdownsEqual(bdA, bdB);
        }

        [Test]
        public void RewardBreakdown_IsDeterministicAcrossRestarts()
        {
            using var h = new TestHarness(0);
            var (first, _) = RunSequence(h);
            var (second, _) = RunSequence(h); // reuse same evaluator after re-Reset

            AssertBreakdownsEqual(first, second);
        }

        private static void AssertBreakdownsEqual(RewardBreakdown a, RewardBreakdown b)
        {
            Assert.AreEqual(a.TotalReward, b.TotalReward, 1e-7f);
            Assert.AreEqual(a.TimePenaltyReward, b.TimePenaltyReward, 1e-7f);
            Assert.AreEqual(a.NoveltyReward, b.NoveltyReward, 1e-7f);
            Assert.AreEqual(a.PotentialReward, b.PotentialReward, 1e-7f);
            Assert.AreEqual(a.StuckPenaltyReward, b.StuckPenaltyReward, 1e-7f);
            Assert.AreEqual(a.OscillationPenaltyReward, b.OscillationPenaltyReward, 1e-7f);
            Assert.AreEqual(a.CollisionPenaltyReward, b.CollisionPenaltyReward, 1e-7f);
            Assert.AreEqual(a.EnergyPenaltyReward, b.EnergyPenaltyReward, 1e-7f);
            Assert.AreEqual(a.OutOfBoundsPenaltyReward, b.OutOfBoundsPenaltyReward, 1e-7f);
            Assert.AreEqual(a.VictimFoundReward, b.VictimFoundReward, 1e-7f);
            Assert.AreEqual(a.VictimRescuedReward, b.VictimRescuedReward, 1e-7f);
            Assert.AreEqual(a.SuccessReward, b.SuccessReward, 1e-7f);

            Assert.AreEqual(a.NoveltyCellsVisited, b.NoveltyCellsVisited);
            Assert.AreEqual(a.StuckEvents, b.StuckEvents);
            Assert.AreEqual(a.OscillationEvents, b.OscillationEvents);
            Assert.AreEqual(a.CollisionEvents, b.CollisionEvents);
            Assert.AreEqual(a.EnergyEvents, b.EnergyEvents);
            Assert.AreEqual(a.OutOfBoundsEvents, b.OutOfBoundsEvents);
            Assert.AreEqual(a.VictimFoundEvents, b.VictimFoundEvents);
            Assert.AreEqual(a.VictimRescuedEvents, b.VictimRescuedEvents);
            Assert.AreEqual(a.SuccessEvents, b.SuccessEvents);
        }
    }
}
