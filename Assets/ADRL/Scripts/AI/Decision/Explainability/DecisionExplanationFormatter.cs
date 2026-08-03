namespace ADRL.AI.Decision.Explainability
{
    using System.Text;

    /// <summary>
    /// Pure, deterministic formatter that renders a <see cref="DecisionExplanation"/>
    /// into human-readable text. It is a stateless function of the explanation's
    /// narrative <see cref="DecisionExplanation.Reasons"/>: reasons are grouped by
    /// their section in the order they were composed, so the same explanation
    /// always produces byte-identical output. Formatting is observational only - it
    /// never mutates the explanation and never influences decisions.
    /// </summary>
    /// <example>
    /// Assessment
    /// ----------
    ///   Target detected (0.91)
    ///
    /// Mission
    /// -------
    ///   RescueVictim
    /// </example>
    public static class DecisionExplanationFormatter
    {
        /// <summary>
        /// Renders the explanation as a sectioned, deterministic multi-line string.
        /// </summary>
        public static string Format(DecisionExplanation explanation)
        {
            var output = new StringBuilder();
            string currentSection = null;

            foreach (var reason in explanation.Reasons)
            {
                if (!reason.IsValid)
                    continue;

                if (reason.Section != currentSection)
                {
                    if (output.Length > 0)
                        output.AppendLine();

                    currentSection = reason.Section;
                    output.AppendLine(reason.Section);
                    output.AppendLine(new string('-', reason.Section.Length));
                }

                output.AppendLine("  " + reason.Text);
            }

            return output.ToString();
        }
    }
}
