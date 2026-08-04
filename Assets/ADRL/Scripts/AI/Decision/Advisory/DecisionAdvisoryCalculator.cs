namespace ADRL.AI.Decision.Advisory
{
    using System.Collections.Generic;
    using ADRL.AI.Decision.Analytics;
    using ADRL.AI.Decision.Evaluation;
    using ADRL.AI.Decision.Explainability;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Telemetry;
    using ADRL.AI.Decision.Trace;
    using UnityEngine;

    /// <summary>
    /// Pure, stateless calculator that converts the decision pipeline's completed
    /// observations - the <see cref="DecisionEvaluationSnapshot"/>, the
    /// <see cref="DecisionAnalyticsSnapshot"/>, the
    /// <see cref="DecisionTelemetrySnapshot"/>, the latest
    /// <see cref="DecisionTraceFrame"/> and the latest
    /// <see cref="DecisionExplanation"/> - into an immutable
    /// <see cref="DecisionAdvisorySnapshot"/> recommending what an operator should
    /// do about the decisions made. It is the single owner of advisory computation:
    /// it reads the five inputs only, never caches, allocates nothing beyond the
    /// returned snapshot and its recommendations, and never influences decisions.
    /// Deterministic by construction - the same inputs always yield the same
    /// advisory.
    /// </summary>
    /// <remarks>
    /// Rules (evaluated in a fixed order, each signal maps to at most one
    /// recommendation type): excellent overall quality (90+) yields
    /// <c>MaintainCurrentStrategy</c>; critically low knowledge coverage (&lt; 30)
    /// yields <c>IncreaseKnowledgeCoverage</c>; low knowledge coverage (&lt; 50)
    /// yields <c>ReviewKnowledgeCoverage</c>; low mission suitability (&lt; 50)
    /// yields <c>ReviewMissionAllocation</c>; unbalanced mission distribution
    /// (&gt; 80) yields <c>IncreaseMissionPriority</c>; unbalanced behaviour
    /// distribution (&gt; 80) yields <c>IncreaseSearchPersistence</c>; low
    /// optimization benefit (&lt; 50) yields <c>ReviewOptimization</c>; low
    /// confidence quality (&lt; 50) yields <c>ReviewSensorConfidence</c>; an
    /// active search mission with low knowledge coverage yields
    /// <c>IncreaseSearchRadius</c>; a high execution speed multiplier (&gt; 1.1)
    /// yields <c>ReduceSpeedMultiplier</c>; a known hazard near the drone
    /// (&lt; 15 world units) while not avoiding yields <c>IncreaseObstacleAvoidance</c>.
    /// When no rule fires the advisory falls back to <c>MaintainCurrentStrategy</c>.
    /// Recommendations are ordered highest-priority-first (ties broken by type
    /// order) and the overall recommendation is the first one.
    /// </remarks>
    public static class DecisionAdvisoryCalculator
    {
        /// <summary>Knowledge coverage below which coverage is considered critical.</summary>
        public const float CriticalKnowledgeCoverage = 30f;

        /// <summary>Knowledge coverage below which coverage is considered low.</summary>
        public const float LowKnowledgeCoverage = 50f;

        /// <summary>Behaviour/mission balance above which the distribution is considered unbalanced.</summary>
        public const float UnbalancedDistribution = 80f;

        /// <summary>Component scores below which the component is considered low.</summary>
        public const float LowComponentScore = 50f;

        /// <summary>Mission suitability below which allocation is considered low.</summary>
        public const float LowMissionAllocation = 50f;

        /// <summary>Average execution speed multiplier above which speed is considered high.</summary>
        public const float HighSpeedMultiplier = 1.1f;

        /// <summary>Nearest known-hazard distance below which the hazard is considered near.</summary>
        public const float NearHazardDistance = 15f;

        /// <summary>Overall quality at or above which the strategy is excellent.</summary>
        public const float ExcellentQuality = 90f;

        /// <summary>
        /// Computes the immutable advisory snapshot for the given completed
        /// observations. O(n) where n is the enum count of the telemetry
        /// distributions. Allocates only the returned snapshot and its
        /// recommendations.
        /// </summary>
        public static DecisionAdvisorySnapshot Calculate(
            DecisionEvaluationSnapshot evaluation,
            DecisionAnalyticsSnapshot analytics,
            DecisionTelemetrySnapshot telemetry,
            DecisionTraceFrame trace,
            DecisionExplanation explanation)
        {
            if (analytics.DecisionCount <= 0 || evaluation.DecisionStep <= 0)
                return DecisionAdvisorySnapshot.Empty;

            var step = trace != null ? trace.DecisionStep : evaluation.DecisionStep;
            var timestamp = trace != null ? trace.DecisionTimestamp : evaluation.DecisionTimestamp;

            var recommendations = new List<DecisionRecommendation>();
            var maxPriority = DecisionRecommendationPriority.None;

            if (evaluation.DecisionQualityScore >= ExcellentQuality)
            {
                AddRecommendation(
                    recommendations,
                    ref maxPriority,
                    DecisionRecommendationType.MaintainCurrentStrategy,
                    DecisionRecommendationPriority.None,
                    DecisionRecommendationSeverity.None,
                    "Current decision quality is sound",
                    "Maintain the current strategy",
                    evaluation.DecisionQualityScore,
                    step,
                    timestamp);
            }
            else
            {
                var knowledgeCoverage = evaluation.KnowledgeCoverageScore;
                if (knowledgeCoverage < CriticalKnowledgeCoverage)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.IncreaseKnowledgeCoverage,
                        DecisionRecommendationPriority.High,
                        DecisionRecommendationSeverity.Critical,
                        "Knowledge coverage is critical",
                        "Increase knowledge gathering during search",
                        ClampPercentage(100f - knowledgeCoverage),
                        step,
                        timestamp);
                }
                else if (knowledgeCoverage < LowKnowledgeCoverage)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.ReviewKnowledgeCoverage,
                        DecisionRecommendationPriority.Medium,
                        DecisionRecommendationSeverity.Medium,
                        "Knowledge coverage is low",
                        "Review knowledge gathering coverage",
                        ClampPercentage(100f - knowledgeCoverage),
                        step,
                        timestamp);
                }

                if (evaluation.MissionSuitabilityScore < LowMissionAllocation)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.ReviewMissionAllocation,
                        DecisionRecommendationPriority.Medium,
                        DecisionRecommendationSeverity.Medium,
                        "Mission allocation is low",
                        "Review mission allocation across objectives",
                        ClampPercentage(100f - evaluation.MissionSuitabilityScore),
                        step,
                        timestamp);
                }

                if (analytics.MissionBalanceScore > UnbalancedDistribution)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.IncreaseMissionPriority,
                        DecisionRecommendationPriority.Medium,
                        DecisionRecommendationSeverity.High,
                        "Mission allocation is unbalanced",
                        "Increase focus on the priority mission",
                        ClampPercentage(analytics.MissionBalanceScore),
                        step,
                        timestamp);
                }

                if (analytics.BehaviourBalanceScore > UnbalancedDistribution)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.IncreaseSearchPersistence,
                        DecisionRecommendationPriority.Medium,
                        DecisionRecommendationSeverity.High,
                        "Behaviour selection is unbalanced",
                        "Increase search persistence",
                        ClampPercentage(analytics.BehaviourBalanceScore),
                        step,
                        timestamp);
                }

                if (evaluation.OptimizationBenefitScore < LowComponentScore)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.ReviewOptimization,
                        DecisionRecommendationPriority.Medium,
                        DecisionRecommendationSeverity.High,
                        "Optimization confidence is low",
                        "Review optimization of behaviour execution",
                        ClampPercentage(100f - evaluation.OptimizationBenefitScore),
                        step,
                        timestamp);
                }

                if (evaluation.ConfidenceQualityScore < LowComponentScore)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.ReviewSensorConfidence,
                        DecisionRecommendationPriority.Medium,
                        DecisionRecommendationSeverity.High,
                        "Decision confidence is low",
                        "Review sensor confidence and fusion quality",
                        ClampPercentage(100f - evaluation.ConfidenceQualityScore),
                        step,
                        timestamp);
                }

                var dominantMission = DominantMissionState(telemetry.MissionDistribution);
                if (IsSearchMission(dominantMission) && knowledgeCoverage < LowKnowledgeCoverage)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.IncreaseSearchRadius,
                        DecisionRecommendationPriority.Low,
                        DecisionRecommendationSeverity.Low,
                        "Searching with low knowledge coverage",
                        "Increase the search radius",
                        ClampPercentage(100f - knowledgeCoverage),
                        step,
                        timestamp);
                }

                if (telemetry.AverageExecutionSpeedMultiplier > HighSpeedMultiplier)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.ReduceSpeedMultiplier,
                        DecisionRecommendationPriority.Low,
                        DecisionRecommendationSeverity.Low,
                        "Execution speed is high",
                        "Reduce the speed multiplier",
                        ClampPercentage((telemetry.AverageExecutionSpeedMultiplier - 1f) * 100f),
                        step,
                        timestamp);
                }

                if (HazardNear(explanation) && telemetry.CurrentBehaviour != BehaviourState.Avoid)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.IncreaseObstacleAvoidance,
                        DecisionRecommendationPriority.High,
                        DecisionRecommendationSeverity.High,
                        "Hazard detected without avoidance",
                        "Increase obstacle avoidance behaviour",
                        ClampPercentage(100f - explanation.Knowledge.NearestHazardDistance),
                        step,
                        timestamp);
                }

                if (recommendations.Count == 0)
                {
                    AddRecommendation(
                        recommendations,
                        ref maxPriority,
                        DecisionRecommendationType.MaintainCurrentStrategy,
                        DecisionRecommendationPriority.None,
                        DecisionRecommendationSeverity.None,
                        "Current decision quality is sound",
                        "Maintain the current strategy",
                        evaluation.DecisionQualityScore,
                        step,
                        timestamp);
                }
            }

            recommendations.Sort(Compare);

            var array = recommendations.ToArray();
            var overall = array.Length > 0 ? array[0].Type : DecisionRecommendationType.None;
            var confidence = array.Length > 0 ? array[0].Confidence : 0f;

            return new DecisionAdvisorySnapshot(
                step,
                timestamp,
                overall,
                array,
                confidence,
                RequiresAttention(maxPriority),
                DecisionAdvisorySnapshot.StatusFor(maxPriority));
        }

        /// <summary>
        /// True when the advisory's strongest priority is Medium or High, i.e. an
        /// actionable recommendation exists.
        /// </summary>
        private static bool RequiresAttention(DecisionRecommendationPriority maxPriority)
        {
            return maxPriority == DecisionRecommendationPriority.Medium
                || maxPriority == DecisionRecommendationPriority.High;
        }

        private static void AddRecommendation(
            List<DecisionRecommendation> recommendations,
            ref DecisionRecommendationPriority maxPriority,
            DecisionRecommendationType type,
            DecisionRecommendationPriority priority,
            DecisionRecommendationSeverity severity,
            string reason,
            string suggestedAction,
            float confidence,
            int step,
            float timestamp)
        {
            recommendations.Add(
                new DecisionRecommendation(
                    type,
                    priority,
                    severity,
                    reason,
                    suggestedAction,
                    ClampPercentage(confidence),
                    step,
                    timestamp));

            if (priority > maxPriority)
                maxPriority = priority;
        }

        /// <summary>
        /// Total-order comparator: priority descending, then type ascending. Since
        /// no two recommendations in an advisory share a type, the order is strict
        /// and therefore independent of the sorting algorithm used.
        /// </summary>
        private static int Compare(DecisionRecommendation a, DecisionRecommendation b)
        {
            var priority = b.Priority.CompareTo(a.Priority);
            if (priority != 0)
                return priority;

            return a.Type.CompareTo(b.Type);
        }

        private static MissionTaskState DominantMissionState(DecisionMissionCount[] distribution)
        {
            var dominant = MissionTaskState.Idle;
            var maxCount = 0;
            if (distribution != null)
            {
                for (var i = 0; i < distribution.Length; i++)
                {
                    if (distribution[i].Count > maxCount)
                    {
                        maxCount = distribution[i].Count;
                        dominant = distribution[i].Mission;
                    }
                }
            }

            return dominant;
        }

        private static bool IsSearchMission(MissionTaskState state)
        {
            return state == MissionTaskState.SearchArea || state == MissionTaskState.ResumeSearch;
        }

        private static bool HazardNear(DecisionExplanation explanation)
        {
            return explanation.Knowledge.NearestHazardDistance > 0f
                && explanation.Knowledge.NearestHazardDistance < NearHazardDistance;
        }

        private static float ClampPercentage(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 0f;

            return Mathf.Clamp(value, 0f, 100f);
        }
    }
}
