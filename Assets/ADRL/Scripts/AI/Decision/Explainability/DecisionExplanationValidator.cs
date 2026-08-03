namespace ADRL.AI.Decision.Explainability
{
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// Pure validation utilities for a <see cref="DecisionExplanation"/>. Each check
    /// verifies one invariant of a built explanation; <see cref="IsValid"/> requires
    /// all of them to hold. Validation never mutates the explanation and never
    /// influences decisions - it exists so the runtime, the tests and the smoke test
    /// can independently confirm a built explanation is complete and internally
    /// consistent (winner ↔ behaviour ↔ executor ↔ command, mission valid, candidate
    /// count consistent, timestamp and step valid, no null references).
    /// </summary>
    /// <remarks>
    /// The behaviour↔executor and executor↔command contracts mirror the Phase 8.4
    /// executor pipeline: the executor resolving a behaviour is named
    /// <c>{Behaviour}Executor</c>, the idle executor always commands
    /// <see cref="ADRL.AI.DecisionMaking.DroneCommand.Idle"/> and every other
    /// executor always commands movement. The canonical empty explanation validates
    /// successfully.
    /// </remarks>
    public static class DecisionExplanationValidator
    {
        /// <summary>True when every component payload of the explanation is present.</summary>
        public static bool IsComplete(DecisionExplanation explanation)
        {
            return explanation.CandidateTasks != null
                && explanation.ScoredCandidates != null
                && explanation.Reasons != null
                && explanation.Executor != null;
        }

        /// <summary>True when no reference member of the explanation is null.</summary>
        public static bool HasNoNullReferences(DecisionExplanation explanation)
        {
            return IsComplete(explanation);
        }

        /// <summary>
        /// True when the explanation holds a real mission objective, or the canonical
        /// idle/no-op objective (before the first step).
        /// </summary>
        public static bool MissionValid(DecisionExplanation explanation)
        {
            return explanation.Mission.IsValid || explanation.Mission.State == MissionTaskState.Idle;
        }

        /// <summary>
        /// True when the selected behaviour is the one the winning objective maps to
        /// (Idle→Idle, Search/Resume→Search, Investigate/Rescue→Approach, Avoid→Avoid).
        /// </summary>
        public static bool WinnerMatchesBehaviour(DecisionExplanation explanation)
        {
            return explanation.Behaviour == Map(explanation.Winning.Task.State);
        }

        /// <summary>
        /// True when the executor that produced the command is the one owning the
        /// selected behaviour (named <c>{Behaviour}Executor</c>), or is empty before
        /// the first step.
        /// </summary>
        public static bool BehaviourMatchesExecutor(DecisionExplanation explanation)
        {
            if (explanation.Executor.Length == 0)
                return true;

            return explanation.Executor == ExecutorName(explanation.Behaviour);
        }

        /// <summary>
        /// True when the resolved command agrees with the executor that produced it:
        /// the idle executor always commands an idle command and every other executor
        /// always commands movement.
        /// </summary>
        public static bool ExecutorMatchesCommand(DecisionExplanation explanation)
        {
            if (explanation.Executor.Length == 0)
                return true;

            if (explanation.Executor == "IdleExecutor")
                return explanation.Command.IsIdle;

            return !explanation.Command.IsIdle;
        }

        /// <summary>True when the candidate and scored-priority lists agree in length.</summary>
        public static bool CandidateCountValid(DecisionExplanation explanation)
        {
            return explanation.CandidateTasks != null
                && explanation.ScoredCandidates != null
                && explanation.CandidateTasks.Length == explanation.ScoredCandidates.Length;
        }

        /// <summary>True when the decision timestamp is a non-negative step-clock value.</summary>
        public static bool TimestampValid(DecisionExplanation explanation)
        {
            return explanation.DecisionTimestamp >= 0f;
        }

        /// <summary>True when the decision step is non-negative.</summary>
        public static bool StepValid(DecisionExplanation explanation)
        {
            return explanation.DecisionStep >= 0;
        }

        /// <summary>True when every invariant of the explanation holds.</summary>
        public static bool IsValid(DecisionExplanation explanation)
        {
            return IsComplete(explanation)
                && HasNoNullReferences(explanation)
                && MissionValid(explanation)
                && WinnerMatchesBehaviour(explanation)
                && BehaviourMatchesExecutor(explanation)
                && ExecutorMatchesCommand(explanation)
                && CandidateCountValid(explanation)
                && TimestampValid(explanation)
                && StepValid(explanation);
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

        /// <summary>The executor name owning the given behaviour.</summary>
        private static string ExecutorName(BehaviourState behaviour)
        {
            switch (behaviour)
            {
                case BehaviourState.Idle:
                    return "IdleExecutor";
                case BehaviourState.Search:
                    return "SearchExecutor";
                case BehaviourState.Approach:
                    return "ApproachExecutor";
                case BehaviourState.Avoid:
                    return "AvoidExecutor";
                default:
                    return string.Empty;
            }
        }
    }
}
