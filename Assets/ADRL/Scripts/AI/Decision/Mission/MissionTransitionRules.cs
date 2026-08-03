namespace ADRL.AI.Decision.Mission
{
    using ADRL.AI.Decision.Memory;

    /// <summary>
    /// Pure, deterministic mission-transition policy. It owns no state: given the
    /// same assessment, memory, current task, step clock and policy it always
    /// produces the same next task. Current perception always overrides any
    /// remembered objective - an imminent hazard and a perceived target are handled
    /// before any continuation logic is consulted.
    /// </summary>
    /// <remarks>
    /// Transition contract (all deterministic):
    /// <list type="bullet">
    /// <item>ANY state to <see cref="MissionTaskState.AvoidHazard"/> on an imminent hazard (priority-driven).</item>
    /// <item>Any active state to <see cref="MissionTaskState.InvestigateTarget"/> when a target is perceived.</item>
    /// <item>Investigate to <see cref="MissionTaskState.RescueVictim"/> once a victim is confirmed.</item>
    /// <item>Investigate to <see cref="MissionTaskState.SearchArea"/> when the perceived target is lost
    /// before confirmation and the victim is no longer in memory (recovery path - prevents an
    /// abandoned investigation from dangling indefinitely).</item>
    /// <item>Rescue to <see cref="MissionTaskState.ResumeSearch"/> once the victim leaves perception and memory.</item>
    /// <item>Resume to <see cref="MissionTaskState.SearchArea"/> after <see cref="MissionPolicy.ResumeTimeout"/> steps.</item>
    /// <item>Avoid to its <see cref="MissionTask.PreviousState"/> once the hazard clears and the
    /// <see cref="MissionPolicy.TransitionCooldown"/> elapses.</item>
    /// <item>Idle to <see cref="MissionTaskState.SearchArea"/> on the first viable assessment.</item>
    /// </list>
    /// An invalid assessment holds the current task unchanged (no fabrication from
    /// garbage data); the behaviour selector independently guards invalid
    /// perception by resolving it to Idle.
    /// </remarks>
    public static class MissionTransitionRules
    {
        /// <summary>
        /// Computes the next mission task from the current perception, the bounded
        /// behaviour memory and the current task at the given step clock.
        /// </summary>
        public static MissionTask Transition(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask current,
            float now,
            MissionPolicy policy)
        {
            if (!assessment.IsValid)
                return current;

            var hazard = assessment.ObstacleProximity >= policy.HazardProximityThreshold;
            var target = assessment.TargetDetected;
            var victimConfirmed = target && assessment.TargetProximity >= policy.VictimConfirmationProximity;
            var victimInMind = memory.LastVictimSeen.IsValid
                && memory.LastVictimSeen.Confidence >= policy.ConfidenceThreshold;

            // Safety is the highest-priority objective unless the configured
            // priorities deliberately rank a confirmed victim first.
            if (hazard && policy.HazardPriority >= policy.VictimPriority)
                return Avoid(current, now);

            if (target)
            {
                if (victimConfirmed || current.State == MissionTaskState.RescueVictim)
                    return Enter(MissionTaskState.RescueVictim, current, now);

                return Enter(MissionTaskState.InvestigateTarget, current, now);
            }

            if (hazard)
                return Avoid(current, now);

            switch (current.State)
            {
                case MissionTaskState.AvoidHazard:
                    return now - current.EntryStep >= policy.TransitionCooldown
                        ? Restore(current, now)
                        : current;

                case MissionTaskState.RescueVictim:
                    return victimInMind
                        ? current
                        : Enter(MissionTaskState.ResumeSearch, current, now);

                case MissionTaskState.ResumeSearch:
                    return now - current.EntryStep >= policy.ResumeTimeout
                        ? Enter(MissionTaskState.SearchArea, current, now)
                        : current;

                case MissionTaskState.InvestigateTarget:
                    return victimInMind
                        ? current
                        : Enter(MissionTaskState.SearchArea, current, now);

                case MissionTaskState.SearchArea:
                    return current;

                case MissionTaskState.Idle:
                    return Enter(MissionTaskState.SearchArea, current, now);

                default:
                    return current;
            }
        }

        /// <summary>
        /// Enters (or continues) the hazard override. Staying keeps the original
        /// entry step and previous task, so the cooldown counts continuously.
        /// </summary>
        private static MissionTask Avoid(MissionTask current, float now)
        {
            return current.State == MissionTaskState.AvoidHazard
                ? current
                : Enter(MissionTaskState.AvoidHazard, current, now);
        }

        /// <summary>
        /// Enters the given state, remembering the current state for hazard
        /// restoration. Re-entering the current state is a no-op.
        /// </summary>
        private static MissionTask Enter(MissionTaskState state, MissionTask current, float now)
        {
            return state == current.State
                ? current
                : new MissionTask(state, now, current.State, true);
        }

        /// <summary>Restores the task recorded before the hazard override.</summary>
        private static MissionTask Restore(MissionTask current, float now)
        {
            var previous = current.PreviousState;
            return new MissionTask(previous, now, previous, true);
        }
    }
}
