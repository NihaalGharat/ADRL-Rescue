namespace ADRL.AI.Decision.Prioritization
{
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// The single owner of deterministic objective scoring. It converts each
    /// <see cref="TaskCandidate"/> produced by the <see cref="TaskCandidateGenerator"/>
    /// into a scored <see cref="TaskPriority"/>. It never decides which objectives are
    /// available (that belongs to the generator) and never chooses a winner (that
    /// belongs to the <see cref="TaskPrioritizer"/>); it only scores. Pure and
    /// stateless: it never modifies runtime state.
    /// </summary>
    /// <remarks>
    /// Scoring model (all deterministic, all configurable through
    /// <see cref="PriorityPolicy"/>):
    /// <code>
    /// perception score = BasePriority + ConfidenceBonus - DistancePenalty * (1 - proximity)
    /// continuity score = BasePriority + MemoryBonus (victim objectives)
    ///                 or BasePriority               (other continuity objectives)
    /// cooldown         = CooldownPenalty subtracted from a hazard re-scored while
    ///                    the mission is already committed to avoiding
    /// </code>
    /// The formula applied to a candidate is chosen by its
    /// <see cref="CandidateOrigin"/>: perception-backed candidates use the candidate's
    /// proximity, continuity candidates use the memory bonus.
    /// </remarks>
    public sealed class PriorityEvaluator : IPriorityEvaluator
    {
        public TaskPriority[] Evaluate(TaskCandidate[] candidates, MissionTask current, PriorityPolicy policy)
        {
            var result = new TaskPriority[candidates.Length];

            for (var i = 0; i < candidates.Length; i++)
                result[i] = Score(candidates[i], current, policy);

            return result;
        }

        private static TaskPriority Score(TaskCandidate candidate, MissionTask current, PriorityPolicy policy)
        {
            switch (candidate.Origin)
            {
                case CandidateOrigin.CurrentPerception:
                    return PerceptionScore(candidate, current, policy);
                case CandidateOrigin.Continuity:
                    return ContinuityScore(candidate, policy);
                default:
                    return TaskPriority.Invalid;
            }
        }

        private static TaskPriority PerceptionScore(TaskCandidate candidate, MissionTask current, PriorityPolicy policy)
        {
            float score;

            switch (candidate.Task.State)
            {
                case MissionTaskState.AvoidHazard:
                    score = policy.HazardPriority + policy.ConfidenceBonus
                        - policy.DistancePenalty * (1f - candidate.Proximity);
                    break;
                case MissionTaskState.RescueVictim:
                case MissionTaskState.InvestigateTarget:
                    score = policy.VictimPriority + policy.ConfidenceBonus
                        - policy.DistancePenalty * (1f - candidate.Proximity);
                    break;
                default:
                    return TaskPriority.Invalid;
            }

            if (candidate.Task.State == MissionTaskState.AvoidHazard
                && current.State == MissionTaskState.AvoidHazard)
            {
                score -= policy.CooldownPenalty;
            }

            return new TaskPriority(candidate.Task, score, candidate.Confidence, candidate.Task.IsValid);
        }

        private static TaskPriority ContinuityScore(TaskCandidate candidate, PriorityPolicy policy)
        {
            float score;

            switch (candidate.Task.State)
            {
                case MissionTaskState.RescueVictim:
                case MissionTaskState.InvestigateTarget:
                    score = policy.VictimPriority + policy.MemoryBonus;
                    break;
                case MissionTaskState.AvoidHazard:
                    score = policy.HazardPriority;
                    break;
                case MissionTaskState.ResumeSearch:
                    score = policy.ResumePriority;
                    break;
                case MissionTaskState.SearchArea:
                    score = policy.SearchPriority;
                    break;
                case MissionTaskState.Idle:
                    score = policy.IdlePriority;
                    break;
                default:
                    return TaskPriority.Invalid;
            }

            return new TaskPriority(candidate.Task, score, candidate.Confidence, candidate.Task.IsValid);
        }
    }
}
