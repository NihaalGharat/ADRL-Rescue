namespace ADRL.AI.Decision.Prioritization
{
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// Immutable snapshot of an objective that is currently available. It couples
    /// the candidate <see cref="Task"/> with the <see cref="Origin"/> that made it
    /// available, plus the deterministic evidence (<see cref="Proximity"/> and
    /// <see cref="Confidence"/>) the scorer consumes. Produced only by the
    /// <see cref="TaskCandidateGenerator"/> and consumed only by the
    /// <see cref="PriorityEvaluator"/>; never mutated in place.
    /// </summary>
    /// <remarks>
    /// The generator is the only owner of <em>what is available</em> and of the
    /// evidence behind it; the scorer is the only owner of <em>how it is scored</em>.
    /// This snapshot is the contract between them.
    /// </remarks>
    public readonly struct TaskCandidate
    {
        /// <summary>The candidate objective itself.</summary>
        public readonly MissionTask Task;

        /// <summary>Whether the candidate is perception-backed or a continuity carry-over.</summary>
        public readonly CandidateOrigin Origin;

        /// <summary>
        /// The assessed proximity backing a perception candidate (0 for continuity
        /// candidates), used for the deterministic distance penalty.
        /// </summary>
        public readonly float Proximity;

        /// <summary>
        /// Evidence confidence backing the candidate in (0, 1]: 1 for current
        /// perception, the decaying memory confidence for a remembered victim.
        /// </summary>
        public readonly float Confidence;

        public TaskCandidate(MissionTask task, CandidateOrigin origin, float proximity, float confidence)
        {
            Task = task;
            Origin = origin;
            Proximity = proximity;
            Confidence = confidence;
        }
    }
}
