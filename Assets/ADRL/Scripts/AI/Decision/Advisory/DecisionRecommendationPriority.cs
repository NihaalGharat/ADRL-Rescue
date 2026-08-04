namespace ADRL.AI.Decision.Advisory
{
    /// <summary>
    /// How urgently a <see cref="DecisionRecommendation"/> should be acted on.
    /// The calculator emits recommendations with an explicit priority and the
    /// snapshot always presents them highest-priority-first, so the formatter can
    /// group them into High/Medium/Low sections. <see cref="None"/> is used for
    /// non-actionable recommendations such as <c>MaintainCurrentStrategy</c>.
    /// </summary>
    public enum DecisionRecommendationPriority
    {
        /// <summary>No action is required (e.g. maintain the current strategy).</summary>
        None,

        /// <summary>Act at leisure - the condition is informational.</summary>
        Low,

        /// <summary>Act soon - the condition deserves attention.</summary>
        Medium,

        /// <summary>Act now - the condition is significant.</summary>
        High,
    }
}
