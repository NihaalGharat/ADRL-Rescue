namespace ADRL.AI.Decision.Analytics
{
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Pure, deterministic formatter that renders a
    /// <see cref="DecisionAnalyticsSnapshot"/> into a fixed-format engineering
    /// report describing the health of the decision system. It is a stateless
    /// function of the snapshot's fields: the same analytics always produces
    /// byte-identical output (floats rendered with the invariant culture and
    /// fixed column widths). Formatting is observational only - it never mutates
    /// the snapshot and never influences decisions.
    /// </summary>
    /// <example>
    /// Decision Analytics
    /// Health Score........92%
    /// Decision Confidence.89%
    /// Execution Confidence.95%
    /// Optimization........90%
    /// Knowledge Usage.....81%
    /// Memory Usage........77%
    /// Behaviour Balance...Good
    /// Mission Balance.....Excellent
    /// Behaviour Entropy...1.42
    /// Mission Entropy.....0.91
    /// Total Decisions.....271
    /// </example>
    public static class DecisionAnalyticsFormatter
    {
        /// <summary>Width of the dotted label column.</summary>
        public const int LabelWidth = 20;

        /// <summary>
        /// Renders the analytics snapshot as a deterministic multi-line report.
        /// Allocates only the final string.
        /// </summary>
        public static string Format(DecisionAnalyticsSnapshot analytics)
        {
            var output = new StringBuilder();
            output.AppendLine("Decision Analytics");
            AppendLine(output, "Health Score", analytics.DecisionHealthScore.ToString("F0", CultureInfo.InvariantCulture) + "%");
            AppendLine(output, "Decision Confidence", analytics.AverageDecisionConfidence.ToString("F0", CultureInfo.InvariantCulture) + "%");
            AppendLine(output, "Execution Confidence", analytics.AverageExecutionConfidence.ToString("F0", CultureInfo.InvariantCulture) + "%");
            AppendLine(output, "Optimization", analytics.AverageOptimizationConfidence.ToString("F0", CultureInfo.InvariantCulture) + "%");
            AppendLine(output, "Knowledge Usage", analytics.KnowledgeUtilizationRate.ToString("F0", CultureInfo.InvariantCulture) + "%");
            AppendLine(output, "Memory Usage", analytics.MemoryUtilizationRate.ToString("F0", CultureInfo.InvariantCulture) + "%");
            AppendLine(output, "Behaviour Balance", BalanceLabel(analytics.BehaviourBalanceScore));
            AppendLine(output, "Mission Balance", BalanceLabel(analytics.MissionBalanceScore));
            AppendLine(output, "Behaviour Entropy", analytics.BehaviourEntropy.ToString("F2", CultureInfo.InvariantCulture));
            AppendLine(output, "Mission Entropy", analytics.MissionEntropy.ToString("F2", CultureInfo.InvariantCulture));
            AppendLine(output, "Total Decisions", analytics.DecisionCount.ToString(CultureInfo.InvariantCulture));
            return output.ToString();
        }

        /// <summary>
        /// Deterministic textual rating of a balance score in [0, 100]:
        /// excellent at 80+, good at 60+, fair at 40+, poor at 20+ and very poor
        /// below 20.
        /// </summary>
        public static string BalanceLabel(float score)
        {
            if (score >= 80f)
                return "Excellent";
            if (score >= 60f)
                return "Good";
            if (score >= 40f)
                return "Fair";
            if (score >= 20f)
                return "Poor";
            return "Very Poor";
        }

        private static void AppendLine(StringBuilder output, string label, string value)
        {
            output.Append(label.PadRight(LabelWidth, '.'));
            output.AppendLine(value);
        }
    }
}
