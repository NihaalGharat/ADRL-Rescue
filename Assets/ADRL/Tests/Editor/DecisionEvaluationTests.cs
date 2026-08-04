namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Analytics;
    using ADRL.AI.Decision.Evaluation;
    using ADRL.AI.Decision.Explainability;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.Decision.Telemetry;
    using ADRL.AI.Decision.Trace;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 9.5 Autonomous Decision Quality Evaluation Framework:
    /// the <see cref="DecisionEvaluationCalculator"/> is the single owner of
    /// quality-evaluation computation (a pure, stateless projection of the
    /// decision analytics, telemetry, trace and explanation into an immutable
    /// <see cref="DecisionEvaluationSnapshot"/>), the pure helpers in
    /// <see cref="DecisionEvaluationMetrics"/> own the component scores, grade and
    /// status, the <see cref="DecisionEvaluationFormatter"/> renders the snapshot
    /// deterministically, the <see cref="DecisionEvaluationValidator"/> confirms
    /// structural soundness, and the <c>DecisionEngine</c> computes, exposes and
    /// synchronizes the evaluation without any behavioural change. Evaluation is
    /// strictly observational.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionEvaluationTests
    {
        private static ISensorReading ObstacleReading(float proximity) => new SensorReading(new[]
        {
            0f, 0f, 0f, 0f, 0f, 0f, proximity, 0f, 0f, 0f, 0f, 0f,
        });

        private static ISensorReading NeutralReading() => new SensorReading(new[]
        {
            0.1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        });

        private static DecisionEngine CreateEngine()
        {
            return new DecisionEngine(
                DecisionContext.Default,
                new FogOfWarSituationAssessor(DecisionContext.Default),
                new BehaviourSelector(DecisionContext.Default));
        }

        /// <summary>
        /// Builds a telemetry snapshot with the given decision count and averages,
        /// defaulting to consistent distributions (all weight on Avoid / AvoidHazard
        /// so totals match the decision count).
        /// </summary>
        private static DecisionTelemetrySnapshot TelemetrySnapshot(
            int decisionCount,
            float averageDecisionConfidence = 0f,
            float averageOptimizationConfidence = 0f,
            float averageCandidateCount = 0f,
            float averageSpeedMultiplier = 0f,
            float averageTurnRateMultiplier = 0f,
            int knowledgeRecordCount = 0,
            int memoryRecordCount = 0,
            DecisionBehaviourCount[] behaviours = null,
            DecisionMissionCount[] missions = null)
        {
            return new DecisionTelemetrySnapshot(
                decisionCount,
                averageDecisionConfidence,
                averageOptimizationConfidence,
                averageCandidateCount,
                averageSpeedMultiplier,
                averageTurnRateMultiplier,
                knowledgeRecordCount,
                memoryRecordCount,
                BehaviourState.Idle,
                MissionTaskState.Idle,
                "AvoidExecutor",
                decisionCount,
                decisionCount,
                behaviours ?? new DecisionBehaviourCount[]
                {
                    new(BehaviourState.Idle, 0),
                    new(BehaviourState.Search, 0),
                    new(BehaviourState.Approach, 0),
                    new(BehaviourState.Avoid, decisionCount),
                },
                missions ?? new DecisionMissionCount[]
                {
                    new(MissionTaskState.Idle, 0),
                    new(MissionTaskState.SearchArea, 0),
                    new(MissionTaskState.InvestigateTarget, 0),
                    new(MissionTaskState.RescueVictim, 0),
                    new(MissionTaskState.AvoidHazard, decisionCount),
                    new(MissionTaskState.ResumeSearch, 0),
                });
        }

        /// <summary>
        /// Builds an evaluation snapshot with the given metrics, defaulting the
        /// grade to the canonical grade of the quality score and the status to
        /// Critical (callers that want a valid status pass it explicitly).
        /// </summary>
        private static DecisionEvaluationSnapshot EvaluationSnapshot(
            int decisionStep = 0,
            float decisionTimestamp = 0f,
            float quality = 0f,
            float behaviourSuitability = 0f,
            float confidenceQuality = 0f,
            float optimizationBenefit = 0f,
            float missionSuitability = 0f,
            float knowledgeCoverage = 0f,
            float consistency = 0f,
            string grade = null,
            DecisionEvaluationStatus status = DecisionEvaluationStatus.Critical)
        {
            return new DecisionEvaluationSnapshot(
                decisionStep,
                decisionTimestamp,
                quality,
                behaviourSuitability,
                confidenceQuality,
                optimizationBenefit,
                missionSuitability,
                knowledgeCoverage,
                consistency,
                grade ?? DecisionEvaluationMetrics.Grade(quality),
                status);
        }

        private static DecisionTraceFrame TraceFrame(int step, float timestamp)
        {
            return new DecisionTraceFrame(
                step,
                timestamp,
                SituationSnapshot.Invalid,
                MissionTask.Invalid,
                BehaviourState.Idle,
                BehaviourExecutionProfile.Empty,
                DroneCommand.Idle,
                DecisionExplanation.Empty,
                DecisionDiagnostics.Empty,
                TaskPriority.Invalid,
                0,
                0,
                0,
                string.Empty,
                0f,
                new DecisionReason[0]);
        }

        private static string Label(string name)
        {
            return name.PadRight(DecisionEvaluationFormatter.LabelWidth, '.');
        }

        private static void AssertEvaluationsEqual(DecisionEvaluationSnapshot a, DecisionEvaluationSnapshot b)
        {
            Assert.AreEqual(a.DecisionStep, b.DecisionStep);
            Assert.AreEqual(a.DecisionTimestamp, b.DecisionTimestamp, 1e-4f);
            Assert.AreEqual(a.DecisionQualityScore, b.DecisionQualityScore, 1e-4f);
            Assert.AreEqual(a.BehaviourSuitabilityScore, b.BehaviourSuitabilityScore, 1e-4f);
            Assert.AreEqual(a.ConfidenceQualityScore, b.ConfidenceQualityScore, 1e-4f);
            Assert.AreEqual(a.OptimizationBenefitScore, b.OptimizationBenefitScore, 1e-4f);
            Assert.AreEqual(a.MissionSuitabilityScore, b.MissionSuitabilityScore, 1e-4f);
            Assert.AreEqual(a.KnowledgeCoverageScore, b.KnowledgeCoverageScore, 1e-4f);
            Assert.AreEqual(a.ConsistencyScore, b.ConsistencyScore, 1e-4f);
            Assert.AreEqual(a.EvaluationGrade, b.EvaluationGrade);
            Assert.AreEqual(a.OverallStatus, b.OverallStatus);
        }

        /// <summary>
        /// Runs the full analytics + evaluation derivation for a deterministic
        /// scenario and returns the resulting evaluation snapshot.
        /// </summary>
        private static DecisionEvaluationSnapshot EvaluateScenario(
            int decisionCount,
            float decisionConfidence,
            float optimizationConfidence,
            int knowledge,
            int memory,
            DecisionBehaviourCount[] behaviours,
            DecisionMissionCount[] missions,
            DecisionTraceFrame trace)
        {
            var telemetry = TelemetrySnapshot(
                decisionCount,
                averageDecisionConfidence: decisionConfidence,
                averageOptimizationConfidence: optimizationConfidence,
                knowledgeRecordCount: knowledge,
                memoryRecordCount: memory,
                behaviours: behaviours,
                missions: missions);
            var analytics = DecisionAnalyticsCalculator.Calculate(telemetry);
            return DecisionEvaluationCalculator.Calculate(analytics, telemetry, trace, DecisionExplanation.Empty);
        }

        [Test]
        public void Calculator_ComputesQualityScore()
        {
            var evaluation = EvaluateScenario(
                4,
                0.9f,
                0.9f,
                2,
                4,
                null,
                null,
                TraceFrame(4, 4f));

            Assert.AreEqual(4, evaluation.DecisionStep);
            Assert.AreEqual(4f, evaluation.DecisionTimestamp, 1e-4f);
            Assert.AreEqual(100f, evaluation.BehaviourSuitabilityScore, 1e-4f);
            Assert.AreEqual(100f, evaluation.MissionSuitabilityScore, 1e-4f);
            Assert.AreEqual(90f, evaluation.ConfidenceQualityScore, 1e-4f);
            Assert.AreEqual(90f, evaluation.OptimizationBenefitScore, 1e-4f);
            Assert.AreEqual(50f, evaluation.KnowledgeCoverageScore, 1e-4f);
            Assert.AreEqual(100f, evaluation.ConsistencyScore, 1e-4f);
            Assert.AreEqual(90f, evaluation.DecisionQualityScore, 1e-3f);
            Assert.AreEqual("Excellent", evaluation.EvaluationGrade);
            Assert.AreEqual(DecisionEvaluationStatus.Optimal, evaluation.OverallStatus);
            Assert.IsTrue(evaluation.IsValid);
            Assert.IsTrue(DecisionEvaluationValidator.IsValid(evaluation));
        }

        [Test]
        public void Calculator_ProducesEmptyForNoDecisions()
        {
            var evaluation = DecisionEvaluationCalculator.Calculate(
                DecisionAnalyticsSnapshot.Empty,
                DecisionTelemetrySnapshot.Empty,
                null,
                DecisionExplanation.Empty);

            Assert.AreEqual(0, evaluation.DecisionStep);
            Assert.AreEqual(0f, evaluation.DecisionQualityScore, 1e-4f);
            Assert.AreEqual("Critical", evaluation.EvaluationGrade);
            Assert.AreEqual(DecisionEvaluationStatus.Critical, evaluation.OverallStatus);
            Assert.IsTrue(evaluation.IsValid);
            Assert.IsTrue(DecisionEvaluationValidator.IsValid(evaluation));
            AssertEvaluationsEqual(evaluation, DecisionEvaluationSnapshot.Empty);
        }

        [Test]
        public void Calculator_UniformDistributionReducesConsistency()
        {
            var evaluation = EvaluateScenario(
                4,
                0.9f,
                0.9f,
                2,
                4,
                new DecisionBehaviourCount[]
                {
                    new(BehaviourState.Idle, 1),
                    new(BehaviourState.Search, 1),
                    new(BehaviourState.Approach, 1),
                    new(BehaviourState.Avoid, 1),
                },
                null,
                TraceFrame(4, 4f));

            Assert.AreEqual(0f, evaluation.ConsistencyScore, 1e-4f);
            Assert.AreEqual(25f, evaluation.BehaviourSuitabilityScore, 1e-4f);
            Assert.AreEqual(100f, evaluation.MissionSuitabilityScore, 1e-4f);
            Assert.AreEqual(56.25f, evaluation.DecisionQualityScore, 1e-3f);
            Assert.AreEqual("Poor", evaluation.EvaluationGrade);
            Assert.AreEqual(DecisionEvaluationStatus.Deficient, evaluation.OverallStatus);
        }

        [Test]
        public void Calculator_UsesTraceStepAndTimestamp()
        {
            var withTrace = EvaluateScenario(
                4,
                0.9f,
                0.9f,
                2,
                4,
                null,
                null,
                TraceFrame(7, 3.5f));
            Assert.AreEqual(7, withTrace.DecisionStep);
            Assert.AreEqual(3.5f, withTrace.DecisionTimestamp, 1e-4f);

            var withoutTrace = EvaluateScenario(
                4,
                0.9f,
                0.9f,
                2,
                4,
                null,
                null,
                null);
            Assert.AreEqual(4, withoutTrace.DecisionStep);
            Assert.AreEqual(4f, withoutTrace.DecisionTimestamp, 1e-4f);
        }

        [Test]
        public void Calculator_DeterministicOutput()
        {
            var first = EvaluateScenario(3, 0.9f, 0.85f, 2, 1, null, null, TraceFrame(3, 3f));
            var second = EvaluateScenario(3, 0.9f, 0.85f, 2, 1, null, null, TraceFrame(3, 3f));

            AssertEvaluationsEqual(first, second);
            Assert.AreEqual(
                DecisionEvaluationFormatter.Format(first),
                DecisionEvaluationFormatter.Format(second));
        }

        [Test]
        public void Formatter_DeterministicOutput()
        {
            var evaluation = EvaluateScenario(2, 0.9f, 0.9f, 1, 2, null, null, TraceFrame(2, 2f));

            var first = DecisionEvaluationFormatter.Format(evaluation);
            var second = DecisionEvaluationFormatter.Format(evaluation);

            Assert.AreEqual(first, second);
            Assert.IsTrue(first.Length > 0);
        }

        [Test]
        public void Formatter_ContainsSections()
        {
            var evaluation = EvaluateScenario(4, 0.9f, 0.9f, 2, 4, null, null, TraceFrame(4, 4f));
            var output = DecisionEvaluationFormatter.Format(evaluation);

            Assert.IsTrue(output.Contains("Decision Quality"));
            Assert.IsTrue(output.Contains(Label("Overall Score") + "90.0"));
            Assert.IsTrue(output.Contains(Label("Grade") + "Excellent"));
            Assert.IsTrue(output.Contains(Label("Behaviour Suitability") + "100"));
            Assert.IsTrue(output.Contains(Label("Mission Suitability") + "100"));
            Assert.IsTrue(output.Contains(Label("Optimization") + "90"));
            Assert.IsTrue(output.Contains(Label("Consistency") + "100"));
            Assert.IsTrue(output.Contains(Label("Knowledge") + "50"));
            Assert.IsTrue(output.Contains(Label("Confidence") + "90"));
        }

        [Test]
        public void Formatter_EmptyOutput()
        {
            var output = DecisionEvaluationFormatter.Format(DecisionEvaluationSnapshot.Empty);

            Assert.AreEqual(DecisionEvaluationFormatter.Format(DecisionEvaluationSnapshot.Empty), output);
            Assert.IsTrue(output.Contains("Decision Quality"));
            Assert.IsTrue(output.Contains(Label("Overall Score") + "0.0"));
            Assert.IsTrue(output.Contains(Label("Grade") + "Critical"));
        }

        [Test]
        public void Validator_AcceptsValidSnapshot()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var evaluation = engine.LastEvaluation;

            Assert.IsTrue(evaluation.IsValid);
            Assert.IsTrue(DecisionEvaluationValidator.NoNegatives(evaluation));
            Assert.IsTrue(DecisionEvaluationValidator.NoNaN(evaluation));
            Assert.IsTrue(DecisionEvaluationValidator.NoInfinity(evaluation));
            Assert.IsTrue(DecisionEvaluationValidator.ScoresInRange(evaluation));
            Assert.IsTrue(DecisionEvaluationValidator.GradeValid(evaluation));
            Assert.IsTrue(DecisionEvaluationValidator.StatusValid(evaluation));
            Assert.IsTrue(DecisionEvaluationValidator.SnapshotComplete(evaluation));
            Assert.IsTrue(DecisionEvaluationValidator.IsValid(evaluation));
            Assert.IsTrue(DecisionEvaluationValidator.IsValid(DecisionEvaluationSnapshot.Empty));
        }

        [Test]
        public void Validator_RejectsNegative()
        {
            var negativeScore = EvaluationSnapshot(
                quality: 90f,
                behaviourSuitability: -5f,
                status: DecisionEvaluationStatus.Optimal);
            Assert.IsFalse(DecisionEvaluationValidator.NoNegatives(negativeScore));
            Assert.IsFalse(DecisionEvaluationValidator.ScoresInRange(negativeScore));
            Assert.IsFalse(DecisionEvaluationValidator.IsValid(negativeScore));

            var negativeStep = EvaluationSnapshot(decisionStep: -1);
            Assert.IsFalse(DecisionEvaluationValidator.NoNegatives(negativeStep));
            Assert.IsFalse(DecisionEvaluationValidator.SnapshotComplete(negativeStep));
            Assert.IsFalse(DecisionEvaluationValidator.IsValid(negativeStep));
        }

        [Test]
        public void Validator_RejectsNaNOrInfinity()
        {
            var nan = EvaluationSnapshot(quality: float.NaN);
            Assert.IsFalse(DecisionEvaluationValidator.NoNaN(nan));
            Assert.IsFalse(DecisionEvaluationValidator.ScoresInRange(nan));
            Assert.IsFalse(DecisionEvaluationValidator.IsValid(nan));

            var infinity = EvaluationSnapshot(knowledgeCoverage: float.PositiveInfinity);
            Assert.IsFalse(DecisionEvaluationValidator.NoInfinity(infinity));
            Assert.IsFalse(DecisionEvaluationValidator.ScoresInRange(infinity));
            Assert.IsFalse(DecisionEvaluationValidator.IsValid(infinity));
        }

        [Test]
        public void Validator_RejectsInvalidGradeOrStatus()
        {
            var wrongGrade = EvaluationSnapshot(
                quality: 95f,
                grade: "Good",
                status: DecisionEvaluationStatus.Optimal);
            Assert.IsFalse(DecisionEvaluationValidator.GradeValid(wrongGrade));
            Assert.IsFalse(DecisionEvaluationValidator.IsValid(wrongGrade));

            var wrongStatus = EvaluationSnapshot(
                quality: 95f,
                status: DecisionEvaluationStatus.Good);
            Assert.IsFalse(DecisionEvaluationValidator.StatusValid(wrongStatus));
            Assert.IsFalse(DecisionEvaluationValidator.IsValid(wrongStatus));

            var missingGrade = new DecisionEvaluationSnapshot(
                0,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                null,
                DecisionEvaluationStatus.Critical);
            Assert.IsFalse(DecisionEvaluationValidator.SnapshotComplete(missingGrade));
            Assert.IsFalse(DecisionEvaluationValidator.IsValid(missingGrade));
        }

        [Test]
        public void Metrics_ComputeSuitabilityAndCoverage()
        {
            Assert.AreEqual(100f, DecisionEvaluationMetrics.BehaviourSuitability(100f), 1e-4f);
            Assert.AreEqual(25.5f, DecisionEvaluationMetrics.BehaviourSuitability(25.5f), 1e-4f);
            Assert.AreEqual(0f, DecisionEvaluationMetrics.BehaviourSuitability(-10f), 1e-4f);
            Assert.AreEqual(100f, DecisionEvaluationMetrics.BehaviourSuitability(150f), 1e-4f);
            Assert.AreEqual(80f, DecisionEvaluationMetrics.MissionSuitability(80f), 1e-4f);
            Assert.AreEqual(50f, DecisionEvaluationMetrics.KnowledgeCoverage(50f), 1e-4f);
            Assert.AreEqual(100f, DecisionEvaluationMetrics.KnowledgeCoverage(150f), 1e-4f);
            Assert.AreEqual(0f, DecisionEvaluationMetrics.KnowledgeCoverage(-5f), 1e-4f);
            Assert.AreEqual(0f, DecisionEvaluationMetrics.KnowledgeCoverage(float.NaN), 1e-4f);
        }

        [Test]
        public void Metrics_ComputeConfidenceAndOptimization()
        {
            Assert.AreEqual(90f, DecisionEvaluationMetrics.ConfidenceQuality(0.9f), 1e-4f);
            Assert.AreEqual(100f, DecisionEvaluationMetrics.ConfidenceQuality(1f), 1e-4f);
            Assert.AreEqual(50f, DecisionEvaluationMetrics.ConfidenceQuality(0.5f), 1e-4f);
            Assert.AreEqual(100f, DecisionEvaluationMetrics.ConfidenceQuality(2f), 1e-4f);
            Assert.AreEqual(0f, DecisionEvaluationMetrics.ConfidenceQuality(-0.5f), 1e-4f);
            Assert.AreEqual(90f, DecisionEvaluationMetrics.OptimizationBenefit(0.9f), 1e-4f);
            Assert.AreEqual(75f, DecisionEvaluationMetrics.OptimizationBenefit(0.75f), 1e-4f);
            Assert.AreEqual(0f, DecisionEvaluationMetrics.OptimizationBenefit(float.NaN), 1e-4f);
        }

        [Test]
        public void Metrics_ComputeConsistencyAndGrade()
        {
            Assert.AreEqual(0f, DecisionEvaluationMetrics.Consistency(100f), 1e-4f);
            Assert.AreEqual(100f, DecisionEvaluationMetrics.Consistency(0f), 1e-4f);
            Assert.AreEqual(74.8f, DecisionEvaluationMetrics.Consistency(25.2f), 1e-3f);

            Assert.AreEqual("Excellent", DecisionEvaluationMetrics.Grade(95f));
            Assert.AreEqual("Excellent", DecisionEvaluationMetrics.Grade(90f));
            Assert.AreEqual("Good", DecisionEvaluationMetrics.Grade(89f));
            Assert.AreEqual("Good", DecisionEvaluationMetrics.Grade(75f));
            Assert.AreEqual("Fair", DecisionEvaluationMetrics.Grade(74f));
            Assert.AreEqual("Fair", DecisionEvaluationMetrics.Grade(60f));
            Assert.AreEqual("Poor", DecisionEvaluationMetrics.Grade(59f));
            Assert.AreEqual("Poor", DecisionEvaluationMetrics.Grade(40f));
            Assert.AreEqual("Critical", DecisionEvaluationMetrics.Grade(39f));
            Assert.AreEqual("Critical", DecisionEvaluationMetrics.Grade(0f));
        }

        [Test]
        public void Metrics_OverallQualityWeighted()
        {
            Assert.AreEqual(100f, DecisionEvaluationMetrics.OverallQuality(100f, 100f, 100f, 100f, 100f, 100f), 1e-4f);
            Assert.AreEqual(0f, DecisionEvaluationMetrics.OverallQuality(0f, 0f, 0f, 0f, 0f, 0f), 1e-4f);
            Assert.AreEqual(25f, DecisionEvaluationMetrics.OverallQuality(100f, 0f, 0f, 0f, 0f, 0f), 1e-4f);
            Assert.AreEqual(20f, DecisionEvaluationMetrics.OverallQuality(0f, 100f, 0f, 0f, 0f, 0f), 1e-4f);
            Assert.AreEqual(15f, DecisionEvaluationMetrics.OverallQuality(0f, 0f, 100f, 0f, 0f, 0f), 1e-4f);
            Assert.AreEqual(10f, DecisionEvaluationMetrics.OverallQuality(0f, 0f, 0f, 100f, 0f, 0f), 1e-4f);
            Assert.AreEqual(15f, DecisionEvaluationMetrics.OverallQuality(0f, 0f, 0f, 0f, 100f, 0f), 1e-4f);
            Assert.AreEqual(15f, DecisionEvaluationMetrics.OverallQuality(0f, 0f, 0f, 0f, 0f, 100f), 1e-4f);
            Assert.AreEqual(100f, DecisionEvaluationMetrics.OverallQuality(200f, 200f, 200f, 200f, 200f, 200f), 1e-4f);
            Assert.AreEqual(0f, DecisionEvaluationMetrics.OverallQuality(-50f, -50f, -50f, -50f, -50f, -50f), 1e-4f);

            var totalWeight = DecisionEvaluationMetrics.BehaviourSuitabilityWeight
                + DecisionEvaluationMetrics.MissionSuitabilityWeight
                + DecisionEvaluationMetrics.ConfidenceQualityWeight
                + DecisionEvaluationMetrics.OptimizationBenefitWeight
                + DecisionEvaluationMetrics.KnowledgeCoverageWeight
                + DecisionEvaluationMetrics.ConsistencyWeight;
            Assert.AreEqual(1f, totalWeight, 1e-4f);

            Assert.AreEqual(DecisionEvaluationStatus.Optimal, DecisionEvaluationMetrics.Status(95f));
            Assert.AreEqual(DecisionEvaluationStatus.Good, DecisionEvaluationMetrics.Status(75f));
            Assert.AreEqual(DecisionEvaluationStatus.Acceptable, DecisionEvaluationMetrics.Status(60f));
            Assert.AreEqual(DecisionEvaluationStatus.Deficient, DecisionEvaluationMetrics.Status(40f));
            Assert.AreEqual(DecisionEvaluationStatus.Critical, DecisionEvaluationMetrics.Status(10f));
        }

        [Test]
        public void Integration_EngineEvaluationAfterSteps()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(NeutralReading());
            engine.Decide(ObstacleReading(0.9f));

            var evaluation = engine.LastEvaluation;

            Assert.AreEqual(3, evaluation.DecisionStep);
            Assert.GreaterOrEqual(evaluation.DecisionQualityScore, 0f);
            Assert.LessOrEqual(evaluation.DecisionQualityScore, 100f);
            Assert.IsFalse(string.IsNullOrEmpty(evaluation.EvaluationGrade));
            Assert.Greater(evaluation.DecisionQualityScore, 0f);
            Assert.IsTrue(DecisionEvaluationValidator.IsValid(evaluation));
            AssertEvaluationsEqual(evaluation, engine.GetEvaluation());
        }

        [Test]
        public void Integration_DeterministicEvaluation()
        {
            var first = CreateEngine();
            first.Decide(ObstacleReading(0.9f));
            first.Decide(NeutralReading());

            var second = CreateEngine();
            second.Decide(ObstacleReading(0.9f));
            second.Decide(NeutralReading());

            AssertEvaluationsEqual(first.LastEvaluation, second.LastEvaluation);
            Assert.AreEqual(
                DecisionEvaluationFormatter.Format(first.LastEvaluation),
                DecisionEvaluationFormatter.Format(second.LastEvaluation));
        }

        [Test]
        public void Integration_DiagnosticsCarriesEvaluation()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(ObstacleReading(0.9f));

            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual(engine.LastEvaluation.DecisionStep, diagnostics.LastEvaluation.DecisionStep);
            Assert.AreEqual(
                engine.LastEvaluation.DecisionStep,
                engine.LastSnapshot.Diagnostics.LastEvaluation.DecisionStep);
            Assert.AreEqual(
                engine.LastEvaluation.DecisionQualityScore,
                diagnostics.LastEvaluation.DecisionQualityScore,
                1e-4f);
            Assert.AreEqual(0, engine.LastTraceFrame.Diagnostics.LastEvaluation.DecisionStep);
        }

        [Test]
        public void Integration_ResetClearsEvaluationAndBehaviourUnchanged()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            var commandBefore = engine.LastSnapshot.Command;
            engine.Decide(NeutralReading());

            Assert.AreEqual(2, engine.LastEvaluation.DecisionStep);

            engine.Reset();

            AssertEvaluationsEqual(engine.LastEvaluation, DecisionEvaluationSnapshot.Empty);
            AssertEvaluationsEqual(engine.GetEvaluation(), DecisionEvaluationSnapshot.Empty);
            AssertEvaluationsEqual(engine.GetDiagnostics().LastEvaluation, DecisionEvaluationSnapshot.Empty);
            Assert.IsTrue(DecisionEvaluationValidator.IsValid(engine.LastEvaluation));

            engine.Decide(ObstacleReading(0.9f));
            Assert.AreEqual(BehaviourState.Avoid, engine.LastSnapshot.Behaviour);
            Assert.AreEqual(commandBefore, engine.LastSnapshot.Command);
            Assert.AreEqual(1, engine.LastEvaluation.DecisionStep);
        }
    }
}
