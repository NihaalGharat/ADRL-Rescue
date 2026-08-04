namespace ADRL.AI.Decision.Evaluation
{
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Pure, deterministic formatter that renders a
    /// <see cref="DecisionEvaluationSnapshot"/> into a fixed-format decision
    /// quality report. It is a stateless function of the snapshot's fields: the
    /// same evaluation always produces byte-identical output (floats rendered
    /// with the invariant culture and fixed column widths, no timestamps).
    /// Formatting is observational only - it never mutates the snapshot and never
    /// influences decisions.
    /// </summary>
    /// <example>
    /// Decision Quality
    /// Overall Score...........92.5
    /// Grade..................Excellent
    /// Behaviour Suitability....95
    /// Mission Suitability.....90
    /// Optimization...........88
    /// Consistency...........100
    /// Knowledge..............91
    /// Confidence.............89
    /// </example>
    public static class DecisionEvaluationFormatter
    {
        /// <summary>Width of the dotted label column.</summary>
        public const int LabelWidth = 22;

        /// <summary>
        /// Renders the evaluation snapshot as a deterministic multi-line report.
        /// Allocates only the final string.
        /// </summary>
        public static string Format(DecisionEvaluationSnapshot evaluation)
        {
            var output = new StringBuilder();
            output.AppendLine("Decision Quality");
            AppendLine(output, "Overall Score", evaluation.DecisionQualityScore.ToString("F1", CultureInfo.InvariantCulture));
            AppendLine(output, "Grade", evaluation.EvaluationGrade ?? string.Empty);
            AppendLine(output, "Behaviour Suitability", evaluation.BehaviourSuitabilityScore.ToString("F0", CultureInfo.InvariantCulture));
            AppendLine(output, "Mission Suitability", evaluation.MissionSuitabilityScore.ToString("F0", CultureInfo.InvariantCulture));
            AppendLine(output, "Optimization", evaluation.OptimizationBenefitScore.ToString("F0", CultureInfo.InvariantCulture));
            AppendLine(output, "Consistency", evaluation.ConsistencyScore.ToString("F0", CultureInfo.InvariantCulture));
            AppendLine(output, "Knowledge", evaluation.KnowledgeCoverageScore.ToString("F0", CultureInfo.InvariantCulture));
            AppendLine(output, "Confidence", evaluation.ConfidenceQualityScore.ToString("F0", CultureInfo.InvariantCulture));
            return output.ToString();
        }

        private static void AppendLine(StringBuilder output, string label, string value)
        {
            output.Append(label.PadRight(LabelWidth, '.'));
            output.AppendLine(value);
        }
    }
}
