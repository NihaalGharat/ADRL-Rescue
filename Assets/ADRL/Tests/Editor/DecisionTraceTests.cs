namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Explainability;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.Decision.Trace;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 9.2 Autonomous Decision Trace &amp; Replay Framework: the
    /// <see cref="DecisionTraceBuilder"/> is the single owner of trace composition
    /// (a pure, stateless, deterministic function of a built snapshot and its
    /// explanation), the <see cref="DecisionTraceFrame"/> is the immutable,
    /// replayable record of one decision step, the <see cref="DecisionTraceStore"/>
    /// is the bounded, deterministic owner of trace history (append, retrieval,
    /// oldest-first eviction, enumerate), <see cref="DecisionReplay"/> reads trace
    /// frames without ever writing runtime state, the
    /// <see cref="DecisionReplayValidator"/> confirms replay consistency, and the
    /// <c>DecisionEngine</c> records and exposes the last trace frame without any
    /// behavioural change. Tracing is strictly observational.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionTraceTests
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

        private static MissionTask Task(MissionTaskState state) => new(state, 1f, MissionTaskState.SearchArea, true);

        /// <summary>
        /// Builds a minimal trace frame with the given step clock, for store and
        /// replay tests. The frame is structurally valid (positive clocks, present
        /// executor and empty reasons) but not cross-checked against a real
        /// explanation unless the test builds one.
        /// </summary>
        private static DecisionTraceFrame Frame(int step, DecisionReason[] reasons = null)
        {
            return new DecisionTraceFrame(
                step,
                step,
                SituationSnapshot.Invalid,
                new MissionTask(MissionTaskState.AvoidHazard, 0f, MissionTaskState.SearchArea, true),
                BehaviourState.Avoid,
                BehaviourExecutionProfile.Empty,
                DroneCommand.Idle,
                DecisionExplanation.Empty,
                DecisionDiagnostics.Empty,
                TaskPriority.Invalid,
                0,
                0,
                0,
                "AvoidExecutor",
                0f,
                reasons ?? System.Array.Empty<DecisionReason>());
        }

        /// <summary>
        /// Returns a copy of a source frame with the given members overridden, so
        /// validator-rejection tests can build inconsistent frames concisely.
        /// </summary>
        private static DecisionTraceFrame AlteredFrame(
            DecisionTraceFrame source,
            MissionTask? mission = null,
            BehaviourState? behaviour = null,
            DroneCommand? command = null,
            DecisionExplanation? explanation = null,
            DecisionDiagnostics? diagnostics = null,
            TaskPriority? winningTask = null,
            int? knowledgeCount = null,
            float? optimizationConfidence = null,
            int? step = null,
            float? timestamp = null)
        {
            return new DecisionTraceFrame(
                step ?? source.DecisionStep,
                timestamp ?? source.DecisionTimestamp,
                source.Assessment,
                mission ?? source.Mission,
                behaviour ?? source.Behaviour,
                source.OptimizationProfile,
                command ?? source.Command,
                explanation ?? source.Explanation,
                diagnostics ?? source.Diagnostics,
                winningTask ?? source.WinningTask,
                source.CandidateCount,
                knowledgeCount ?? source.KnowledgeCount,
                source.MemoryCount,
                source.ExecutorName,
                optimizationConfidence ?? source.OptimizationConfidence,
                source.Reasons);
        }

        private static void AssertFramesEqual(DecisionTraceFrame a, DecisionTraceFrame b)
        {
            Assert.AreEqual(a.DecisionStep, b.DecisionStep);
            Assert.AreEqual(a.DecisionTimestamp, b.DecisionTimestamp, 1e-6f);
            Assert.AreEqual(a.Assessment, b.Assessment);
            Assert.AreEqual(a.Mission, b.Mission);
            Assert.AreEqual(a.Behaviour, b.Behaviour);
            Assert.AreEqual(a.OptimizationProfile.SpeedMultiplier, b.OptimizationProfile.SpeedMultiplier, 1e-6f);
            Assert.AreEqual(a.OptimizationProfile.TurnRateMultiplier, b.OptimizationProfile.TurnRateMultiplier, 1e-6f);
            Assert.AreEqual(a.OptimizationProfile.ExecutionConfidence, b.OptimizationProfile.ExecutionConfidence, 1e-6f);
            Assert.AreEqual(a.Command, b.Command);
            Assert.AreEqual(a.WinningTask, b.WinningTask);
            Assert.AreEqual(a.CandidateCount, b.CandidateCount);
            Assert.AreEqual(a.KnowledgeCount, b.KnowledgeCount);
            Assert.AreEqual(a.MemoryCount, b.MemoryCount);
            Assert.AreEqual(a.ExecutorName, b.ExecutorName);
            Assert.AreEqual(a.OptimizationConfidence, b.OptimizationConfidence, 1e-6f);
            Assert.AreEqual(a.Reasons.Length, b.Reasons.Length);
            AssertExplanationsEqual(a.Explanation, b.Explanation);
            Assert.AreEqual(a.Diagnostics.StepCount, b.Diagnostics.StepCount);
            Assert.AreEqual(a.Diagnostics.LastBehaviour, b.Diagnostics.LastBehaviour);
            Assert.AreEqual(a.Diagnostics.CandidateCount, b.Diagnostics.CandidateCount);
            Assert.AreEqual(a.Diagnostics.KnowledgeRecordCount, b.Diagnostics.KnowledgeRecordCount);
        }

        private static void AssertExplanationsEqual(DecisionExplanation a, DecisionExplanation b)
        {
            Assert.AreEqual(a.Assessment, b.Assessment);
            Assert.AreEqual(a.Knowledge.RecordCount, b.Knowledge.RecordCount);
            Assert.AreEqual(a.Knowledge.KnownVictims, b.Knowledge.KnownVictims);
            Assert.AreEqual(a.Knowledge.KnownHazards, b.Knowledge.KnownHazards);
            Assert.AreEqual(a.Knowledge.KnownObstacles, b.Knowledge.KnownObstacles);
            Assert.AreEqual(a.Knowledge.NearestVictimDistance, b.Knowledge.NearestVictimDistance, 1e-6f);
            Assert.AreEqual(a.Knowledge.NearestHazardDistance, b.Knowledge.NearestHazardDistance, 1e-6f);
            Assert.AreEqual(a.Memory.LastBehaviour, b.Memory.LastBehaviour);
            Assert.AreEqual(a.Memory.RecordCount, b.Memory.RecordCount);
            Assert.AreEqual(a.Mission, b.Mission);
            Assert.AreEqual(a.CandidateTasks.Length, b.CandidateTasks.Length);
            Assert.AreEqual(a.ScoredCandidates.Length, b.ScoredCandidates.Length);
            Assert.AreEqual(a.Winning, b.Winning);
            Assert.AreEqual(a.Behaviour, b.Behaviour);
            Assert.AreEqual(a.Executor, b.Executor);
            Assert.AreEqual(a.OptimizationProfile.SpeedMultiplier, b.OptimizationProfile.SpeedMultiplier, 1e-6f);
            Assert.AreEqual(a.OptimizationProfile.TurnRateMultiplier, b.OptimizationProfile.TurnRateMultiplier, 1e-6f);
            Assert.AreEqual(a.OptimizationProfile.ExecutionConfidence, b.OptimizationProfile.ExecutionConfidence, 1e-6f);
            Assert.AreEqual(a.Command, b.Command);
            Assert.AreEqual(a.DecisionTimestamp, b.DecisionTimestamp, 1e-6f);
            Assert.AreEqual(a.DecisionStep, b.DecisionStep);
            Assert.AreEqual(a.Reasons.Length, b.Reasons.Length);
            for (var i = 0; i < a.Reasons.Length; i++)
            {
                Assert.AreEqual(a.Reasons[i].Section, b.Reasons[i].Section);
                Assert.AreEqual(a.Reasons[i].Text, b.Reasons[i].Text);
            }
        }

        [Test]
        public void Append_StoresFrame()
        {
            var store = new DecisionTraceStore();
            var frame = Frame(1);

            store.Append(frame);

            Assert.AreEqual(1, store.Count);
            Assert.AreSame(frame, store.Get(0));
            Assert.AreSame(frame, store.Latest);
            Assert.AreEqual(DecisionTraceStore.DefaultCapacity, store.MaximumCapacity);
        }

        [Test]
        public void Retrieval_GetReturnsFrame()
        {
            var store = new DecisionTraceStore();
            store.Append(Frame(1));
            store.Append(Frame(2));

            Assert.AreEqual(2, store.Count);
            Assert.AreEqual(1, store.Get(0).DecisionStep);
            Assert.AreEqual(2, store.Get(1).DecisionStep);
            Assert.AreEqual(2, store.Latest.DecisionStep);
            Assert.IsTrue(ReferenceEquals(DecisionTraceFrame.Empty, store.Get(5)));
        }

        [Test]
        public void Eviction_EvictsOldest()
        {
            var store = new DecisionTraceStore(3);
            store.Append(Frame(1));
            store.Append(Frame(2));
            store.Append(Frame(3));
            store.Append(Frame(4));

            Assert.AreEqual(3, store.Count);
            Assert.AreEqual(2, store.Get(0).DecisionStep);
            Assert.AreEqual(3, store.Get(1).DecisionStep);
            Assert.AreEqual(4, store.Get(2).DecisionStep);
            Assert.AreEqual(4, store.Latest.DecisionStep);
        }

        [Test]
        public void ReplayLatest_ReturnsNewest()
        {
            var store = new DecisionTraceStore();
            store.Append(Frame(1));
            store.Append(Frame(2));

            Assert.AreEqual(2, DecisionReplay.ReplayLatest(store).DecisionStep);
            Assert.AreSame(store.Latest, DecisionReplay.ReplayLatest(store));
        }

        [Test]
        public void ReplayRange_ReturnsOwnedCopy()
        {
            var store = new DecisionTraceStore();
            store.Append(Frame(1));
            store.Append(Frame(2));
            store.Append(Frame(3));

            var range = DecisionReplay.ReplayRange(store, 0, 2);

            Assert.AreEqual(3, range.Length);
            Assert.AreEqual(1, range[0].DecisionStep);
            Assert.AreEqual(2, range[1].DecisionStep);
            Assert.AreEqual(3, range[2].DecisionStep);
            Assert.AreEqual(0, DecisionReplay.ReplayRange(store, 2, 0).Length);
            Assert.AreEqual(0, DecisionReplay.ReplayRange(store, 5, 7).Length);
            Assert.AreEqual(0, DecisionReplay.ReplayRange(null, 0, 1).Length);
        }

        [Test]
        public void DeterministicOrdering_AppendOrder()
        {
            var store = new DecisionTraceStore();
            store.Append(Frame(1));
            store.Append(Frame(2));
            store.Append(Frame(3));

            var all = store.Enumerate();

            Assert.AreEqual(3, all.Length);
            Assert.AreEqual(1, all[0].DecisionStep);
            Assert.AreEqual(2, all[1].DecisionStep);
            Assert.AreEqual(3, all[2].DecisionStep);

            var other = new DecisionTraceStore();
            other.Append(Frame(1));
            other.Append(Frame(2));
            other.Append(Frame(3));
            var otherAll = other.Enumerate();

            Assert.AreEqual(all.Length, otherAll.Length);
            for (var i = 0; i < all.Length; i++)
                Assert.AreEqual(all[i].DecisionStep, otherAll[i].DecisionStep);
        }

        [Test]
        public void ImmutableFrame_OwnsReasons()
        {
            var reasons = new[]
            {
                new DecisionReason("Assessment", "No target detected"),
                new DecisionReason("Mission", "AvoidHazard"),
            };

            var frame = Frame(1, reasons);
            reasons[0] = new DecisionReason("Assessment", "Mutated");

            Assert.AreEqual(2, frame.Reasons.Length);
            Assert.AreEqual("No target detected", frame.Reasons[0].Text);
            Assert.AreEqual("AvoidHazard", frame.Reasons[1].Text);
            Assert.IsTrue(frame.IsValid);
            Assert.IsTrue(DecisionReplayValidator.FrameValid(frame));
        }

        [Test]
        public void BehaviourSynchronization()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var frame = engine.LastTraceFrame;

            Assert.IsNotNull(frame);
            Assert.AreEqual(BehaviourState.Avoid, frame.Behaviour);
            Assert.AreEqual(engine.LastSnapshot.Behaviour, frame.Behaviour);
            Assert.AreEqual(frame.Explanation.Behaviour, frame.Behaviour);
            Assert.IsTrue(DecisionReplayValidator.BehaviourMatchesTrace(frame));
        }

        [Test]
        public void MissionSynchronization()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var frame = engine.LastTraceFrame;

            Assert.AreEqual(MissionTaskState.AvoidHazard, frame.Mission.State);
            Assert.AreEqual(engine.LastSnapshot.Mission, frame.Mission);
            Assert.AreEqual(engine.LastSnapshot.Mission, frame.Explanation.Mission);
            Assert.IsTrue(DecisionReplayValidator.MissionMatchesTrace(frame));
        }

        [Test]
        public void OptimizationSynchronization()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var frame = engine.LastTraceFrame;

            Assert.AreEqual(engine.LastSnapshot.ExecutionProfile.ExecutionConfidence, frame.OptimizationConfidence, 1e-6f);
            Assert.AreEqual(frame.OptimizationProfile.ExecutionConfidence, frame.OptimizationConfidence, 1e-6f);
            Assert.AreEqual(frame.Explanation.OptimizationProfile.ExecutionConfidence, frame.OptimizationConfidence, 1e-6f);
            Assert.Greater(frame.OptimizationConfidence, 0f);
            Assert.IsTrue(DecisionReplayValidator.OptimizationSynchronized(frame));
        }

        [Test]
        public void CommandSynchronization()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var frame = engine.LastTraceFrame;

            Assert.IsFalse(frame.Command.IsIdle);
            Assert.AreEqual(engine.LastSnapshot.Command, frame.Command);
            Assert.AreEqual(engine.GetDiagnostics().LastCommand, frame.Command);
            Assert.AreEqual(frame.Explanation.Command, frame.Command);
            Assert.IsTrue(DecisionReplayValidator.CommandSynchronized(frame));
        }

        [Test]
        public void KnowledgeSynchronization()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var frame = engine.LastTraceFrame;

            Assert.AreEqual(engine.LastSnapshot.Knowledge.Count, frame.KnowledgeCount);
            Assert.AreEqual(engine.KnowledgeStore.Count, frame.KnowledgeCount);
            Assert.AreEqual(engine.GetDiagnostics().KnowledgeRecordCount, frame.KnowledgeCount);
            Assert.Greater(frame.KnowledgeCount, 0);
            Assert.IsTrue(DecisionReplayValidator.KnowledgeSynchronized(frame));
        }

        [Test]
        public void MemorySynchronization()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var frame = engine.LastTraceFrame;

            Assert.AreEqual(engine.LastSnapshot.Memory.Count, frame.MemoryCount);
            Assert.AreEqual(engine.LastSnapshot.Memory.LastBehaviour, frame.Explanation.Memory.LastBehaviour);
            Assert.IsTrue(frame.MemoryCount >= 0);
        }

        [Test]
        public void ExplanationSynchronization()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var frame = engine.LastTraceFrame;

            Assert.IsNotNull(frame);
            Assert.AreEqual(engine.LastExplanation.DecisionStep, frame.Explanation.DecisionStep);
            Assert.AreEqual(engine.LastExplanation.DecisionTimestamp, frame.Explanation.DecisionTimestamp, 1e-6f);
            Assert.AreEqual(engine.LastExplanation.Winning, frame.Explanation.Winning);
            Assert.AreEqual(engine.LastExplanation.Behaviour, frame.Explanation.Behaviour);
            Assert.AreEqual(engine.LastExplanation.Mission, frame.Explanation.Mission);
            Assert.AreEqual(engine.LastExplanation.Executor, frame.Explanation.Executor);
            AssertExplanationsEqual(engine.LastExplanation, frame.Explanation);
        }

        [Test]
        public void DiagnosticsSynchronization()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var frame = engine.LastTraceFrame;

            Assert.AreEqual(engine.GetDiagnostics().StepCount, frame.Diagnostics.StepCount);
            Assert.AreEqual(engine.GetDiagnostics().LastBehaviour, frame.Diagnostics.LastBehaviour);
            Assert.AreEqual(engine.GetDiagnostics().LastCommand, frame.Diagnostics.LastCommand);
            Assert.AreEqual(engine.GetDiagnostics().CandidateCount, frame.Diagnostics.CandidateCount);
            Assert.AreEqual(engine.GetDiagnostics().KnowledgeRecordCount, frame.Diagnostics.KnowledgeRecordCount);
            Assert.AreEqual(engine.GetDiagnostics().DecisionTimestamp, frame.Diagnostics.DecisionTimestamp, 1e-6f);
            AssertExplanationsEqual(engine.GetDiagnostics().LastExplanation, frame.Diagnostics.LastExplanation);
            Assert.AreSame(frame, engine.LastSnapshot.Trace);
            Assert.AreSame(frame, engine.LastSnapshot.Diagnostics.LastTraceFrame);
            Assert.AreSame(frame, engine.GetDiagnostics().LastTraceFrame);
            Assert.IsTrue(DecisionReplayValidator.DiagnosticsSynchronized(frame));
        }

        [Test]
        public void ReplayValidator_AcceptsValid()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var frame = engine.LastTraceFrame;

            Assert.IsTrue(DecisionReplayValidator.FrameValid(frame));
            Assert.IsTrue(DecisionReplayValidator.BehaviourMatchesTrace(frame));
            Assert.IsTrue(DecisionReplayValidator.MissionMatchesTrace(frame));
            Assert.IsTrue(DecisionReplayValidator.ExplanationMatchesTrace(frame));
            Assert.IsTrue(DecisionReplayValidator.DiagnosticsSynchronized(frame));
            Assert.IsTrue(DecisionReplayValidator.CommandSynchronized(frame));
            Assert.IsTrue(DecisionReplayValidator.KnowledgeSynchronized(frame));
            Assert.IsTrue(DecisionReplayValidator.OptimizationSynchronized(frame));
            Assert.IsTrue(DecisionReplayValidator.IsValid(frame));

            var replayed = DecisionReplay.ReplayLatest(engine.TraceStore);
            Assert.IsTrue(DecisionReplayValidator.IsValid(replayed));
        }

        [Test]
        public void ReplayValidator_RejectsMismatch()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            var valid = engine.LastTraceFrame;
            Assert.IsTrue(DecisionReplayValidator.IsValid(valid));

            var wrongBehaviour = AlteredFrame(valid, behaviour: BehaviourState.Idle);
            Assert.IsFalse(DecisionReplayValidator.BehaviourMatchesTrace(wrongBehaviour));
            Assert.IsFalse(DecisionReplayValidator.IsValid(wrongBehaviour));

            var wrongMission = AlteredFrame(valid, mission: Task(MissionTaskState.SearchArea));
            Assert.IsFalse(DecisionReplayValidator.MissionMatchesTrace(wrongMission));
            Assert.IsFalse(DecisionReplayValidator.IsValid(wrongMission));

            var wrongCommand = AlteredFrame(valid, command: DroneCommand.Idle);
            Assert.IsFalse(DecisionReplayValidator.CommandSynchronized(wrongCommand));
            Assert.IsFalse(DecisionReplayValidator.IsValid(wrongCommand));

            var wrongCount = AlteredFrame(valid, knowledgeCount: valid.KnowledgeCount + 1);
            Assert.IsFalse(DecisionReplayValidator.KnowledgeSynchronized(wrongCount));
            Assert.IsFalse(DecisionReplayValidator.IsValid(wrongCount));

            var wrongConfidence = AlteredFrame(valid, optimizationConfidence: 0f);
            Assert.IsFalse(DecisionReplayValidator.OptimizationSynchronized(wrongConfidence));
            Assert.IsFalse(DecisionReplayValidator.IsValid(wrongConfidence));

            var wrongDiagnostics = AlteredFrame(valid, diagnostics: DecisionDiagnostics.Empty);
            Assert.IsFalse(DecisionReplayValidator.DiagnosticsSynchronized(wrongDiagnostics));
            Assert.IsFalse(DecisionReplayValidator.IsValid(wrongDiagnostics));

            var wrongStep = AlteredFrame(valid, step: valid.DecisionStep + 1);
            Assert.IsFalse(DecisionReplayValidator.ExplanationMatchesTrace(wrongStep));
            Assert.IsFalse(DecisionReplayValidator.IsValid(wrongStep));
        }

        [Test]
        public void EmptyTrace_EmptyStore()
        {
            var store = new DecisionTraceStore();
            Assert.AreEqual(0, store.Count);
            Assert.IsTrue(ReferenceEquals(DecisionTraceFrame.Empty, store.Latest));
            Assert.IsTrue(ReferenceEquals(DecisionTraceFrame.Empty, store.Get(0)));
            Assert.IsTrue(ReferenceEquals(DecisionTraceFrame.Empty, DecisionReplay.ReplayLatest(store)));
            Assert.AreEqual(0, DecisionReplay.ReplayFrame(store, 0).DecisionStep);
            Assert.AreEqual(0, store.Enumerate().Length);
            Assert.IsTrue(DecisionReplayValidator.IsValid(DecisionTraceFrame.Empty));

            var engine = CreateEngine();
            Assert.IsTrue(ReferenceEquals(DecisionTraceFrame.Empty, engine.LastTraceFrame));
            Assert.AreEqual(0, engine.TraceStore.Count);
        }

        [Test]
        public void Reset_ClearsTraceStore()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(NeutralReading());

            Assert.AreEqual(2, engine.TraceStore.Count);
            Assert.AreEqual(2, engine.LastTraceFrame.DecisionStep);

            engine.Reset();

            Assert.AreEqual(0, engine.TraceStore.Count);
            Assert.IsTrue(ReferenceEquals(DecisionTraceFrame.Empty, engine.LastTraceFrame));
            Assert.IsTrue(ReferenceEquals(DecisionTraceFrame.Empty, engine.LastSnapshot.Trace));
        }

        [Test]
        public void SameSnapshot_SameTrace()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            var other = CreateEngine();
            other.Decide(ObstacleReading(0.9f));

            AssertFramesEqual(engine.LastTraceFrame, other.LastTraceFrame);

            var builder = new DecisionTraceBuilder();
            var a = builder.Build(engine.LastSnapshot, engine.LastExplanation);
            var b = builder.Build(other.LastSnapshot, other.LastExplanation);
            AssertFramesEqual(a, b);
        }
    }
}
