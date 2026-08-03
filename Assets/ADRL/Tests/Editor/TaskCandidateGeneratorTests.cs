namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 8.7.1 Candidate Generation Separation: the
    /// <see cref="TaskCandidateGenerator"/> is the single owner of "what objectives
    /// are currently available". It derives the valid candidate objectives from
    /// perception, behaviour memory and the mission coordinator's task, emits them in
    /// a fixed deterministic order with no duplicates, never scores or arbitrates, and
    /// is pure, deterministic and stateless.
    /// </summary>
    [TestFixture]
    internal sealed class TaskCandidateGeneratorTests
    {
        private static readonly MissionPolicy DefaultMission = MissionPolicy.Default;

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

        private static TaskCandidate[] Generate(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask current)
        {
            return new TaskCandidateGenerator().Generate(assessment, memory, current, DefaultMission);
        }

        private static TaskCandidate? CandidateFor(TaskCandidate[] candidates, MissionTaskState state)
        {
            foreach (var candidate in candidates)
            {
                if (candidate.Task.State == state)
                    return candidate;
            }

            return null;
        }

        [Test]
        public void HazardAssessment_GeneratesAvoidCandidate()
        {
            // Current task is already avoiding, so only the perception candidate is
            // available and no competing continuity objective is fabricated.
            var candidates = Generate(Hazard(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.AvoidHazard));

            Assert.AreEqual(1, candidates.Length);
            var avoid = candidates[0];
            Assert.AreEqual(MissionTaskState.AvoidHazard, avoid.Task.State);
            Assert.AreEqual(CandidateOrigin.CurrentPerception, avoid.Origin);
            Assert.AreEqual(0.9f, avoid.Proximity, 1e-6f);
            Assert.AreEqual(1f, avoid.Confidence, 1e-6f);
        }

        [Test]
        public void ConfirmedVictim_GeneratesRescueCandidate()
        {
            var candidates = Generate(Victim(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea));

            var rescue = CandidateFor(candidates, MissionTaskState.RescueVictim);
            Assert.IsTrue(rescue.HasValue);
            Assert.AreEqual(CandidateOrigin.CurrentPerception, rescue.Value.Origin);
            Assert.AreEqual(0.9f, rescue.Value.Proximity, 1e-6f);
            Assert.AreEqual(1f, rescue.Value.Confidence, 1e-6f);
        }

        [Test]
        public void NeutralAssessment_GeneratesContinuityCandidate()
        {
            var candidates = Generate(Neutral(), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea));

            Assert.AreEqual(1, candidates.Length);
            var continuity = candidates[0];
            Assert.AreEqual(MissionTaskState.SearchArea, continuity.Task.State);
            Assert.AreEqual(CandidateOrigin.Continuity, continuity.Origin);
            Assert.AreEqual(0f, continuity.Proximity, 1e-6f);
            Assert.AreEqual(1f, continuity.Confidence, 1e-6f);
        }

        [Test]
        public void NoDuplicateCandidates()
        {
            var rich = Generate(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim));
            var continuity = Generate(Neutral(), VictimInMind(), Task(MissionTaskState.RescueVictim));

            AssertNoDuplicateStates(rich);
            AssertNoDuplicateStates(continuity);
        }

        [Test]
        public void DeterministicOrdering()
        {
            var a = Generate(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim));
            var b = Generate(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim));

            Assert.AreEqual(a.Length, b.Length);
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i].Task.State, b[i].Task.State);
                Assert.AreEqual(a[i].Origin, b[i].Origin);
            }
        }

        [Test]
        public void SameInputs_SameCandidateList()
        {
            var a = Generate(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim));
            var b = Generate(HazardAndVictim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim));

            AssertSameCandidates(a, b);
        }

        [Test]
        public void Generation_IsIndependentFromScoring()
        {
            // The generator never reads a PriorityPolicy; scoring configuration must
            // not influence what objectives are available.
            var withDefaultScoring = Generate(Victim(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea));
            var withCustomScoring = new TaskCandidateGenerator().Generate(
                Victim(0.9f),
                BehaviourMemory.Empty,
                Task(MissionTaskState.SearchArea),
                new MissionPolicy(5f, 3f, 0.65f, 0.75f, 1f, 0.8f, 0.25f));

            AssertSameCandidates(withDefaultScoring, withCustomScoring);
        }

        [Test]
        public void Generation_IsIndependentFromPrioritization()
        {
            // The generated list is produced before any arbitration and is the exact
            // set the scorer and prioritizer consume; the winner must come from it.
            var candidates = Generate(HazardAndVictim(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea));

            var evaluator = new PriorityEvaluator();
            var scored = evaluator.Evaluate(candidates, Task(MissionTaskState.SearchArea), PriorityPolicy.Default);
            var winner = new TaskPrioritizer(evaluator, PriorityPolicy.Default).Select(scored);

            Assert.IsTrue(winner.IsValid);
            Assert.AreEqual(MissionTaskState.AvoidHazard, winner.Task.State);
            Assert.IsTrue(CandidateFor(candidates, winner.Task.State).HasValue);
        }

        [Test]
        public void GeneratedCandidates_AreAlwaysValid()
        {
            var scenarios = new[]
            {
                Generate(Hazard(0.9f), BehaviourMemory.Empty, Task(MissionTaskState.SearchArea)),
                Generate(Victim(0.9f), VictimInMind(), Task(MissionTaskState.RescueVictim)),
                Generate(Neutral(), BehaviourMemory.Empty, Task(MissionTaskState.Idle)),
            };

            foreach (var candidates in scenarios)
            {
                Assert.Greater(candidates.Length, 0);
                foreach (var candidate in candidates)
                    Assert.IsTrue(candidate.Task.IsValid);
            }
        }

        [Test]
        public void InvalidAssessment_ProducesNoCandidates()
        {
            var candidates = Generate(SituationSnapshot.Invalid, VictimInMind(), Task(MissionTaskState.SearchArea));

            Assert.AreEqual(0, candidates.Length);
        }

        [Test]
        public void Continuity_KeepsCoordinatorTaskIdentity()
        {
            var current = Task(MissionTaskState.SearchArea);
            var candidates = Generate(Neutral(), BehaviourMemory.Empty, current);

            Assert.AreEqual(1, candidates.Length);
            Assert.AreEqual(current.State, candidates[0].Task.State);
            Assert.AreEqual(current.EntryStep, candidates[0].Task.EntryStep);
            Assert.AreEqual(current.PreviousState, candidates[0].Task.PreviousState);
        }

        private static void AssertSameCandidates(TaskCandidate[] a, TaskCandidate[] b)
        {
            Assert.AreEqual(a.Length, b.Length);
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i].Task.State, b[i].Task.State);
                Assert.AreEqual(a[i].Origin, b[i].Origin);
                Assert.AreEqual(a[i].Proximity, b[i].Proximity, 1e-6f);
                Assert.AreEqual(a[i].Confidence, b[i].Confidence, 1e-6f);
            }
        }

        private static void AssertNoDuplicateStates(TaskCandidate[] candidates)
        {
            for (var i = 0; i < candidates.Length; i++)
            {
                for (var j = i + 1; j < candidates.Length; j++)
                {
                    Assert.AreNotEqual(
                        candidates[i].Task.State,
                        candidates[j].Task.State,
                        "Each objective state must be emitted at most once per generation.");
                }
            }
        }
    }
}
