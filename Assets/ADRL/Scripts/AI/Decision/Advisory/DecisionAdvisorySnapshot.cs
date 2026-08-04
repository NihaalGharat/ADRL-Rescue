namespace ADRL.AI.Decision.Advisory
{
    /// <summary>
    /// Immutable, deterministic snapshot of the decision pipeline's advisory: a
    /// pure projection of the decision evaluation, analytics, telemetry, trace and
    /// explanation that answers "what should the operator do about the decisions
    /// made?". It contains the overall recommendation type, the ordered
    /// recommendation list (highest priority first), the advisory confidence (a
    /// percentage in [0, 100]), the requires-attention flag and the deterministic
    /// status string (Nominal/Advisory/Attention/Critical). It is strictly
    /// read-only - it recommends only and never influences decision making,
    /// prioritization, mission selection, optimization or execution. All members
    /// are readonly.
    /// </summary>
    /// <remarks>
    /// The overall recommendation is the highest-priority recommendation, the
    /// advisory confidence is the confidence of that recommendation and the status
    /// follows the strongest priority present (<see cref="StatusFor"/>). The
    /// recommendations array is ordered highest-priority-first and never contains
    /// duplicate types. <see cref="Empty"/> is the canonical pre-observation
    /// snapshot.
    /// </remarks>
    public readonly struct DecisionAdvisorySnapshot
    {
        /// <summary>Canonical status when no actionable recommendation exists.</summary>
        public const string NominalStatus = "Nominal";

        /// <summary>Canonical status when only low-priority recommendations exist.</summary>
        public const string AdvisoryStatus = "Advisory";

        /// <summary>Canonical status when a medium-priority recommendation exists.</summary>
        public const string AttentionStatus = "Attention";

        /// <summary>Canonical status when a high-priority recommendation exists.</summary>
        public const string CriticalStatus = "Critical";

        /// <summary>The deterministic step clock of the latest advised decision.</summary>
        public readonly int DecisionStep;

        /// <summary>The deterministic timestamp of the latest advised decision.</summary>
        public readonly float DecisionTimestamp;

        /// <summary>The single most important recommendation of the latest advisory.</summary>
        public readonly DecisionRecommendationType OverallRecommendation;

        /// <summary>
        /// The ordered recommendations of the latest advisory, highest priority
        /// first, with no duplicate types. Owned copy; immutable elements.
        /// </summary>
        public readonly DecisionRecommendation[] Recommendations;

        /// <summary>Confidence of the overall recommendation, a percentage in [0, 100].</summary>
        public readonly float AdvisoryConfidence;

        /// <summary>True when an actionable (medium or high priority) recommendation exists.</summary>
        public readonly bool RequiresAttention;

        /// <summary>The deterministic status of the latest advisory.</summary>
        public readonly string Status;

        public DecisionAdvisorySnapshot(
            int decisionStep,
            float decisionTimestamp,
            DecisionRecommendationType overallRecommendation,
            DecisionRecommendation[] recommendations,
            float advisoryConfidence,
            bool requiresAttention,
            string status)
        {
            DecisionStep = decisionStep;
            DecisionTimestamp = decisionTimestamp;
            OverallRecommendation = overallRecommendation;
            Recommendations = recommendations ?? new DecisionRecommendation[0];
            AdvisoryConfidence = advisoryConfidence;
            RequiresAttention = requiresAttention;
            Status = status ?? NominalStatus;
        }

        /// <summary>
        /// True when the snapshot is structurally sound: the recommendations array
        /// is present, every clock is non-negative, the confidence is in [0, 100]
        /// and the status string is present. The canonical <see cref="Empty"/>
        /// snapshot is valid.
        /// </summary>
        public bool IsValid =>
            Recommendations != null
            && DecisionStep >= 0
            && DecisionTimestamp >= 0f
            && AdvisoryConfidence >= 0f
            && AdvisoryConfidence <= 100f
            && Status != null
            && Status.Length > 0
            && OverallRecommendation >= DecisionRecommendationType.None
            && OverallRecommendation <= DecisionRecommendationType.ReviewMissionAllocation;

        /// <summary>
        /// The canonical status of an advisory whose strongest priority is the
        /// given one: None stays Nominal, Low is Advisory, Medium is Attention and
        /// High is Critical.
        /// </summary>
        public static string StatusFor(DecisionRecommendationPriority maxPriority)
        {
            switch (maxPriority)
            {
                case DecisionRecommendationPriority.High:
                    return CriticalStatus;
                case DecisionRecommendationPriority.Medium:
                    return AttentionStatus;
                case DecisionRecommendationPriority.Low:
                    return AdvisoryStatus;
                default:
                    return NominalStatus;
            }
        }

        /// <summary>An empty, pre-observation advisory snapshot.</summary>
        public static DecisionAdvisorySnapshot Empty => _empty;

        private static readonly DecisionAdvisorySnapshot _empty = new(
            0,
            0f,
            DecisionRecommendationType.None,
            new DecisionRecommendation[0],
            0f,
            false,
            NominalStatus);
    }
}
