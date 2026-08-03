namespace ADRL.AI.Decision.Context
{
    using ADRL.AI.Decision.Mission;
    using UnityEngine;

    /// <summary>
    /// Pure validation utilities for a <see cref="DecisionContextSnapshot"/>. Each
    /// check verifies one invariant of a built context; <see cref="IsValid"/>
    /// requires all of them to hold. Validation never mutates the snapshot and never
    /// influences decisions - it exists so the runtime, the tests and the smoke test
    /// can independently confirm a built context is complete and internally
    /// consistent.
    /// </summary>
    /// <remarks>
    /// The checks mirror the Phase 8.8 contract: the snapshot is complete, holds no
    /// null references, the behaviour agrees with the winning objective, the mission
    /// objective is present, the embedded diagnostics agree field-for-field with the
    /// rest of the snapshot (so the step can be replayed), the candidate count is
    /// consistent, and the resolved command agrees with the diagnostics. The
    /// canonical empty snapshot validates successfully.
    /// </remarks>
    public static class DecisionContextValidator
    {
        /// <summary>True when every component payload of the snapshot is present.</summary>
        public static bool IsComplete(DecisionContextSnapshot snapshot)
        {
            return snapshot.Candidates != null && snapshot.Memory != null;
        }

        /// <summary>True when no reference member of the snapshot is null.</summary>
        public static bool HasNoNullReferences(DecisionContextSnapshot snapshot)
        {
            return IsComplete(snapshot)
                && snapshot.RuntimeState.CurrentExecutor != null
                && snapshot.Diagnostics.SelectedExecutor != null;
        }

        /// <summary>
        /// True when the selected behaviour is the one the winning objective maps to
        /// (Idle→Idle, Search/Resume→Search, Investigate/Rescue→Approach, Avoid→Avoid).
        /// </summary>
        public static bool BehaviourMatchesWinner(DecisionContextSnapshot snapshot)
        {
            return snapshot.Behaviour == Map(snapshot.Winning.Task.State);
        }

        /// <summary>
        /// True when the snapshot holds a real mission objective, or the canonical
        /// idle/no-op objective (before the first step).
        /// </summary>
        public static bool MissionValid(DecisionContextSnapshot snapshot)
        {
            return snapshot.Mission.IsValid || snapshot.Mission.State == MissionTaskState.Idle;
        }

        /// <summary>
        /// True when the embedded diagnostics agree field-for-field with the rest of
        /// the snapshot, so the step can be replayed from one consistent context.
        /// </summary>
        public static bool DiagnosticsSynchronized(DecisionContextSnapshot snapshot)
        {
            return snapshot.Diagnostics.LastAssessment.Equals(snapshot.Assessment)
                && snapshot.Diagnostics.LastMissionTask.Equals(snapshot.Mission)
                && snapshot.Diagnostics.LastWinning.Equals(snapshot.Winning)
                && snapshot.Diagnostics.LastBehaviour == snapshot.Behaviour
                && snapshot.Diagnostics.LastCommand.Equals(snapshot.Command)
                && snapshot.Diagnostics.CandidateCount == snapshot.Candidates.Length
                && Mathf.Approximately(snapshot.Diagnostics.DecisionTimestamp, snapshot.RuntimeState.DecisionTimestamp)
                && snapshot.Diagnostics.StepCount == snapshot.RuntimeState.DecisionStep
                && string.Equals(snapshot.Diagnostics.SelectedExecutor, snapshot.RuntimeState.CurrentExecutor);
        }

        /// <summary>True when the diagnostics candidate count matches the generated candidates.</summary>
        public static bool CandidateCountValid(DecisionContextSnapshot snapshot)
        {
            return snapshot.Candidates != null
                && snapshot.Diagnostics.CandidateCount == snapshot.Candidates.Length;
        }

        /// <summary>True when the resolved command agrees with the diagnostics command.</summary>
        public static bool CommandValid(DecisionContextSnapshot snapshot)
        {
            return snapshot.Command.IsIdle == snapshot.Diagnostics.LastCommand.IsIdle
                && snapshot.Command.MoveDirection.Equals(snapshot.Diagnostics.LastCommand.MoveDirection)
                && Mathf.Approximately(snapshot.Command.Yaw, snapshot.Diagnostics.LastCommand.Yaw);
        }

        /// <summary>True when every invariant of the snapshot holds.</summary>
        public static bool IsValid(DecisionContextSnapshot snapshot)
        {
            return IsComplete(snapshot)
                && HasNoNullReferences(snapshot)
                && BehaviourMatchesWinner(snapshot)
                && MissionValid(snapshot)
                && DiagnosticsSynchronized(snapshot)
                && CandidateCountValid(snapshot)
                && CommandValid(snapshot);
        }

        /// <summary>The behaviour the mission task maps to for neutral situations.</summary>
        private static BehaviourState Map(MissionTaskState state)
        {
            switch (state)
            {
                case MissionTaskState.Idle:
                    return BehaviourState.Idle;
                case MissionTaskState.SearchArea:
                case MissionTaskState.ResumeSearch:
                    return BehaviourState.Search;
                case MissionTaskState.InvestigateTarget:
                case MissionTaskState.RescueVictim:
                    return BehaviourState.Approach;
                case MissionTaskState.AvoidHazard:
                    return BehaviourState.Avoid;
                default:
                    return BehaviourState.Search;
            }
        }
    }
}
