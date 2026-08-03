namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 8.6 Mission Task Coordination Framework: the
    /// <see cref="MissionCoordinator"/> sits between behaviour memory and behaviour
    /// selection, owns the current mission task and its deterministic transitions,
    /// never overrides current perception, and is fully deterministic on the engine's
    /// step clock. Verifies every named transition, hazard override, previous-task
    /// restoration, cooldown/timeout behaviour, invalid-assessment handling and
    /// mission continuity.
    /// </summary>
    [TestFixture]
    internal sealed class MissionCoordinatorTests
    {
        private static readonly MemoryPolicy ShortMemory = new(
            victimMemoryDuration: 3f,
            obstacleMemoryDuration: 2f,
            confidenceDecay: 0.25f,
            refreshThreshold: 5f,
            maxRecords: 4);

        private static readonly MissionPolicy TestPolicy = new(
            resumeTimeout: 3f,
            transitionCooldown: 2f,
            hazardProximityThreshold: 0.65f,
            victimConfirmationProximity: 0.75f,
            hazardPriority: 1f,
            victimPriority: 0.8f,
            confidenceThreshold: 0.25f);

        private static readonly DecisionContext DefaultContext = DecisionContext.Default;

        private static ISensorReading VictimReading(float proximity) => new SensorReading(new[]
        {
            proximity, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        });

        private static ISensorReading ObstacleReading(float proximity) => new SensorReading(new[]
        {
            0f, 0f, 0f, 0f, 0f, 0f, proximity, 0f, 0f, 0f, 0f, 0f,
        });

        private static ISensorReading NeutralReading() => new SensorReading(new[]
        {
            0.1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        });

        private static ISensorReading EmptyReading() => new SensorReading(new float[0]);

        private static DecisionEngine CreateEngine()
        {
            return new DecisionEngine(
                DefaultContext,
                new FogOfWarSituationAssessor(DefaultContext),
                new BehaviourSelector(DefaultContext),
                ShortMemory,
                TestPolicy);
        }

        [Test]
        public void InitialMission_IsIdle()
        {
            var engine = CreateEngine();

            Assert.AreEqual(MissionTaskState.Idle, engine.Mission.CurrentTask.State);
            Assert.IsTrue(engine.Mission.CurrentTask.IsValid);
        }

        [Test]
        public void ValidAssessment_StartsSearchArea()
        {
            var engine = CreateEngine();

            var result = engine.Decide(NeutralReading());

            Assert.AreEqual(MissionTaskState.SearchArea, engine.Mission.CurrentTask.State);
            Assert.AreEqual(BehaviourState.Search, result.Behaviour);
        }

        [Test]
        public void TargetDetected_TransitionsToInvestigate()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());

            var result = engine.Decide(VictimReading(0.5f));

            Assert.AreEqual(MissionTaskState.InvestigateTarget, engine.Mission.CurrentTask.State);
            Assert.AreEqual(BehaviourState.Approach, result.Behaviour);
        }

        [Test]
        public void ConfirmedVictim_TransitionsToRescue()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            engine.Decide(VictimReading(0.5f));

            var result = engine.Decide(VictimReading(0.95f));

            Assert.AreEqual(MissionTaskState.RescueVictim, engine.Mission.CurrentTask.State);
            Assert.AreEqual(BehaviourState.Approach, result.Behaviour);
        }

        [Test]
        public void RescueCompleted_TransitionsToResumeSearch()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            engine.Decide(VictimReading(0.5f));
            engine.Decide(VictimReading(0.95f));
            Assert.AreEqual(MissionTaskState.RescueVictim, engine.Mission.CurrentTask.State);

            // The victim leaves perception but stays in mind: rescue continues.
            engine.Decide(NeutralReading());
            engine.Decide(NeutralReading());
            engine.Decide(NeutralReading());
            Assert.AreEqual(MissionTaskState.RescueVictim, engine.Mission.CurrentTask.State);

            // Victim memory (duration 3, inclusive) expires one step later: rescue done.
            var result = engine.Decide(NeutralReading());

            Assert.AreEqual(MissionTaskState.ResumeSearch, engine.Mission.CurrentTask.State);
            Assert.AreEqual(BehaviourState.Search, result.Behaviour);
        }

        [Test]
        public void ResumeSearch_TransitionsToSearchArea_AfterTimeout()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            engine.Decide(VictimReading(0.5f));
            engine.Decide(VictimReading(0.95f));
            engine.Decide(NeutralReading());
            engine.Decide(NeutralReading());
            engine.Decide(NeutralReading());
            engine.Decide(NeutralReading());
            Assert.AreEqual(MissionTaskState.ResumeSearch, engine.Mission.CurrentTask.State);

            // ResumeTimeout is 3 steps: the next two neutral steps stay in ResumeSearch.
            engine.Decide(NeutralReading());
            Assert.AreEqual(MissionTaskState.ResumeSearch, engine.Mission.CurrentTask.State);
            engine.Decide(NeutralReading());
            Assert.AreEqual(MissionTaskState.ResumeSearch, engine.Mission.CurrentTask.State);

            // The third step crosses the timeout and resumes the area sweep.
            var result = engine.Decide(NeutralReading());

            Assert.AreEqual(MissionTaskState.SearchArea, engine.Mission.CurrentTask.State);
            Assert.AreEqual(BehaviourState.Search, result.Behaviour);
        }

        [Test]
        public void HazardDetected_OverridesAnyMission()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            engine.Decide(VictimReading(0.5f));
            Assert.AreEqual(MissionTaskState.InvestigateTarget, engine.Mission.CurrentTask.State);

            var result = engine.Decide(ObstacleReading(0.9f));

            Assert.AreEqual(MissionTaskState.AvoidHazard, engine.Mission.CurrentTask.State);
            Assert.AreEqual(BehaviourState.Avoid, result.Behaviour);
        }

        [Test]
        public void AvoidHazard_Cooldown_DelaysRestoration()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            engine.Decide(VictimReading(0.5f));
            engine.Decide(ObstacleReading(0.9f));
            Assert.AreEqual(MissionTaskState.AvoidHazard, engine.Mission.CurrentTask.State);

            // Hazard gone but the cooldown (2 steps) has not elapsed: still avoiding,
            // and behaviour follows the mission even though the obstacle is no longer
            // imminent in the current perception.
            var result = engine.Decide(NeutralReading());

            Assert.AreEqual(MissionTaskState.AvoidHazard, engine.Mission.CurrentTask.State);
            Assert.AreEqual(BehaviourState.Avoid, result.Behaviour);
        }

        [Test]
        public void AvoidHazard_RestoresPreviousTask_AfterCooldown()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            engine.Decide(VictimReading(0.5f));
            engine.Decide(ObstacleReading(0.9f));
            engine.Decide(NeutralReading());
            Assert.AreEqual(MissionTaskState.AvoidHazard, engine.Mission.CurrentTask.State);

            // Cooldown (2 steps) elapsed: the mission returns to the task that was
            // interrupted (Investigating the perceived target), not to search.
            var result = engine.Decide(NeutralReading());

            Assert.AreEqual(MissionTaskState.InvestigateTarget, engine.Mission.CurrentTask.State);
            Assert.AreEqual(BehaviourState.Approach, result.Behaviour);
        }

        [Test]
        public void Mission_IsDeterministic_ForIdenticalHistory()
        {
            var a = CreateEngine();
            var b = CreateEngine();

            var history = new[]
            {
                NeutralReading(),
                VictimReading(0.5f),
                VictimReading(0.95f),
                NeutralReading(),
                ObstacleReading(0.9f),
                NeutralReading(),
                NeutralReading(),
                NeutralReading(),
            };

            foreach (var reading in history)
            {
                var ra = a.Decide(reading);
                var rb = b.Decide(reading);

                Assert.AreEqual(ra.Behaviour, rb.Behaviour);
                Assert.AreEqual(a.Mission.CurrentTask.State, b.Mission.CurrentTask.State);
                Assert.AreEqual(ra.Command.MoveDirection, rb.Command.MoveDirection);
                Assert.AreEqual(ra.Command.Yaw, rb.Command.Yaw);
            }
        }

        [Test]
        public void InvalidAssessment_ProducesIdleDecision_AndHoldsMission()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            Assert.AreEqual(MissionTaskState.SearchArea, engine.Mission.CurrentTask.State);

            var result = engine.Decide(EmptyReading());

            // No fabrication from garbage data: the decision is Idle and the mission
            // objective is preserved for when viable perception returns.
            Assert.AreEqual(BehaviourState.Idle, result.Behaviour);
            Assert.IsTrue(result.Command.IsIdle);
            Assert.AreEqual(MissionTaskState.SearchArea, engine.Mission.CurrentTask.State);
        }

        [Test]
        public void MissionContinuity_HoldsWhileVictimInMind()
        {
            var engine = CreateEngine();
            engine.Decide(NeutralReading());
            engine.Decide(VictimReading(0.5f));
            Assert.AreEqual(MissionTaskState.InvestigateTarget, engine.Mission.CurrentTask.State);

            // The perceived target leaves the reading but stays in short-term memory,
            // so the investigate objective continues across neutral frames.
            var a = engine.Decide(NeutralReading());
            var b = engine.Decide(NeutralReading());

            Assert.AreEqual(MissionTaskState.InvestigateTarget, engine.Mission.CurrentTask.State);
            Assert.AreEqual(BehaviourState.Approach, a.Behaviour);
            Assert.AreEqual(BehaviourState.Approach, b.Behaviour);
        }
    }
}
