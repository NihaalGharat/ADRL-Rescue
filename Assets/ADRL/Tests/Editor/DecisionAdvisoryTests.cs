namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Advisory;
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
    /// Verifies the Phase 10.0 Autonomous Decision Advisory Framework: the
    /// <see cref="DecisionAdvisoryCalculator"/> is the single owner of advisory
    /// computation (a pure, stateless projection of the decision evaluation,
    /// analytics, telemetry, trace and explanation into an immutable
    /// <see cref="DecisionAdvisorySnapshot"/>), the
    /// <see cref="DecisionAdvisoryFormatter"/> renders the snapshot
    /// deterministically, the <see cref="DecisionAdvisoryValidator"/> confirms
    /// structural soundness, and the <c>DecisionEngine</c> computes, exposes and
    /// synchronizes the advisory without any behavioural change. Advisory is
    /// strictly observational.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionAdvisoryTests
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
        /// Builds a telemetry snapshot with the given decision count, averages,
        /// speed multiplier, current behaviour and distributions, defaulting to
        /// consistent all-Avoid / all-AvoidHazard distributions so totals match
        /// the decision count.
        /// </summary>
        private static DecisionTelemetrySnapshot TelemetrySnapshot(
            int decisionCount,
            float averageDecisionConfidence = 0f,
            float averageOptimizationConfidence = 0f,
            float averageSpeedMultiplier = 0f,
            int knowledgeRecordCount = 0,
            int memoryRecordCount = 0,
            BehaviourState currentBehaviour = BehaviourState.Idle,
            DecisionBehaviourCount[] behaviours = null,
            DecisionMissionCount[] missions = null)
        {
            return new DecisionTelemetrySnapshot(
                decisionCount,
                averageDecisionConfidence,
                averageOptimizationConfidence,
                0f,
                averageSpeedMultiplier,
                0f,
                knowledgeRecordCount,
                memoryRecordCount,
                currentBehaviour,
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
        /// Builds an analytics snapshot with the given decision count and balance
        /// scores, zeroing every other field so only the advisory inputs it reads
        /// are controlled by the caller.
        /// </summary>
        private static DecisionAnalyticsSnapshot AnalyticsSnapshot(
            int decisionCount,
            float behaviourBalance = 0f,
            float missionBalance = 0f)
        {
            return new DecisionAnalyticsSnapshot(
                decisionCount,
                0f,
                0f,
                0f,
                0f,
                0f,
                behaviourBalance,
                missionBalance,
                0f,
                0f,
                0f,
                decisionCount,
                decisionCount,
                new BehaviourAnalytics[0],
                new MissionAnalytics[0]);
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

        /// <summary>
        /// Builds an explanation carrying a known hazard at the given distance.
        /// </summary>
        private static DecisionExplanation ExplanationWithHazard(float nearestHazardDistance)
        {
            return new DecisionExplanation(
                SituationSnapshot.Invalid,
                new DecisionExplanation.KnowledgeSummary(1, 0, 1, 0, 0f, nearestHazardDistance),
                DecisionExplanation.MemorySummary.Empty,
                MissionTask.Invalid,
                new MissionTaskState[0],
                new TaskPriority[0],
                TaskPriority.Invalid,
                BehaviourState.Idle,
                string.Empty,
                BehaviourExecutionProfile.Empty,
                DroneCommand.Idle,
                0f,
                0,
                new DecisionReason[0]);
        }

        private static DecisionRecommendation Recommendation(
            DecisionRecommendationType type,
            DecisionRecommendationPriority priority = DecisionRecommendationPriority.Low,
            DecisionRecommendationSeverity severity = DecisionRecommendationSeverity.Low,
            float confidence = 50f)
        {
            return new DecisionRecommendation(
                type,
                priority,
                severity,
                "reason",
                "action",
                confidence,
                1,
                1f);
        }

        private static DecisionAdvisorySnapshot AdvisorySnapshot(
            int step,
            DecisionRecommendationType overall,
            DecisionRecommendation[] recommendations,
            float confidence,
            bool requiresAttention,
            string status)
        {
            return new DecisionAdvisorySnapshot(
                step,
                step,
                overall,
                recommendations,
                confidence,
                requiresAttention,
                status);
        }

        private static string Label(string name)
        {
            return name.PadRight(DecisionAdvisoryFormatter.LabelWidth, '.');
        }

        private static void AssertAdvisoriesEqual(DecisionAdvisorySnapshot a, DecisionAdvisorySnapshot b)
        {
            Assert.AreEqual(a.DecisionStep, b.DecisionStep);
            Assert.AreEqual(a.DecisionTimestamp, b.DecisionTimestamp, 1e-4f);
            Assert.AreEqual(a.OverallRecommendation, b.OverallRecommendation);
            Assert.AreEqual(a.AdvisoryConfidence, b.AdvisoryConfidence, 1e-4f);
            Assert.AreEqual(a.RequiresAttention, b.RequiresAttention);
            Assert.AreEqual(a.Status, b.Status);
            Assert.AreEqual(a.Recommendations.Length, b.Recommendations.Length);
            for (var i = 0; i < a.Recommendations.Length; i++)
            {
                Assert.AreEqual(a.Recommendations[i].Type, b.Recommendations[i].Type);
                Assert.AreEqual(a.Recommendations[i].Priority, b.Recommendations[i].Priority);
                Assert.AreEqual(a.Recommendations[i].Severity, b.Recommendations[i].Severity);
                Assert.AreEqual(a.Recommendations[i].Reason, b.Recommendations[i].Reason);
                Assert.AreEqual(a.Recommendations[i].SuggestedAction, b.Recommendations[i].SuggestedAction);
                Assert.AreEqual(a.Recommendations[i].Confidence, b.Recommendations[i].Confidence, 1e-4f);
                Assert.AreEqual(a.Recommendations[i].DecisionStep, b.Recommendations[i].DecisionStep);
                Assert.AreEqual(a.Recommendations[i].Timestamp, b.Recommendations[i].Timestamp, 1e-4f);
            }
        }

        /// <summary>
        /// Runs the full analytics + evaluation + advisory derivation for a
        /// deterministic scenario and returns the resulting advisory snapshot.
        /// </summary>
        private static DecisionAdvisorySnapshot AdvisoryScenario(
            int decisionCount,
            float decisionConfidence,
            float optimizationConfidence,
            int knowledge,
            int memory,
            DecisionBehaviourCount[] behaviours = null,
            DecisionMissionCount[] missions = null,
            DecisionTraceFrame trace = null,
            float averageSpeedMultiplier = 0f)
        {
            var telemetry = TelemetrySnapshot(
                decisionCount,
                averageDecisionConfidence: decisionConfidence,
                averageOptimizationConfidence: optimizationConfidence,
                averageSpeedMultiplier: averageSpeedMultiplier,
                knowledgeRecordCount: knowledge,
                memoryRecordCount: memory,
                behaviours: behaviours,
                missions: missions);
            var analytics = DecisionAnalyticsCalculator.Calculate(telemetry);
            var evaluation = DecisionEvaluationCalculator.Calculate(analytics, telemetry, trace, DecisionExplanation.Empty);
            return DecisionAdvisoryCalculator.Calculate(evaluation, analytics, telemetry, trace, DecisionExplanation.Empty);
        }

        [Test]
        public void Calculator_ProducesEmptyForNoDecisions()
        {
            var advisory = DecisionAdvisoryCalculator.Calculate(
                DecisionEvaluationSnapshot.Empty,
                DecisionAnalyticsSnapshot.Empty,
                DecisionTelemetrySnapshot.Empty,
                null,
                DecisionExplanation.Empty);

            AssertAdvisoriesEqual(advisory, DecisionAdvisorySnapshot.Empty);
            Assert.IsTrue(advisory.IsValid);
            Assert.IsTrue(DecisionAdvisoryValidator.IsValid(advisory));

            var withEvaluationButNoAnalytics = DecisionAdvisoryCalculator.Calculate(
                EvaluationSnapshot(decisionStep: 3, decisionTimestamp: 3f),
                DecisionAnalyticsSnapshot.Empty,
                DecisionTelemetrySnapshot.Empty,
                null,
                DecisionExplanation.Empty);

            AssertAdvisoriesEqual(withEvaluationButNoAnalytics, DecisionAdvisorySnapshot.Empty);
        }

        [Test]
        public void Calculator_UsesTraceStepAndTimestamp()
        {
            var withTrace = AdvisoryScenario(
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
            Assert.AreEqual(7, withTrace.Recommendations[0].DecisionStep);
            Assert.AreEqual(3.5f, withTrace.Recommendations[0].Timestamp, 1e-4f);

            var withoutTrace = AdvisoryScenario(4, 0.9f, 0.9f, 2, 4, null, null, null);
            Assert.AreEqual(4, withoutTrace.DecisionStep);
            Assert.AreEqual(4f, withoutTrace.DecisionTimestamp, 1e-4f);
        }

        [Test]
        public void Calculator_DeterministicOutput()
        {
            var first = AdvisoryScenario(3, 0.9f, 0.85f, 1, 1, null, null, TraceFrame(3, 3f));
            var second = AdvisoryScenario(3, 0.9f, 0.85f, 1, 1, null, null, TraceFrame(3, 3f));

            AssertAdvisoriesEqual(first, second);
            Assert.AreEqual(
                DecisionAdvisoryFormatter.Format(first),
                DecisionAdvisoryFormatter.Format(second));
        }

        [Test]
        public void Calculator_ExcellentQualityMaintainsStrategy()
        {
            var advisory = AdvisoryScenario(4, 0.9f, 0.9f, 2, 4, null, null, TraceFrame(4, 4f));

            Assert.AreEqual(DecisionRecommendationType.MaintainCurrentStrategy, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationType.MaintainCurrentStrategy, advisory.Recommendations[0].Type);
            Assert.AreEqual(DecisionRecommendationPriority.None, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.None, advisory.Recommendations[0].Severity);
            Assert.AreEqual(90f, advisory.AdvisoryConfidence, 1e-4f);
            Assert.IsFalse(advisory.RequiresAttention);
            Assert.AreEqual(DecisionAdvisorySnapshot.NominalStatus, advisory.Status);
        }

        [Test]
        public void Calculator_CriticalKnowledgeIncreasesCoverage()
        {
            var advisory = AdvisoryScenario(4, 0.9f, 0.9f, 1, 4, null, null, TraceFrame(4, 4f));

            Assert.AreEqual(DecisionRecommendationType.IncreaseKnowledgeCoverage, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationPriority.High, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.Critical, advisory.Recommendations[0].Severity);
            Assert.AreEqual(75f, advisory.AdvisoryConfidence, 1e-4f);
            Assert.IsTrue(advisory.RequiresAttention);
            Assert.AreEqual(DecisionAdvisorySnapshot.CriticalStatus, advisory.Status);
        }

        [Test]
        public void Calculator_LowKnowledgeReviewsCoverage()
        {
            var advisory = AdvisoryScenario(5, 0.9f, 0.9f, 2, 5, null, null, TraceFrame(5, 5f));

            Assert.AreEqual(DecisionRecommendationType.ReviewKnowledgeCoverage, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationPriority.Medium, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.Medium, advisory.Recommendations[0].Severity);
            Assert.AreEqual(60f, advisory.AdvisoryConfidence, 1e-4f);
            Assert.IsTrue(advisory.RequiresAttention);
            Assert.AreEqual(DecisionAdvisorySnapshot.AttentionStatus, advisory.Status);
        }

        [Test]
        public void Calculator_LowMissionSuitabilityReviewsAllocation()
        {
            var evaluation = EvaluationSnapshot(
                decisionStep: 3,
                decisionTimestamp: 3f,
                quality: 60f,
                behaviourSuitability: 80f,
                confidenceQuality: 80f,
                optimizationBenefit: 80f,
                missionSuitability: 40f,
                knowledgeCoverage: 80f,
                consistency: 80f,
                grade: "Fair",
                status: DecisionEvaluationStatus.Acceptable);
            var analytics = AnalyticsSnapshot(3, behaviourBalance: 10f, missionBalance: 10f);
            var advisory = DecisionAdvisoryCalculator.Calculate(
                evaluation,
                analytics,
                DecisionTelemetrySnapshot.Empty,
                null,
                DecisionExplanation.Empty);

            Assert.AreEqual(DecisionRecommendationType.ReviewMissionAllocation, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationPriority.Medium, advisory.Recommendations[0].Priority);
            Assert.AreEqual(60f, advisory.AdvisoryConfidence, 1e-4f);
        }

        [Test]
        public void Calculator_UnbalancedMissionIncreasesPriority()
        {
            var evaluation = EvaluationSnapshot(
                decisionStep: 3,
                decisionTimestamp: 3f,
                quality: 80f,
                behaviourSuitability: 90f,
                confidenceQuality: 90f,
                optimizationBenefit: 90f,
                missionSuitability: 90f,
                knowledgeCoverage: 90f,
                consistency: 90f,
                grade: "Good",
                status: DecisionEvaluationStatus.Good);
            var analytics = AnalyticsSnapshot(3, missionBalance: 90f);
            var advisory = DecisionAdvisoryCalculator.Calculate(
                evaluation,
                analytics,
                DecisionTelemetrySnapshot.Empty,
                null,
                DecisionExplanation.Empty);

            Assert.AreEqual(DecisionRecommendationType.IncreaseMissionPriority, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationPriority.Medium, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.High, advisory.Recommendations[0].Severity);
        }

        [Test]
        public void Calculator_UnbalancedBehaviourIncreasesPersistence()
        {
            var evaluation = EvaluationSnapshot(
                decisionStep: 3,
                decisionTimestamp: 3f,
                quality: 80f,
                behaviourSuitability: 90f,
                confidenceQuality: 90f,
                optimizationBenefit: 90f,
                missionSuitability: 90f,
                knowledgeCoverage: 90f,
                consistency: 90f,
                grade: "Good",
                status: DecisionEvaluationStatus.Good);
            var analytics = AnalyticsSnapshot(3, behaviourBalance: 90f);
            var advisory = DecisionAdvisoryCalculator.Calculate(
                evaluation,
                analytics,
                DecisionTelemetrySnapshot.Empty,
                null,
                DecisionExplanation.Empty);

            Assert.AreEqual(DecisionRecommendationType.IncreaseSearchPersistence, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationPriority.Medium, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.High, advisory.Recommendations[0].Severity);
        }

        [Test]
        public void Calculator_LowOptimizationReviewsOptimization()
        {
            var evaluation = EvaluationSnapshot(
                decisionStep: 3,
                decisionTimestamp: 3f,
                quality: 70f,
                behaviourSuitability: 90f,
                confidenceQuality: 90f,
                optimizationBenefit: 30f,
                missionSuitability: 90f,
                knowledgeCoverage: 90f,
                consistency: 90f,
                grade: "Fair",
                status: DecisionEvaluationStatus.Acceptable);
            var analytics = AnalyticsSnapshot(3);
            var advisory = DecisionAdvisoryCalculator.Calculate(
                evaluation,
                analytics,
                DecisionTelemetrySnapshot.Empty,
                null,
                DecisionExplanation.Empty);

            Assert.AreEqual(DecisionRecommendationType.ReviewOptimization, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationPriority.Medium, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.High, advisory.Recommendations[0].Severity);
            Assert.AreEqual(70f, advisory.AdvisoryConfidence, 1e-4f);
        }

        [Test]
        public void Calculator_LowConfidenceReviewsSensor()
        {
            var evaluation = EvaluationSnapshot(
                decisionStep: 3,
                decisionTimestamp: 3f,
                quality: 70f,
                behaviourSuitability: 90f,
                confidenceQuality: 30f,
                optimizationBenefit: 90f,
                missionSuitability: 90f,
                knowledgeCoverage: 90f,
                consistency: 90f,
                grade: "Fair",
                status: DecisionEvaluationStatus.Acceptable);
            var analytics = AnalyticsSnapshot(3);
            var advisory = DecisionAdvisoryCalculator.Calculate(
                evaluation,
                analytics,
                DecisionTelemetrySnapshot.Empty,
                null,
                DecisionExplanation.Empty);

            Assert.AreEqual(DecisionRecommendationType.ReviewSensorConfidence, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationPriority.Medium, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.High, advisory.Recommendations[0].Severity);
            Assert.AreEqual(70f, advisory.AdvisoryConfidence, 1e-4f);
        }

        [Test]
        public void Calculator_SearchMissionWithLowKnowledgeIncreasesRadius()
        {
            var missions = new DecisionMissionCount[]
            {
                new(MissionTaskState.Idle, 0),
                new(MissionTaskState.SearchArea, 5),
                new(MissionTaskState.InvestigateTarget, 0),
                new(MissionTaskState.RescueVictim, 0),
                new(MissionTaskState.AvoidHazard, 0),
                new(MissionTaskState.ResumeSearch, 0),
            };
            var advisory = AdvisoryScenario(5, 0.9f, 0.9f, 2, 5, null, missions, TraceFrame(5, 5f));

            Assert.AreEqual(DecisionRecommendationType.ReviewKnowledgeCoverage, advisory.OverallRecommendation);
            Assert.AreEqual(2, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationType.ReviewKnowledgeCoverage, advisory.Recommendations[0].Type);
            Assert.AreEqual(DecisionRecommendationPriority.Medium, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationType.IncreaseSearchRadius, advisory.Recommendations[1].Type);
            Assert.AreEqual(DecisionRecommendationPriority.Low, advisory.Recommendations[1].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.Low, advisory.Recommendations[1].Severity);
            Assert.AreEqual(60f, advisory.Recommendations[1].Confidence, 1e-4f);
            Assert.IsTrue(advisory.RequiresAttention);
            Assert.AreEqual(DecisionAdvisorySnapshot.AttentionStatus, advisory.Status);
        }

        [Test]
        public void Calculator_HighSpeedMultiplierReducesSpeed()
        {
            var evaluation = EvaluationSnapshot(
                decisionStep: 4,
                decisionTimestamp: 4f,
                quality: 80f,
                behaviourSuitability: 90f,
                confidenceQuality: 90f,
                optimizationBenefit: 90f,
                missionSuitability: 90f,
                knowledgeCoverage: 90f,
                consistency: 90f,
                grade: "Good",
                status: DecisionEvaluationStatus.Good);
            var analytics = AnalyticsSnapshot(4);
            var telemetry = TelemetrySnapshot(4, averageSpeedMultiplier: 1.5f);
            var advisory = DecisionAdvisoryCalculator.Calculate(
                evaluation,
                analytics,
                telemetry,
                null,
                DecisionExplanation.Empty);

            Assert.AreEqual(DecisionRecommendationType.ReduceSpeedMultiplier, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationPriority.Low, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.Low, advisory.Recommendations[0].Severity);
            Assert.AreEqual(50f, advisory.AdvisoryConfidence, 1e-4f);
            Assert.IsFalse(advisory.RequiresAttention);
            Assert.AreEqual(DecisionAdvisorySnapshot.AdvisoryStatus, advisory.Status);
        }

        [Test]
        public void Calculator_NearHazardAdvisory()
        {
            var evaluation = EvaluationSnapshot(
                decisionStep: 4,
                decisionTimestamp: 4f,
                quality: 80f,
                behaviourSuitability: 90f,
                confidenceQuality: 90f,
                optimizationBenefit: 90f,
                missionSuitability: 90f,
                knowledgeCoverage: 90f,
                consistency: 90f,
                grade: "Good",
                status: DecisionEvaluationStatus.Good);
            var analytics = AnalyticsSnapshot(4);

            var searching = TelemetrySnapshot(4, currentBehaviour: BehaviourState.Search);
            var advisory = DecisionAdvisoryCalculator.Calculate(
                evaluation,
                analytics,
                searching,
                null,
                ExplanationWithHazard(10f));

            Assert.AreEqual(DecisionRecommendationType.IncreaseObstacleAvoidance, advisory.OverallRecommendation);
            Assert.AreEqual(1, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationPriority.High, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationSeverity.High, advisory.Recommendations[0].Severity);
            Assert.AreEqual(90f, advisory.AdvisoryConfidence, 1e-4f);
            Assert.IsTrue(advisory.RequiresAttention);
            Assert.AreEqual(DecisionAdvisorySnapshot.CriticalStatus, advisory.Status);

            var avoiding = TelemetrySnapshot(4, currentBehaviour: BehaviourState.Avoid);
            var silent = DecisionAdvisoryCalculator.Calculate(
                evaluation,
                analytics,
                avoiding,
                null,
                ExplanationWithHazard(10f));

            Assert.AreEqual(DecisionRecommendationType.MaintainCurrentStrategy, silent.OverallRecommendation);
            Assert.AreEqual(1, silent.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationType.MaintainCurrentStrategy, silent.Recommendations[0].Type);
            Assert.AreEqual(DecisionRecommendationPriority.None, silent.Recommendations[0].Priority);
            Assert.IsFalse(silent.RequiresAttention);
            Assert.AreEqual(DecisionAdvisorySnapshot.NominalStatus, silent.Status);
        }

        [Test]
        public void Calculator_OrdersRecommendationsPriorityFirst()
        {
            var evaluation = EvaluationSnapshot(
                decisionStep: 4,
                decisionTimestamp: 4f,
                quality: 80f,
                behaviourSuitability: 90f,
                confidenceQuality: 40f,
                optimizationBenefit: 40f,
                missionSuitability: 40f,
                knowledgeCoverage: 40f,
                consistency: 90f,
                grade: "Good",
                status: DecisionEvaluationStatus.Good);
            var analytics = AnalyticsSnapshot(4, behaviourBalance: 90f, missionBalance: 90f);
            var missions = new DecisionMissionCount[]
            {
                new(MissionTaskState.Idle, 0),
                new(MissionTaskState.SearchArea, 4),
                new(MissionTaskState.InvestigateTarget, 0),
                new(MissionTaskState.RescueVictim, 0),
                new(MissionTaskState.AvoidHazard, 0),
                new(MissionTaskState.ResumeSearch, 0),
            };
            var telemetry = TelemetrySnapshot(
                4,
                averageSpeedMultiplier: 1.5f,
                currentBehaviour: BehaviourState.Search,
                missions: missions);
            var advisory = DecisionAdvisoryCalculator.Calculate(
                evaluation,
                analytics,
                telemetry,
                null,
                ExplanationWithHazard(10f));

            Assert.AreEqual(9, advisory.Recommendations.Length);
            Assert.AreEqual(DecisionRecommendationType.IncreaseObstacleAvoidance, advisory.OverallRecommendation);
            Assert.AreEqual(DecisionRecommendationPriority.High, advisory.Recommendations[0].Priority);
            Assert.AreEqual(DecisionRecommendationType.IncreaseSearchPersistence, advisory.Recommendations[1].Type);
            Assert.AreEqual(DecisionRecommendationType.IncreaseMissionPriority, advisory.Recommendations[2].Type);
            Assert.AreEqual(DecisionRecommendationType.ReviewOptimization, advisory.Recommendations[3].Type);
            Assert.AreEqual(DecisionRecommendationType.ReviewSensorConfidence, advisory.Recommendations[4].Type);
            Assert.AreEqual(DecisionRecommendationType.ReviewKnowledgeCoverage, advisory.Recommendations[5].Type);
            Assert.AreEqual(DecisionRecommendationType.ReviewMissionAllocation, advisory.Recommendations[6].Type);
            Assert.AreEqual(DecisionRecommendationType.IncreaseSearchRadius, advisory.Recommendations[7].Type);
            Assert.AreEqual(DecisionRecommendationType.ReduceSpeedMultiplier, advisory.Recommendations[8].Type);
            Assert.AreEqual(90f, advisory.AdvisoryConfidence, 1e-4f);
            Assert.IsTrue(advisory.RequiresAttention);
            Assert.AreEqual(DecisionAdvisorySnapshot.CriticalStatus, advisory.Status);
            Assert.IsTrue(DecisionAdvisoryValidator.NoDuplicateRecommendations(advisory));
            Assert.IsTrue(DecisionAdvisoryValidator.PriorityOrdered(advisory));
            Assert.IsTrue(DecisionAdvisoryValidator.IsValid(advisory));
        }

        [Test]
        public void Formatter_DeterministicAndContainsSections()
        {
            var advisory = AdvisoryScenario(4, 0.9f, 0.9f, 1, 4, null, null, TraceFrame(4, 4f));

            var first = DecisionAdvisoryFormatter.Format(advisory);
            var second = DecisionAdvisoryFormatter.Format(advisory);

            Assert.AreEqual(first, second);
            Assert.IsTrue(first.Length > 0);
            Assert.IsTrue(first.Contains("Decision Advisory Report"));
            Assert.IsTrue(first.Contains(Label("Overall Recommendation") + "IncreaseKnowledgeCoverage"));
            Assert.IsTrue(first.Contains(Label("Advisory Confidence") + "75.0"));
            Assert.IsTrue(first.Contains("High Priority"));
            Assert.IsTrue(first.Contains("Medium Priority"));
            Assert.IsTrue(first.Contains("Low Priority"));
            Assert.IsTrue(first.Contains("Summary"));
            Assert.IsTrue(first.Contains(Label("Total Recommendations") + "1"));
            Assert.IsTrue(first.Contains(Label("Requires Attention") + "True"));
            Assert.IsTrue(first.Contains(Label("Status") + "Critical"));
        }

        [Test]
        public void Validator_AcceptsValidSnapshot()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var advisory = engine.LastAdvisory;

            Assert.IsTrue(advisory.IsValid);
            Assert.IsTrue(DecisionAdvisoryValidator.NoDuplicateRecommendations(advisory));
            Assert.IsTrue(DecisionAdvisoryValidator.PriorityOrdered(advisory));
            Assert.IsTrue(DecisionAdvisoryValidator.ConfidenceInRange(advisory));
            Assert.IsTrue(DecisionAdvisoryValidator.SeverityValid(advisory));
            Assert.IsTrue(DecisionAdvisoryValidator.RecommendationTypeValid(advisory));
            Assert.IsTrue(DecisionAdvisoryValidator.SnapshotComplete(advisory));
            Assert.IsTrue(DecisionAdvisoryValidator.IsValid(advisory));
            Assert.IsTrue(DecisionAdvisoryValidator.IsValid(DecisionAdvisorySnapshot.Empty));
        }

        [Test]
        public void Validator_RejectsMalformedSnapshots()
        {
            var duplicate = AdvisorySnapshot(
                1,
                DecisionRecommendationType.IncreaseSearchRadius,
                new[]
                {
                    Recommendation(DecisionRecommendationType.IncreaseSearchRadius),
                    Recommendation(DecisionRecommendationType.IncreaseSearchRadius),
                },
                50f,
                false,
                DecisionAdvisorySnapshot.AdvisoryStatus);
            Assert.IsFalse(DecisionAdvisoryValidator.NoDuplicateRecommendations(duplicate));
            Assert.IsFalse(DecisionAdvisoryValidator.IsValid(duplicate));

            var unordered = AdvisorySnapshot(
                1,
                DecisionRecommendationType.IncreaseSearchRadius,
                new[]
                {
                    Recommendation(DecisionRecommendationType.IncreaseSearchRadius, DecisionRecommendationPriority.Low),
                    Recommendation(DecisionRecommendationType.ReduceSpeedMultiplier, DecisionRecommendationPriority.High),
                },
                50f,
                true,
                DecisionAdvisorySnapshot.CriticalStatus);
            Assert.IsFalse(DecisionAdvisoryValidator.PriorityOrdered(unordered));
            Assert.IsFalse(DecisionAdvisoryValidator.IsValid(unordered));

            var badConfidence = AdvisorySnapshot(
                1,
                DecisionRecommendationType.IncreaseSearchRadius,
                new[]
                {
                    Recommendation(DecisionRecommendationType.IncreaseSearchRadius, confidence: 150f),
                },
                150f,
                false,
                DecisionAdvisorySnapshot.AdvisoryStatus);
            Assert.IsFalse(DecisionAdvisoryValidator.ConfidenceInRange(badConfidence));
            Assert.IsFalse(DecisionAdvisoryValidator.IsValid(badConfidence));

            var badSeverity = AdvisorySnapshot(
                1,
                DecisionRecommendationType.IncreaseSearchRadius,
                new[]
                {
                    Recommendation(DecisionRecommendationType.IncreaseSearchRadius, severity: (DecisionRecommendationSeverity)99),
                },
                50f,
                false,
                DecisionAdvisorySnapshot.AdvisoryStatus);
            Assert.IsFalse(DecisionAdvisoryValidator.SeverityValid(badSeverity));
            Assert.IsFalse(DecisionAdvisoryValidator.IsValid(badSeverity));

            var badOverall = AdvisorySnapshot(
                1,
                DecisionRecommendationType.ReviewOptimization,
                new[]
                {
                    Recommendation(DecisionRecommendationType.IncreaseSearchRadius),
                },
                50f,
                false,
                DecisionAdvisorySnapshot.AdvisoryStatus);
            Assert.IsFalse(DecisionAdvisoryValidator.RecommendationTypeValid(badOverall));
            Assert.IsFalse(DecisionAdvisoryValidator.IsValid(badOverall));

            var noneWithRecs = AdvisorySnapshot(
                1,
                DecisionRecommendationType.None,
                new[]
                {
                    Recommendation(DecisionRecommendationType.IncreaseSearchRadius),
                },
                50f,
                false,
                DecisionAdvisorySnapshot.AdvisoryStatus);
            Assert.IsFalse(DecisionAdvisoryValidator.RecommendationTypeValid(noneWithRecs));

            var badStatus = AdvisorySnapshot(
                1,
                DecisionRecommendationType.IncreaseSearchRadius,
                new[]
                {
                    Recommendation(DecisionRecommendationType.IncreaseSearchRadius),
                },
                50f,
                false,
                "UnknownStatus");
            Assert.IsFalse(DecisionAdvisoryValidator.SnapshotComplete(badStatus));
            Assert.IsFalse(DecisionAdvisoryValidator.IsValid(badStatus));
        }

        [Test]
        public void Integration_EngineAdvisoryAfterSteps()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(NeutralReading());
            engine.Decide(ObstacleReading(0.9f));

            var advisory = engine.LastAdvisory;

            Assert.AreEqual(3, advisory.DecisionStep);
            Assert.GreaterOrEqual(advisory.AdvisoryConfidence, 0f);
            Assert.LessOrEqual(advisory.AdvisoryConfidence, 100f);
            Assert.Greater(advisory.Recommendations.Length, 0);
            Assert.IsTrue(advisory.IsValid);
            Assert.IsTrue(DecisionAdvisoryValidator.IsValid(advisory));
            AssertAdvisoriesEqual(advisory, engine.GetAdvisory());
            Assert.AreEqual(engine.LastEvaluation.DecisionStep, engine.GetDiagnostics().LastAdvisory.DecisionStep);
            Assert.AreEqual(engine.LastAdvisory.DecisionStep, engine.LastSnapshot.Diagnostics.LastAdvisory.DecisionStep);
        }

        [Test]
        public void Integration_ResetClearsAdvisory()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(NeutralReading());

            Assert.AreEqual(2, engine.LastAdvisory.DecisionStep);

            engine.Reset();

            AssertAdvisoriesEqual(engine.LastAdvisory, DecisionAdvisorySnapshot.Empty);
            AssertAdvisoriesEqual(engine.GetAdvisory(), DecisionAdvisorySnapshot.Empty);
            AssertAdvisoriesEqual(engine.GetDiagnostics().LastAdvisory, DecisionAdvisorySnapshot.Empty);
            Assert.IsTrue(DecisionAdvisoryValidator.IsValid(engine.LastAdvisory));

            engine.Decide(ObstacleReading(0.9f));
            Assert.AreEqual(1, engine.LastAdvisory.DecisionStep);
            Assert.AreEqual(1, engine.LastEvaluation.DecisionStep);
        }
    }
}
