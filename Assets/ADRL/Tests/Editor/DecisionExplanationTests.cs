namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Explainability;
    using ADRL.AI.Decision.Knowledge;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 9.1 Autonomous Decision Explainability Framework: the
    /// <see cref="DecisionExplanationBuilder"/> is the single owner of explanation
    /// composition (a pure, stateless, deterministic function of a built
    /// <see cref="DecisionContextSnapshot"/>), the <see cref="DecisionExplanation"/>
    /// is the immutable, complete explanation of one decision step, the
    /// <see cref="DecisionExplanationFormatter"/> renders it deterministically into
    /// human-readable text, the <see cref="DecisionExplanationValidator"/> confirms
    /// the explanation is complete and internally consistent, and the
    /// <c>DecisionEngine</c> builds and exposes the last explanation without any
    /// behavioural change. Explainability is strictly observational.
    /// </summary>
    [TestFixture]
    internal sealed class DecisionExplanationTests
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
        /// Returns a copy of a source explanation with the given members overridden,
        /// so validator-rejection tests can build invalid explanations concisely.
        /// </summary>
        private static DecisionExplanation Altered(
            DecisionExplanation source,
            TaskPriority[] scored = null,
            TaskPriority? winning = null,
            BehaviourState? behaviour = null,
            string executor = null,
            int? step = null,
            float? timestamp = null)
        {
            return new DecisionExplanation(
                source.Assessment,
                source.Knowledge,
                source.Memory,
                source.Mission,
                source.CandidateTasks,
                scored ?? source.ScoredCandidates,
                winning ?? source.Winning,
                behaviour ?? source.Behaviour,
                executor ?? source.Executor,
                source.OptimizationProfile,
                source.Command,
                timestamp ?? source.DecisionTimestamp,
                step ?? source.DecisionStep,
                source.Reasons);
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
            Assert.AreEqual(a.Memory.LastVictimSeen, b.Memory.LastVictimSeen);
            Assert.AreEqual(a.Memory.LastObstacleSeen, b.Memory.LastObstacleSeen);
            Assert.AreEqual(a.Memory.RecordCount, b.Memory.RecordCount);
            Assert.AreEqual(a.Mission, b.Mission);
            Assert.AreEqual(a.CandidateTasks.Length, b.CandidateTasks.Length);
            for (var i = 0; i < a.CandidateTasks.Length; i++)
                Assert.AreEqual(a.CandidateTasks[i], b.CandidateTasks[i]);
            Assert.AreEqual(a.ScoredCandidates.Length, b.ScoredCandidates.Length);
            for (var i = 0; i < a.ScoredCandidates.Length; i++)
                Assert.AreEqual(a.ScoredCandidates[i], b.ScoredCandidates[i]);
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
        public void Build_ExplanationFromSnapshot()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;

            Assert.IsTrue(explanation.IsValid);
            Assert.AreEqual(1, explanation.DecisionStep);
            Assert.AreEqual(MissionTaskState.AvoidHazard, explanation.Mission.State);
            Assert.AreEqual(BehaviourState.Avoid, explanation.Behaviour);
            Assert.AreEqual("AvoidExecutor", explanation.Executor);
            Assert.IsFalse(explanation.Command.IsIdle);
            Assert.AreEqual(engine.GetDiagnostics().DecisionTimestamp, explanation.DecisionTimestamp, 1e-6f);
            Assert.AreEqual(engine.LastSnapshot.Diagnostics.DecisionTimestamp, explanation.DecisionTimestamp, 1e-6f);
            Assert.Greater(explanation.Reasons.Length, 0);
            Assert.IsTrue(DecisionExplanationValidator.IsValid(explanation));
        }

        [Test]
        public void Empty_ExplanationValid()
        {
            var engine = CreateEngine();

            Assert.AreEqual(DecisionExplanation.Empty, engine.LastExplanation);
            Assert.AreEqual(0, engine.LastExplanation.DecisionStep);
            Assert.IsTrue(engine.LastExplanation.IsValid);
            Assert.IsTrue(DecisionExplanationValidator.IsValid(engine.LastExplanation));
            Assert.IsTrue(DecisionExplanationValidator.IsValid(DecisionExplanation.Empty));
        }

        [Test]
        public void Explanation_IsImmutable()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var first = engine.LastExplanation;
            engine.Decide(NeutralReading());

            Assert.AreEqual(1, first.DecisionStep);
            Assert.AreEqual(2, engine.LastExplanation.DecisionStep);
            Assert.AreEqual(MissionTaskState.AvoidHazard, first.Mission.State);

            var assessment = new SituationSnapshot(true, 0.9f, 0f, 0f, 0f, true);
            var mission = Task(MissionTaskState.RescueVictim);
            var winning = new TaskPriority(mission, 0.9f, 1f, true);
            var candidates = new[] { new TaskCandidate(mission, CandidateOrigin.CurrentPerception, 0.9f, 1f) };
            var scored = new[] { winning };
            var runtime = new DecisionRuntimeState(
                decisionStep: 1,
                episodeStep: 1,
                decisionTimestamp: 0f,
                currentBehaviour: BehaviourState.Approach,
                currentMission: mission,
                lastCommand: DroneCommand.Idle,
                currentExecutor: "ApproachExecutor",
                currentWinner: winning);
            var diagnostics = DecisionDiagnostics.From(runtime, assessment, candidates.Length);
            var snapshot = new DecisionContextBuilder()
                .Build(assessment, BehaviourMemory.Empty, mission, candidates, winning,
                    BehaviourState.Approach, DroneCommand.Idle, diagnostics, runtime)
                .WithScoredCandidates(scored);

            var explanation = new DecisionExplanationBuilder().Build(snapshot);

            snapshot.Candidates[0] = default;
            snapshot.ScoredCandidates[0] = default;

            Assert.AreEqual(1, explanation.CandidateTasks.Length);
            Assert.AreEqual(MissionTaskState.RescueVictim, explanation.CandidateTasks[0]);
            Assert.AreEqual(1, explanation.ScoredCandidates.Length);
            Assert.IsTrue(explanation.ScoredCandidates[0].IsValid);
            Assert.AreEqual(0.9f, explanation.ScoredCandidates[0].PriorityScore, 1e-6f);
        }

        [Test]
        public void Assessment_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;

            Assert.AreEqual(engine.LastSnapshot.Assessment, explanation.Assessment);
            Assert.IsFalse(explanation.Assessment.TargetDetected);
            Assert.AreEqual(0.9f, explanation.Assessment.ObstacleProximity, 1e-6f);
        }

        [Test]
        public void Knowledge_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;

            Assert.AreEqual(engine.LastSnapshot.Knowledge.Count, explanation.Knowledge.RecordCount);
            Assert.AreEqual(engine.KnowledgeStore.CountOf(KnowledgeType.Victim), explanation.Knowledge.KnownVictims);
            Assert.AreEqual(engine.KnowledgeStore.CountOf(KnowledgeType.Hazard), explanation.Knowledge.KnownHazards);
            Assert.AreEqual(engine.KnowledgeStore.CountOf(KnowledgeType.Obstacle), explanation.Knowledge.KnownObstacles);
            Assert.Greater(explanation.Knowledge.KnownHazards, 0);
            Assert.AreEqual(engine.LastSnapshot.Diagnostics.NearestHazardDistance,
                explanation.Knowledge.NearestHazardDistance, 1e-6f);
        }

        [Test]
        public void Memory_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;
            var memory = engine.LastSnapshot.Memory;

            Assert.AreEqual(memory.LastBehaviour, explanation.Memory.LastBehaviour);
            Assert.AreEqual(memory.Count, explanation.Memory.RecordCount);
            Assert.AreEqual(memory.LastObstacleSeen, explanation.Memory.LastObstacleSeen);
            Assert.AreEqual(memory.LastVictimSeen, explanation.Memory.LastVictimSeen);
        }

        [Test]
        public void Mission_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;

            Assert.AreEqual(engine.LastSnapshot.Mission, explanation.Mission);
            Assert.AreEqual(MissionTaskState.AvoidHazard, explanation.Mission.State);
        }

        [Test]
        public void Candidates_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;
            var snapshot = engine.LastSnapshot;

            Assert.AreEqual(snapshot.Candidates.Length, explanation.CandidateTasks.Length);
            for (var i = 0; i < snapshot.Candidates.Length; i++)
                Assert.AreEqual(snapshot.Candidates[i].Task.State, explanation.CandidateTasks[i]);
        }

        [Test]
        public void Priority_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;
            var snapshot = engine.LastSnapshot;

            Assert.AreEqual(snapshot.ScoredCandidates.Length, explanation.ScoredCandidates.Length);
            Assert.GreaterOrEqual(explanation.ScoredCandidates.Length, 1);
            for (var i = 0; i < snapshot.ScoredCandidates.Length; i++)
            {
                Assert.AreEqual(snapshot.ScoredCandidates[i].Task.State, explanation.ScoredCandidates[i].Task.State);
                Assert.AreEqual(snapshot.ScoredCandidates[i].PriorityScore,
                    explanation.ScoredCandidates[i].PriorityScore, 1e-6f);
            }
        }

        [Test]
        public void Winner_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;

            Assert.AreEqual(engine.LastSnapshot.Winning, explanation.Winning);
            Assert.AreEqual(MissionTaskState.AvoidHazard, explanation.Winning.Task.State);
        }

        [Test]
        public void Behaviour_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;

            Assert.AreEqual(engine.LastSnapshot.Behaviour, explanation.Behaviour);
            Assert.AreEqual(BehaviourState.Avoid, explanation.Behaviour);
        }

        [Test]
        public void Executor_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;

            Assert.AreEqual(engine.LastSnapshot.RuntimeState.CurrentExecutor, explanation.Executor);
            Assert.AreEqual("AvoidExecutor", explanation.Executor);
        }

        [Test]
        public void Optimization_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;
            var profile = engine.LastSnapshot.ExecutionProfile;

            Assert.AreEqual(profile.SpeedMultiplier, explanation.OptimizationProfile.SpeedMultiplier, 1e-6f);
            Assert.AreEqual(profile.TurnRateMultiplier, explanation.OptimizationProfile.TurnRateMultiplier, 1e-6f);
            Assert.AreEqual(profile.ExecutionConfidence, explanation.OptimizationProfile.ExecutionConfidence, 1e-6f);
            Assert.Greater(explanation.OptimizationProfile.ExecutionConfidence, 0f);
        }

        [Test]
        public void Command_Captured()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;

            Assert.AreEqual(engine.LastSnapshot.Command, explanation.Command);
            Assert.IsFalse(explanation.Command.IsIdle);
        }

        [Test]
        public void Formatter_Deterministic()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;
            var first = DecisionExplanationFormatter.Format(explanation);
            var second = DecisionExplanationFormatter.Format(explanation);

            Assert.AreEqual(first, second);

            var other = CreateEngine();
            other.Decide(ObstacleReading(0.9f));
            Assert.AreEqual(first, DecisionExplanationFormatter.Format(other.LastExplanation));
        }

        [Test]
        public void Formatter_ContainsSections()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var output = DecisionExplanationFormatter.Format(engine.LastExplanation);

            Assert.IsTrue(output.Contains("Assessment"));
            Assert.IsTrue(output.Contains("Knowledge"));
            Assert.IsTrue(output.Contains("Mission"));
            Assert.IsTrue(output.Contains("Candidates"));
            Assert.IsTrue(output.Contains("Priority"));
            Assert.IsTrue(output.Contains("Winner"));
            Assert.IsTrue(output.Contains("Behaviour"));
            Assert.IsTrue(output.Contains("Executor"));
            Assert.IsTrue(output.Contains("Optimization"));
            Assert.IsTrue(output.Contains("Command"));
            Assert.IsTrue(output.Contains("AvoidHazard"));
            Assert.IsTrue(output.Contains("AvoidExecutor"));
            Assert.IsTrue(output.Contains("Known Hazards: 1"));
        }

        [Test]
        public void Validator_AcceptsValidExplanation()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var explanation = engine.LastExplanation;

            Assert.IsTrue(DecisionExplanationValidator.IsComplete(explanation));
            Assert.IsTrue(DecisionExplanationValidator.HasNoNullReferences(explanation));
            Assert.IsTrue(DecisionExplanationValidator.MissionValid(explanation));
            Assert.IsTrue(DecisionExplanationValidator.WinnerMatchesBehaviour(explanation));
            Assert.IsTrue(DecisionExplanationValidator.BehaviourMatchesExecutor(explanation));
            Assert.IsTrue(DecisionExplanationValidator.ExecutorMatchesCommand(explanation));
            Assert.IsTrue(DecisionExplanationValidator.CandidateCountValid(explanation));
            Assert.IsTrue(DecisionExplanationValidator.TimestampValid(explanation));
            Assert.IsTrue(DecisionExplanationValidator.StepValid(explanation));
            Assert.IsTrue(DecisionExplanationValidator.IsValid(explanation));
        }

        [Test]
        public void Validator_RejectsInvalidExplanation()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            var valid = engine.LastExplanation;
            Assert.IsTrue(DecisionExplanationValidator.IsValid(valid));

            var wrongBehaviour = Altered(valid, behaviour: BehaviourState.Idle);
            Assert.IsFalse(DecisionExplanationValidator.WinnerMatchesBehaviour(wrongBehaviour));
            Assert.IsFalse(DecisionExplanationValidator.IsValid(wrongBehaviour));

            var wrongCount = Altered(valid, scored: System.Array.Empty<TaskPriority>());
            Assert.IsFalse(DecisionExplanationValidator.CandidateCountValid(wrongCount));
            Assert.IsFalse(DecisionExplanationValidator.IsValid(wrongCount));

            var wrongExecutor = Altered(valid, executor: "SearchExecutor");
            Assert.IsFalse(DecisionExplanationValidator.BehaviourMatchesExecutor(wrongExecutor));
            Assert.IsFalse(DecisionExplanationValidator.IsValid(wrongExecutor));

            var wrongCommand = Altered(valid, executor: "IdleExecutor");
            Assert.IsFalse(DecisionExplanationValidator.ExecutorMatchesCommand(wrongCommand));
            Assert.IsFalse(DecisionExplanationValidator.IsValid(wrongCommand));

            var badStep = Altered(valid, step: -1);
            Assert.IsFalse(DecisionExplanationValidator.StepValid(badStep));
            Assert.IsFalse(DecisionExplanationValidator.IsValid(badStep));

            var badTimestamp = Altered(valid, timestamp: -1f);
            Assert.IsFalse(DecisionExplanationValidator.TimestampValid(badTimestamp));
            Assert.IsFalse(DecisionExplanationValidator.IsValid(badTimestamp));
        }

        [Test]
        public void SameSnapshot_SameExplanation()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var snapshot = engine.LastSnapshot;
            var builder = new DecisionExplanationBuilder();

            var a = builder.Build(snapshot);
            var b = builder.Build(snapshot);

            AssertExplanationsEqual(a, b);
            Assert.AreEqual(DecisionExplanationFormatter.Format(a), DecisionExplanationFormatter.Format(b));
        }

        [Test]
        public void Explanation_DoesNotModifySnapshot()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            var snapshot = engine.LastSnapshot;

            var explanation = new DecisionExplanationBuilder().Build(snapshot);

            Assert.AreEqual(1, engine.LastSnapshot.RuntimeState.DecisionStep);
            Assert.AreEqual(snapshot.Mission, engine.LastSnapshot.Mission);
            Assert.AreEqual(snapshot.Winning, engine.LastSnapshot.Winning);
            Assert.AreEqual(snapshot.Behaviour, engine.LastSnapshot.Behaviour);
            Assert.AreEqual(snapshot.Command, engine.LastSnapshot.Command);
            Assert.AreEqual(snapshot.Candidates.Length, engine.LastSnapshot.Candidates.Length);
            for (var i = 0; i < snapshot.Candidates.Length; i++)
                Assert.AreEqual(snapshot.Candidates[i], engine.LastSnapshot.Candidates[i]);

            Assert.AreEqual(snapshot.ScoredCandidates.Length, engine.LastSnapshot.ScoredCandidates.Length);
            Assert.IsTrue(DecisionContextValidator.IsValid(engine.LastSnapshot));
            Assert.IsTrue(DecisionExplanationValidator.IsValid(explanation));
        }
    }
}
