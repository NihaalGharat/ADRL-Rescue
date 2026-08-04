namespace ADRL.AI.Decision.Telemetry
{
    /// <summary>
    /// Pure validator that confirms a <see cref="DecisionTelemetrySnapshot"/> is
    /// structurally sound and internally consistent: no negatives, no NaN, no
    /// infinity, distribution totals matching the decision count, snapshot
    /// completeness, valid decision counts, in-range averages and valid current
    /// values. It returns plain bools and never throws, so it can be used safely
    /// on any snapshot including the canonical <see cref="DecisionTelemetrySnapshot.Empty"/>.
    /// Validation is observational only - it never mutates telemetry and never
    /// influences decisions.
    /// </summary>
    public static class DecisionTelemetryValidator
    {
        /// <summary>
        /// True when every individual validation passes: no negatives, no
        /// NaN/Infinity, distribution totals match, snapshot complete, counts
        /// valid, averages in range and current values valid.
        /// </summary>
        public static bool IsValid(DecisionTelemetrySnapshot telemetry)
        {
            return NoNegatives(telemetry)
                && NoNaNOrInfinity(telemetry)
                && DistributionTotalsMatch(telemetry)
                && SnapshotComplete(telemetry)
                && CountsValid(telemetry)
                && AveragesInRange(telemetry)
                && CurrentValuesValid(telemetry);
        }

        /// <summary>True when no count or average in the snapshot is negative.</summary>
        public static bool NoNegatives(DecisionTelemetrySnapshot telemetry)
        {
            return telemetry.DecisionCount >= 0
                && telemetry.KnowledgeRecordCount >= 0
                && telemetry.MemoryRecordCount >= 0
                && telemetry.LatestDecisionStep >= 0
                && telemetry.LatestDecisionTimestamp >= 0f
                && telemetry.AverageDecisionConfidence >= 0f
                && telemetry.AverageOptimizationConfidence >= 0f
                && telemetry.AverageCandidateCount >= 0f
                && telemetry.AverageExecutionSpeedMultiplier >= 0f
                && telemetry.AverageTurnRateMultiplier >= 0f
                && DistributionCountsNonNegative(telemetry);
        }

        /// <summary>True when no average or latest timestamp in the snapshot is NaN or infinity.</summary>
        public static bool NoNaNOrInfinity(DecisionTelemetrySnapshot telemetry)
        {
            return IsFinite(telemetry.AverageDecisionConfidence)
                && IsFinite(telemetry.AverageOptimizationConfidence)
                && IsFinite(telemetry.AverageCandidateCount)
                && IsFinite(telemetry.AverageExecutionSpeedMultiplier)
                && IsFinite(telemetry.AverageTurnRateMultiplier)
                && IsFinite(telemetry.LatestDecisionTimestamp);
        }

        /// <summary>
        /// True when every distribution count is non-negative and the behaviour and
        /// mission distribution totals both equal the snapshot's decision count.
        /// </summary>
        public static bool DistributionTotalsMatch(DecisionTelemetrySnapshot telemetry)
        {
            if (telemetry.BehaviourDistribution == null || telemetry.MissionDistribution == null)
                return false;

            if (TotalOf(telemetry.BehaviourDistribution) != telemetry.DecisionCount)
                return false;

            if (TotalOf(telemetry.MissionDistribution) != telemetry.DecisionCount)
                return false;

            return DistributionCountsNonNegative(telemetry);
        }

        /// <summary>True when the snapshot's reference payloads are present and complete.</summary>
        public static bool SnapshotComplete(DecisionTelemetrySnapshot telemetry)
        {
            return telemetry.CurrentExecutor != null
                && telemetry.BehaviourDistribution != null
                && telemetry.MissionDistribution != null;
        }

        /// <summary>True when the decision counts and latest clocks are non-negative.</summary>
        public static bool CountsValid(DecisionTelemetrySnapshot telemetry)
        {
            return telemetry.DecisionCount >= 0
                && telemetry.KnowledgeRecordCount >= 0
                && telemetry.MemoryRecordCount >= 0
                && telemetry.LatestDecisionStep >= 0;
        }

        /// <summary>
        /// True when every average is within its documented range: confidences in
        /// [0, 1], candidate count non-negative and execution multipliers
        /// non-negative.
        /// </summary>
        public static bool AveragesInRange(DecisionTelemetrySnapshot telemetry)
        {
            return telemetry.AverageDecisionConfidence >= 0f
                && telemetry.AverageDecisionConfidence <= 1f
                && telemetry.AverageOptimizationConfidence >= 0f
                && telemetry.AverageOptimizationConfidence <= 1f
                && telemetry.AverageCandidateCount >= 0f
                && telemetry.AverageExecutionSpeedMultiplier >= 0f
                && telemetry.AverageTurnRateMultiplier >= 0f;
        }

        /// <summary>True when the current-value projection holds a present executor.</summary>
        public static bool CurrentValuesValid(DecisionTelemetrySnapshot telemetry)
        {
            return telemetry.CurrentExecutor != null;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool DistributionCountsNonNegative(DecisionTelemetrySnapshot telemetry)
        {
            if (telemetry.BehaviourDistribution != null)
            {
                for (var i = 0; i < telemetry.BehaviourDistribution.Length; i++)
                {
                    if (telemetry.BehaviourDistribution[i].Count < 0)
                        return false;
                }
            }

            if (telemetry.MissionDistribution != null)
            {
                for (var i = 0; i < telemetry.MissionDistribution.Length; i++)
                {
                    if (telemetry.MissionDistribution[i].Count < 0)
                        return false;
                }
            }

            return true;
        }

        private static long TotalOf(DecisionBehaviourCount[] distribution)
        {
            long total = 0;
            for (var i = 0; i < distribution.Length; i++)
                total += distribution[i].Count;
            return total;
        }

        private static long TotalOf(DecisionMissionCount[] distribution)
        {
            long total = 0;
            for (var i = 0; i < distribution.Length; i++)
                total += distribution[i].Count;
            return total;
        }
    }
}
