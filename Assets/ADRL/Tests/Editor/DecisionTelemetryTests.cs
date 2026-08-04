namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
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
    /// Verifies the Phase 9.3 Autonomous Decision Telemetry Framework: the
    /// <see cref="DecisionTelemetryCollector"/> is the single runtime owner of
    /// telemetry (observing every completed decision frame after tracing and
    /// producing immutable <see cref="DecisionTelemetrySnapshot"/> projections),
    /// the internal statistics accumulator drives O(1) allocation-free running
    /// totals, the <see cref="DecisionTelemetryFormatter"/> renders the snapshot
    /// deterministically, the <see cref="DecisionTelemetryValidator"/> confirms
    /// structural soundness, and the <c>DecisionEngine</c> observes, exposes and
    /// synchronizes telemetry without any behavioural change. Telemetry is
    /// strictly observational.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionTelemetryTests
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
        /// Builds a minimal trace frame with the given step clock and telemetry
        /// inputs, for collector and statistics unit tests. The frame is
        /// structurally valid (positive clocks, present executor and empty reasons)
        /// but not cross-checked against a real explanation.
        /// </summary>
        private static DecisionTraceFrame TraceFrame(
            int step,
            BehaviourState behaviour = BehaviourState.Avoid,
            MissionTaskState mission = MissionTaskState.AvoidHazard,
            float confidence = 0.9f,
            float optimizationConfidence = 1f,
            int candidateCount = 1,
            int knowledgeCount = 1,
            int memoryCount = 1,
            float speedMultiplier = 1.1f,
            float turnRateMultiplier = 1.2f,
            string executor = "AvoidExecutor")
        {
            var task = new MissionTask(mission, 0f, MissionTaskState.SearchArea, true);
            var profile = new BehaviourExecutionProfile(
                speedMultiplier,
                turnRateMultiplier,
                0.5f,
                0f,
                0f,
                optimizationConfidence);

            return new DecisionTraceFrame(
                step,
                step,
                SituationSnapshot.Invalid,
                task,
                behaviour,
                profile,
                DroneCommand.Idle,
                DecisionExplanation.Empty,
                DecisionDiagnostics.Empty,
                new TaskPriority(task, 0.986f, confidence, true),
                candidateCount,
                knowledgeCount,
                memoryCount,
                executor,
                optimizationConfidence,
                System.Array.Empty<DecisionReason>());
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

        private static int BehaviourCountOf(DecisionTelemetrySnapshot telemetry, BehaviourState behaviour)
        {
            for (var i = 0; i < telemetry.BehaviourDistribution.Length; i++)
            {
                if (telemetry.BehaviourDistribution[i].Behaviour == behaviour)
                    return telemetry.BehaviourDistribution[i].Count;
            }

            return 0;
        }

        private static int MissionCountOf(DecisionTelemetrySnapshot telemetry, MissionTaskState mission)
        {
            for (var i = 0; i < telemetry.MissionDistribution.Length; i++)
            {
                if (telemetry.MissionDistribution[i].Mission == mission)
                    return telemetry.MissionDistribution[i].Count;
            }

            return 0;
        }

        private static int BehaviourTotal(DecisionTelemetrySnapshot telemetry)
        {
            var total = 0;
            for (var i = 0; i < telemetry.BehaviourDistribution.Length; i++)
                total += telemetry.BehaviourDistribution[i].Count;
            return total;
        }

        private static void AssertSnapshotsEqual(DecisionTelemetrySnapshot a, DecisionTelemetrySnapshot b)
        {
            Assert.AreEqual(a.DecisionCount, b.DecisionCount);
            Assert.AreEqual(a.AverageDecisionConfidence, b.AverageDecisionConfidence, 1e-6f);
            Assert.AreEqual(a.AverageOptimizationConfidence, b.AverageOptimizationConfidence, 1e-6f);
            Assert.AreEqual(a.AverageCandidateCount, b.AverageCandidateCount, 1e-6f);
            Assert.AreEqual(a.AverageExecutionSpeedMultiplier, b.AverageExecutionSpeedMultiplier, 1e-6f);
            Assert.AreEqual(a.AverageTurnRateMultiplier, b.AverageTurnRateMultiplier, 1e-6f);
            Assert.AreEqual(a.KnowledgeRecordCount, b.KnowledgeRecordCount);
            Assert.AreEqual(a.MemoryRecordCount, b.MemoryRecordCount);
            Assert.AreEqual(a.CurrentBehaviour, b.CurrentBehaviour);
            Assert.AreEqual(a.CurrentMission, b.CurrentMission);
            Assert.AreEqual(a.CurrentExecutor, b.CurrentExecutor);
            Assert.AreEqual(a.LatestDecisionStep, b.LatestDecisionStep);
            Assert.AreEqual(a.LatestDecisionTimestamp, b.LatestDecisionTimestamp, 1e-6f);
            Assert.AreEqual(a.BehaviourDistribution.Length, b.BehaviourDistribution.Length);
            Assert.AreEqual(a.MissionDistribution.Length, b.MissionDistribution.Length);

            for (var i = 0; i < a.BehaviourDistribution.Length; i++)
            {
                Assert.AreEqual(a.BehaviourDistribution[i].Behaviour, b.BehaviourDistribution[i].Behaviour);
                Assert.AreEqual(a.BehaviourDistribution[i].Count, b.BehaviourDistribution[i].Count);
            }

            for (var i = 0; i < a.MissionDistribution.Length; i++)
            {
                Assert.AreEqual(a.MissionDistribution[i].Mission, b.MissionDistribution[i].Mission);
                Assert.AreEqual(a.MissionDistribution[i].Count, b.MissionDistribution[i].Count);
            }
        }

        [Test]
        public void Collector_ObserveAccumulatesDecisionCount()
        {
            var collector = new DecisionTelemetryCollector();
            collector.Observe(TraceFrame(1));
            collector.Observe(TraceFrame(2));
            collector.Observe(TraceFrame(3));

            var snap = collector.GetSnapshot();

            Assert.AreEqual(3, snap.DecisionCount);
            Assert.AreEqual(3, snap.LatestDecisionStep);
            Assert.IsTrue(DecisionTelemetryValidator.IsValid(snap));
        }

        [Test]
        public void Collector_EmptySnapshotBeforeAnyObservation()
        {
            var collector = new DecisionTelemetryCollector();

            var snap = collector.GetSnapshot();

            Assert.AreEqual(0, snap.DecisionCount);
            Assert.AreEqual(0f, snap.AverageDecisionConfidence, 1e-6f);
            Assert.AreEqual(0f, snap.AverageOptimizationConfidence, 1e-6f);
            Assert.AreEqual(0, snap.LatestDecisionStep);
            Assert.AreEqual(0, BehaviourTotal(snap));
            Assert.IsTrue(snap.IsValid);
            Assert.IsTrue(DecisionTelemetryValidator.IsValid(snap));
        }

        [Test]
        public void Collector_ResetClearsTelemetry()
        {
            var collector = new DecisionTelemetryCollector();
            collector.Observe(TraceFrame(1));
            collector.Observe(TraceFrame(2));

            collector.Reset();

            var snap = collector.GetSnapshot();
            Assert.AreEqual(0, snap.DecisionCount);
            Assert.AreEqual(0f, snap.AverageDecisionConfidence, 1e-6f);
            Assert.AreEqual(0f, snap.AverageOptimizationConfidence, 1e-6f);
            Assert.AreEqual(0, snap.LatestDecisionStep);
            Assert.AreEqual(0, BehaviourTotal(snap));
            Assert.IsTrue(DecisionTelemetryValidator.IsValid(snap));
        }

        [Test]
        public void Collector_NullFrameIgnored()
        {
            var collector = new DecisionTelemetryCollector();
            collector.Observe(TraceFrame(1));

            collector.Observe(null);

            var snap = collector.GetSnapshot();
            Assert.AreEqual(1, snap.DecisionCount);
            Assert.IsTrue(DecisionTelemetryValidator.IsValid(snap));
        }

        [Test]
        public void Collector_SnapshotIsOwnedProjection()
        {
            var collector = new DecisionTelemetryCollector();
            collector.Observe(TraceFrame(1, behaviour: BehaviourState.Avoid));

            var first = collector.GetSnapshot();
            first.BehaviourDistribution[3] = new DecisionBehaviourCount(BehaviourState.Avoid, 99);

            var second = collector.GetSnapshot();

            Assert.AreEqual(1, BehaviourCountOf(second, BehaviourState.Avoid));
            Assert.AreNotSame(first.BehaviourDistribution, second.BehaviourDistribution);
        }

        [Test]
        public void Statistics_RunningAverages()
        {
            var collector = new DecisionTelemetryCollector();
            collector.Observe(TraceFrame(
                1,
                confidence: 0.8f,
                optimizationConfidence: 0.9f,
                candidateCount: 2,
                speedMultiplier: 1.0f,
                turnRateMultiplier: 1.0f));
            collector.Observe(TraceFrame(
                2,
                confidence: 1.0f,
                optimizationConfidence: 1.0f,
                candidateCount: 4,
                speedMultiplier: 1.2f,
                turnRateMultiplier: 1.4f));

            var snap = collector.GetSnapshot();

            Assert.AreEqual(0.9f, snap.AverageDecisionConfidence, 1e-6f);
            Assert.AreEqual(0.95f, snap.AverageOptimizationConfidence, 1e-6f);
            Assert.AreEqual(3f, snap.AverageCandidateCount, 1e-6f);
            Assert.AreEqual(1.1f, snap.AverageExecutionSpeedMultiplier, 1e-6f);
            Assert.AreEqual(1.2f, snap.AverageTurnRateMultiplier, 1e-6f);
        }

        [Test]
        public void Statistics_BehaviourDistribution()
        {
            var collector = new DecisionTelemetryCollector();
            collector.Observe(TraceFrame(1, behaviour: BehaviourState.Avoid));
            collector.Observe(TraceFrame(2, behaviour: BehaviourState.Search));
            collector.Observe(TraceFrame(3, behaviour: BehaviourState.Avoid));

            var snap = collector.GetSnapshot();

            Assert.AreEqual(4, snap.BehaviourDistribution.Length);
            Assert.AreEqual(2, BehaviourCountOf(snap, BehaviourState.Avoid));
            Assert.AreEqual(1, BehaviourCountOf(snap, BehaviourState.Search));
            Assert.AreEqual(0, BehaviourCountOf(snap, BehaviourState.Approach));
            Assert.AreEqual(3, BehaviourTotal(snap));
        }

        [Test]
        public void Statistics_MissionDistribution()
        {
            var collector = new DecisionTelemetryCollector();
            collector.Observe(TraceFrame(1, mission: MissionTaskState.AvoidHazard));
            collector.Observe(TraceFrame(2, mission: MissionTaskState.SearchArea));
            collector.Observe(TraceFrame(3, mission: MissionTaskState.AvoidHazard));

            var snap = collector.GetSnapshot();

            Assert.AreEqual(6, snap.MissionDistribution.Length);
            Assert.AreEqual(2, MissionCountOf(snap, MissionTaskState.AvoidHazard));
            Assert.AreEqual(1, MissionCountOf(snap, MissionTaskState.SearchArea));
            Assert.AreEqual(0, MissionCountOf(snap, MissionTaskState.RescueVictim));
        }

        [Test]
        public void Statistics_LatestValuesTracked()
        {
            var collector = new DecisionTelemetryCollector();
            collector.Observe(TraceFrame(
                1,
                behaviour: BehaviourState.Search,
                mission: MissionTaskState.SearchArea,
                knowledgeCount: 3,
                memoryCount: 5,
                executor: "SearchExecutor"));
            collector.Observe(TraceFrame(
                2,
                behaviour: BehaviourState.Avoid,
                mission: MissionTaskState.AvoidHazard,
                knowledgeCount: 4,
                memoryCount: 6,
                executor: "AvoidExecutor"));

            var snap = collector.GetSnapshot();

            Assert.AreEqual(BehaviourState.Avoid, snap.CurrentBehaviour);
            Assert.AreEqual(MissionTaskState.AvoidHazard, snap.CurrentMission);
            Assert.AreEqual("AvoidExecutor", snap.CurrentExecutor);
            Assert.AreEqual(2, snap.LatestDecisionStep);
            Assert.AreEqual(2f, snap.LatestDecisionTimestamp, 1e-6f);
            Assert.AreEqual(4, snap.KnowledgeRecordCount);
            Assert.AreEqual(6, snap.MemoryRecordCount);
        }

        [Test]
        public void Formatter_DeterministicOutput()
        {
            var collector = new DecisionTelemetryCollector();
            collector.Observe(TraceFrame(1));
            collector.Observe(TraceFrame(2, behaviour: BehaviourState.Search));

            var snap = collector.GetSnapshot();
            var first = DecisionTelemetryFormatter.Format(snap);
            var second = DecisionTelemetryFormatter.Format(snap);

            Assert.AreEqual(first, second);
            Assert.IsTrue(first.Length > 0);
            Assert.AreEqual(
                DecisionTelemetryFormatter.Format(TelemetrySnapshot(3)),
                DecisionTelemetryFormatter.Format(TelemetrySnapshot(3)));
        }

        [Test]
        public void Formatter_ContainsSections()
        {
            var snap = TelemetrySnapshot(
                2,
                averageDecisionConfidence: 0.9f,
                averageOptimizationConfidence: 1f,
                averageCandidateCount: 2f,
                averageSpeedMultiplier: 1.1f,
                averageTurnRateMultiplier: 1.2f,
                knowledgeRecordCount: 1,
                memoryRecordCount: 2);

            var output = DecisionTelemetryFormatter.Format(snap);

            Assert.IsTrue(output.Contains("=== Decision Telemetry ==="));
            Assert.IsTrue(output.Contains("Decisions: 2"));
            Assert.IsTrue(output.Contains("Average Confidence: 0.9"));
            Assert.IsTrue(output.Contains("Average Optimization Confidence: 1"));
            Assert.IsTrue(output.Contains("Behaviour Distribution"));
            Assert.IsTrue(output.Contains("Avoid: 2"));
            Assert.IsTrue(output.Contains("Mission Distribution"));
            Assert.IsTrue(output.Contains("AvoidHazard: 2"));
            Assert.IsTrue(output.Contains("Knowledge Records: 1"));
            Assert.IsTrue(output.Contains("Memory Records: 2"));
            Assert.IsTrue(output.Contains("Average Candidates: 2"));
            Assert.IsTrue(output.Contains("Execution Speed: 1.1"));
            Assert.IsTrue(output.Contains("Turn Multiplier: 1.2"));
        }

        [Test]
        public void Formatter_EmptySnapshotOutput()
        {
            var output = DecisionTelemetryFormatter.Format(DecisionTelemetrySnapshot.Empty);

            Assert.AreEqual(DecisionTelemetryFormatter.Format(DecisionTelemetrySnapshot.Empty), output);
            Assert.IsTrue(output.Contains("Decisions: 0"));
            Assert.IsTrue(output.Contains("Average Confidence: 0"));
            Assert.IsTrue(output.Contains("Behaviour Distribution"));
            Assert.IsTrue(output.Contains("Mission Distribution"));
            Assert.IsTrue(output.Contains("Current Executor: "));
        }

        [Test]
        public void Validator_AcceptsValidSnapshot()
        {
            var snap = TelemetrySnapshot(
                3,
                averageDecisionConfidence: 0.9f,
                averageOptimizationConfidence: 1f,
                averageCandidateCount: 2f,
                averageSpeedMultiplier: 1.1f,
                averageTurnRateMultiplier: 1.2f,
                knowledgeRecordCount: 1,
                memoryRecordCount: 2);

            Assert.IsTrue(snap.IsValid);
            Assert.IsTrue(DecisionTelemetryValidator.NoNegatives(snap));
            Assert.IsTrue(DecisionTelemetryValidator.NoNaNOrInfinity(snap));
            Assert.IsTrue(DecisionTelemetryValidator.DistributionTotalsMatch(snap));
            Assert.IsTrue(DecisionTelemetryValidator.SnapshotComplete(snap));
            Assert.IsTrue(DecisionTelemetryValidator.CountsValid(snap));
            Assert.IsTrue(DecisionTelemetryValidator.AveragesInRange(snap));
            Assert.IsTrue(DecisionTelemetryValidator.CurrentValuesValid(snap));
            Assert.IsTrue(DecisionTelemetryValidator.IsValid(snap));
            Assert.IsTrue(DecisionTelemetryValidator.IsValid(DecisionTelemetrySnapshot.Empty));

            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            Assert.IsTrue(DecisionTelemetryValidator.IsValid(engine.LastTelemetry));
        }

        [Test]
        public void Validator_RejectsNegative()
        {
            var negativeAverage = TelemetrySnapshot(1, averageDecisionConfidence: -0.5f);
            Assert.IsFalse(DecisionTelemetryValidator.NoNegatives(negativeAverage));
            Assert.IsFalse(DecisionTelemetryValidator.AveragesInRange(negativeAverage));
            Assert.IsFalse(DecisionTelemetryValidator.IsValid(negativeAverage));

            var negativeCount = TelemetrySnapshot(0, behaviours: new[]
            {
                new DecisionBehaviourCount(BehaviourState.Idle, -1),
            });
            Assert.IsFalse(DecisionTelemetryValidator.NoNegatives(negativeCount));
            Assert.IsFalse(DecisionTelemetryValidator.IsValid(negativeCount));
        }

        [Test]
        public void Validator_RejectsNaNOrInfinity()
        {
            var nan = TelemetrySnapshot(1, averageDecisionConfidence: float.NaN);
            Assert.IsFalse(DecisionTelemetryValidator.NoNaNOrInfinity(nan));
            Assert.IsFalse(DecisionTelemetryValidator.IsValid(nan));

            var infinity = TelemetrySnapshot(1, averageCandidateCount: float.PositiveInfinity);
            Assert.IsFalse(DecisionTelemetryValidator.NoNaNOrInfinity(infinity));
            Assert.IsFalse(DecisionTelemetryValidator.IsValid(infinity));
        }

        [Test]
        public void Validator_RejectsDistributionMismatch()
        {
            var mismatched = TelemetrySnapshot(2, behaviours: new[]
            {
                new DecisionBehaviourCount(BehaviourState.Avoid, 3),
            });
            Assert.IsFalse(DecisionTelemetryValidator.DistributionTotalsMatch(mismatched));
            Assert.IsFalse(DecisionTelemetryValidator.IsValid(mismatched));

            var incomplete = default(DecisionTelemetrySnapshot);
            Assert.IsFalse(DecisionTelemetryValidator.SnapshotComplete(incomplete));
            Assert.IsFalse(DecisionTelemetryValidator.DistributionTotalsMatch(incomplete));
            Assert.IsFalse(DecisionTelemetryValidator.IsValid(incomplete));
        }

        [Test]
        public void Integration_EngineTelemetryAfterSteps()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(NeutralReading());
            engine.Decide(ObstacleReading(0.9f));

            var telemetry = engine.LastTelemetry;

            Assert.AreEqual(3, telemetry.DecisionCount);
            Assert.AreEqual(3, engine.GetTelemetry().DecisionCount);
            Assert.AreEqual(3, engine.TelemetryCollector.GetSnapshot().DecisionCount);
            Assert.AreEqual(4, telemetry.BehaviourDistribution.Length);
            Assert.GreaterOrEqual(BehaviourCountOf(telemetry, BehaviourState.Avoid), 2);
            Assert.AreEqual(engine.GetDiagnostics().StepCount, telemetry.LatestDecisionStep);
            Assert.AreEqual(engine.GetDiagnostics().KnowledgeRecordCount, telemetry.KnowledgeRecordCount);
            Assert.AreEqual(engine.KnowledgeStore.Count, telemetry.KnowledgeRecordCount);
            Assert.IsTrue(DecisionTelemetryValidator.IsValid(telemetry));
        }

        [Test]
        public void Integration_DeterministicTelemetry()
        {
            var first = CreateEngine();
            first.Decide(ObstacleReading(0.9f));
            first.Decide(NeutralReading());

            var second = CreateEngine();
            second.Decide(ObstacleReading(0.9f));
            second.Decide(NeutralReading());

            AssertSnapshotsEqual(first.LastTelemetry, second.LastTelemetry);
            Assert.AreEqual(
                DecisionTelemetryFormatter.Format(first.LastTelemetry),
                DecisionTelemetryFormatter.Format(second.LastTelemetry));
        }

        [Test]
        public void Integration_DiagnosticsCarriesTelemetry()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(ObstacleReading(0.9f));

            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual(2, diagnostics.LastTelemetry.DecisionCount);
            Assert.AreEqual(engine.LastTelemetry.DecisionCount, diagnostics.LastTelemetry.DecisionCount);
            Assert.AreEqual(engine.LastSnapshot.Diagnostics.LastTelemetry.DecisionCount, diagnostics.LastTelemetry.DecisionCount);
            Assert.AreEqual(engine.LastTelemetry.AverageDecisionConfidence, diagnostics.LastTelemetry.AverageDecisionConfidence, 1e-6f);
            Assert.AreEqual(engine.LastTelemetry.AverageOptimizationConfidence, diagnostics.LastTelemetry.AverageOptimizationConfidence, 1e-6f);
            Assert.AreEqual(engine.LastTelemetry.DecisionCount, engine.GetTelemetry().DecisionCount);

            Assert.AreEqual(0, engine.LastTraceFrame.Diagnostics.LastTelemetry.DecisionCount);
        }

        [Test]
        public void Integration_ResetClearsTelemetryAndBehaviourUnchanged()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            var commandBefore = engine.LastSnapshot.Command;
            engine.Decide(NeutralReading());

            Assert.AreEqual(2, engine.LastTelemetry.DecisionCount);

            engine.Reset();

            Assert.AreEqual(0, engine.LastTelemetry.DecisionCount);
            Assert.AreEqual(0, engine.GetTelemetry().DecisionCount);
            Assert.AreEqual(0, engine.GetDiagnostics().LastTelemetry.DecisionCount);
            Assert.IsTrue(DecisionTelemetryValidator.IsValid(engine.LastTelemetry));

            engine.Decide(ObstacleReading(0.9f));
            Assert.AreEqual(BehaviourState.Avoid, engine.LastSnapshot.Behaviour);
            Assert.AreEqual(commandBefore, engine.LastSnapshot.Command);
            Assert.AreEqual(1, engine.LastTelemetry.DecisionCount);
        }
    }
}
