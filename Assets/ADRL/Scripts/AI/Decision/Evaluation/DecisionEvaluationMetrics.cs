namespace ADRL.AI.Decision.Evaluation
{
    using UnityEngine;

    /// <summary>
    /// Pure, stateless evaluation helpers: the single owner of decision-quality
    /// metric math. Every helper is a deterministic function of its inputs only,
    /// returns a percentage in [0, 100] (clamped, NaN and infinity degrade to 0)
    /// and never touches runtime state, so the same inputs always yield the same
    /// scores. The helpers are consumed by the
    /// <see cref="DecisionEvaluationCalculator"/> and are also directly testable.
    /// </summary>
    /// <remarks>
    /// The six components answer the evaluation questions: behaviour suitability
    /// (was the selected behaviour appropriate? - measured as how decisively one
    /// behaviour dominated selection), mission suitability (was mission selection
    /// appropriate? - the dominant objective share), confidence quality (was
    /// confidence reasonable? - the average decision confidence), optimization
    /// benefit (was optimization beneficial? - the optimized profile's execution
    /// confidence), knowledge coverage (was sufficient knowledge available? - the
    /// knowledge utilization rate) and consistency (was the decision consistent? -
    /// the complement of behaviour-distribution evenness). The overall quality is
    /// the weighted combination of the six components and the grade/status follow
    /// the canonical bands.
    /// </remarks>
    public static class DecisionEvaluationMetrics
    {
        /// <summary>Weight of the behaviour-suitability component.</summary>
        public const float BehaviourSuitabilityWeight = 0.25f;

        /// <summary>Weight of the mission-suitability component.</summary>
        public const float MissionSuitabilityWeight = 0.20f;

        /// <summary>Weight of the confidence-quality component.</summary>
        public const float ConfidenceQualityWeight = 0.15f;

        /// <summary>Weight of the optimization-benefit component.</summary>
        public const float OptimizationBenefitWeight = 0.10f;

        /// <summary>Weight of the knowledge-coverage component.</summary>
        public const float KnowledgeCoverageWeight = 0.15f;

        /// <summary>Weight of the consistency component.</summary>
        public const float ConsistencyWeight = 0.15f;

        /// <summary>
        /// Behaviour suitability from the share of the dominant behaviour, in
        /// [0, 100]. The more decisively one behaviour dominated selection, the
        /// more appropriate the committed selection is considered.
        /// </summary>
        public static float BehaviourSuitability(float dominantBehaviourShare)
        {
            return ClampPercentage(dominantBehaviourShare);
        }

        /// <summary>
        /// Mission suitability from the share of the dominant mission objective,
        /// in [0, 100].
        /// </summary>
        public static float MissionSuitability(float dominantMissionShare)
        {
            return ClampPercentage(dominantMissionShare);
        }

        /// <summary>
        /// Confidence quality from the average decision confidence, in [0, 1];
        /// scaled to [0, 100]. Higher average confidence means more reasonable
        /// decisions.
        /// </summary>
        public static float ConfidenceQuality(float averageDecisionConfidence)
        {
            return ClampPercentage(averageDecisionConfidence * 100f);
        }

        /// <summary>
        /// Optimization benefit from the optimized profile's execution confidence,
        /// in [0, 1]; scaled to [0, 100]. The optimized execution retaining high
        /// confidence means the optimization was beneficial.
        /// </summary>
        public static float OptimizationBenefit(float optimizationConfidence)
        {
            return ClampPercentage(optimizationConfidence * 100f);
        }

        /// <summary>
        /// Knowledge coverage from the knowledge-utilization rate, in [0, 100].
        /// </summary>
        public static float KnowledgeCoverage(float knowledgeUtilizationRate)
        {
            return ClampPercentage(knowledgeUtilizationRate);
        }

        /// <summary>
        /// Consistency from the behaviour-distribution balance (evenness), in
        /// [0, 100]: the complement of evenness, so a distribution dominated by
        /// one behaviour is fully consistent while a uniform distribution is not.
        /// </summary>
        public static float Consistency(float behaviourBalanceScore)
        {
            return ClampPercentage(100f - behaviourBalanceScore);
        }

        /// <summary>
        /// The overall decision quality score, in [0, 100], as the weighted sum
        /// of the six component scores (all in [0, 100]), clamped to [0, 100].
        /// </summary>
        public static float OverallQuality(
            float behaviourSuitability,
            float missionSuitability,
            float confidenceQuality,
            float optimizationBenefit,
            float knowledgeCoverage,
            float consistency)
        {
            var score = BehaviourSuitabilityWeight * behaviourSuitability
                + MissionSuitabilityWeight * missionSuitability
                + ConfidenceQualityWeight * confidenceQuality
                + OptimizationBenefitWeight * optimizationBenefit
                + KnowledgeCoverageWeight * knowledgeCoverage
                + ConsistencyWeight * consistency;

            return ClampPercentage(score);
        }

        /// <summary>
        /// The canonical grade of a quality score in [0, 100]: Excellent at 90+,
        /// Good at 75+, Fair at 60+, Poor at 40+, Critical below 40.
        /// </summary>
        public static string Grade(float qualityScore)
        {
            if (qualityScore >= 90f)
                return "Excellent";
            if (qualityScore >= 75f)
                return "Good";
            if (qualityScore >= 60f)
                return "Fair";
            if (qualityScore >= 40f)
                return "Poor";
            return "Critical";
        }

        /// <summary>
        /// The status band of a quality score in [0, 100], using the same
        /// thresholds as <see cref="Grade"/> but expressed as a
        /// <see cref="DecisionEvaluationStatus"/>.
        /// </summary>
        public static DecisionEvaluationStatus Status(float qualityScore)
        {
            if (qualityScore >= 90f)
                return DecisionEvaluationStatus.Optimal;
            if (qualityScore >= 75f)
                return DecisionEvaluationStatus.Good;
            if (qualityScore >= 60f)
                return DecisionEvaluationStatus.Acceptable;
            if (qualityScore >= 40f)
                return DecisionEvaluationStatus.Deficient;
            return DecisionEvaluationStatus.Critical;
        }

        private static float ClampPercentage(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 0f;

            return Mathf.Clamp(value, 0f, 100f);
        }
    }
}
