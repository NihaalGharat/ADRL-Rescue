namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 8.7 Autonomous Task Prioritization Framework: the
    /// <see cref="TaskPrioritizer"/> arbitrates among simultaneously-valid
    /// objectives through the pure, deterministic <see cref="PriorityEvaluator"/>,
    /// scoring is fully configurable via <see cref="PriorityPolicy"/>, the winner
    /// is deterministic and stable, invalid candidates are ignored, and the
    /// <c>DecisionEngine</c> consumes the prioritizer between the mission
    /// coordinator and behaviour selection.
    /// </summary>
    [TestFixture]
    internal sealed class TaskPrioritizerTests
    {
        private static readonly MissionPolicy DefaultMission = MissionPolicy.Default;
        private static readonly PriorityPolicy DefaultPolicy = PriorityPolicy.Default;

        private static SituationSnapshot Hazard(float proximity) => new(false, 0f, 0f, proximity, 0f, true);

        private static SituationSnapshot Victim(float proximity) => new(true, proximity, 0f, 0f, 0f, true);

        private static SituationSnapshot HazardAndVictim(float proximity) => new(true, proximity, 0f, proximity, 0f, true);

        private static SituationSnapshot Neutral() => new(false, 0f, 0f, 0f, 0f, true);

        private static MissionTask Task(MissionTaskState state) => new(state, 1f, MissionTaskState.SearchArea, true);

        private static BehaviourMemory VictimInMind()
        {
            var service = new BehaviourMemoryService(new MemoryPolicy(10f, 6f, 0.25f, 5f, 4));
            service.Update(Victim(0.5f), 0f);
            return service.Memory;
        }

        private static TaskPriority[] Evaluate(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask current,
            PriorityPolicy policy)
        {
            var candidates = new TaskCandidateGenerator().Generate(assessment, memory, current, DefaultMission);
            return new PriorityEvaluator().Evaluate(candidates, current, policy);
        }

        private static TaskPriority CandidateFor(TaskPriority[] candidates, MissionTaskState state)
        {
            foreach (var candidate in candidates)
            {
                if (candidate.IsValid && candidate.Task.State == state)
                    return candidate;
            }

            return TaskPriority.Invalid;
        }

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

        [Test]
        public void HigherScore_Wins()
        {
            var prioritizer = new TaskPrioritizer(new PriorityEvaluator(), DefaultPolicy);

            var winner = prioritizer.Select(Hazard(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea), DefaultMission);

            Assert.AreEqual(MissionTaskState.AvoidHazard, winner.Task.State);
            Assert.IsTrue(winner.IsValid);
            Assert.Greater(winner.PriorityScore, 0f);
        }

        [Test]
        public void EqualScores_TieBreaksDeterministically()
        {
            var first = new TaskPriority(Task(MissionTaskState.SearchArea), 0.5f, 1f, true);
            var second = new TaskPriority(Task(MissionTaskState.ResumeSearch), 0.5f, 1f, true);
            var prioritizer = new TaskPrioritizer(new FixedEvaluator(first, second), DefaultPolicy);

            var a = prioritizer.Select(Neutral(), BehaviourMemory.Empty, Task(MissionTaskState.Idle), DefaultMission);
            var b = prioritizer.Select(Neutral(), BehaviourMemory.Empty, Task(MissionTaskState.Idle), DefaultMission);

            Assert.AreEqual(MissionTaskState.SearchArea, a.Task.State);
            Assert.AreEqual(a.Task.State, b.Task.State);
            Assert.AreEqual(a.PriorityScore, b.PriorityScore, 1e-6f);
        }

        [Test]
        public void VictimOutranksSearch()
        {
            var candidates = Evaluate(Victim(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea), DefaultPolicy);

            var rescue = CandidateFor(candidates, MissionTaskState.RescueVictim);
            var search = CandidateFor(candidates, MissionTaskState.SearchArea);

            Assert.IsTrue(rescue.IsValid);
            Assert.IsTrue(search.IsValid);
            Assert.Greater(rescue.PriorityScore, search.PriorityScore);
        }

        [Test]
        public void HazardOutranksVictim()
        {
            var candidates = Evaluate(HazardAndVictim(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea), DefaultPolicy);

            var hazard = CandidateFor(candidates, MissionTaskState.AvoidHazard);
            var rescue = CandidateFor(candidates, MissionTaskState.RescueVictim);

            Assert.IsTrue(hazard.IsValid);
            Assert.IsTrue(rescue.IsValid);
            Assert.Greater(hazard.PriorityScore, rescue.PriorityScore);
        }

        [Test]
        public void CooldownPenalty_LowersScore()
        {
            var avoiding = Evaluate(Hazard(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.AvoidHazard), DefaultPolicy);
            var searching = Evaluate(Hazard(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea), DefaultPolicy);

            var inCooldown = CandidateFor(avoiding, MissionTaskState.AvoidHazard);
            var plain = CandidateFor(searching, MissionTaskState.AvoidHazard);

            Assert.IsTrue(inCooldown.IsValid);
            Assert.IsTrue(plain.IsValid);
            Assert.AreEqual(plain.PriorityScore - inCooldown.PriorityScore, DefaultPolicy.CooldownPenalty, 1e-6f);
        }

        [Test]
        public void MemoryBonus_IncreasesScore()
        {
            var withMemory = Evaluate(Neutral(), VictimInMind(), Task(MissionTaskState.RescueVictim), DefaultPolicy);
            var withoutBonus = Evaluate(
                Neutral(), VictimInMind(), Task(MissionTaskState.RescueVictim),
                new PriorityPolicy(
                    victimPriority: 0.8f, hazardPriority: 1f, searchPriority: 0.4f, resumePriority: 0.5f,
                    idlePriority: 0f, memoryBonus: 0f, confidenceBonus: 0.2f, distancePenalty: 0.25f,
                    cooldownPenalty: 0.2f, priorityTieTolerance: 0.001f));

            var rescue = CandidateFor(withMemory, MissionTaskState.RescueVictim);
            var baseRescue = CandidateFor(withoutBonus, MissionTaskState.RescueVictim);

            Assert.IsTrue(rescue.IsValid);
            Assert.IsTrue(baseRescue.IsValid);
            Assert.AreEqual(DefaultPolicy.VictimPriority + DefaultPolicy.MemoryBonus, rescue.PriorityScore, 1e-6f);
            Assert.AreEqual(DefaultPolicy.VictimPriority, baseRescue.PriorityScore, 1e-6f);
            Assert.Greater(rescue.PriorityScore, baseRescue.PriorityScore);
        }

        [Test]
        public void DistancePenalty_ReducesScore()
        {
            var near = Evaluate(Victim(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea), DefaultPolicy);
            var far = Evaluate(Victim(0.8f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea), DefaultPolicy);

            var rescueNear = CandidateFor(near, MissionTaskState.RescueVictim);
            var rescueFar = CandidateFor(far, MissionTaskState.RescueVictim);

            Assert.IsTrue(rescueNear.IsValid);
            Assert.IsTrue(rescueFar.IsValid);
            Assert.Greater(rescueNear.PriorityScore, rescueFar.PriorityScore);
            Assert.AreEqual(rescueNear.PriorityScore - rescueFar.PriorityScore, DefaultPolicy.DistancePenalty * 0.1f, 1e-6f);
        }

        [Test]
        public void InvalidCandidates_AreIgnored()
        {
            var valid = new TaskPriority(Task(MissionTaskState.SearchArea), 0.5f, 1f, true);
            var prioritizer = new TaskPrioritizer(new FixedEvaluator(TaskPriority.Invalid, valid), DefaultPolicy);

            var winner = prioritizer.Select(Neutral(), BehaviourMemory.Empty, Task(MissionTaskState.Idle), DefaultMission);

            Assert.IsTrue(winner.IsValid);
            Assert.AreEqual(MissionTaskState.SearchArea, winner.Task.State);
        }

        [Test]
        public void NoDuplicateCandidateStates()
        {
            var rich = Evaluate(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim), DefaultPolicy);
            var continuity = Evaluate(Neutral(), VictimInMind(), Task(MissionTaskState.RescueVictim), DefaultPolicy);

            AssertNoDuplicateStates(rich);
            AssertNoDuplicateStates(continuity);
        }

        [Test]
        public void StableOrdering()
        {
            var a = Evaluate(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim), DefaultPolicy);
            var b = Evaluate(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim), DefaultPolicy);

            Assert.AreEqual(a.Length, b.Length);
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i].Task.State, b[i].Task.State);
                Assert.AreEqual(a[i].PriorityScore, b[i].PriorityScore, 1e-6f);
            }
        }

        [Test]
        public void SameInputs_SameOutputs()
        {
            var prioritizer = new TaskPrioritizer(new PriorityEvaluator(), DefaultPolicy);

            var a = prioritizer.Select(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim), DefaultMission);
            var b = prioritizer.Select(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim), DefaultMission);

            Assert.AreEqual(a.Task.State, b.Task.State);
            Assert.AreEqual(a.PriorityScore, b.PriorityScore, 1e-6f);
        }

        [Test]
        public void DecisionEngine_UsesPrioritizer()
        {
            var engine = CreateEngine();

            var result = engine.Decide(ObstacleReading(0.9f));
            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual(MissionTaskState.AvoidHazard, diagnostics.LastWinning.Task.State);
            Assert.AreEqual(MissionTaskState.AvoidHazard, diagnostics.LastMissionTask.State);
            Assert.AreEqual(BehaviourState.Avoid, result.Behaviour);
            Assert.AreEqual(BehaviourState.Avoid, diagnostics.LastBehaviour);
            Assert.AreEqual(1, diagnostics.StepCount);
        }

        [Test]
        public void Diagnostics_ExposeWinningScore()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            var diagnostics = engine.GetDiagnostics();

            var expected = DefaultPolicy.HazardPriority + DefaultPolicy.ConfidenceBonus
                - DefaultPolicy.DistancePenalty * (1f - 0.9f)
                - DefaultPolicy.CooldownPenalty;

            Assert.Greater(diagnostics.WinningPriorityScore, 0f);
            Assert.AreEqual(expected, diagnostics.WinningPriorityScore, 1e-4f);
        }

        [Test]
        public void Diagnostics_ExposeCandidateCount()
        {
            var engine = CreateEngine();

            engine.Decide(ObstacleReading(0.9f));
            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual(1, diagnostics.CandidateCount);
        }

        [Test]
        public void Diagnostics_ExposeSelectedExecutor()
        {
            var engine = CreateEngine();

            engine.Decide(ObstacleReading(0.9f));
            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual("AvoidExecutor", diagnostics.SelectedExecutor);
        }

        [Test]
        public void Diagnostics_ExposeCommand()
        {
            var engine = CreateEngine();

            var result = engine.Decide(ObstacleReading(0.9f));
            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual(result.Command.MoveDirection, diagnostics.LastCommand.MoveDirection);
            Assert.AreEqual(result.Command.Yaw, diagnostics.LastCommand.Yaw);
            Assert.AreEqual(result.Command.IsIdle, diagnostics.LastCommand.IsIdle);
        }

        [Test]
        public void Diagnostics_ExposeDecisionTimestamp()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            engine.Decide(VictimReading(0.9f));

            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual(2, diagnostics.StepCount);
            Assert.AreEqual(1f, diagnostics.DecisionTimestamp, 1e-6f);
        }

        [Test]
        public void PolicyConstants_Respected()
        {
            var custom = new PriorityPolicy(
                victimPriority: 1.5f, hazardPriority: 1f, searchPriority: 0.4f, resumePriority: 0.5f,
                idlePriority: 0f, memoryBonus: 0.15f, confidenceBonus: 0.2f, distancePenalty: 0.25f,
                cooldownPenalty: 0.2f, priorityTieTolerance: 0.001f);

            var candidates = Evaluate(HazardAndVictim(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea), custom);

            var rescue = CandidateFor(candidates, MissionTaskState.RescueVictim);
            var hazard = CandidateFor(candidates, MissionTaskState.AvoidHazard);

            Assert.IsTrue(rescue.IsValid);
            Assert.IsTrue(hazard.IsValid);
            Assert.AreEqual(1.5f + 0.2f - 0.25f * 0.1f, rescue.PriorityScore, 1e-6f);
            Assert.AreEqual(1f + 0.2f - 0.25f * 0.1f, hazard.PriorityScore, 1e-6f);
            Assert.Greater(rescue.PriorityScore, hazard.PriorityScore);

            Assert.AreEqual(0.8f, DefaultPolicy.VictimPriority, 1e-6f);
            Assert.AreEqual(1f, DefaultPolicy.HazardPriority, 1e-6f);
            Assert.Greater(DefaultPolicy.HazardPriority, DefaultPolicy.VictimPriority);
            Assert.Less(DefaultPolicy.IdlePriority, DefaultPolicy.SearchPriority);
        }

        [Test]
        public void BehaviourFollowsWinningTask()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());

            var result = engine.Decide(VictimReading(0.9f));
            var diagnostics = engine.GetDiagnostics();

            Assert.AreEqual(MissionTaskState.RescueVictim, diagnostics.LastWinning.Task.State);
            Assert.AreEqual(BehaviourState.Approach, result.Behaviour);
        }

        private static void AssertNoDuplicateStates(TaskPriority[] candidates)
        {
            for (var i = 0; i < candidates.Length; i++)
            {
                for (var j = i + 1; j < candidates.Length; j++)
                {
                    Assert.AreNotEqual(
                        candidates[i].Task.State,
                        candidates[j].Task.State,
                        "Each objective state must be emitted at most once per evaluation.");
                }
            }
        }

        private sealed class FixedEvaluator : IPriorityEvaluator
        {
            private readonly TaskPriority[] _candidates;

            public FixedEvaluator(params TaskPriority[] candidates)
            {
                _candidates = candidates;
            }

            public TaskPriority[] Evaluate(
                TaskCandidate[] candidates,
                MissionTask current,
                PriorityPolicy policy)
            {
                return _candidates;
            }
        }
    }
}
