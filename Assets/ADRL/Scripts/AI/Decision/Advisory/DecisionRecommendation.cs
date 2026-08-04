namespace ADRL.AI.Decision.Advisory
{
    /// <summary>
    /// Immutable, deterministic advisory recommendation produced by the
    /// <see cref="DecisionAdvisoryCalculator"/>. It carries the advice type, its
    /// priority and severity, the human-readable reason, the suggested action, the
    /// confidence of the recommendation (a percentage in [0, 100]) and the
    /// deterministic step and timestamp of the decision it refers to. It is
    /// strictly read-only - it recommends only and never modifies runtime
    /// behaviour, decisions, prioritization, mission selection, optimization or
    /// execution. All members are readonly.
    /// </summary>
    public readonly struct DecisionRecommendation
    {
        /// <summary>The kind of recommendation being made.</summary>
        public readonly DecisionRecommendationType Type;

        /// <summary>How urgently the recommendation should be acted on.</summary>
        public readonly DecisionRecommendationPriority Priority;

        /// <summary>How serious the underlying condition is.</summary>
        public readonly DecisionRecommendationSeverity Severity;

        /// <summary>Human-readable reason for the recommendation.</summary>
        public readonly string Reason;

        /// <summary>Human-readable suggested action.</summary>
        public readonly string SuggestedAction;

        /// <summary>Confidence of the recommendation, a percentage in [0, 100].</summary>
        public readonly float Confidence;

        /// <summary>The deterministic step clock of the decision the recommendation refers to.</summary>
        public readonly int DecisionStep;

        /// <summary>The deterministic timestamp of the decision the recommendation refers to.</summary>
        public readonly float Timestamp;

        public DecisionRecommendation(
            DecisionRecommendationType type,
            DecisionRecommendationPriority priority,
            DecisionRecommendationSeverity severity,
            string reason,
            string suggestedAction,
            float confidence,
            int decisionStep,
            float timestamp)
        {
            Type = type;
            Priority = priority;
            Severity = severity;
            Reason = reason ?? string.Empty;
            SuggestedAction = suggestedAction ?? string.Empty;
            Confidence = confidence;
            DecisionStep = decisionStep;
            Timestamp = timestamp;
        }

        /// <summary>
        /// True when the recommendation is structurally sound: a defined type, a
        /// present reason and suggested action, a confidence in [0, 100] and
        /// non-negative step and timestamp clocks.
        /// </summary>
        public bool IsValid =>
            Type >= DecisionRecommendationType.None
            && Type <= DecisionRecommendationType.ReviewMissionAllocation
            && Reason != null
            && SuggestedAction != null
            && Confidence >= 0f
            && Confidence <= 100f
            && DecisionStep >= 0
            && Timestamp >= 0f;
    }
}
