namespace ADRL.AI.Decision.Advisory
{
    /// <summary>
    /// Pure validator that confirms a <see cref="DecisionAdvisorySnapshot"/> is
    /// structurally sound and internally consistent: no duplicate recommendation
    /// types, recommendations ordered highest-priority-first, every confidence in
    /// [0, 100], valid severities, valid recommendation types with the overall
    /// recommendation matching the strongest one, and a complete snapshot with a
    /// canonical status. It returns plain bools and never throws, so it can be
    /// used safely on any snapshot including the canonical
    /// <see cref="DecisionAdvisorySnapshot.Empty"/>. Validation is observational
    /// only - it never mutates the advisory and never influences decisions.
    /// </summary>
    public static class DecisionAdvisoryValidator
    {
        /// <summary>
        /// True when every individual validation passes: no duplicates, ordered
        /// priorities, in-range confidence, valid severities, valid types and a
        /// complete snapshot.
        /// </summary>
        public static bool IsValid(DecisionAdvisorySnapshot advisory)
        {
            return NoDuplicateRecommendations(advisory)
                && PriorityOrdered(advisory)
                && ConfidenceInRange(advisory)
                && SeverityValid(advisory)
                && RecommendationTypeValid(advisory)
                && SnapshotComplete(advisory);
        }

        /// <summary>True when no two recommendations share a recommendation type.</summary>
        public static bool NoDuplicateRecommendations(DecisionAdvisorySnapshot advisory)
        {
            for (var i = 0; i < advisory.Recommendations.Length; i++)
            {
                for (var j = i + 1; j < advisory.Recommendations.Length; j++)
                {
                    if (advisory.Recommendations[i].Type == advisory.Recommendations[j].Type)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// True when the recommendations are ordered highest-priority-first, i.e.
        /// no recommendation has a lower priority than the one after it.
        /// </summary>
        public static bool PriorityOrdered(DecisionAdvisorySnapshot advisory)
        {
            for (var i = 0; i + 1 < advisory.Recommendations.Length; i++)
            {
                if (advisory.Recommendations[i].Priority < advisory.Recommendations[i + 1].Priority)
                    return false;
            }

            return true;
        }

        /// <summary>True when every recommendation confidence and the advisory confidence are in [0, 100].</summary>
        public static bool ConfidenceInRange(DecisionAdvisorySnapshot advisory)
        {
            if (advisory.AdvisoryConfidence < 0f || advisory.AdvisoryConfidence > 100f)
                return false;

            for (var i = 0; i < advisory.Recommendations.Length; i++)
            {
                if (advisory.Recommendations[i].Confidence < 0f
                    || advisory.Recommendations[i].Confidence > 100f)
                    return false;
            }

            return true;
        }

        /// <summary>True when every recommendation severity is a defined severity value.</summary>
        public static bool SeverityValid(DecisionAdvisorySnapshot advisory)
        {
            for (var i = 0; i < advisory.Recommendations.Length; i++)
            {
                var severity = advisory.Recommendations[i].Severity;
                if (severity < DecisionRecommendationSeverity.None
                    || severity > DecisionRecommendationSeverity.Critical)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// True when every recommendation type is defined and actionable (not
        /// <see cref="DecisionRecommendationType.None"/>), the overall
        /// recommendation is defined, equals the strongest recommendation when
        /// recommendations exist, and is <see cref="DecisionRecommendationType.None"/>
        /// only when no recommendations exist.
        /// </summary>
        public static bool RecommendationTypeValid(DecisionAdvisorySnapshot advisory)
        {
            var overall = advisory.OverallRecommendation;
            if (overall < DecisionRecommendationType.None
                || overall > DecisionRecommendationType.ReviewMissionAllocation)
                return false;

            if (advisory.Recommendations.Length == 0)
                return overall == DecisionRecommendationType.None;

            if (overall == DecisionRecommendationType.None)
                return false;

            if (overall != advisory.Recommendations[0].Type)
                return false;

            for (var i = 0; i < advisory.Recommendations.Length; i++)
            {
                var type = advisory.Recommendations[i].Type;
                if (type <= DecisionRecommendationType.None
                    || type > DecisionRecommendationType.ReviewMissionAllocation)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// True when the snapshot is complete: the recommendations array is present,
        /// the step and timestamp clocks are non-negative and the status is a
        /// canonical advisory status.
        /// </summary>
        public static bool SnapshotComplete(DecisionAdvisorySnapshot advisory)
        {
            return advisory.Recommendations != null
                && advisory.DecisionStep >= 0
                && advisory.DecisionTimestamp >= 0f
                && IsCanonicalStatus(advisory.Status);
        }

        private static bool IsCanonicalStatus(string status)
        {
            return status == DecisionAdvisorySnapshot.NominalStatus
                || status == DecisionAdvisorySnapshot.AdvisoryStatus
                || status == DecisionAdvisorySnapshot.AttentionStatus
                || status == DecisionAdvisorySnapshot.CriticalStatus;
        }
    }
}
