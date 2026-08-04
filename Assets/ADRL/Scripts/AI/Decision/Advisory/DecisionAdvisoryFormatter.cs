namespace ADRL.AI.Decision.Advisory
{
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Pure, deterministic formatter that renders a
    /// <see cref="DecisionAdvisorySnapshot"/> into a fixed-format decision
    /// advisory report. It is a stateless function of the snapshot's fields: the
    /// same advisory always produces byte-identical output (enums and strings
    /// verbatim, floats rendered with the invariant culture and fixed column
    /// widths, no timestamps). Formatting is observational only - it never mutates
    /// the snapshot and never influences decisions.
    /// </summary>
    /// <example>
    /// Decision Advisory Report
    /// Overall Recommendation.IncreaseKnowledgeCoverage
    /// Advisory Confidence.......75.0
    /// High Priority
    ///   IncreaseKnowledgeCoverage | Knowledge coverage is critical -&gt; Increase knowledge gathering during search | 75
    /// Medium Priority
    /// Low Priority
    /// Summary
    /// Total Recommendations.....1
    /// Requires Attention.......True
    /// Status...................Critical
    /// </example>
    public static class DecisionAdvisoryFormatter
    {
        /// <summary>Width of the dotted label column.</summary>
        public const int LabelWidth = 22;

        /// <summary>
        /// Renders the advisory snapshot as a deterministic multi-line report.
        /// Allocates only the final string.
        /// </summary>
        public static string Format(DecisionAdvisorySnapshot advisory)
        {
            var output = new StringBuilder();
            output.AppendLine("Decision Advisory Report");
            AppendLine(output, "Overall Recommendation", advisory.OverallRecommendation.ToString());
            AppendLine(
                output,
                "Advisory Confidence",
                advisory.AdvisoryConfidence.ToString("F1", CultureInfo.InvariantCulture));
            AppendSection(output, "High Priority", advisory, DecisionRecommendationPriority.High);
            AppendSection(output, "Medium Priority", advisory, DecisionRecommendationPriority.Medium);
            AppendSection(output, "Low Priority", advisory, DecisionRecommendationPriority.Low);
            output.AppendLine("Summary");
            AppendLine(
                output,
                "Total Recommendations",
                advisory.Recommendations.Length.ToString(CultureInfo.InvariantCulture));
            AppendLine(output, "Requires Attention", advisory.RequiresAttention ? "True" : "False");
            AppendLine(output, "Status", advisory.Status ?? string.Empty);
            return output.ToString();
        }

        private static void AppendSection(
            StringBuilder output,
            string title,
            DecisionAdvisorySnapshot advisory,
            DecisionRecommendationPriority priority)
        {
            output.AppendLine(title);
            for (var i = 0; i < advisory.Recommendations.Length; i++)
            {
                var recommendation = advisory.Recommendations[i];
                if (recommendation.Priority != priority)
                    continue;

                output.Append("  ");
                output.AppendLine(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} | {1} -> {2} | {3:F0}",
                        recommendation.Type,
                        recommendation.Reason,
                        recommendation.SuggestedAction,
                        recommendation.Confidence));
            }
        }

        private static void AppendLine(StringBuilder output, string label, string value)
        {
            output.Append(label.PadRight(LabelWidth, '.'));
            output.AppendLine(value);
        }
    }
}
