namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Execution;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;
    using UnityEngine;

    /// <summary>
    /// Verifies the Phase 8.9 Autonomous Behaviour Optimization Framework: the
    /// <see cref="BehaviourOptimizer"/> refines <em>how</em> the already-selected
    /// behaviour executes without ever changing the mission objective, the priority
    /// decision or the behaviour itself, the executors apply the resulting
    /// <see cref="BehaviourExecutionProfile"/> as pure scaling of their base
    /// movement constants, and the <see cref="OptimizationValidator"/> confirms every
    /// produced profile is valid and deterministic.
    /// </summary>
    [TestFixture]
    internal sealed class BehaviourOptimizerTests
    {
        private static MissionTask Task(MissionTaskState state) => new(state, 1f, MissionTaskState.SearchArea, true);

        private static SituationSnapshot ObstacleSituation(float proximity) => new(false, 0f, 0f, proximity, 0f, true);

        private static SituationSnapshot TargetSituation(float proximity) => new(true, proximity, 0f, 0f, 0f, true);

        private static BehaviourMemory MemoryWithVictim()
        {
            var memory = new BehaviourMemory(2);
            memory.Store(BehaviourState.Approach, 0f);
            return memory;
        }

        private static BehaviourMemory MemoryWithObstacle()
        {
            var memory = new BehaviourMemory(2);
            memory.Store(BehaviourState.Avoid, 0f);
            return memory;
        }

        private static DecisionContextSnapshot Snapshot(
            BehaviourState behaviour,
            MissionTaskState missionState,
            float winningConfidence,
            SituationSnapshot assessment,
            BehaviourMemory memory)
        {
            var mission = Task(missionState);
            var winning = new TaskPriority(mission, 1f, winningConfidence, true);
            return new DecisionContextSnapshot(
                assessment,
                memory,
                mission,
                new TaskCandidate[0],
                winning,
                behaviour,
                DroneCommand.Idle,
                DecisionDiagnostics.Empty,
                DecisionRuntimeState.Empty);
        }

        private static BehaviourOptimizer Optimizer(OptimizationPolicy? policy = null)
        {
            return new BehaviourOptimizer(policy ?? OptimizationPolicy.Default);
        }

        private static ISensorReading ObstacleReading(float proximity) => new SensorReading(new[]
        {
            0f, 0f, 0f, 0f, 0f, 0f, proximity, 0f, 0f, 0f, 0f, 0f,
        });

        private static DecisionEngine CreateEngine()
        {
            return new DecisionEngine(
                DecisionContext.Default,
                new FogOfWarSituationAssessor(DecisionContext.Default),
                new BehaviourSelector(DecisionContext.Default));
        }

        private static void AssertProfileValid(BehaviourExecutionProfile profile)
        {
            Assert.IsTrue(OptimizationValidator.IsValid(profile));
            Assert.IsTrue(OptimizationValidator.ConfidenceInRange(profile));
            Assert.IsTrue(OptimizationValidator.SpeedMultiplierInRange(profile));
            Assert.IsTrue(OptimizationValidator.TurnRateMultiplierInRange(profile));
            Assert.IsTrue(OptimizationValidator.CautionInRange(profile));
        }

        private static void AssertCommandEqual(DroneCommand a, DroneCommand b)
        {
            Assert.AreEqual(a.IsIdle, b.IsIdle);
            Assert.AreEqual(a.MoveDirection.x, b.MoveDirection.x, 1e-5f);
            Assert.AreEqual(a.MoveDirection.y, b.MoveDirection.y, 1e-5f);
            Assert.AreEqual(a.MoveDirection.z, b.MoveDirection.z, 1e-5f);
            Assert.AreEqual(a.Yaw, b.Yaw, 1e-5f);
        }

        [Test]
        public void IdenticalInputs_ProduceIdenticalProfile()
        {
            var optimizer = Optimizer();
            var snapshot = Snapshot(BehaviourState.Avoid, MissionTaskState.AvoidHazard, 1f, ObstacleSituation(0.9f), MemoryWithObstacle());

            var a = optimizer.Optimize(BehaviourState.Avoid, snapshot);
            var b = optimizer.Optimize(BehaviourState.Avoid, snapshot);

            Assert.AreEqual(a.SpeedMultiplier, b.SpeedMultiplier, 1e-6f);
            Assert.AreEqual(a.TurnRateMultiplier, b.TurnRateMultiplier, 1e-6f);
            Assert.AreEqual(a.CautionLevel, b.CautionLevel, 1e-6f);
            Assert.AreEqual(a.ExecutionConfidence, b.ExecutionConfidence, 1e-6f);
            Assert.IsTrue(OptimizationValidator.IsDeterministic(optimizer, BehaviourState.Avoid, snapshot));
        }

        [Test]
        public void Search_OptimizesSpeed()
        {
            var optimizer = Optimizer();
            var snapshot = Snapshot(BehaviourState.Search, MissionTaskState.SearchArea, 1f, ObstacleSituation(0.1f), BehaviourMemory.Empty);

            var profile = optimizer.Optimize(BehaviourState.Search, snapshot);

            Assert.AreEqual(1.15f, profile.SpeedMultiplier, 1e-6f);
            Assert.AreEqual(1.30f, profile.TurnRateMultiplier, 1e-6f);
            AssertProfileValid(profile);
        }

        [Test]
        public void Approach_OptimizesTurnRate()
        {
            var optimizer = Optimizer();
            var snapshot = Snapshot(BehaviourState.Approach, MissionTaskState.InvestigateTarget, 1f, TargetSituation(0.9f), MemoryWithVictim());

            var profile = optimizer.Optimize(BehaviourState.Approach, snapshot);

            Assert.AreEqual(1.20f, profile.SpeedMultiplier, 1e-6f);
            Assert.AreEqual(1.30f, profile.TurnRateMultiplier, 1e-6f);
            AssertProfileValid(profile);
        }

        [Test]
        public void Avoid_RaisesCaution()
        {
            var optimizer = Optimizer();
            var snapshot = Snapshot(BehaviourState.Avoid, MissionTaskState.AvoidHazard, 1f, ObstacleSituation(0.9f), MemoryWithObstacle());

            var profile = optimizer.Optimize(BehaviourState.Avoid, snapshot);

            Assert.Greater(profile.CautionLevel, 0f);
            Assert.Less(profile.SpeedMultiplier, 1f);
            Assert.GreaterOrEqual(profile.TurnRateMultiplier, OptimizationValidator.TurnRateMultiplierMin);
            AssertProfileValid(profile);
        }

        [Test]
        public void Rescue_OptimizesSpeed()
        {
            var optimizer = Optimizer();
            var snapshot = Snapshot(BehaviourState.Approach, MissionTaskState.RescueVictim, 1f, TargetSituation(0.9f), MemoryWithVictim());

            var profile = optimizer.Optimize(BehaviourState.Approach, snapshot);

            Assert.AreEqual(OptimizationPolicy.Default.VictimApproachDistance, profile.PreferredDistance, 1e-6f);
            Assert.AreEqual(1.10f, profile.SpeedMultiplier, 1e-6f);
            AssertProfileValid(profile);
        }

        [Test]
        public void Idle_ReturnsEmptyProfile()
        {
            var optimizer = Optimizer();
            var snapshot = Snapshot(BehaviourState.Idle, MissionTaskState.Idle, 0f, SituationSnapshot.Invalid, BehaviourMemory.Empty);

            var profile = optimizer.Optimize(BehaviourState.Idle, snapshot);

            Assert.IsTrue(profile.IsEmpty);
            Assert.AreEqual(BehaviourExecutionProfile.Empty, profile);
            Assert.IsTrue(OptimizationValidator.IsValid(profile));
        }

        [Test]
        public void ConfidenceScaling_ScalesConfidence()
        {
            var full = Optimizer().Optimize(
                BehaviourState.Search,
                Snapshot(BehaviourState.Search, MissionTaskState.SearchArea, 1f, ObstacleSituation(0.1f), BehaviourMemory.Empty));

            var scaledPolicy = new OptimizationPolicy(
                1.15f, 1.20f, 0.75f, 1.10f, 1.30f, 0.85f, 0.60f, 1.5f, 0.5f, 0.5f);
            var half = new BehaviourOptimizer(scaledPolicy).Optimize(
                BehaviourState.Search,
                Snapshot(BehaviourState.Search, MissionTaskState.SearchArea, 1f, ObstacleSituation(0.1f), BehaviourMemory.Empty));

            Assert.AreEqual(1f, full.ExecutionConfidence, 1e-6f);
            Assert.AreEqual(0.5f, half.ExecutionConfidence, 1e-6f);
            Assert.AreEqual(1.15f, full.SpeedMultiplier, 1e-6f);
            Assert.AreEqual(1.00f, half.SpeedMultiplier, 1e-6f);
        }

        [Test]
        public void Smoothing_AppliedFromPolicy()
        {
            var optimizer = Optimizer();
            var snapshot = Snapshot(BehaviourState.Search, MissionTaskState.SearchArea, 1f, ObstacleSituation(0.1f), BehaviourMemory.Empty);

            var profile = optimizer.Optimize(BehaviourState.Search, snapshot);

            Assert.AreEqual(OptimizationPolicy.Default.SmoothingFactor, profile.Smoothness, 1e-6f);
        }

        [Test]
        public void SpeedMultiplier_ClampedToRange()
        {
            var lowPolicy = new OptimizationPolicy(1.15f, 1.20f, 0.1f, 1.10f, 1.30f, 0.85f, 0.60f, 1.5f, 1f, 0.5f);
            var low = new BehaviourOptimizer(lowPolicy).Optimize(
                BehaviourState.Avoid,
                Snapshot(BehaviourState.Avoid, MissionTaskState.AvoidHazard, 1f, ObstacleSituation(0.9f), MemoryWithObstacle()));
            Assert.AreEqual(OptimizationValidator.SpeedMultiplierMin, low.SpeedMultiplier, 1e-6f);
            Assert.IsTrue(OptimizationValidator.SpeedMultiplierInRange(low));

            var highPolicy = new OptimizationPolicy(1.15f, 1.20f, 5f, 1.10f, 1.30f, 0.85f, 0.60f, 1.5f, 1f, 0.5f);
            var high = new BehaviourOptimizer(highPolicy).Optimize(
                BehaviourState.Avoid,
                Snapshot(BehaviourState.Avoid, MissionTaskState.AvoidHazard, 1f, ObstacleSituation(0.9f), MemoryWithObstacle()));
            Assert.AreEqual(OptimizationValidator.SpeedMultiplierMax, high.SpeedMultiplier, 1e-6f);
            Assert.IsTrue(OptimizationValidator.SpeedMultiplierInRange(high));
        }

        [Test]
        public void TurnRateMultiplier_ClampedToRange()
        {
            var policy = new OptimizationPolicy(1.15f, 1.20f, 0.75f, 1.10f, 5f, 3f, 0.60f, 1.5f, 1f, 0.5f);
            var optimizer = new BehaviourOptimizer(policy);
            var snapshot = Snapshot(BehaviourState.Search, MissionTaskState.SearchArea, 1f, ObstacleSituation(0.1f), BehaviourMemory.Empty);

            var profile = optimizer.Optimize(BehaviourState.Search, snapshot);

            Assert.AreEqual(OptimizationValidator.TurnRateMultiplierMax, profile.TurnRateMultiplier, 1e-6f);
            Assert.IsTrue(OptimizationValidator.TurnRateMultiplierInRange(profile));
        }

        [Test]
        public void ProfileValidity_Accepted()
        {
            var optimizer = Optimizer();

            AssertProfileValid(optimizer.Optimize(
                BehaviourState.Search,
                Snapshot(BehaviourState.Search, MissionTaskState.SearchArea, 1f, ObstacleSituation(0.1f), BehaviourMemory.Empty)));
            AssertProfileValid(optimizer.Optimize(
                BehaviourState.Approach,
                Snapshot(BehaviourState.Approach, MissionTaskState.RescueVictim, 1f, TargetSituation(0.9f), MemoryWithVictim())));
            AssertProfileValid(optimizer.Optimize(
                BehaviourState.Avoid,
                Snapshot(BehaviourState.Avoid, MissionTaskState.AvoidHazard, 1f, ObstacleSituation(0.9f), MemoryWithObstacle())));
        }

        [Test]
        public void Validator_RejectsOutOfRangeProfile()
        {
            var tooFast = new BehaviourExecutionProfile(5f, 1f, 0f, 0f, 0f, 0.5f);
            var badConfidence = new BehaviourExecutionProfile(1f, 1f, 0f, 0f, 0f, 1.5f);
            var badCaution = new BehaviourExecutionProfile(1f, 1f, 2f, 0f, 0f, 0.5f);
            var badTurn = new BehaviourExecutionProfile(1f, 0f, 0f, 0f, 0f, 0.5f);

            Assert.IsFalse(OptimizationValidator.IsValid(tooFast));
            Assert.IsFalse(OptimizationValidator.IsValid(badConfidence));
            Assert.IsFalse(OptimizationValidator.IsValid(badCaution));
            Assert.IsFalse(OptimizationValidator.IsValid(badTurn));
            Assert.IsFalse(OptimizationValidator.SpeedMultiplierInRange(tooFast));
            Assert.IsFalse(OptimizationValidator.ConfidenceInRange(badConfidence));
            Assert.IsFalse(OptimizationValidator.CautionInRange(badCaution));
            Assert.IsFalse(OptimizationValidator.TurnRateMultiplierInRange(badTurn));
        }

        [Test]
        public void Validator_ConfirmsDeterminism()
        {
            var optimizer = Optimizer();
            var snapshot = Snapshot(BehaviourState.Avoid, MissionTaskState.AvoidHazard, 1f, ObstacleSituation(0.9f), MemoryWithObstacle());

            Assert.IsTrue(OptimizationValidator.IsDeterministic(optimizer, BehaviourState.Avoid, snapshot));
            Assert.IsTrue(OptimizationValidator.IsDeterministic(optimizer, BehaviourState.Search, snapshot));
            Assert.IsTrue(OptimizationValidator.IsDeterministic(optimizer, BehaviourState.Idle, snapshot));
        }

        [Test]
        public void ExecutorLegacyPath_MatchesEmptyProfile()
        {
            var assessment = ObstacleSituation(0.8f);
            var search = new SearchExecutor(0.6f, 0.15f);
            var approach = new ApproachExecutor(0.8f, 0.8f, 0.2f);
            var avoid = new AvoidExecutor(0.3f, 0.9f, 0.25f);
            var idle = new IdleExecutor();

            AssertCommandEqual(search.Resolve(assessment), search.Resolve(assessment, BehaviourExecutionProfile.Empty));
            AssertCommandEqual(approach.Resolve(assessment), approach.Resolve(assessment, BehaviourExecutionProfile.Empty));
            AssertCommandEqual(avoid.Resolve(assessment), avoid.Resolve(assessment, BehaviourExecutionProfile.Empty));
            AssertCommandEqual(idle.Resolve(assessment), idle.Resolve(assessment, BehaviourExecutionProfile.Empty));
        }

        [Test]
        public void EngineIntegration_OptimizationApplied()
        {
            var engine = CreateEngine();
            var result = engine.Decide(ObstacleReading(0.9f));

            var snapshot = engine.LastSnapshot;
            var profile = snapshot.ExecutionProfile;

            Assert.AreEqual(BehaviourState.Avoid, snapshot.Behaviour);
            Assert.AreEqual(MissionTaskState.AvoidHazard, snapshot.Mission.State);
            Assert.AreEqual(MissionTaskState.AvoidHazard, snapshot.Winning.Task.State);
            Assert.IsFalse(result.Command.IsIdle);

            Assert.Greater(profile.ExecutionConfidence, 0f);
            AssertProfileValid(profile);

            Assert.AreEqual(profile.SpeedMultiplier, snapshot.RuntimeState.ExecutionSpeedMultiplier, 1e-6f);
            Assert.AreEqual(profile.TurnRateMultiplier, snapshot.RuntimeState.ExecutionTurnRate, 1e-6f);
            Assert.AreEqual(profile.ExecutionConfidence, snapshot.RuntimeState.ExecutionConfidence, 1e-6f);
            Assert.AreEqual(snapshot.RuntimeState.DecisionTimestamp, snapshot.RuntimeState.OptimizationTimestamp, 1e-6f);

            var expected = new AvoidExecutor(0.3f, 0.9f, 0.25f)
                .Resolve(snapshot.Assessment, profile);
            AssertCommandEqual(expected, result.Command);
            AssertCommandEqual(expected, snapshot.Command);

            Assert.IsTrue(DecisionContextValidator.IsValid(snapshot));

            // Determinism: a second engine over the same reading yields the same
            // profile and the same command.
            var second = CreateEngine();
            var repeat = second.Decide(ObstacleReading(0.9f));
            var repeatProfile = second.LastSnapshot.ExecutionProfile;

            Assert.AreEqual(profile.SpeedMultiplier, repeatProfile.SpeedMultiplier, 1e-6f);
            Assert.AreEqual(profile.TurnRateMultiplier, repeatProfile.TurnRateMultiplier, 1e-6f);
            Assert.AreEqual(profile.ExecutionConfidence, repeatProfile.ExecutionConfidence, 1e-6f);
            AssertCommandEqual(result.Command, repeat.Command);
        }
    }
}
