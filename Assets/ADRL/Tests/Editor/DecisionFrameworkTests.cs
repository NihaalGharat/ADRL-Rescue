namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 8.2 Autonomous Decision Framework: situation
    /// assessment, behaviour selection, and deterministic command resolution are
    /// all decoupled from sensors/actuators and hold their single-ownership and
    /// determinism contracts.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionFrameworkTests
    {
        private static readonly DecisionContext DefaultContext = DecisionContext.Default;

        private static ISensorReading Reading(params float[] values) => new SensorReading(values);

        [Test]
        public void InvalidReading_YieldsInvalidAssessment()
        {
            var assessor = new FogOfWarSituationAssessor(DefaultContext);
            var assessment = assessor.Assess(Reading());

            Assert.IsFalse(assessment.IsValid);
            Assert.IsFalse(assessment.TargetDetected);
        }

        [Test]
        public void Assessor_DetectsTargetFromLiveBand()
        {
            var assessor = new FogOfWarSituationAssessor(DefaultContext);
            // Six bands: [proximity, liveness] per pair.
            var reading = Reading(
                0f, 0f,
                0f, 0f,
                0.5f, 1f,
                0f, 0f,
                0f, 0f,
                0f, 0f);

            var assessment = assessor.Assess(reading);

            Assert.IsTrue(assessment.IsValid);
            Assert.IsTrue(assessment.TargetDetected);
            Assert.Greater(assessment.TargetProximity, 0f);
        }

        [Test]
        public void Assessor_ComputesTargetSide_FromBandPosition()
        {
            var assessor = new FogOfWarSituationAssessor(DefaultContext);
            // Far-left band (index 0) carries the target.
            var assessment = assessor.Assess(Reading(
                0.8f, 1f,
                0f, 0f,
                0f, 0f,
                0f, 0f,
                0f, 0f,
                0f, 0f));

            Assert.IsTrue(assessment.TargetDetected);
            Assert.AreEqual(-1f, assessment.TargetSide, 0.001f);
        }

        [Test]
        public void Selector_Idles_WhenAssessmentInvalid()
        {
            var selector = new BehaviourSelector(DefaultContext);
            Assert.AreEqual(BehaviourState.Idle, selector.Select(SituationSnapshot.Invalid));
        }

        [Test]
        public void Selector_AvoidsBeforeApproachingTarget()
        {
            var selector = new BehaviourSelector(DefaultContext);
            var snapshot = new SituationSnapshot(true, 0.5f, 0f, 0.9f, 0f, true);

            Assert.AreEqual(BehaviourState.Avoid, selector.Select(snapshot));
        }

        [Test]
        public void Selector_Approaches_WhenTargetDetectedAndNoImminentObstacle()
        {
            var selector = new BehaviourSelector(DefaultContext);
            var snapshot = new SituationSnapshot(true, 0.6f, 0f, 0.1f, 0f, true);

            Assert.AreEqual(BehaviourState.Approach, selector.Select(snapshot));
        }

        [Test]
        public void Selector_Searches_WhenNoTargetAndNoObstacle()
        {
            var selector = new BehaviourSelector(DefaultContext);
            var snapshot = new SituationSnapshot(false, 0f, 0f, 0f, 0f, true);

            Assert.AreEqual(BehaviourState.Search, selector.Select(snapshot));
        }

        [Test]
        public void Engine_ResolvesAndTracksStepState()
        {
            var engine = new DecisionEngine(
                DefaultContext,
                new FogOfWarSituationAssessor(DefaultContext),
                new BehaviourSelector(DefaultContext));

            Assert.AreEqual(0, engine.GetDiagnostics().StepCount);

            var result = engine.Decide(Reading(0.7f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f));

            Assert.AreEqual(BehaviourState.Approach, result.Behaviour);
            Assert.IsFalse(result.Command.IsIdle);
            Assert.AreEqual(1, engine.GetDiagnostics().StepCount);
            Assert.AreEqual(BehaviourState.Approach, engine.GetDiagnostics().LastBehaviour);
        }

        [Test]
        public void Engine_ProducesIdleCommand_ForInvalidReading()
        {
            var engine = new DecisionEngine(
                DefaultContext,
                new FogOfWarSituationAssessor(DefaultContext),
                new BehaviourSelector(DefaultContext));

            var result = engine.Decide(Reading());

            Assert.AreEqual(BehaviourState.Idle, result.Behaviour);
            Assert.IsTrue(result.Command.IsIdle);
            Assert.AreEqual(DroneCommand.Idle, result.Command);
        }

        [Test]
        public void Engine_Deterministic_ForSameReading()
        {
            var a = new DecisionEngine(DefaultContext, new FogOfWarSituationAssessor(DefaultContext), new BehaviourSelector(DefaultContext));
            var b = new DecisionEngine(DefaultContext, new FogOfWarSituationAssessor(DefaultContext), new BehaviourSelector(DefaultContext));

            var fused = Reading(0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.3f, 0f);

            var ra = a.Decide(fused);
            var rb = b.Decide(fused);

            Assert.AreEqual(ra.Behaviour, rb.Behaviour);
            Assert.AreEqual(ra.Command.MoveDirection, rb.Command.MoveDirection);
            Assert.AreEqual(ra.Command.Yaw, rb.Command.Yaw);
        }

        [Test]
        public void Engine_Reset_RestoresFreshState()
        {
            var engine = new DecisionEngine(
                DefaultContext,
                new FogOfWarSituationAssessor(DefaultContext),
                new BehaviourSelector(DefaultContext));

            engine.Decide(Reading(0.7f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f));
            Assert.AreEqual(1, engine.GetDiagnostics().StepCount);

            engine.Reset();

            Assert.AreEqual(0, engine.GetDiagnostics().StepCount);
            Assert.AreEqual(BehaviourState.Idle, engine.GetDiagnostics().LastBehaviour);
        }
    }
}