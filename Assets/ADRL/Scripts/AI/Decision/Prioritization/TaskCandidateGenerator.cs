namespace ADRL.AI.Decision.Prioritization
{
    using System;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// The single owner of candidate-objective availability. It answers one
    /// question - "which mission objectives are currently available?" - by deriving
    /// the currently-valid objective states from current perception and the mission
    /// coordinator's current task, and emitting each as a <see cref="TaskCandidate"/>
    /// in a fixed deterministic order. It never assigns scores, compares candidates,
    /// chooses winners or modifies runtime state: it is a pure, stateless function
    /// of its inputs. Scoring is owned by <see cref="PriorityEvaluator"/> and winner
    /// selection by <see cref="TaskPrioritizer"/>.
    /// </summary>
    /// <remarks>
    /// Availability is derived from the mission policy thresholds exactly as the
    /// mission coordinator derives them, so the generator and the coordinator always
    /// agree on what counts as a hazard, a confirmed victim, a perceived target and
    /// a remembered victim. Current-perception imperatives (hazard, confirmed victim,
    /// unconfirmed target) are emitted first; the coordinator's objective follows as
    /// a continuity candidate so a neutral situation never fabricates a competing
    /// objective. An invalid (empty) assessment yields no candidates at all - with no
    /// perception there is nothing currently available.
    /// </remarks>
    public sealed class TaskCandidateGenerator
    {
        private const int MaxCandidates = 6;

        /// <summary>
        /// Returns every objective currently available for the given assessment,
        /// behaviour memory, mission task and mission policy, in a fixed
        /// deterministic order with no duplicate objective states.
        /// </summary>
        public TaskCandidate[] Generate(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask current,
            MissionPolicy mission)
        {
            var buffer = new TaskCandidate[MaxCandidates];
            var count = 0;

            if (!assessment.IsValid)
                return new TaskCandidate[0];

            var hazardImminent = assessment.ObstacleProximity >= mission.HazardProximityThreshold;
            var victimConfirmed = assessment.TargetDetected &&
                assessment.TargetProximity >= mission.VictimConfirmationProximity;
            var targetPerceived = assessment.TargetDetected &&
                assessment.TargetProximity < mission.VictimConfirmationProximity;
            var victimInMind = memory.LastVictimSeen.IsValid &&
                memory.LastVictimSeen.Confidence >= mission.ConfidenceThreshold;

            if (hazardImminent)
            {
                buffer[count++] = CurrentPerception(
                    MissionTaskState.AvoidHazard, current, assessment.ObstacleProximity, 1f);
            }

            if (victimConfirmed)
            {
                buffer[count++] = CurrentPerception(
                    MissionTaskState.RescueVictim, current, assessment.TargetProximity, 1f);
            }

            if (targetPerceived)
            {
                buffer[count++] = CurrentPerception(
                    MissionTaskState.InvestigateTarget, current, assessment.TargetProximity, 1f);
            }

            // Continuity candidates: when current perception is silent about a state,
            // the coordinator's objective keeps its claim, so the mission narrative
            // (cooldown, resume, previous-task restoration) is never overridden by a
            // fabricated competing objective.
            switch (current.State)
            {
                case MissionTaskState.RescueVictim when !victimConfirmed:
                    buffer[count++] = Continuity(
                        current, victimInMind ? memory.LastVictimSeen.Confidence : 1f);
                    break;

                case MissionTaskState.InvestigateTarget when !targetPerceived:
                    buffer[count++] = Continuity(
                        current, victimInMind ? memory.LastVictimSeen.Confidence : 1f);
                    break;

                case MissionTaskState.AvoidHazard when !hazardImminent && !targetPerceived && !victimConfirmed:
                    buffer[count++] = Continuity(current, 1f);
                    break;

                case MissionTaskState.ResumeSearch:
                    buffer[count++] = Continuity(current, 1f);
                    break;

                case MissionTaskState.SearchArea:
                    buffer[count++] = Continuity(current, 1f);
                    break;

                case MissionTaskState.Idle:
                    buffer[count++] = Continuity(current, 1f);
                    break;
            }

            var result = new TaskCandidate[count];
            Array.Copy(buffer, result, count);
            return result;
        }

        private static TaskCandidate CurrentPerception(
            MissionTaskState state, MissionTask current, float proximity, float confidence)
        {
            return new TaskCandidate(
                CandidateTask(state, current), CandidateOrigin.CurrentPerception, proximity, confidence);
        }

        private static TaskCandidate Continuity(MissionTask current, float confidence)
        {
            return new TaskCandidate(current, CandidateOrigin.Continuity, 0f, confidence);
        }

        private static MissionTask CandidateTask(MissionTaskState state, MissionTask current)
        {
            return state == current.State
                ? current
                : new MissionTask(state, current.EntryStep, current.State, true);
        }
    }
}
