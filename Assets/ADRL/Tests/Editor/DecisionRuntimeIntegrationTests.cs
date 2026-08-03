namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 8.3 runtime authority handover contract of the Decision
    /// Framework: the <see cref="DecisionEngine"/> produces exactly one command per
    /// fused reading, handles invalid readings deterministically, and its command
    /// output is the single authority the actuator layer consumes. These tests
    /// mirror the runtime call site in <c>DroneAgent.OnActionReceived</c> without
    /// requiring a play-mode session.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionRuntimeIntegrationTests
    {
        private static readonly DecisionContext DefaultContext = DecisionContext.Default;

        private static DecisionEngine CreateEngine()
        {
            return new DecisionEngine(
                DefaultContext,
                new FogOfWarSituationAssessor(DefaultContext),
                new BehaviourSelector(DefaultContext));
        }

        private static ISensorReading Reading(params float[] values) => new SensorReading(values);

        [Test]
        public void RuntimeFusedReading_ProducesSingleNonIdleCommand()
        {
            var engine = CreateEngine();

            // A live victim band ahead of the drone (proximity 0.7, liveness 1)
            // amid otherwise empty bands, as the runtime fused provider would emit.
            var result = engine.Decide(Reading(
                0f, 0f, 0f, 0f, 0f, 0f,
                0.7f, 1f, 0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f, 0.1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f));

            Assert.AreEqual(BehaviourState.Approach, result.Behaviour);
            Assert.IsFalse(result.Command.IsIdle);
            Assert.Greater(result.Command.MoveDirection.z, 0f, "Approach must drive forward.");
            Assert.AreEqual(1, engine.GetDiagnostics().StepCount);
        }

        [Test]
        public void RuntimeStep_CannotProduceTwoCommandsFromOneReading()
        {
            var engine = CreateEngine();
            var fused = Reading(
                0f, 0f, 0f, 0f, 0f, 0f, 0.7f, 1f, 0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

            var a = engine.Decide(fused);
            var b = engine.Decide(fused);

            Assert.AreEqual(2, engine.GetDiagnostics().StepCount);

            // Two invocations must resolve identically; there is exactly one command
            // authority and no second generator can produce a divergent command.
            Assert.AreEqual(a.Behaviour, b.Behaviour);
            Assert.AreEqual(a.Command.MoveDirection, b.Command.MoveDirection);
            Assert.AreEqual(a.Command.Yaw, b.Command.Yaw);
        }

        [Test]
        public void InvalidFusedReading_ProducesIdle_WithoutAdvancingMeaningfulState()
        {
            var engine = CreateEngine();
            var result = engine.Decide(Reading());

            Assert.AreEqual(BehaviourState.Idle, result.Behaviour);
            Assert.IsTrue(result.Command.IsIdle);
            Assert.AreEqual(DroneCommand.Idle, result.Command);
        }

        [Test]
        public void EngineCommand_IsDeterministic_AcrossIdenticalRuntimeInputs()
        {
            var a = CreateEngine();
            var b = CreateEngine();
            var fused = Reading(0.6f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                               0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.2f, 0f);

            var ra = a.Decide(fused);
            var rb = b.Decide(fused);

            Assert.AreEqual(ra.Behaviour, rb.Behaviour);
            Assert.AreEqual(ra.Command.MoveDirection, rb.Command.MoveDirection);
            Assert.AreEqual(ra.Command.Yaw, rb.Command.Yaw);
        }
    }
}