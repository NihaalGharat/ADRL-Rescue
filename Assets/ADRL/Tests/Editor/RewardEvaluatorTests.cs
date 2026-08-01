namespace ADRL.Tests.Editor.Rewards
{
    using NUnit.Framework;
    using UnityEngine;
    using ADRL.AI.DecisionMaking;
    using ADRL.AI.Rewards;
    using ADRL.Core.Events;

    /// <summary>
    /// Verifies the M3 reward mathematics: time penalty, novelty-per-cell,
    /// potential shaping F = gamma*Phi(s') - Phi(s), stuck/oscillation detection,
    /// reward scaling, continuous-reward clipping, terminal-reward bypass, and
    /// total-reward accumulation. All assertions use the public API only.
    /// </summary>
    [TestFixture]
    internal sealed class RewardEvaluatorTests
    {
        private TestHarness _h;

        [SetUp]
        public void SetUp() => _h = new TestHarness(droneId: 0);

        [TearDown]
        public void TearDown() => _h?.Dispose();

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new RewardEvaluator(null, new FakeRewardSink(), new EventBus(), 0));
            Assert.Throws<System.ArgumentNullException>(
                () => new RewardEvaluator(_h.Config, null, new EventBus(), 0));
            Assert.Throws<System.ArgumentNullException>(
                () => new RewardEvaluator(_h.Config, new FakeRewardSink(), null, 0));
        }

        [Test]
        public void Constructor_SubscribesToTerminalEvents()
        {
            // A newly constructed evaluator reacts to its own drone's terminal events.
            _h.EventBus.Publish(new DroneEnergyDepletedEvent(0));
            Assert.AreEqual(-15f, _h.Evaluator.CurrentBreakdown.EnergyPenaltyReward, 1e-6f);
        }

        [Test]
        public void TimePenalty_AppliedEveryStep_CorrectMagnitude()
        {
            // Disable stuck detection so it does not interfere with time penalty isolation.
            TestUtilities.SetPrivateField(_h.Config, "_stuckDetectionWindow", 1000f);

            // First step visits the cell (novelty+potential). Second step on the
            // same cell yields time penalty only, isolating the rate.
            _h.Evaluator.UpdateStep(1f, Vector3.zero, DroneCommand.Idle);
            var bd1 = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(-0.01f, bd1.TimePenaltyReward, 1e-6f);

            _h.Evaluator.UpdateStep(1f, Vector3.zero, DroneCommand.Idle);
            var bd2 = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(-0.02f, bd2.TimePenaltyReward, 1e-6f);

            // Second step reward: time penalty only (no stuck, no novelty, no potential on same cell).
            Assert.AreEqual(-0.01f, _h.Evaluator.EpisodeReward - bd1.TotalReward, 1e-6f);
            Assert.AreEqual(bd1.NoveltyCellsVisited, bd2.NoveltyCellsVisited);
        }

        [Test]
        public void Novelty_RewardedOncePerCell_RepeatedCellsIgnored()
        {
            var p0 = new Vector3(0f, 0f, 0f);      // cell (0,0)
            var p1 = new Vector3(2.5f, 0f, 0f);     // cell (1,0) - cellSize 2
            var p2 = new Vector3(5f, 0f, 0f);       // cell (2,0)

            const float dt = 0.02f;
            _h.Evaluator.UpdateStep(dt, p0, DroneCommand.Idle); // new
            _h.Evaluator.UpdateStep(dt, p1, DroneCommand.Idle); // new
            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(2, bd.NoveltyCellsVisited);
            Assert.AreEqual(0.10f, bd.NoveltyReward, 1e-6f);

            // Repeat a visited cell: no change to novelty accounting.
            _h.Evaluator.UpdateStep(dt, p1, DroneCommand.Idle);
            bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(2, bd.NoveltyCellsVisited);
            Assert.AreEqual(0.10f, bd.NoveltyReward, 1e-6f);

            // A third distinct cell grants the final novelty bonus.
            _h.Evaluator.UpdateStep(dt, p2, DroneCommand.Idle);
            bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(3, bd.NoveltyCellsVisited);
            Assert.AreEqual(0.15f, bd.NoveltyReward, 1e-6f);
        }

        [Test]
        public void PotentialShaping_FollowsFGammaPhiFormula()
        {
            // Phi(s) = visited cell count. F = gamma*Phi(s') - Phi(s).
            // ShapingScale=0.1, gamma=0.99, RewardScale=1 (defaults).
            var p0 = new Vector3(0f, 0f, 0f);
            var p1 = new Vector3(2.5f, 0f, 0f);
            var p2 = new Vector3(5f, 0f, 0f);
            const float dt = 0.02f;

            _h.Evaluator.UpdateStep(dt, p0, DroneCommand.Idle); // Phi:0->1, F=0.1*(0.99*1-0)=0.099
            _h.Evaluator.UpdateStep(dt, p1, DroneCommand.Idle); // Phi:1->2, F=0.1*(0.99*2-1)=0.098
            _h.Evaluator.UpdateStep(dt, p2, DroneCommand.Idle); // Phi:2->3, F=0.1*(0.99*3-2)=0.097

            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(0.294f, bd.PotentialReward, 1e-5f); // 0.099+0.098+0.097
            Assert.AreEqual(3, bd.NoveltyCellsVisited);
        }

        [Test]
        public void PotentialShaping_Disabled_DeliversNoPotentialReward()
        {
            TestUtilities.SetPrivateField(_h.Config, "_shapingEnabled", false);
            _h.Evaluator.Reset(Vector3.zero);

            _h.Evaluator.UpdateStep(0.02f, new Vector3(2.5f, 0f, 0f), DroneCommand.Idle);

            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(0f, bd.PotentialReward, 0f);
            Assert.AreEqual(0.05f, bd.NoveltyReward, 1e-6f);
        }

        [Test]
        public void StuckPenalty_FiresOncePerWindow_AndIsClipped()
        {
            // StuckPenalty default -0.5 is a continuous reward -> clipped to MinStepReward.
            // Idle command => oscillation tracking is skipped (command.IsIdle == true).
            const float dt = 1f;
            _h.Evaluator.UpdateStep(dt, Vector3.zero, DroneCommand.Idle); // timer 1 (<2)
            _h.Evaluator.UpdateStep(dt, Vector3.zero, DroneCommand.Idle); // timer 2 (>=2), dist 0 < 0.1

            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(-0.1f, bd.StuckPenaltyReward, 1e-6f); // -0.5 clipped to -0.1
            Assert.AreEqual(1, bd.StuckEvents);
        }

        [Test]
        public void StuckPenalty_SkippedWhenMovingBeyondThreshold()
        {
            const float dt = 1f;
            _h.Evaluator.UpdateStep(dt, Vector3.zero, DroneCommand.Idle);      // establishes origin
            _h.Evaluator.UpdateStep(dt, new Vector3(2f, 0f, 0f), DroneCommand.Idle); // dist 2 >= 0.1

            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(0f, bd.StuckPenaltyReward, 0f);
            Assert.AreEqual(0, bd.StuckEvents);
        }

        [Test]
        public void Oscillation_Penalty_FiresAfterThreshold_ResetsWindow()
        {
            // Isolate oscillation: disable the stuck window so it never competes.
            TestUtilities.SetPrivateField(_h.Config, "_stuckDetectionWindow", 1000f);

            const float dt = 0.25f;
            var cmd = RewardTestCommands.Forward; // non-idle => reversal tracking active

            // Positions alternate on x by 10 units => one horizontal reversal per step.
            for (int i = 0; i < 8; i++)
            {
                var x = (i % 2 == 0) ? 0f : 10f;
                _h.Evaluator.UpdateStep(dt, new Vector3(x, 0f, 0f), cmd);
            }

            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(1, bd.OscillationEvents);
            Assert.AreEqual(-0.1f, bd.OscillationPenaltyReward, 1e-6f); // -0.5 clipped to -0.1

            // Window must reset: another 8 steps produce exactly one more penalty.
            for (int i = 8; i < 16; i++)
            {
                var x = (i % 2 == 0) ? 0f : 10f;
                _h.Evaluator.UpdateStep(dt, new Vector3(x, 0f, 0f), cmd);
            }

            bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(2, bd.OscillationEvents);
            Assert.AreEqual(-0.2f, bd.OscillationPenaltyReward, 1e-6f);
        }

        [Test]
        public void RewardScale_ScalesContinuousRewards()
        {
            TestUtilities.SetPrivateField(_h.Config, "_rewardScale", 2f);
            _h.Evaluator.Reset(Vector3.zero);

            // First step: new cell => potential 0.099*2=0.198, novelty 0.05*2=0.10, time -0.01*2=-0.02.
            _h.Evaluator.UpdateStep(1f, Vector3.zero, DroneCommand.Idle);
            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(-0.02f, bd.TimePenaltyReward, 1e-6f);
            Assert.AreEqual(0.198f, bd.PotentialReward, 1e-6f);
            Assert.AreEqual(0.10f, bd.NoveltyReward, 1e-6f);
        }

        [Test]
        public void ContinuousRewards_AreClipped_ToStepRange()
        {
            // Large deltaTime pushes the time reward past MaxStepReward (0.1).
            _h.Evaluator.UpdateStep(100f, Vector3.zero, DroneCommand.Idle); // raw -1.0 -> clamped -0.1
            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(-0.1f, bd.TimePenaltyReward, 1e-6f);
        }

        [Test]
        public void TerminalRewards_BypassClipping_AndAreNotScaled()
        {
            // Default EnergyDepletedPenalty=-15 is way outside the continuous clip
            // range; terminal rewards use AddReward (no clamp, no scale).
            _h.Evaluator.Reset(Vector3.zero);
            _h.EventBus.Publish(new DroneEnergyDepletedEvent(0));
            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(-15f, bd.EnergyPenaltyReward, 1e-6f);
            Assert.AreEqual(-15f, _h.Evaluator.EpisodeReward, 1e-6f);

            _h.EventBus.Publish(new DroneOutOfBoundsEvent(0));
            bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(-10f, bd.OutOfBoundsPenaltyReward, 1e-6f);
        }

        [Test]
        public void TerminalRewards_WrongDroneId_Ignored()
        {
            _h.Evaluator.Reset(Vector3.zero);
            _h.EventBus.Publish(new DroneEnergyDepletedEvent(999));
            Assert.AreEqual(0f, _h.Evaluator.CurrentBreakdown.EnergyPenaltyReward, 0f);
            Assert.AreEqual(0, _h.Sink.Count);
        }

        [Test]
        public void TotalReward_MatchesSinkAccumulation()
        {
            _h.Evaluator.UpdateStep(0.5f, new Vector3(2.5f, 0f, 0f), DroneCommand.Idle);
            _h.Evaluator.UpdateStep(0.5f, new Vector3(5f, 0f, 0f), DroneCommand.Idle);
            _h.Evaluator.UpdateStep(0.5f, new Vector3(2.5f, 0f, 0f), DroneCommand.Idle);
            _h.EventBus.Publish(new DroneOutOfBoundsEvent(0));

            var bd = _h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(_h.Sink.Sum, _h.Evaluator.EpisodeReward, 1e-6f);
            Assert.AreEqual(_h.Sink.Sum, bd.TotalReward, 1e-6f);
        }
    }
}
