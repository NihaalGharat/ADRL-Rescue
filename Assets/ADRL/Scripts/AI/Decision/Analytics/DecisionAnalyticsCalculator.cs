namespace ADRL.AI.Decision.Analytics
{
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Telemetry;
    using UnityEngine;

    /// <summary>
    /// Pure, stateless calculator that converts a
    /// <see cref="DecisionTelemetrySnapshot"/> into an immutable
    /// <see cref="DecisionAnalyticsSnapshot"/> describing the health of the
    /// decision pipeline. It is the single owner of analytics computation: it
    /// reads the telemetry snapshot only, never caches, allocates nothing beyond
    /// the returned snapshot and its analytics arrays, and never influences
    /// decisions. Deterministic by construction - the same telemetry always yields
    /// the same analytics.
    /// </summary>
    /// <remarks>
    /// Derivation: the confidence averages are the telemetry confidence averages
    /// scaled to percentages (the trace records the optimized execution profile's
    /// confidence as its optimization confidence, so the execution and
    /// optimization averages are the same signal). Utilization is the record count
    /// relative to the decision count (clamped to [0, 100]). Balance is the
    /// normalized Shannon entropy (evenness) of the behaviour/mission
    /// distributions, in [0, 100]; entropy is the unnormalized Shannon entropy
    /// (natural logarithm). The health score is the weighted combination defined
    /// by <see cref="DecisionHealthCalculator"/>. Null or truncated telemetry
    /// distributions degrade gracefully to zero-count analytics, never exceptions.
    /// </remarks>
    public static class DecisionAnalyticsCalculator
    {
        private static readonly BehaviourState[] BehaviourValues =
            (BehaviourState[])System.Enum.GetValues(typeof(BehaviourState));

        private static readonly MissionTaskState[] MissionValues =
            (MissionTaskState[])System.Enum.GetValues(typeof(MissionTaskState));

        /// <summary>
        /// Computes the immutable analytics snapshot for the given telemetry.
        /// O(n) where n is the enum count. Never allocates beyond the snapshot and
        /// its owned analytics arrays.
        /// </summary>
        public static DecisionAnalyticsSnapshot Calculate(DecisionTelemetrySnapshot telemetry)
        {
            var decisionCount = telemetry.DecisionCount;
            var decisionConfidence = Percentage(telemetry.AverageDecisionConfidence);
            var executionConfidence = Percentage(telemetry.AverageOptimizationConfidence);
            var optimizationConfidence = Percentage(telemetry.AverageOptimizationConfidence);
            var knowledgeUtilization = Utilization(decisionCount, telemetry.KnowledgeRecordCount);
            var memoryUtilization = Utilization(decisionCount, telemetry.MemoryRecordCount);
            var behaviourBalance = BalanceOf(decisionCount, telemetry.BehaviourDistribution, BehaviourValues.Length);
            var missionBalance = BalanceOf(decisionCount, telemetry.MissionDistribution, MissionValues.Length);
            var health = DecisionHealthCalculator.Calculate(
                decisionConfidence,
                executionConfidence,
                optimizationConfidence,
                knowledgeUtilization,
                memoryUtilization,
                behaviourBalance);

            return new DecisionAnalyticsSnapshot(
                decisionCount,
                decisionConfidence,
                executionConfidence,
                optimizationConfidence,
                knowledgeUtilization,
                memoryUtilization,
                behaviourBalance,
                missionBalance,
                EntropyOf(decisionCount, telemetry.BehaviourDistribution),
                EntropyOf(decisionCount, telemetry.MissionDistribution),
                health,
                telemetry.LatestDecisionStep,
                telemetry.LatestDecisionTimestamp,
                BuildBehaviourAnalytics(decisionCount, telemetry.BehaviourDistribution),
                BuildMissionAnalytics(decisionCount, telemetry.MissionDistribution));
        }

        private static float Percentage(float value)
        {
            return value * 100f;
        }

        private static float Utilization(int decisionCount, int recordCount)
        {
            if (decisionCount <= 0)
                return 0f;

            return Mathf.Clamp01(recordCount / (float)decisionCount) * 100f;
        }

        private static float ShareOf(int decisionCount, int count)
        {
            if (decisionCount <= 0)
                return 0f;

            return count / (float)decisionCount * 100f;
        }

        private static float EntropyOf(int decisionCount, DecisionBehaviourCount[] distribution)
        {
            if (decisionCount <= 0)
                return 0f;

            var entropy = 0f;
            if (distribution != null)
            {
                for (var i = 0; i < distribution.Length; i++)
                {
                    var count = distribution[i].Count;
                    if (count <= 0)
                        continue;

                    var share = count / (float)decisionCount;
                    entropy -= share * Mathf.Log(share);
                }
            }

            return entropy;
        }

        private static float EntropyOf(int decisionCount, DecisionMissionCount[] distribution)
        {
            if (decisionCount <= 0)
                return 0f;

            var entropy = 0f;
            if (distribution != null)
            {
                for (var i = 0; i < distribution.Length; i++)
                {
                    var count = distribution[i].Count;
                    if (count <= 0)
                        continue;

                    var share = count / (float)decisionCount;
                    entropy -= share * Mathf.Log(share);
                }
            }

            return entropy;
        }

        private static float BalanceOf(int decisionCount, DecisionBehaviourCount[] distribution, int categoryCount)
        {
            if (decisionCount <= 0)
                return 0f;

            if (categoryCount <= 1)
                return 100f;

            return Mathf.Clamp01(EntropyOf(decisionCount, distribution) / Mathf.Log(categoryCount)) * 100f;
        }

        private static float BalanceOf(int decisionCount, DecisionMissionCount[] distribution, int categoryCount)
        {
            if (decisionCount <= 0)
                return 0f;

            if (categoryCount <= 1)
                return 100f;

            return Mathf.Clamp01(EntropyOf(decisionCount, distribution) / Mathf.Log(categoryCount)) * 100f;
        }

        private static BehaviourAnalytics[] BuildBehaviourAnalytics(int decisionCount, DecisionBehaviourCount[] distribution)
        {
            var result = new BehaviourAnalytics[BehaviourValues.Length];
            for (var i = 0; i < result.Length; i++)
            {
                var count = distribution != null && i < distribution.Length
                    ? distribution[i].Count
                    : 0;

                result[i] = new BehaviourAnalytics(
                    BehaviourValues[i],
                    count,
                    ShareOf(decisionCount, count));
            }

            return result;
        }

        private static MissionAnalytics[] BuildMissionAnalytics(int decisionCount, DecisionMissionCount[] distribution)
        {
            var result = new MissionAnalytics[MissionValues.Length];
            for (var i = 0; i < result.Length; i++)
            {
                var count = distribution != null && i < distribution.Length
                    ? distribution[i].Count
                    : 0;

                result[i] = new MissionAnalytics(
                    MissionValues[i],
                    count,
                    ShareOf(decisionCount, count));
            }

            return result;
        }
    }
}
