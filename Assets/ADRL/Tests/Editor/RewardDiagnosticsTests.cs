namespace ADRL.Tests.Editor.Rewards
{
    using NUnit.Framework;
    using UnityEngine;
    using ADRL.Tests.Editor;
    using ADRL.AI.DecisionMaking;
    using ADRL.AI.Rewards;
    using ADRL.Core.Events;

    /// <summary>
    /// Verifies the M4 reward diagnostics model: live vs snapshot breakdowns,
    /// the Reset snapshot contract, category-sum invariant, counter increments,
    /// and snapshot immutability.
    /// </summary>
    [TestFixture]
    internal sealed class RewardDiagnosticsTests
    {
        private TestHarness _h;

        [SetUp]
        public void SetUp() => _h = new TestHarness(droneId: 0);

        [TearDown]
        public void TearDown() => _h?.Dispose();

        [Test]
        public void CurrentBreakdown_TracksEpisodeReward()
        {
            _h.Evaluator.UpdateStep(0.5f, Vector3.zero, DroneCommand.Idle);
            var bd = _h.Evaluator.CurrentBreakdown;

            Assert.AreEqual(_h.Evaluator.EpisodeReward, bd.TotalReward, 1e-6f);
            Assert.AreEqual(_h.Evaluator.EpisodeReward, _h.Sink.Sum, 1e-6f);
        }

        [Test]
        public void CategoryRewards_SumToTotalReward()
        {
            _h.Evaluator.UpdateStep(0.5f, new Vector3(2.5f, 0f, 0f), DroneCommand.Idle);
            _h.Evaluator.UpdateStep(0.5f, Vector3.zero, DroneCommand.Idle);
            _h.Evaluator.UpdateStep(0.5f, new Vector3(5f, 0f, 0f), DroneCommand.Idle);
            _h.EventBus.Publish(new DroneOutOfBoundsEvent(0));

            var bd = _h.Evaluator.CurrentBreakdown;
            var categorySum = bd.TimePenaltyReward + bd.NoveltyReward + bd.PotentialReward +
                              bd.StuckPenaltyReward + bd.OscillationPenaltyReward +
                              bd.EnergyPenaltyReward +
                              bd.OutOfBoundsPenaltyReward + bd.VictimFoundReward +
                              bd.VictimRescuedReward + bd.SuccessReward;

            Assert.AreEqual(bd.TotalReward, categorySum, 1e-5f);
        }

        [Test]
        public void Reset_SnapshotsPreviousEpisode_IntoLastEpisodeBreakdown()
        {
            // Episode 1: one new cell.
            // Float arithmetic: 0.05 + (0.1 * 0.99) - 0.01 = 0.05 + 0.099000001 - 0.01 = 0.139
            _h.Evaluator.UpdateStep(1f, Vector3.zero, DroneCommand.Idle);
            var priorTotal = _h.Evaluator.EpisodeReward;
            var priorCells = _h.Evaluator.CurrentBreakdown.NoveltyCellsVisited;
            Assert.AreEqual(0.139f, priorTotal, 1e-5f);

            // Start episode 2: the previous totals must be preserved on the snapshot.
            _h.Evaluator.Reset(Vector3.zero);
            var last = _h.Evaluator.LastEpisodeBreakdown;
            Assert.AreEqual(priorTotal, last.TotalReward, 1e-6f);
            Assert.AreEqual(priorCells, last.NoveltyCellsVisited);
            Assert.AreEqual(0f, _h.Evaluator.EpisodeReward, 0f);
            Assert.AreEqual(0, _h.Evaluator.CurrentBreakdown.NoveltyCellsVisited);
        }

        [Test]
        public void CurrentBreakdown_Snapshot_IsImmutable()
        {
            _h.Evaluator.UpdateStep(1f, Vector3.zero, DroneCommand.Idle);
            var snap = _h.Evaluator.CurrentBreakdown;

            _h.Evaluator.UpdateStep(1f, new Vector3(2.5f, 0f, 0f), DroneCommand.Idle);
            var now = _h.Evaluator.CurrentBreakdown;

            // Captured snapshot must not change after further accrual.
            // Float arithmetic: 0.05 + (0.1 * 0.99) - 0.01 = 0.139 (not 0.138)
            Assert.AreEqual(0.139f, snap.TotalReward, 1e-5f);
            Assert.AreEqual(1, snap.NoveltyCellsVisited);
            Assert.AreNotEqual(now.NoveltyCellsVisited, snap.NoveltyCellsVisited);
            Assert.AreNotEqual(now.PotentialReward, snap.PotentialReward);
        }

        [Test]
        public void EventCounters_IncrementPerCategory()
        {
            // 1 stuck window (Idle command => no oscillation), 2 terminal events,
            // 3 distinct cells for novelty.
            _h.Evaluator.UpdateStep(1f, Vector3.zero, DroneCommand.Idle);        // cell(0,0); stuck timer 1
            _h.Evaluator.UpdateStep(1f, Vector3.zero, DroneCommand.Idle);        // stuck timer 2 -> penalty
            _h.EventBus.Publish(new DroneEnergyDepletedEvent(0));
            _h.EventBus.Publish(new DroneOutOfBoundsEvent(0));
            _h.Evaluator.UpdateStep(0.02f, new Vector3(2.5f, 0f, 0f), DroneCommand.Idle); // cell(1,0) new
            _h.Evaluator.UpdateStep(0.02f, new Vector3(5f, 0f, 0f), DroneCommand.Idle);   // cell(2,0) new

            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(3, bd.NoveltyCellsVisited);
            Assert.AreEqual(1, bd.StuckEvents);
            Assert.AreEqual(1, bd.EnergyEvents);
            Assert.AreEqual(1, bd.OutOfBoundsEvents);
            Assert.AreEqual(0, bd.OscillationEvents);
            Assert.AreEqual(0, bd.CollisionEvents);
            Assert.AreEqual(0, bd.VictimFoundEvents);
            Assert.AreEqual(0, bd.VictimRescuedEvents);
            Assert.AreEqual(0, bd.SuccessEvents);
        }

        [Test]
        public void Reset_ClearsCurrentBreakdown()
        {
            _h.Evaluator.UpdateStep(0.5f, new Vector3(2.5f, 0f, 0f), DroneCommand.Idle);
            _h.Evaluator.Reset(Vector3.forward);

            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(0f, bd.TotalReward, 0f);
            Assert.AreEqual(0f, bd.NoveltyReward, 0f);
            Assert.AreEqual(0f, bd.TimePenaltyReward, 0f);
            Assert.AreEqual(0, bd.NoveltyCellsVisited);
        }
    }
}
