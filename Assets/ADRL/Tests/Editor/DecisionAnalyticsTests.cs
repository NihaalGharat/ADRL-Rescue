namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Analytics;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Telemetry;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 9.4 Autonomous Decision Analytics Framework: the
    /// <see cref="DecisionAnalyticsCalculator"/> is the single owner of analytics
    /// computation (a pure, stateless projection of
    /// <see cref="DecisionTelemetrySnapshot"/> into an immutable
    /// <see cref="DecisionAnalyticsSnapshot"/>), <see cref="DecisionHealthCalculator"/>
    /// owns the weighted health score, the <see cref="DecisionAnalyticsFormatter"/>
    /// renders the snapshot deterministically, the
    /// <see cref="DecisionAnalyticsValidator"/> confirms structural soundness, and
    /// the <c>DecisionEngine</c> computes, exposes and synchronizes analytics
    /// without any behavioural change. Analytics is strictly observational.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionAnalyticsTests
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
        /// Builds an analytics snapshot with the given metrics, defaulting to
        /// zero-count behaviour/mission analytics arrays.
        /// </summary>
        private static DecisionAnalyticsSnapshot AnalyticsSnapshot(
            int decisionCount = 0,
            float decisionConfidence = 0f,
            float executionConfidence = 0f,
            float optimizationConfidence = 0f,
            float knowledgeUtilization = 0f,
            float memoryUtilization = 0f,
            float behaviourBalance = 0f,
            float missionBalance = 0f,
            float behaviourEntropy = 0f,
            float missionEntropy = 0f,
            float health = 0f,
            int decisionStep = 0,
            float decisionTimestamp = 0f,
            BehaviourAnalytics[] behaviours = null,
            MissionAnalytics[] missions = null)
        {
            return new DecisionAnalyticsSnapshot(
                decisionCount,
                decisionConfidence,
                executionConfidence,
                optimizationConfidence,
                knowledgeUtilization,
                memoryUtilization,
                behaviourBalance,
                missionBalance,
                behaviourEntropy,
                missionEntropy,
                health,
                decisionStep,
                decisionTimestamp,
                behaviours ?? new BehaviourAnalytics[]
                {
                    new(BehaviourState.Idle, 0, 0f),
                    new(BehaviourState.Search, 0, 0f),
                    new(BehaviourState.Approach, 0, 0f),
                    new(BehaviourState.Avoid, 0, 0f),
                },
                missions ?? new MissionAnalytics[]
                {
                    new(MissionTaskState.Idle, 0, 0f),
                    new(MissionTaskState.SearchArea, 0, 0f),
                    new(MissionTaskState.InvestigateTarget, 0, 0f),
                    new(MissionTaskState.RescueVictim, 0, 0f),
                    new(MissionTaskState.AvoidHazard, 0, 0f),
                    new(MissionTaskState.ResumeSearch, 0, 0f),
                });
        }

        private static int BehaviourCountOf(DecisionAnalyticsSnapshot analytics, BehaviourState behaviour)
        {
            for (var i = 0; i < analytics.BehaviourAnalytics.Length; i++)
            {
                if (analytics.BehaviourAnalytics[i].Behaviour == behaviour)
                    return analytics.BehaviourAnalytics[i].Count;
            }

            return 0;
        }

        private static void AssertSnapshotsEqual(DecisionAnalyticsSnapshot a, DecisionAnalyticsSnapshot b)
        {
            Assert.AreEqual(a.DecisionCount, b.DecisionCount);
            Assert.AreEqual(a.AverageDecisionConfidence, b.AverageDecisionConfidence, 1e-4f);
            Assert.AreEqual(a.AverageExecutionConfidence, b.AverageExecutionConfidence, 1e-4f);
            Assert.AreEqual(a.AverageOptimizationConfidence, b.AverageOptimizationConfidence, 1e-4f);
            Assert.AreEqual(a.KnowledgeUtilizationRate, b.KnowledgeUtilizationRate, 1e-4f);
            Assert.AreEqual(a.MemoryUtilizationRate, b.MemoryUtilizationRate, 1e-4f);
            Assert.AreEqual(a.BehaviourBalanceScore, b.BehaviourBalanceScore, 1e-4f);
            Assert.AreEqual(a.MissionBalanceScore, b.MissionBalanceScore, 1e-4f);
            Assert.AreEqual(a.BehaviourEntropy, b.BehaviourEntropy, 1e-4f);
            Assert.AreEqual(a.MissionEntropy, b.MissionEntropy, 1e-4f);
            Assert.AreEqual(a.DecisionHealthScore, b.DecisionHealthScore, 1e-4f);
            Assert.AreEqual(a.DecisionStep, b.DecisionStep);
            Assert.AreEqual(a.DecisionTimestamp, b.DecisionTimestamp, 1e-4f);
            Assert.AreEqual(a.BehaviourAnalytics.Length, b.BehaviourAnalytics.Length);
            Assert.AreEqual(a.MissionAnalytics.Length, b.MissionAnalytics.Length);

            for (var i = 0; i < a.BehaviourAnalytics.Length; i++)
            {
                Assert.AreEqual(a.BehaviourAnalytics[i].Behaviour, b.BehaviourAnalytics[i].Behaviour);
                Assert.AreEqual(a.BehaviourAnalytics[i].Count, b.BehaviourAnalytics[i].Count);
                Assert.AreEqual(a.BehaviourAnalytics[i].Share, b.BehaviourAnalytics[i].Share, 1e-4f);
            }

            for (var i = 0; i < a.MissionAnalytics.Length; i++)
            {
                Assert.AreEqual(a.MissionAnalytics[i].Mission, b.MissionAnalytics[i].Mission);
                Assert.AreEqual(a.MissionAnalytics[i].Count, b.MissionAnalytics[i].Count);
                Assert.AreEqual(a.MissionAnalytics[i].Share, b.MissionAnalytics[i].Share, 1e-4f);
            }
        }

        [Test]
        public void Calculator_ComputesAveragesAndHealth()
        {
            var telemetry = TelemetrySnapshot(
                2,
                averageDecisionConfidence: 0.9f,
                averageOptimizationConfidence: 0.8f,
                knowledgeRecordCount: 1,
                memoryRecordCount: 2);

            var analytics = DecisionAnalyticsCalculator.Calculate(telemetry);

            Assert.AreEqual(2, analytics.DecisionCount);
            Assert.AreEqual(90f, analytics.AverageDecisionConfidence, 1e-4f);
            Assert.AreEqual(80f, analytics.AverageExecutionConfidence, 1e-4f);
            Assert.AreEqual(80f, analytics.AverageOptimizationConfidence, 1e-4f);
            Assert.AreEqual(50f, analytics.KnowledgeUtilizationRate, 1e-4f);
            Assert.AreEqual(100f, analytics.MemoryUtilizationRate, 1e-4f);
            Assert.AreEqual(78.5f, analytics.DecisionHealthScore, 1e-4f);
            Assert.IsTrue(analytics.IsValid);
            Assert.IsTrue(DecisionAnalyticsValidator.IsValid(analytics));
        }

        [Test]
        public void Calculator_EmptyTelemetryProducesEmptyAnalytics()
        {
            var analytics = DecisionAnalyticsCalculator.Calculate(DecisionTelemetrySnapshot.Empty);

            Assert.AreEqual(0, analytics.DecisionCount);
            Assert.AreEqual(0f, analytics.AverageDecisionConfidence, 1e-4f);
            Assert.AreEqual(0f, analytics.AverageExecutionConfidence, 1e-4f);
            Assert.AreEqual(0f, analytics.AverageOptimizationConfidence, 1e-4f);
            Assert.AreEqual(0f, analytics.KnowledgeUtilizationRate, 1e-4f);
            Assert.AreEqual(0f, analytics.MemoryUtilizationRate, 1e-4f);
            Assert.AreEqual(0f, analytics.BehaviourBalanceScore, 1e-4f);
            Assert.AreEqual(0f, analytics.MissionBalanceScore, 1e-4f);
            Assert.AreEqual(0f, analytics.BehaviourEntropy, 1e-4f);
            Assert.AreEqual(0f, analytics.MissionEntropy, 1e-4f);
            Assert.AreEqual(0f, analytics.DecisionHealthScore, 1e-4f);
            Assert.AreEqual(4, analytics.BehaviourAnalytics.Length);
            Assert.AreEqual(6, analytics.MissionAnalytics.Length);
            Assert.AreEqual(0, BehaviourCountOf(analytics, BehaviourState.Avoid));
            Assert.IsTrue(analytics.IsValid);
            Assert.IsTrue(DecisionAnalyticsValidator.IsValid(analytics));
        }

        [Test]
        public void Calculator_ComputesUtilizationRates()
        {
            var partial = DecisionAnalyticsCalculator.Calculate(
                TelemetrySnapshot(4, knowledgeRecordCount: 1, memoryRecordCount: 3));
            Assert.AreEqual(25f, partial.KnowledgeUtilizationRate, 1e-4f);
            Assert.AreEqual(75f, partial.MemoryUtilizationRate, 1e-4f);

            var saturated = DecisionAnalyticsCalculator.Calculate(
                TelemetrySnapshot(3, knowledgeRecordCount: 9, memoryRecordCount: 0));
            Assert.AreEqual(100f, saturated.KnowledgeUtilizationRate, 1e-4f);
            Assert.AreEqual(0f, saturated.MemoryUtilizationRate, 1e-4f);
        }

        [Test]
        public void Calculator_ComputesBalanceAndEntropy()
        {
            var telemetry = TelemetrySnapshot(
                4,
                averageDecisionConfidence: 0.9f,
                behaviours: new DecisionBehaviourCount[]
                {
                    new(BehaviourState.Idle, 1),
                    new(BehaviourState.Search, 0),
                    new(BehaviourState.Approach, 1),
                    new(BehaviourState.Avoid, 2),
                },
                missions: new DecisionMissionCount[]
                {
                    new(MissionTaskState.Idle, 2),
                    new(MissionTaskState.SearchArea, 0),
                    new(MissionTaskState.InvestigateTarget, 0),
                    new(MissionTaskState.RescueVictim, 0),
                    new(MissionTaskState.AvoidHazard, 2),
                    new(MissionTaskState.ResumeSearch, 0),
                });

            var analytics = DecisionAnalyticsCalculator.Calculate(telemetry);

            Assert.AreEqual(1.0397f, analytics.BehaviourEntropy, 1e-3f);
            Assert.AreEqual(75f, analytics.BehaviourBalanceScore, 1e-4f);
            Assert.AreEqual(0.6931f, analytics.MissionEntropy, 1e-3f);
            Assert.AreEqual(38.6853f, analytics.MissionBalanceScore, 1e-3f);
            Assert.AreEqual(2, BehaviourCountOf(analytics, BehaviourState.Avoid));

            var avoid = analytics.BehaviourAnalytics[3];
            Assert.AreEqual(BehaviourState.Avoid, avoid.Behaviour);
            Assert.AreEqual(2, avoid.Count);
            Assert.AreEqual(50f, avoid.Share, 1e-4f);
        }

        [Test]
        public void Calculator_DeterministicOutput()
        {
            var telemetry = TelemetrySnapshot(
                3,
                averageDecisionConfidence: 0.9f,
                averageOptimizationConfidence: 0.85f,
                knowledgeRecordCount: 2,
                memoryRecordCount: 1);

            var first = DecisionAnalyticsCalculator.Calculate(telemetry);
            var second = DecisionAnalyticsCalculator.Calculate(telemetry);

            AssertSnapshotsEqual(first, second);
            Assert.AreNotSame(first.BehaviourAnalytics, second.BehaviourAnalytics);
            Assert.AreEqual(
                DecisionAnalyticsFormatter.Format(first),
                DecisionAnalyticsFormatter.Format(second));
        }

        [Test]
        public void Formatter_DeterministicOutput()
        {
            var analytics = DecisionAnalyticsCalculator.Calculate(
                TelemetrySnapshot(2, averageDecisionConfidence: 0.9f));

            var first = DecisionAnalyticsFormatter.Format(analytics);
            var second = DecisionAnalyticsFormatter.Format(analytics);

            Assert.AreEqual(first, second);
            Assert.IsTrue(first.Length > 0);
        }

        [Test]
        public void Formatter_ContainsSections()
        {
            var analytics = DecisionAnalyticsCalculator.Calculate(TelemetrySnapshot(
                2,
                averageDecisionConfidence: 0.92f,
                averageOptimizationConfidence: 0.95f,
                knowledgeRecordCount: 1,
                memoryRecordCount: 2));

            var output = DecisionAnalyticsFormatter.Format(analytics);

            Assert.IsTrue(output.Contains("Decision Analytics"));
            Assert.IsTrue(output.Contains("Health Score........"));
            Assert.IsTrue(output.Contains("Decision Confidence."));
            Assert.IsTrue(output.Contains("Execution Confidence"));
            Assert.IsTrue(output.Contains("Knowledge Usage....."));
            Assert.IsTrue(output.Contains("Memory Usage........"));
            Assert.IsTrue(output.Contains("Behaviour Balance..."));
            Assert.IsTrue(output.Contains("Mission Balance....."));
            Assert.IsTrue(output.Contains("Decision Confidence.92%"));
            Assert.IsTrue(output.Contains("Knowledge Usage.....50%"));
            Assert.IsTrue(output.Contains("Memory Usage........100%"));
            Assert.IsTrue(output.Contains("Total Decisions.....2"));
        }

        [Test]
        public void Formatter_EmptyAnalyticsOutput()
        {
            var output = DecisionAnalyticsFormatter.Format(DecisionAnalyticsSnapshot.Empty);

            Assert.AreEqual(DecisionAnalyticsFormatter.Format(DecisionAnalyticsSnapshot.Empty), output);
            Assert.IsTrue(output.Contains("Decision Analytics"));
            Assert.IsTrue(output.Contains("Health Score........0%"));
            Assert.IsTrue(output.Contains("Total Decisions.....0"));
        }

        [Test]
        public void Validator_AcceptsValidSnapshot()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var analytics = engine.LastAnalytics;

            Assert.IsTrue(analytics.IsValid);
            Assert.IsTrue(DecisionAnalyticsValidator.NoNegatives(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.NoNaN(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.NoInfinity(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.HealthScoreInRange(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.EntropyFinite(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.BalanceInRange(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.UtilizationInRange(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.ConfidenceInRange(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.SnapshotComplete(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.IsValid(analytics));
            Assert.IsTrue(DecisionAnalyticsValidator.IsValid(DecisionAnalyticsSnapshot.Empty));
        }

        [Test]
        public void Validator_RejectsNegative()
        {
            var negativeConfidence = AnalyticsSnapshot(decisionConfidence: -5f);
            Assert.IsFalse(DecisionAnalyticsValidator.NoNegatives(negativeConfidence));
            Assert.IsFalse(DecisionAnalyticsValidator.ConfidenceInRange(negativeConfidence));
            Assert.IsFalse(DecisionAnalyticsValidator.IsValid(negativeConfidence));

            var negativeShare = AnalyticsSnapshot(behaviours: new[]
            {
                new BehaviourAnalytics(BehaviourState.Idle, 1, -10f),
            });
            Assert.IsFalse(DecisionAnalyticsValidator.NoNegatives(negativeShare));
            Assert.IsFalse(DecisionAnalyticsValidator.IsValid(negativeShare));
        }

        [Test]
        public void Validator_RejectsNaNOrInfinity()
        {
            var nan = AnalyticsSnapshot(health: float.NaN);
            Assert.IsFalse(DecisionAnalyticsValidator.NoNaN(nan));
            Assert.IsFalse(DecisionAnalyticsValidator.HealthScoreInRange(nan));
            Assert.IsFalse(DecisionAnalyticsValidator.IsValid(nan));

            var infinity = AnalyticsSnapshot(behaviourEntropy: float.PositiveInfinity);
            Assert.IsFalse(DecisionAnalyticsValidator.NoInfinity(infinity));
            Assert.IsFalse(DecisionAnalyticsValidator.EntropyFinite(infinity));
            Assert.IsFalse(DecisionAnalyticsValidator.IsValid(infinity));
        }

        [Test]
        public void Validator_RejectsOutOfRange()
        {
            var highHealth = AnalyticsSnapshot(health: 150f);
            Assert.IsFalse(DecisionAnalyticsValidator.HealthScoreInRange(highHealth));
            Assert.IsFalse(DecisionAnalyticsValidator.IsValid(highHealth));

            var highBalance = AnalyticsSnapshot(behaviourBalance: 120f);
            Assert.IsFalse(DecisionAnalyticsValidator.BalanceInRange(highBalance));
            Assert.IsFalse(DecisionAnalyticsValidator.IsValid(highBalance));

            var highUtilization = AnalyticsSnapshot(knowledgeUtilization: 110f);
            Assert.IsFalse(DecisionAnalyticsValidator.UtilizationInRange(highUtilization));
            Assert.IsFalse(DecisionAnalyticsValidator.IsValid(highUtilization));

            var incomplete = default(DecisionAnalyticsSnapshot);
            Assert.IsFalse(DecisionAnalyticsValidator.SnapshotComplete(incomplete));
            Assert.IsFalse(DecisionAnalyticsValidator.IsValid(incomplete));
        }

        [Test]
        public void Health_WeightedComputation()
        {
            Assert.AreEqual(100f, DecisionHealthCalculator.Calculate(100f, 100f, 100f, 100f, 100f, 100f), 1e-4f);
            Assert.AreEqual(50f, DecisionHealthCalculator.Calculate(50f, 50f, 50f, 50f, 50f, 50f), 1e-4f);
            Assert.AreEqual(35f, DecisionHealthCalculator.Calculate(100f, 0f, 0f, 0f, 0f, 0f), 1e-4f);
            Assert.AreEqual(25f, DecisionHealthCalculator.Calculate(0f, 100f, 0f, 0f, 0f, 0f), 1e-4f);
            Assert.AreEqual(15f, DecisionHealthCalculator.Calculate(0f, 0f, 100f, 0f, 0f, 0f), 1e-4f);
            Assert.AreEqual(10f, DecisionHealthCalculator.Calculate(0f, 0f, 0f, 100f, 0f, 0f), 1e-4f);
            Assert.AreEqual(10f, DecisionHealthCalculator.Calculate(0f, 0f, 0f, 0f, 100f, 0f), 1e-4f);
            Assert.AreEqual(5f, DecisionHealthCalculator.Calculate(0f, 0f, 0f, 0f, 0f, 100f), 1e-4f);
        }

        [Test]
        public void Health_ClampedToRange()
        {
            Assert.AreEqual(100f, DecisionHealthCalculator.Calculate(200f, 200f, 200f, 200f, 200f, 200f), 1e-4f);
            Assert.AreEqual(0f, DecisionHealthCalculator.Calculate(-50f, -50f, -50f, -50f, -50f, -50f), 1e-4f);
        }

        [Test]
        public void Health_ZeroInputsProduceZero()
        {
            Assert.AreEqual(0f, DecisionHealthCalculator.Calculate(0f, 0f, 0f, 0f, 0f, 0f), 1e-4f);
        }

        [Test]
        public void Health_PerfectInputsProduceHundred()
        {
            Assert.AreEqual(100f, DecisionHealthCalculator.Calculate(100f, 100f, 100f, 100f, 100f, 100f), 1e-4f);

            var totalWeight = DecisionHealthCalculator.DecisionConfidenceWeight
                + DecisionHealthCalculator.ExecutionConfidenceWeight
                + DecisionHealthCalculator.OptimizationConfidenceWeight
                + DecisionHealthCalculator.KnowledgeUsageWeight
                + DecisionHealthCalculator.MemoryUsageWeight
                + DecisionHealthCalculator.BehaviourBalanceWeight;
            Assert.AreEqual(1f, totalWeight, 1e-4f);
        }

        [Test]
        public void Integration_EngineAnalyticsAfterSteps()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(NeutralReading());
            engine.Decide(ObstacleReading(0.9f));

            var analytics = engine.LastAnalytics;

            Assert.AreEqual(3, analytics.DecisionCount);
            Assert.AreEqual(3, engine.GetAnalytics().DecisionCount);
            Assert.AreEqual(engine.LastTelemetry.LatestDecisionStep, analytics.DecisionStep);
            Assert.AreEqual(engine.LastTelemetry.LatestDecisionTimestamp, analytics.DecisionTimestamp, 1e-6f);
            Assert.GreaterOrEqual(analytics.DecisionHealthScore, 0f);
            Assert.LessOrEqual(analytics.DecisionHealthScore, 100f);
            Assert.GreaterOrEqual(analytics.BehaviourEntropy, 0f);
            Assert.GreaterOrEqual(analytics.MissionEntropy, 0f);
            Assert.GreaterOrEqual(BehaviourCountOf(analytics, BehaviourState.Avoid), 2);
            Assert.IsTrue(DecisionAnalyticsValidator.IsValid(analytics));
        }

        [Test]
        public void Integration_DeterministicAnalytics()
        {
            var first = CreateEngine();
            first.Decide(ObstacleReading(0.9f));
            first.Decide(NeutralReading());

            var second = CreateEngine();
            second.Decide(ObstacleReading(0.9f));
            second.Decide(NeutralReading());

            AssertSnapshotsEqual(first.LastAnalytics, second.LastAnalytics);
            Assert.AreEqual(
                DecisionAnalyticsFormatter.Format(first.LastAnalytics),
                DecisionAnalyticsFormatter.Format(second.LastAnalytics));
        }

        [Test]
        public void Integration_DiagnosticsCarriesAnalytics()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(ObstacleReading(0.9f));

            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual(2, diagnostics.LastAnalytics.DecisionCount);
            Assert.AreEqual(engine.LastAnalytics.DecisionCount, diagnostics.LastAnalytics.DecisionCount);
            Assert.AreEqual(engine.LastSnapshot.Diagnostics.LastAnalytics.DecisionCount, diagnostics.LastAnalytics.DecisionCount);
            Assert.AreEqual(engine.LastAnalytics.DecisionHealthScore, diagnostics.LastAnalytics.DecisionHealthScore, 1e-4f);
            Assert.AreEqual(engine.LastAnalytics.DecisionCount, engine.GetAnalytics().DecisionCount);

            Assert.AreEqual(0, engine.LastTraceFrame.Diagnostics.LastAnalytics.DecisionCount);
        }

        [Test]
        public void Integration_ResetClearsAnalyticsAndBehaviourUnchanged()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            var commandBefore = engine.LastSnapshot.Command;
            engine.Decide(NeutralReading());

            Assert.AreEqual(2, engine.LastAnalytics.DecisionCount);

            engine.Reset();

            Assert.AreEqual(0, engine.LastAnalytics.DecisionCount);
            Assert.AreEqual(0, engine.GetAnalytics().DecisionCount);
            Assert.AreEqual(0, engine.GetDiagnostics().LastAnalytics.DecisionCount);
            Assert.IsTrue(DecisionAnalyticsValidator.IsValid(engine.LastAnalytics));

            engine.Decide(ObstacleReading(0.9f));
            Assert.AreEqual(BehaviourState.Avoid, engine.LastSnapshot.Behaviour);
            Assert.AreEqual(commandBefore, engine.LastSnapshot.Command);
            Assert.AreEqual(1, engine.LastAnalytics.DecisionCount);
        }
    }
}
