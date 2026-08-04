namespace ADRL.AI.Decision.Evaluation
{
    /// <summary>
    /// Pure validator that confirms a <see cref="DecisionEvaluationSnapshot"/> is
    /// structurally sound and internally consistent: no negatives, no NaN, no
    /// infinity, every score in [0, 100], a grade that matches the overall score
    /// and a status that matches it too, and a complete snapshot. It returns plain
    /// bools and never throws, so it can be used safely on any snapshot including
    /// the canonical <see cref="DecisionEvaluationSnapshot.Empty"/>. Validation is
    /// observational only - it never mutates evaluation and never influences
    /// decisions.
    /// </summary>
    public static class DecisionEvaluationValidator
    {
        /// <summary>
        /// True when every individual validation passes: no negatives, no NaN, no
        /// infinity, scores in range, grade valid, status valid and snapshot
        /// complete.
        /// </summary>
        public static bool IsValid(DecisionEvaluationSnapshot evaluation)
        {
            return NoNegatives(evaluation)
                && NoNaN(evaluation)
                && NoInfinity(evaluation)
                && ScoresInRange(evaluation)
                && GradeValid(evaluation)
                && StatusValid(evaluation)
                && SnapshotComplete(evaluation);
        }

        /// <summary>True when no score, step or timestamp is negative.</summary>
        public static bool NoNegatives(DecisionEvaluationSnapshot evaluation)
        {
            return evaluation.DecisionStep >= 0
                && evaluation.DecisionTimestamp >= 0f
                && evaluation.DecisionQualityScore >= 0f
                && evaluation.BehaviourSuitabilityScore >= 0f
                && evaluation.ConfidenceQualityScore >= 0f
                && evaluation.OptimizationBenefitScore >= 0f
                && evaluation.MissionSuitabilityScore >= 0f
                && evaluation.KnowledgeCoverageScore >= 0f
                && evaluation.ConsistencyScore >= 0f;
        }

        /// <summary>True when no numeric field in the snapshot is NaN.</summary>
        public static bool NoNaN(DecisionEvaluationSnapshot evaluation)
        {
            return !float.IsNaN(evaluation.DecisionTimestamp)
                && !float.IsNaN(evaluation.DecisionQualityScore)
                && !float.IsNaN(evaluation.BehaviourSuitabilityScore)
                && !float.IsNaN(evaluation.ConfidenceQualityScore)
                && !float.IsNaN(evaluation.OptimizationBenefitScore)
                && !float.IsNaN(evaluation.MissionSuitabilityScore)
                && !float.IsNaN(evaluation.KnowledgeCoverageScore)
                && !float.IsNaN(evaluation.ConsistencyScore);
        }

        /// <summary>True when no numeric field in the snapshot is infinity.</summary>
        public static bool NoInfinity(DecisionEvaluationSnapshot evaluation)
        {
            return !float.IsInfinity(evaluation.DecisionTimestamp)
                && !float.IsInfinity(evaluation.DecisionQualityScore)
                && !float.IsInfinity(evaluation.BehaviourSuitabilityScore)
                && !float.IsInfinity(evaluation.ConfidenceQualityScore)
                && !float.IsInfinity(evaluation.OptimizationBenefitScore)
                && !float.IsInfinity(evaluation.MissionSuitabilityScore)
                && !float.IsInfinity(evaluation.KnowledgeCoverageScore)
                && !float.IsInfinity(evaluation.ConsistencyScore);
        }

        /// <summary>True when every score is in [0, 100].</summary>
        public static bool ScoresInRange(DecisionEvaluationSnapshot evaluation)
        {
            return evaluation.DecisionQualityScore >= 0f
                && evaluation.DecisionQualityScore <= 100f
                && evaluation.BehaviourSuitabilityScore >= 0f
                && evaluation.BehaviourSuitabilityScore <= 100f
                && evaluation.ConfidenceQualityScore >= 0f
                && evaluation.ConfidenceQualityScore <= 100f
                && evaluation.OptimizationBenefitScore >= 0f
                && evaluation.OptimizationBenefitScore <= 100f
                && evaluation.MissionSuitabilityScore >= 0f
                && evaluation.MissionSuitabilityScore <= 100f
                && evaluation.KnowledgeCoverageScore >= 0f
                && evaluation.KnowledgeCoverageScore <= 100f
                && evaluation.ConsistencyScore >= 0f
                && evaluation.ConsistencyScore <= 100f;
        }

        /// <summary>
        /// True when the grade is present and matches the overall quality score
        /// through the canonical grade table.
        /// </summary>
        public static bool GradeValid(DecisionEvaluationSnapshot evaluation)
        {
            return evaluation.EvaluationGrade != null
                && DecisionEvaluationMetrics.Grade(evaluation.DecisionQualityScore) == evaluation.EvaluationGrade;
        }

        /// <summary>
        /// True when the status matches the overall quality score through the
        /// canonical status bands.
        /// </summary>
        public static bool StatusValid(DecisionEvaluationSnapshot evaluation)
        {
            return DecisionEvaluationMetrics.Status(evaluation.DecisionQualityScore) == evaluation.OverallStatus;
        }

        /// <summary>
        /// True when the snapshot is complete: a non-empty grade string and
        /// non-negative step and timestamp clocks.
        /// </summary>
        public static bool SnapshotComplete(DecisionEvaluationSnapshot evaluation)
        {
            return evaluation.EvaluationGrade != null
                && evaluation.EvaluationGrade.Length > 0
                && evaluation.DecisionStep >= 0
                && evaluation.DecisionTimestamp >= 0f;
        }
    }
}
