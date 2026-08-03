namespace ADRL.AI.Decision.Prioritization
{
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// The single runtime owner of objective arbitration. It selects the winner from
    /// a supplied set of scored candidate objectives: the highest-priority
    /// <see cref="TaskPriority"/> wins. It performs no transitions, generates no
    /// movement, owns no memory, generates no candidates and scores nothing - the
    /// <see cref="TaskCandidateGenerator"/> owns availability and the
    /// <see cref="IPriorityEvaluator"/> owns scoring. The <c>DecisionEngine</c>
    /// remains the only runtime decision authority.
    /// </summary>
    /// <remarks>
    /// Deterministic by construction: the winner is the first candidate with the
    /// maximum score (ties within <see cref="PriorityPolicy.PriorityTieTolerance"/>
    /// resolve to the earlier candidate), so the same inputs always yield the same
    /// winner. The overload taking raw assessment, memory and mission inputs is kept
    /// for backward compatibility; it runs the generation and scoring pipeline before
    /// delegating to <see cref="Select(TaskPriority[])"/>.
    /// </remarks>
    public sealed class TaskPrioritizer
    {
        private readonly IPriorityEvaluator _evaluator;
        private readonly PriorityPolicy _policy;
        private readonly TaskCandidateGenerator _generator;

        public TaskPrioritizer(IPriorityEvaluator evaluator, PriorityPolicy policy, TaskCandidateGenerator generator = null)
        {
            _evaluator = evaluator;
            _policy = policy;
            _generator = generator ?? new TaskCandidateGenerator();
        }

        /// <summary>
        /// Returns the highest-priority candidate from the supplied scored set.
        /// Invalid candidates are ignored. This is the sole arbitration authority.
        /// </summary>
        public TaskPriority Select(TaskPriority[] candidates)
        {
            var winner = TaskPriority.Invalid;
            var best = float.NegativeInfinity;

            foreach (var candidate in candidates)
            {
                if (!candidate.IsValid)
                    continue;

                if (candidate.PriorityScore > best + _policy.PriorityTieTolerance)
                {
                    best = candidate.PriorityScore;
                    winner = candidate;
                }
            }

            return winner;
        }

        /// <summary>
        /// Backward-compatible convenience overload: generates the currently available
        /// candidates, scores them and returns the highest-priority winner for the
        /// given situation, behaviour memory, mission task and mission policy.
        /// </summary>
        public TaskPriority Select(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask current,
            MissionPolicy missionPolicy)
        {
            var candidates = _generator.Generate(assessment, memory, current, missionPolicy);
            var scored = _evaluator.Evaluate(candidates, current, _policy);
            return Select(scored);
        }
    }
}
