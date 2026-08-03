namespace ADRL.AI.Decision.Prioritization
{
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// Computes deterministic priority scores for every supplied candidate objective.
    /// Pure and stateless: given the same candidates, mission task and policy it
    /// always returns the same scores, and it never modifies runtime state. Candidate
    /// availability is owned by <see cref="TaskCandidateGenerator"/>; this evaluator
    /// only converts <see cref="TaskCandidate"/> values into <see cref="TaskPriority"/>
    /// values.
    /// </summary>
    public interface IPriorityEvaluator
    {
        /// <summary>
        /// Score every supplied candidate, ordered deterministically. Unsupported
        /// candidates are emitted as <see cref="TaskPriority.Invalid"/> and are
        /// ignored by the <see cref="TaskPrioritizer"/>.
        /// </summary>
        TaskPriority[] Evaluate(TaskCandidate[] candidates, MissionTask current, PriorityPolicy policy);
    }
}
