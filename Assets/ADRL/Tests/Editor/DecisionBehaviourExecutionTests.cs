namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Execution;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;
    using UnityEngine;

    /// <summary>
    /// Verifies the Phase 8.4 Behaviour Execution layer: each behaviour executor
    /// owns the movement generation for exactly one behaviour, the factory is the
    /// single behaviour-to-executor mapping, and command generation is
    /// deterministic (the same situation snapshot always yields the identical
    /// <see cref="DroneCommand"/>). Also verifies the decision engine delegates to
    /// the factory rather than generating commands itself.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionBehaviourExecutionTests
    {
        private static readonly DecisionContext DefaultContext = DecisionContext.Default;

        private static BehaviourExecutorFactory CreateFactory() => new(DefaultContext);

        private static SituationSnapshot SearchingSnapshot() =>
            new(false, 0f, 0f, 0f, 0f, true);

        private static SituationSnapshot ApproachingSnapshot(float side = 0.8f) =>
            new(true, 0.6f, side, 0.1f, 0f, true);

        private static SituationSnapshot AvoidingSnapshot(float side = 0.8f) =>
            new(false, 0f, 0f, 0.9f, side, true);

        [Test]
        public void IdleExecutor_ReturnsIdleCommand()
        {
            var executor = new IdleExecutor();

            var command = executor.Resolve(SearchingSnapshot());

            Assert.AreEqual(DroneCommand.Idle, command);
            Assert.IsTrue(command.IsIdle);
            Assert.AreEqual(Vector3.zero, command.MoveDirection);
            Assert.AreEqual(0f, command.Yaw);
        }

        [Test]
        public void SearchExecutor_ProducesDeterministicExplorationCommand()
        {
            var executor = new SearchExecutor(0.6f, 0.15f);

            var command = executor.Resolve(SearchingSnapshot());

            Assert.IsFalse(command.IsIdle);
            Assert.AreEqual(0f, command.MoveDirection.x, "Search must not drift laterally.");
            Assert.Greater(command.MoveDirection.z, 0f, "Search must drive forward.");
            Assert.Greater(command.Yaw, 0f, "Search applies a smooth yaw sweep.");

            // Same snapshot -> identical command (no hidden state, no randomness).
            var again = executor.Resolve(SearchingSnapshot());
            Assert.AreEqual(command.MoveDirection, again.MoveDirection);
            Assert.AreEqual(command.Yaw, again.Yaw);
        }

        [Test]
        public void ApproachExecutor_SteersTowardVictim()
        {
            var executor = new ApproachExecutor(0.8f, 0.8f, 0.2f);

            var right = executor.Resolve(ApproachingSnapshot(side: 0.8f));
            var left = executor.Resolve(ApproachingSnapshot(side: -0.8f));

            Assert.IsFalse(right.IsIdle);
            Assert.Greater(right.MoveDirection.z, 0f, "Approach must drive forward.");
            Assert.Greater(right.MoveDirection.x, 0f, "Target on the right -> steer right.");
            Assert.Less(left.MoveDirection.x, 0f, "Target on the left -> steer left.");
            Assert.Greater(right.Yaw, 0f, "Approach yaw follows the target side smoothly.");
            Assert.Less(left.Yaw, 0f, "Approach yaw follows the target side smoothly.");
        }

        [Test]
        public void ApproachExecutor_ClampsSteering()
        {
            var executor = new ApproachExecutor(0.8f, 0.8f, 0.2f);

            var extreme = executor.Resolve(ApproachingSnapshot(side: 2f));

            Assert.LessOrEqual(extreme.MoveDirection.x, 1f, "Steering must stay within [-1, 1].");
            Assert.LessOrEqual(extreme.Yaw, 1f, "Yaw must stay within [-1, 1].");
        }

        [Test]
        public void AvoidExecutor_GeneratesAvoidanceCommand()
        {
            var executor = new AvoidExecutor(0.3f, 0.9f, 0.25f);

            var right = executor.Resolve(AvoidingSnapshot(side: 0.8f));
            var left = executor.Resolve(AvoidingSnapshot(side: -0.8f));

            Assert.IsFalse(right.IsIdle);
            Assert.Less(right.MoveDirection.z, 0f, "Avoid backs off from the obstacle.");
            Assert.Less(right.MoveDirection.x, 0f, "Obstacle on the right -> evade left.");
            Assert.Greater(left.MoveDirection.x, 0f, "Obstacle on the left -> evade right.");
            Assert.Less(right.Yaw, 0f, "Avoid turns away from the obstacle side.");
            Assert.Greater(left.Yaw, 0f, "Avoid turns away from the obstacle side.");
        }

        [Test]
        public void AvoidExecutor_DeterministicSide_NoOscillation()
        {
            var executor = new AvoidExecutor(0.3f, 0.9f, 0.25f);

            var first = executor.Resolve(AvoidingSnapshot(side: 0.8f));
            var second = executor.Resolve(AvoidingSnapshot(side: 0.8f));

            // Deterministic side: same obstacle side always yields the same
            // evasion direction, so there is no left/right oscillation.
            Assert.AreEqual(first.MoveDirection.x, second.MoveDirection.x);
            Assert.AreEqual(first.Yaw, second.Yaw);
        }

        [Test]
        public void Factory_ReturnsCorrectExecutor_ForEachBehaviour()
        {
            var factory = CreateFactory();

            Assert.IsInstanceOf<IdleExecutor>(factory.Get(BehaviourState.Idle));
            Assert.IsInstanceOf<SearchExecutor>(factory.Get(BehaviourState.Search));
            Assert.IsInstanceOf<ApproachExecutor>(factory.Get(BehaviourState.Approach));
            Assert.IsInstanceOf<AvoidExecutor>(factory.Get(BehaviourState.Avoid));
        }

        [Test]
        public void Factory_ExecutorsReportMatchingBehaviour()
        {
            var factory = CreateFactory();

            Assert.AreEqual(BehaviourState.Idle, factory.Get(BehaviourState.Idle).Behaviour);
            Assert.AreEqual(BehaviourState.Search, factory.Get(BehaviourState.Search).Behaviour);
            Assert.AreEqual(BehaviourState.Approach, factory.Get(BehaviourState.Approach).Behaviour);
            Assert.AreEqual(BehaviourState.Avoid, factory.Get(BehaviourState.Avoid).Behaviour);
        }

        [Test]
        public void Factory_IsSingleMapping_NoDuplicateCommandGeneration()
        {
            var factory = CreateFactory();

            // Each behaviour resolves to exactly one executor, and the executor
            // instances are stable across lookups (no per-call generation).
            Assert.AreSame(factory.Get(BehaviourState.Search), factory.Get(BehaviourState.Search));
            Assert.AreSame(factory.Get(BehaviourState.Approach), factory.Get(BehaviourState.Approach));
            Assert.AreSame(factory.Get(BehaviourState.Avoid), factory.Get(BehaviourState.Avoid));
            Assert.AreSame(factory.Get(BehaviourState.Idle), factory.Get(BehaviourState.Idle));

            // Distinct behaviours never share an executor.
            Assert.AreNotSame(factory.Get(BehaviourState.Search), factory.Get(BehaviourState.Approach));
            Assert.AreNotSame(factory.Get(BehaviourState.Approach), factory.Get(BehaviourState.Avoid));
        }

        [Test]
        public void SameSnapshot_ProducesIdenticalCommand_AcrossAllExecutors()
        {
            var executors = new IBehaviourExecutor[]
            {
                new IdleExecutor(),
                new SearchExecutor(0.6f, 0.15f),
                new ApproachExecutor(0.8f, 0.8f, 0.2f),
                new AvoidExecutor(0.3f, 0.9f, 0.25f),
            };

            foreach (var executor in executors)
            {
                var snapshots = new SituationSnapshot[]
                {
                    SearchingSnapshot(),
                    ApproachingSnapshot(),
                    AvoidingSnapshot(),
                };

                foreach (var snapshot in snapshots)
                {
                    var a = executor.Resolve(snapshot);
                    var b = executor.Resolve(snapshot);
                    Assert.AreEqual(a.MoveDirection, b.MoveDirection, executor.Behaviour.ToString());
                    Assert.AreEqual(a.Yaw, b.Yaw, executor.Behaviour.ToString());
                }
            }
        }

        [Test]
        public void DecisionEngine_DelegatesToExecutor_ForResolvedBehaviour()
        {
            // The engine must resolve the command through the factory's executor
            // for the selected behaviour rather than generating movement itself.
            var engine = new DecisionEngine(
                DefaultContext,
                new FogOfWarSituationAssessor(DefaultContext),
                new BehaviourSelector(DefaultContext));
            var factory = CreateFactory();

            // A live victim band ahead of the drone drives the Approach behaviour
            // (two active bands satisfy the minimum-active-ratio gate).
            var fused = new SensorReading(new[]
            {
                0f, 0f, 0f, 0f, 0f, 0f,
                0.7f, 1f, 0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f, 0.1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
            });

            var result = engine.Decide(fused);
            var profile = engine.LastSnapshot.ExecutionProfile;
            var fromFactory = factory.Get(result.Behaviour).Resolve(result.Assessment, profile);

            // The engine command is exactly what its behaviour executor produces for
            // the optimized execution profile: the factory is the single command
            // generator for a behaviour, and the Phase 8.9 optimizer only refines how
            // that executor moves - it never generates movement itself.
            Assert.AreEqual(BehaviourState.Approach, result.Behaviour);
            Assert.IsFalse(result.Command.IsIdle);
            Assert.AreEqual(fromFactory.MoveDirection, result.Command.MoveDirection);
            Assert.AreEqual(fromFactory.Yaw, result.Command.Yaw);
        }
    }
}
