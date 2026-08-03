namespace ADRL.AI.Decision.Trace
{
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Explainability;

    /// <summary>
    /// The single owner of <see cref="DecisionTraceFrame"/> composition. It is a
    /// pure, stateless, deterministic function of the built
    /// <see cref="DecisionContextSnapshot"/> and its
    /// <see cref="DecisionExplanation"/>: given the same inputs it always produces
    /// the same trace frame, and it holds no state between calls. It reads both
    /// inputs only - it never decides, scores, selects, optimizes, executes or
    /// mutates runtime state - so tracing is strictly observational and never
    /// influences the decision chain.
    /// </summary>
    /// <remarks>
    /// The builder projects the already-completed decision chain into the frame:
    /// the explanation supplies the step, timestamp and narrative reasons, the
    /// snapshot supplies the situation, mission, behaviour, optimized profile,
    /// command, synchronized diagnostics, winning objective, candidate count and
    /// the knowledge/memory record counts. Counts and executor names are read
    /// null-safely so a directly constructed snapshot can never crash a diagnostic
    /// path.
    /// </remarks>
    public sealed class DecisionTraceBuilder
    {
        /// <summary>
        /// Builds the immutable trace frame of the given snapshot and its
        /// explanation.
        /// </summary>
        public DecisionTraceFrame Build(DecisionContextSnapshot snapshot, DecisionExplanation explanation)
        {
            return new DecisionTraceFrame(
                explanation.DecisionStep,
                explanation.DecisionTimestamp,
                snapshot.Assessment,
                snapshot.Mission,
                snapshot.Behaviour,
                snapshot.ExecutionProfile,
                snapshot.Command,
                explanation,
                snapshot.Diagnostics,
                snapshot.Winning,
                snapshot.Candidates != null ? snapshot.Candidates.Length : 0,
                snapshot.Knowledge != null ? snapshot.Knowledge.Count : 0,
                snapshot.Memory != null ? snapshot.Memory.Count : 0,
                snapshot.RuntimeState.CurrentExecutor ?? string.Empty,
                snapshot.ExecutionProfile.ExecutionConfidence,
                explanation.Reasons);
        }
    }
}
