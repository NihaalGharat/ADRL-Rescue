namespace ADRL.AI.Decision.Telemetry
{
    using ADRL.AI.Decision.Trace;

    /// <summary>
    /// The single runtime owner of decision telemetry. It observes every completed
    /// decision frame after the engine has traced it and produces immutable
    /// <see cref="DecisionTelemetrySnapshot"/> projections describing the health
    /// and performance of the autonomous decision pipeline. No other class computes
    /// telemetry. It is strictly read-only with respect to the runtime: it never
    /// modifies the engine, the snapshot, the explanation, the trace, knowledge,
    /// memory, behaviour or the command, and it never influences decisions.
    /// </summary>
    /// <remarks>
    /// The update path is O(1) and allocation-free - each observed frame only
    /// advances running totals in the internal
    /// <see cref="DecisionTelemetryStatistics"/> accumulator. A snapshot (which
    /// materializes the distribution arrays) is built only when exported through
    /// <see cref="GetSnapshot"/>, so the steady-state per-decision path allocates
    /// nothing. Deterministic by construction: the identical observed sequence
    /// always produces the identical snapshot.
    /// </remarks>
    public sealed class DecisionTelemetryCollector
    {
        private readonly DecisionTelemetryStatistics _statistics;

        public DecisionTelemetryCollector()
        {
            _statistics = new DecisionTelemetryStatistics();
        }

        /// <summary>
        /// Records one completed decision frame. O(1), allocation-free, never
        /// throws. Read-only with respect to the runtime.
        /// </summary>
        public void Observe(DecisionTraceFrame frame)
        {
            UpdateStatistics(frame);
        }

        /// <summary>
        /// The immutable snapshot of all telemetry observed so far, as a fresh
        /// owned projection (its distribution arrays are built on demand and never
        /// shared for mutation).
        /// </summary>
        public DecisionTelemetrySnapshot GetSnapshot()
        {
            return BuildSnapshot();
        }

        /// <summary>Clears all accumulated telemetry, restoring a fresh state.</summary>
        public void Reset()
        {
            _statistics.Reset();
        }

        private void UpdateStatistics(DecisionTraceFrame frame)
        {
            _statistics.Observe(frame);
        }

        private DecisionTelemetrySnapshot BuildSnapshot()
        {
            return new DecisionTelemetrySnapshot(
                _statistics.DecisionCount,
                _statistics.AverageConfidence,
                _statistics.AverageOptimizationConfidence,
                _statistics.AverageCandidateCount,
                _statistics.AverageSpeedMultiplier,
                _statistics.AverageTurnMultiplier,
                _statistics.KnowledgeCount,
                _statistics.MemoryCount,
                _statistics.LatestBehaviour,
                _statistics.LatestMission,
                _statistics.LatestExecutor,
                _statistics.LatestStep,
                _statistics.LatestTimestamp,
                _statistics.BuildBehaviourDistribution(),
                _statistics.BuildMissionDistribution());
        }
    }
}
