namespace ADRL.AI.Decision.Explainability
{
    /// <summary>
    /// Immutable value object representing one individual reasoning entry of a
    /// decision explanation - a human-readable statement of why a part of the
    /// decision chain happened (for example "Target detected (0.91)",
    /// "Mission RescueVictim", "Approach selected"). Produced only by the
    /// <see cref="DecisionExplanationBuilder"/> and consumed read-only by the
    /// <see cref="DecisionExplanationFormatter"/> and by validation. Never mutated
    /// in place.
    /// </summary>
    public readonly struct DecisionReason
    {
        /// <summary>The section of the explanation this reason belongs to.</summary>
        public readonly string Section;

        /// <summary>The human-readable reasoning text of this entry.</summary>
        public readonly string Text;

        /// <summary>True while this reason holds real, non-empty content.</summary>
        public readonly bool IsValid;

        public DecisionReason(string section, string text)
        {
            Section = section ?? string.Empty;
            Text = text ?? string.Empty;
            IsValid = Section.Length > 0 && Text.Length > 0;
        }

        /// <summary>An empty, unusable reasoning entry.</summary>
        public static DecisionReason Invalid => default;
    }
}
