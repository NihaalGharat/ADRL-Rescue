namespace ADRL.AI.Decision.Prioritization
{
    /// <summary>
    /// Why a candidate objective is currently available. It is the single piece of
    /// evidence the scoring layer needs to choose the correct scoring formula, and
    /// it is determined solely by the <see cref="TaskCandidateGenerator"/> - never by
    /// the scorer.
    /// </summary>
    public enum CandidateOrigin
    {
        /// <summary>The objective is backed by current perception.</summary>
        CurrentPerception,

        /// <summary>
        /// The objective is the coordinator's current task carried forward so a
        /// neutral situation never fabricates a competing objective.
        /// </summary>
        Continuity,
    }
}
