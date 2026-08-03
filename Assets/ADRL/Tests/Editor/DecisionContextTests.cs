namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 8.8 Autonomous Decision Context Framework: the
    /// <see cref="DecisionContextBuilder"/> composes one immutable
    /// <see cref="DecisionContextSnapshot"/> from every subsystem output of a step,
    /// the <see cref="DecisionRuntimeState"/> captures the pure runtime metadata, the
    /// <see cref="DecisionContextValidator"/> confirms a built snapshot is complete
    /// and internally consistent, and the <c>DecisionEngine</c> captures the whole
    /// chain per step with synchronized diagnostics and exposes the last snapshot.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionContextTests
    {
        private static SituationSnapshot HazardAssessment(float proximity) => new(false, 0f, 0f, proximity, 0f, true);

        private static MissionTask Task(MissionTaskState state) => new(state, 1f, MissionTaskState.SearchArea, true);

        private static ISensorReading ObstacleReading(float proximity) => new SensorReading(new[]
        {
            0f, 0f, 0f, 0f, 0f, 0f, proximity, 0f, 0f, 0f, 0f, 0f,
        });

        private static ISensorReading VictimReading(float proximity) => new SensorReading(new[]
        {
            proximity, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
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

        private static BehaviourState BehaviourFor(MissionTaskState state)
        {
            switch (state)
            {
                case MissionTaskState.RescueVictim:
                case MissionTaskState.InvestigateTarget:
                    return BehaviourState.Approach;
                case MissionTaskState.AvoidHazard:
                    return BehaviourState.Avoid;
                case MissionTaskState.SearchArea:
                case MissionTaskState.ResumeSearch:
                    return BehaviourState.Search;
                default:
                    return BehaviourState.Idle;
            }
        }

        /// <summary>
        /// Builds a valid, self-consistent snapshot through the real builder from
        /// fixed inputs, so builder-level tests exercise the exact composition path.
        /// </summary>
        private static DecisionContextSnapshot BuildManual(
            IDecisionContextBuilder builder,
            MissionTaskState state,
            float proximity)
        {
            var mission = Task(state);
            var winning = new TaskPriority(mission, 1f, 1f, true);
            var candidates = new[]
            {
                new TaskCandidate(mission, CandidateOrigin.CurrentPerception, proximity, 1f),
            };
            var behaviour = BehaviourFor(state);
            var runtime = new DecisionRuntimeState(
                decisionStep: 1,
                episodeStep: 1,
                decisionTimestamp: 0f,
                currentBehaviour: behaviour,
                currentMission: mission,
                lastCommand: DroneCommand.Idle,
                currentExecutor: "TestExecutor",
                currentWinner: winning);
            var diagnostics = DecisionDiagnostics.From(runtime, HazardAssessment(proximity), candidates.Length);

            return builder.Build(
                HazardAssessment(proximity),
                BehaviourMemory.Empty,
                mission,
                candidates,
                winning,
                behaviour,
                DroneCommand.Idle,
                diagnostics,
                runtime);
        }

        private static void AssertSnapshotsEqual(DecisionContextSnapshot a, DecisionContextSnapshot b)
        {
            Assert.AreEqual(a.Assessment, b.Assessment);
            Assert.AreSame(a.Memory, b.Memory);
            Assert.AreEqual(a.Mission, b.Mission);
            Assert.AreEqual(a.Candidates.Length, b.Candidates.Length);
            for (var i = 0; i < a.Candidates.Length; i++)
            {
                Assert.AreEqual(a.Candidates[i].Task.State, b.Candidates[i].Task.State);
                Assert.AreEqual(a.Candidates[i].Proximity, b.Candidates[i].Proximity, 1e-6f);
            }

            Assert.AreEqual(a.Winning, b.Winning);
            Assert.AreEqual(a.Behaviour, b.Behaviour);
            Assert.AreEqual(a.Command.IsIdle, b.Command.IsIdle);
            Assert.AreEqual(a.Diagnostics.StepCount, b.Diagnostics.StepCount);
            Assert.AreEqual(a.RuntimeState.DecisionStep, b.RuntimeState.DecisionStep);
        }

        [Test]
        public void Snapshot_BuildsCorrectly()
        {
            var engine = CreateEngine();
            var result = engine.Decide(ObstacleReading(0.9f));
            var snapshot = engine.LastSnapshot;

            Assert.IsTrue(snapshot.Assessment.IsValid);
            Assert.AreEqual(MissionTaskState.AvoidHazard, snapshot.Mission.State);
            Assert.AreEqual(MissionTaskState.AvoidHazard, snapshot.Winning.Task.State);
            Assert.AreEqual(BehaviourState.Avoid, snapshot.Behaviour);
            Assert.AreEqual(1, snapshot.Candidates.Length);
            Assert.IsFalse(snapshot.Command.IsIdle);
            Assert.AreEqual(result.Command.MoveDirection, snapshot.Command.MoveDirection);
            Assert.AreEqual(1, snapshot.Diagnostics.StepCount);
            Assert.AreEqual(1, snapshot.RuntimeState.DecisionStep);
            Assert.AreSame(snapshot.Memory, engine.MemoryService.Memory);
            Assert.IsTrue(DecisionContextValidator.IsValid(snapshot));
        }

        [Test]
        public void Snapshot_IsImmutable()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var first = engine.LastSnapshot;
            engine.Decide(NeutralReading());

            Assert.AreEqual(1, first.Diagnostics.StepCount);
            Assert.AreEqual(1, first.RuntimeState.DecisionStep);
            Assert.AreEqual(1, first.Candidates.Length);
            Assert.AreEqual(MissionTaskState.AvoidHazard, first.Winning.Task.State);
            Assert.AreEqual(2, engine.LastSnapshot.Diagnostics.StepCount);

            var candidates = new[]
            {
                new TaskCandidate(Task(MissionTaskState.SearchArea), CandidateOrigin.Continuity, 0f, 1f),
            };
            var builder = new DecisionContextBuilder();
            var snapshot = builder.Build(
                HazardAssessment(0.9f),
                BehaviourMemory.Empty,
                Task(MissionTaskState.AvoidHazard),
                candidates,
                TaskPriority.Invalid,
                BehaviourState.Idle,
                DroneCommand.Idle,
                DecisionDiagnostics.Empty,
                DecisionRuntimeState.Empty);

            candidates[0] = default;

            Assert.AreEqual(1, snapshot.Candidates.Length);
            Assert.AreEqual(MissionTaskState.SearchArea, snapshot.Candidates[0].Task.State);
        }

        [Test]
        public void Builder_IsDeterministic()
        {
            var builder = new DecisionContextBuilder();

            var a = BuildManual(builder, MissionTaskState.AvoidHazard, 0.9f);
            var b = BuildManual(builder, MissionTaskState.AvoidHazard, 0.9f);

            AssertSnapshotsEqual(a, b);
        }

        [Test]
        public void RuntimeMetadata_Correct()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            engine.Decide(VictimReading(0.9f));

            var state = engine.LastSnapshot.RuntimeState;

            Assert.AreEqual(2, state.DecisionStep);
            Assert.AreEqual(2, state.EpisodeStep);
            Assert.AreEqual(1f, state.DecisionTimestamp, 1e-6f);
            Assert.AreEqual(BehaviourState.Approach, state.CurrentBehaviour);
            Assert.AreEqual(MissionTaskState.RescueVictim, state.CurrentMission.State);
            Assert.AreEqual(MissionTaskState.RescueVictim, state.CurrentWinner.Task.State);
            Assert.AreEqual("ApproachExecutor", state.CurrentExecutor);
            Assert.IsFalse(state.LastCommand.IsIdle);
        }

        [Test]
        public void CandidateCount_MatchesDiagnostics()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var snapshot = engine.LastSnapshot;

            Assert.AreEqual(1, snapshot.Candidates.Length);
            Assert.AreEqual(snapshot.Candidates.Length, snapshot.Diagnostics.CandidateCount);
            Assert.IsTrue(DecisionContextValidator.CandidateCountValid(snapshot));
        }

        [Test]
        public void Behaviour_MatchesWinner()
        {
            var engine = CreateEngine();
            engine.Decide(VictimReading(0.9f));

            var snapshot = engine.LastSnapshot;

            Assert.AreEqual(MissionTaskState.RescueVictim, snapshot.Winning.Task.State);
            Assert.AreEqual(BehaviourState.Approach, snapshot.Behaviour);
            Assert.IsTrue(DecisionContextValidator.BehaviourMatchesWinner(snapshot));
        }

        [Test]
        public void Mission_MatchesDiagnostics()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var snapshot = engine.LastSnapshot;

            Assert.AreEqual(snapshot.Mission, snapshot.Diagnostics.LastMissionTask);
            Assert.AreEqual(snapshot.Mission.State, snapshot.Diagnostics.LastMissionTask.State);
            Assert.IsTrue(DecisionContextValidator.DiagnosticsSynchronized(snapshot));
        }

        [Test]
        public void Command_MatchesExecutor()
        {
            var engine = CreateEngine();
            var result = engine.Decide(ObstacleReading(0.9f));

            var snapshot = engine.LastSnapshot;

            Assert.AreEqual(result.Command.MoveDirection, snapshot.Command.MoveDirection);
            Assert.AreEqual(result.Command.MoveDirection, snapshot.RuntimeState.LastCommand.MoveDirection);
            Assert.AreEqual(snapshot.Command.MoveDirection, snapshot.Diagnostics.LastCommand.MoveDirection);
            Assert.IsFalse(snapshot.Command.IsIdle);
            Assert.IsTrue(DecisionContextValidator.CommandValid(snapshot));
        }

        [Test]
        public void Validator_RejectsInvalidSnapshots()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            var valid = engine.LastSnapshot;
            Assert.IsTrue(DecisionContextValidator.IsValid(valid));

            var badCount = new DecisionDiagnostics(
                valid.Diagnostics.StepCount,
                valid.Diagnostics.LastBehaviour,
                valid.Diagnostics.LastAssessment,
                valid.Diagnostics.LastMissionTask,
                valid.Diagnostics.LastWinning,
                valid.Diagnostics.SelectedExecutor,
                valid.Diagnostics.LastCommand,
                valid.Diagnostics.DecisionTimestamp,
                valid.Diagnostics.CandidateCount + 1);
            var mismatchedCount = new DecisionContextSnapshot(
                valid.Assessment, valid.Memory, valid.Mission, valid.Candidates, valid.Winning,
                valid.Behaviour, valid.Command, badCount, valid.RuntimeState);
            Assert.IsFalse(DecisionContextValidator.CandidateCountValid(mismatchedCount));
            Assert.IsFalse(DecisionContextValidator.IsValid(mismatchedCount));

            var wrongBehaviour = new DecisionContextSnapshot(
                valid.Assessment, valid.Memory, valid.Mission, valid.Candidates, valid.Winning,
                BehaviourState.Idle, valid.Command, valid.Diagnostics, valid.RuntimeState);
            Assert.IsFalse(DecisionContextValidator.BehaviourMatchesWinner(wrongBehaviour));
            Assert.IsFalse(DecisionContextValidator.IsValid(wrongBehaviour));

            var nullCandidates = new DecisionContextSnapshot(
                valid.Assessment, valid.Memory, valid.Mission, null, valid.Winning,
                valid.Behaviour, valid.Command, valid.Diagnostics, valid.RuntimeState);
            Assert.IsFalse(DecisionContextValidator.HasNoNullReferences(nullCandidates));
            Assert.IsFalse(DecisionContextValidator.IsValid(nullCandidates));
        }

        [Test]
        public void EmptySnapshot_Valid()
        {
            var empty = DecisionContextSnapshot.Empty;

            Assert.IsTrue(DecisionContextValidator.IsComplete(empty));
            Assert.IsTrue(DecisionContextValidator.HasNoNullReferences(empty));
            Assert.IsTrue(DecisionContextValidator.MissionValid(empty));
            Assert.IsTrue(DecisionContextValidator.BehaviourMatchesWinner(empty));
            Assert.IsTrue(DecisionContextValidator.DiagnosticsSynchronized(empty));
            Assert.IsTrue(DecisionContextValidator.CandidateCountValid(empty));
            Assert.IsTrue(DecisionContextValidator.CommandValid(empty));
            Assert.IsTrue(DecisionContextValidator.IsValid(empty));

            var engine = CreateEngine();
            Assert.AreEqual(DecisionContextSnapshot.Empty, engine.LastSnapshot);
            Assert.IsTrue(DecisionContextValidator.IsValid(engine.LastSnapshot));
        }

        [Test]
        public void RepeatedBuilds_Identical()
        {
            var builder = new DecisionContextBuilder();

            var a = BuildManual(builder, MissionTaskState.RescueVictim, 0.9f);
            var b = BuildManual(builder, MissionTaskState.RescueVictim, 0.9f);

            AssertSnapshotsEqual(a, b);
            Assert.AreEqual(a.Candidates.Length, b.Candidates.Length);
            for (var i = 0; i < a.Candidates.Length; i++)
            {
                Assert.AreEqual(a.Candidates[i].Task.State, b.Candidates[i].Task.State);
                Assert.AreEqual(a.Candidates[i].Proximity, b.Candidates[i].Proximity, 1e-6f);
            }
        }

        [Test]
        public void NoHiddenState()
        {
            var builder = new DecisionContextBuilder();

            var avoid = BuildManual(builder, MissionTaskState.AvoidHazard, 0.9f);
            var rescue = BuildManual(builder, MissionTaskState.RescueVictim, 0.9f);

            Assert.AreEqual(MissionTaskState.AvoidHazard, avoid.Mission.State);
            Assert.AreEqual(MissionTaskState.RescueVictim, rescue.Mission.State);
            Assert.AreEqual(BehaviourState.Avoid, avoid.Behaviour);
            Assert.AreEqual(BehaviourState.Approach, rescue.Behaviour);

            var avoidAgain = BuildManual(builder, MissionTaskState.AvoidHazard, 0.9f);
            AssertSnapshotsEqual(avoid, avoidAgain);
        }

        [Test]
        public void NullProtection()
        {
            var builder = new DecisionContextBuilder();

            var snapshot = builder.Build(
                SituationSnapshot.Invalid,
                BehaviourMemory.Empty,
                MissionTask.Invalid,
                null,
                TaskPriority.Invalid,
                BehaviourState.Idle,
                DroneCommand.Idle,
                DecisionDiagnostics.Empty,
                DecisionRuntimeState.Empty);

            Assert.AreEqual(0, snapshot.Candidates.Length);
            Assert.AreEqual(DecisionContextSnapshot.Empty, snapshot);
            Assert.IsTrue(DecisionContextValidator.IsValid(snapshot));
        }

        [Test]
        public void Reset_ClearsRuntimeState()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            Assert.AreEqual(1, engine.LastSnapshot.RuntimeState.DecisionStep);
            Assert.AreEqual(1, engine.LastSnapshot.Diagnostics.StepCount);

            engine.Reset();

            Assert.AreEqual(0, engine.LastSnapshot.RuntimeState.DecisionStep);
            Assert.AreEqual(0, engine.LastSnapshot.Diagnostics.StepCount);
            Assert.AreEqual(DecisionContextSnapshot.Empty, engine.LastSnapshot);
            Assert.AreEqual(DecisionDiagnostics.Empty, engine.GetDiagnostics());
            Assert.IsTrue(DecisionContextValidator.IsValid(engine.LastSnapshot));
        }

        [Test]
        public void BackwardCompatibility()
        {
            var engine = CreateEngine();
            var result = engine.Decide(ObstacleReading(0.9f));

            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual(1, diagnostics.StepCount);
            Assert.AreEqual(MissionTaskState.AvoidHazard, diagnostics.LastWinning.Task.State);
            Assert.AreEqual(MissionTaskState.AvoidHazard, diagnostics.LastMissionTask.State);
            Assert.AreEqual("AvoidExecutor", diagnostics.SelectedExecutor);
            Assert.AreEqual(1, diagnostics.CandidateCount);
            Assert.AreEqual(BehaviourState.Avoid, result.Behaviour);
            Assert.AreEqual(BehaviourState.Avoid, diagnostics.LastBehaviour);
            Assert.AreEqual(result.Command.MoveDirection, diagnostics.LastCommand.MoveDirection);
            Assert.AreEqual(result.Command.Yaw, diagnostics.LastCommand.Yaw);
            Assert.AreEqual(0f, diagnostics.DecisionTimestamp, 1e-6f);
        }
    }
}
