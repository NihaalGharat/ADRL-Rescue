namespace ADRL.AI.Decision.Advisory
{
    /// <summary>
    /// How serious the underlying condition behind a
    /// <see cref="DecisionRecommendation"/> is. Distinct from
    /// <see cref="DecisionRecommendationPriority"/>: priority describes urgency of
    /// action, severity describes the seriousness of the condition the
    /// recommendation responds to. <see cref="None"/> marks conditions that carry
    /// no adverse state (e.g. maintaining a sound strategy).
    /// </summary>
    public enum DecisionRecommendationSeverity
    {
        /// <summary>No adverse condition is present.</summary>
        None,

        /// <summary>A minor, low-impact condition.</summary>
        Low,

        /// <summary>A moderate condition worth addressing.</summary>
        Medium,

        /// <summary>A serious condition that should be addressed soon.</summary>
        High,

        /// <summary>A critical condition that warrants immediate attention.</summary>
        Critical,
    }
}
