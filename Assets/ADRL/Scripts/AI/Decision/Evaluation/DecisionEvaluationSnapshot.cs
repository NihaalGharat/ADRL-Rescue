namespace ADRL.AI.Decision.Evaluation
{
    /// <summary>
    /// Deterministic quality status of the decision pipeline, derived from the
    /// overall <see cref="DecisionEvaluationSnapshot.DecisionQualityScore"/>
    /// using the same bands as the grade (Optimal at 90+, Good at 75+, Acceptable
    /// at 60+, Deficient at 40+, Critical below 40). A convenience enum alongside
    /// the human-readable grade string for programmatic inspection.
    /// </summary>
    public enum DecisionEvaluationStatus
    {
        /// <summary>Overall quality in [90, 100].</summary>
        Optimal,

        /// <summary>Overall quality in [75, 90).</summary>
        Good,

        /// <summary>Overall quality in [60, 75).</summary>
        Acceptable,

        /// <summary>Overall quality in [40, 60).</summary>
        Deficient,

        /// <summary>Overall quality below 40.</summary>
        Critical,
    }

    /// <summary>
    /// Immutable, deterministic snapshot of the decision pipeline's evaluation:
    /// a pure projection of the decision analytics, telemetry, trace and
    /// explanation that answers "how good was the decision?". It contains the
    /// overall quality score and grade plus the six component scores (behaviour
    /// suitability, confidence quality, optimization benefit, mission suitability,
    /// knowledge coverage and consistency). It is strictly read-only - it
    /// observes completed decisions only and never influences decision making,
    /// prioritization, mission selection, optimization or execution. All members
    /// are readonly.
    /// </summary>
    /// <remarks>
    /// Every score is a percentage in [0, 100] and is derived deterministically
    /// from the immutable analytics/telemetry inputs by the
    /// <see cref="DecisionEvaluationCalculator"/> through the pure helpers in
    /// <see cref="DecisionEvaluationMetrics"/>. The overall score is the weighted
    /// combination of the six components, the grade follows the canonical table
    /// (90-100 Excellent, 75-89 Good, 60-74 Fair, 40-59 Poor, 0-39 Critical) and
    /// <see cref="OverallStatus"/> carries the same bands as an enum. The same
    /// inputs always yield the same evaluation. <see cref="Empty"/> is the
    /// canonical pre-observation snapshot.
    /// </remarks>
    public readonly struct DecisionEvaluationSnapshot
    {
        /// <summary>The deterministic step clock of the latest evaluated decision.</summary>
        public readonly int DecisionStep;

        /// <summary>The deterministic timestamp of the latest evaluated decision.</summary>
        public readonly float DecisionTimestamp;

        /// <summary>Overall decision quality score, in [0, 100].</summary>
        public readonly float DecisionQualityScore;

        /// <summary>How appropriate the selected behaviour was, in [0, 100].</summary>
        public readonly float BehaviourSuitabilityScore;

        /// <summary>How reasonable the decision confidence was, in [0, 100].</summary>
        public readonly float ConfidenceQualityScore;

        /// <summary>How beneficial the behaviour optimization was, in [0, 100].</summary>
        public readonly float OptimizationBenefitScore;

        /// <summary>How appropriate the mission selection was, in [0, 100].</summary>
        public readonly float MissionSuitabilityScore;

        /// <summary>How much knowledge was available to decide, in [0, 100].</summary>
        public readonly float KnowledgeCoverageScore;

        /// <summary>How consistent the behaviour selection was, in [0, 100].</summary>
        public readonly float ConsistencyScore;

        /// <summary>
        /// The human-readable overall grade: Excellent (90-100), Good (75-89),
        /// Fair (60-74), Poor (40-59) or Critical (0-39).
        /// </summary>
        public readonly string EvaluationGrade;

        /// <summary>The overall quality status as an enum band.</summary>
        public readonly DecisionEvaluationStatus OverallStatus;

        public DecisionEvaluationSnapshot(
            int decisionStep,
            float decisionTimestamp,
            float decisionQualityScore,
            float behaviourSuitabilityScore,
            float confidenceQualityScore,
            float optimizationBenefitScore,
            float missionSuitabilityScore,
            float knowledgeCoverageScore,
            float consistencyScore,
            string evaluationGrade,
            DecisionEvaluationStatus overallStatus)
        {
            DecisionStep = decisionStep;
            DecisionTimestamp = decisionTimestamp;
            DecisionQualityScore = decisionQualityScore;
            BehaviourSuitabilityScore = behaviourSuitabilityScore;
            ConfidenceQualityScore = confidenceQualityScore;
            OptimizationBenefitScore = optimizationBenefitScore;
            MissionSuitabilityScore = missionSuitabilityScore;
            KnowledgeCoverageScore = knowledgeCoverageScore;
            ConsistencyScore = consistencyScore;
            EvaluationGrade = evaluationGrade ?? string.Empty;
            OverallStatus = overallStatus;
        }

        /// <summary>
        /// True when the snapshot is structurally sound: the grade string is
        /// present and every score, step and timestamp is non-negative. The
        /// canonical <see cref="Empty"/> snapshot is valid.
        /// </summary>
        public bool IsValid =>
            EvaluationGrade != null
            && EvaluationGrade.Length > 0
            && DecisionStep >= 0
            && DecisionTimestamp >= 0f
            && DecisionQualityScore >= 0f
            && BehaviourSuitabilityScore >= 0f
            && ConfidenceQualityScore >= 0f
            && OptimizationBenefitScore >= 0f
            && MissionSuitabilityScore >= 0f
            && KnowledgeCoverageScore >= 0f
            && ConsistencyScore >= 0f;

        /// <summary>An empty, pre-observation evaluation snapshot.</summary>
        public static DecisionEvaluationSnapshot Empty => _empty;

        private static readonly DecisionEvaluationSnapshot _empty = new(
            0,
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            "Critical",
            DecisionEvaluationStatus.Critical);
    }
}
