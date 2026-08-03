namespace ADRL.AI.Decision
{
    /// <summary>
    /// Immutable, allocation-free snapshot of the decision framework's running
    /// state. Diagnostic only - it never influences future decisions.
    /// </summary>
    public readonly struct DecisionDiagnostics
    {
        /// <summary>Total number of steps carried out so far.</summary>
        public readonly int StepCount;

        /// <summary>Most recently selected behaviour.</summary>
        public readonly BehaviourState LastBehaviour;

        /// <summary>Most recent assessment the selector reasoned over.</summary>
        public readonly SituationSnapshot LastAssessment;

        public DecisionDiagnostics(int stepCount, BehaviourState lastBehaviour, SituationSnapshot lastAssessment)
        {
            StepCount = stepCount;
            LastBehaviour = lastBehaviour;
            LastAssessment = lastAssessment;
        }

        /// <summary>An idle, zero-step diagnostics payload.</summary>
        public static DecisionDiagnostics Empty => new(0, BehaviourState.Idle, SituationSnapshot.Invalid);
    }
}