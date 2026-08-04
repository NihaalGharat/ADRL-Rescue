namespace ADRL.AI.Decision.Analytics
{
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// Pure validator that confirms a <see cref="DecisionAnalyticsSnapshot"/> is
    /// structurally sound and internally consistent: no negatives, no NaN, no
    /// infinity, health score in [0, 100], finite entropy, balance in [0, 100],
    /// utilization in [0, 100], confidence in [0, 100] and a complete snapshot
    /// whose analytics count totals match the decision count. It returns plain
    /// bools and never throws, so it can be used safely on any snapshot including
    /// the canonical <see cref="DecisionAnalyticsSnapshot.Empty"/>. Validation is
    /// observational only - it never mutates analytics and never influences
    /// decisions.
    /// </summary>
    public static class DecisionAnalyticsValidator
    {
        private static readonly BehaviourState[] BehaviourValues =
            (BehaviourState[])System.Enum.GetValues(typeof(BehaviourState));

        private static readonly MissionTaskState[] MissionValues =
            (MissionTaskState[])System.Enum.GetValues(typeof(MissionTaskState));

        /// <summary>
        /// True when every individual validation passes: no negatives, no NaN, no
        /// infinity, health score in range, entropy finite, balance in range,
        /// utilization in range, confidence in range and snapshot complete.
        /// </summary>
        public static bool IsValid(DecisionAnalyticsSnapshot analytics)
        {
            return NoNegatives(analytics)
                && NoNaN(analytics)
                && NoInfinity(analytics)
                && HealthScoreInRange(analytics)
                && EntropyFinite(analytics)
                && BalanceInRange(analytics)
                && UtilizationInRange(analytics)
                && ConfidenceInRange(analytics)
                && SnapshotComplete(analytics);
        }

        /// <summary>True when no count, share, score, rate, entropy or health value is negative.</summary>
        public static bool NoNegatives(DecisionAnalyticsSnapshot analytics)
        {
            return analytics.DecisionCount >= 0
                && analytics.DecisionStep >= 0
                && analytics.DecisionTimestamp >= 0f
                && analytics.AverageDecisionConfidence >= 0f
                && analytics.AverageExecutionConfidence >= 0f
                && analytics.AverageOptimizationConfidence >= 0f
                && analytics.KnowledgeUtilizationRate >= 0f
                && analytics.MemoryUtilizationRate >= 0f
                && analytics.BehaviourBalanceScore >= 0f
                && analytics.MissionBalanceScore >= 0f
                && analytics.BehaviourEntropy >= 0f
                && analytics.MissionEntropy >= 0f
                && analytics.DecisionHealthScore >= 0f
                && SharesNonNegative(analytics);
        }

        /// <summary>True when no numeric field in the snapshot is NaN.</summary>
        public static bool NoNaN(DecisionAnalyticsSnapshot analytics)
        {
            return !float.IsNaN(analytics.DecisionTimestamp)
                && !float.IsNaN(analytics.AverageDecisionConfidence)
                && !float.IsNaN(analytics.AverageExecutionConfidence)
                && !float.IsNaN(analytics.AverageOptimizationConfidence)
                && !float.IsNaN(analytics.KnowledgeUtilizationRate)
                && !float.IsNaN(analytics.MemoryUtilizationRate)
                && !float.IsNaN(analytics.BehaviourBalanceScore)
                && !float.IsNaN(analytics.MissionBalanceScore)
                && !float.IsNaN(analytics.BehaviourEntropy)
                && !float.IsNaN(analytics.MissionEntropy)
                && !float.IsNaN(analytics.DecisionHealthScore)
                && SharesFinite(analytics);
        }

        /// <summary>True when no numeric field in the snapshot is infinity.</summary>
        public static bool NoInfinity(DecisionAnalyticsSnapshot analytics)
        {
            return !float.IsInfinity(analytics.DecisionTimestamp)
                && !float.IsInfinity(analytics.AverageDecisionConfidence)
                && !float.IsInfinity(analytics.AverageExecutionConfidence)
                && !float.IsInfinity(analytics.AverageOptimizationConfidence)
                && !float.IsInfinity(analytics.KnowledgeUtilizationRate)
                && !float.IsInfinity(analytics.MemoryUtilizationRate)
                && !float.IsInfinity(analytics.BehaviourBalanceScore)
                && !float.IsInfinity(analytics.MissionBalanceScore)
                && !float.IsInfinity(analytics.BehaviourEntropy)
                && !float.IsInfinity(analytics.MissionEntropy)
                && !float.IsInfinity(analytics.DecisionHealthScore)
                && SharesFinite(analytics);
        }

        /// <summary>True when the health score is in [0, 100].</summary>
        public static bool HealthScoreInRange(DecisionAnalyticsSnapshot analytics)
        {
            return analytics.DecisionHealthScore >= 0f
                && analytics.DecisionHealthScore <= 100f;
        }

        /// <summary>True when both entropy values are non-negative and finite.</summary>
        public static bool EntropyFinite(DecisionAnalyticsSnapshot analytics)
        {
            return analytics.BehaviourEntropy >= 0f
                && analytics.MissionEntropy >= 0f
                && !float.IsNaN(analytics.BehaviourEntropy)
                && !float.IsNaN(analytics.MissionEntropy)
                && !float.IsInfinity(analytics.BehaviourEntropy)
                && !float.IsInfinity(analytics.MissionEntropy);
        }

        /// <summary>True when both balance scores are in [0, 100].</summary>
        public static bool BalanceInRange(DecisionAnalyticsSnapshot analytics)
        {
            return analytics.BehaviourBalanceScore >= 0f
                && analytics.BehaviourBalanceScore <= 100f
                && analytics.MissionBalanceScore >= 0f
                && analytics.MissionBalanceScore <= 100f;
        }

        /// <summary>True when both utilization rates are in [0, 100].</summary>
        public static bool UtilizationInRange(DecisionAnalyticsSnapshot analytics)
        {
            return analytics.KnowledgeUtilizationRate >= 0f
                && analytics.KnowledgeUtilizationRate <= 100f
                && analytics.MemoryUtilizationRate >= 0f
                && analytics.MemoryUtilizationRate <= 100f;
        }

        /// <summary>True when every confidence average is in [0, 100].</summary>
        public static bool ConfidenceInRange(DecisionAnalyticsSnapshot analytics)
        {
            return analytics.AverageDecisionConfidence >= 0f
                && analytics.AverageDecisionConfidence <= 100f
                && analytics.AverageExecutionConfidence >= 0f
                && analytics.AverageExecutionConfidence <= 100f
                && analytics.AverageOptimizationConfidence >= 0f
                && analytics.AverageOptimizationConfidence <= 100f;
        }

        /// <summary>
        /// True when the snapshot's analytics arrays are present, contain one entry
        /// per enum value in enum order and their count totals both equal the
        /// decision count.
        /// </summary>
        public static bool SnapshotComplete(DecisionAnalyticsSnapshot analytics)
        {
            if (analytics.BehaviourAnalytics == null || analytics.MissionAnalytics == null)
                return false;

            if (analytics.BehaviourAnalytics.Length != BehaviourValues.Length)
                return false;

            if (analytics.MissionAnalytics.Length != MissionValues.Length)
                return false;

            return TotalOf(analytics.BehaviourAnalytics) == analytics.DecisionCount
                && TotalOf(analytics.MissionAnalytics) == analytics.DecisionCount;
        }

        private static bool SharesNonNegative(DecisionAnalyticsSnapshot analytics)
        {
            if (analytics.BehaviourAnalytics != null)
            {
                for (var i = 0; i < analytics.BehaviourAnalytics.Length; i++)
                {
                    var entry = analytics.BehaviourAnalytics[i];
                    if (entry.Count < 0 || entry.Share < 0f)
                        return false;
                }
            }

            if (analytics.MissionAnalytics != null)
            {
                for (var i = 0; i < analytics.MissionAnalytics.Length; i++)
                {
                    var entry = analytics.MissionAnalytics[i];
                    if (entry.Count < 0 || entry.Share < 0f)
                        return false;
                }
            }

            return true;
        }

        private static bool SharesFinite(DecisionAnalyticsSnapshot analytics)
        {
            if (analytics.BehaviourAnalytics != null)
            {
                for (var i = 0; i < analytics.BehaviourAnalytics.Length; i++)
                {
                    if (!IsFinite(analytics.BehaviourAnalytics[i].Share))
                        return false;
                }
            }

            if (analytics.MissionAnalytics != null)
            {
                for (var i = 0; i < analytics.MissionAnalytics.Length; i++)
                {
                    if (!IsFinite(analytics.MissionAnalytics[i].Share))
                        return false;
                }
            }

            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static long TotalOf(BehaviourAnalytics[] analytics)
        {
            long total = 0;
            for (var i = 0; i < analytics.Length; i++)
                total += analytics[i].Count;
            return total;
        }

        private static long TotalOf(MissionAnalytics[] analytics)
        {
            long total = 0;
            for (var i = 0; i < analytics.Length; i++)
                total += analytics[i].Count;
            return total;
        }
    }
}
