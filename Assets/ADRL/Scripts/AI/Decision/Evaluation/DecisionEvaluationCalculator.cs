namespace ADRL.AI.Decision.Evaluation
{
    using ADRL.AI.Decision.Analytics;
    using ADRL.AI.Decision.Explainability;
    using ADRL.AI.Decision.Telemetry;
    using ADRL.AI.Decision.Trace;

    /// <summary>
    /// Pure, stateless calculator that converts the decision pipeline's completed
    /// observations - the <see cref="DecisionAnalyticsSnapshot"/>, the
    /// <see cref="DecisionTelemetrySnapshot"/>, the latest
    /// <see cref="DecisionTraceFrame"/> and the latest
    /// <see cref="DecisionExplanation"/> - into an immutable
    /// <see cref="DecisionEvaluationSnapshot"/> describing the quality of the
    /// decisions made. It is the single owner of evaluation computation: it reads
    /// the four inputs only, never caches, allocates nothing beyond the returned
    /// snapshot, and never influences decisions. Deterministic by construction -
    /// the same inputs always yield the same evaluation.
    /// </summary>
    /// <remarks>
    /// Derivation: behaviour suitability and mission suitability come from the
    /// dominant behaviour/mission shares of the telemetry distribution, confidence
    /// quality from the analytics decision-confidence average, optimization
    /// benefit from the analytics optimization-confidence average, knowledge
    /// coverage from the analytics knowledge-utilization rate and consistency from
    /// the complement of the analytics behaviour-balance score. The overall score
    /// is the weighted combination of the six components via
    /// <see cref="DecisionEvaluationMetrics"/>, and the grade/status follow the
    /// canonical bands. The evaluated step and timestamp come from the latest trace
    /// frame (falling back to the analytics clocks). Before any decision (a
    /// zero-count analytics input) the calculator returns the canonical
    /// <see cref="DecisionEvaluationSnapshot.Empty"/>. Null or truncated inputs
    /// degrade gracefully, never exceptions.
    /// </remarks>
    public static class DecisionEvaluationCalculator
    {
        /// <summary>
        /// Computes the immutable evaluation snapshot for the given completed
        /// observations. O(n) where n is the enum count of the telemetry
        /// distributions. Never allocates beyond the returned snapshot.
        /// </summary>
        public static DecisionEvaluationSnapshot Calculate(
            DecisionAnalyticsSnapshot analytics,
            DecisionTelemetrySnapshot telemetry,
            DecisionTraceFrame trace,
            DecisionExplanation explanation)
        {
            if (analytics.DecisionCount <= 0)
                return DecisionEvaluationSnapshot.Empty;

            var behaviourSuitability = DecisionEvaluationMetrics.BehaviourSuitability(
                DominantShare(analytics.DecisionCount, telemetry.BehaviourDistribution));
            var missionSuitability = DecisionEvaluationMetrics.MissionSuitability(
                DominantShare(analytics.DecisionCount, telemetry.MissionDistribution));
            var confidenceQuality = DecisionEvaluationMetrics.ConfidenceQuality(
                analytics.AverageDecisionConfidence / 100f);
            var optimizationBenefit = DecisionEvaluationMetrics.OptimizationBenefit(
                analytics.AverageOptimizationConfidence / 100f);
            var knowledgeCoverage = DecisionEvaluationMetrics.KnowledgeCoverage(
                analytics.KnowledgeUtilizationRate);
            var consistency = DecisionEvaluationMetrics.Consistency(
                analytics.BehaviourBalanceScore);

            var quality = DecisionEvaluationMetrics.OverallQuality(
                behaviourSuitability,
                missionSuitability,
                confidenceQuality,
                optimizationBenefit,
                knowledgeCoverage,
                consistency);

            var step = trace != null ? trace.DecisionStep : analytics.DecisionStep;
            var timestamp = trace != null ? trace.DecisionTimestamp : analytics.DecisionTimestamp;

            return new DecisionEvaluationSnapshot(
                step,
                timestamp,
                quality,
                behaviourSuitability,
                confidenceQuality,
                optimizationBenefit,
                missionSuitability,
                knowledgeCoverage,
                consistency,
                DecisionEvaluationMetrics.Grade(quality),
                DecisionEvaluationMetrics.Status(quality));
        }

        private static float DominantShare(int decisionCount, DecisionBehaviourCount[] distribution)
        {
            if (decisionCount <= 0)
                return 0f;

            var maxCount = 0;
            if (distribution != null)
            {
                for (var i = 0; i < distribution.Length; i++)
                {
                    if (distribution[i].Count > maxCount)
                        maxCount = distribution[i].Count;
                }
            }

            return maxCount / (float)decisionCount * 100f;
        }

        private static float DominantShare(int decisionCount, DecisionMissionCount[] distribution)
        {
            if (decisionCount <= 0)
                return 0f;

            var maxCount = 0;
            if (distribution != null)
            {
                for (var i = 0; i < distribution.Length; i++)
                {
                    if (distribution[i].Count > maxCount)
                        maxCount = distribution[i].Count;
                }
            }

            return maxCount / (float)decisionCount * 100f;
        }
    }
}
