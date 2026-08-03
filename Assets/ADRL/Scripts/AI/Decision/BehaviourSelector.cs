namespace ADRL.AI.Decision
{
    using ADRL.AI.Decision.Memory;

    /// <summary>
    /// Deterministic behaviour-selection policy. It ranks the assessed situation
    /// against the <see cref="DecisionContext"/> thresholds and picks the highest
    /// priority state that applies. Safety (avoiding an imminent obstacle) is
    /// ranked above approaching a target; a detected target beats searching; a
    /// non-viable or empty assessment falls back to search/idle.
    /// </summary>
    /// <remarks>
    /// The memory-aware overload preserves behaviour continuity across consecutive
    /// frames without ever letting memory override current perception: current
    /// sensor data always wins. Only when the current situation is neutral does
    /// memory tip the decision - a recently seen victim keeps the drone
    /// approaching until the memory expires, and a recent obstacle keeps the drone
    /// avoiding briefly (preventing left/right oscillation) before resuming search.
    /// </remarks>
    public sealed class BehaviourSelector : IBehaviourSelector
    {
        private readonly DecisionContext _context;

        public BehaviourSelector(DecisionContext context)
        {
            _context = context;
        }

        /// <summary>Legacy, memory-free selection; delegates with an empty memory.</summary>
        public BehaviourState Select(SituationSnapshot assessment)
        {
            return Select(assessment, BehaviourMemory.Empty);
        }

        public BehaviourState Select(SituationSnapshot assessment, BehaviourMemory memory)
        {
            if (!assessment.IsValid)
                return BehaviourState.Idle;

            var obstacleImminent = assessment.ObstacleProximity >= _context.AvoidThreshold;

            // Current perception always has priority over memory.
            if (obstacleImminent)
                return BehaviourState.Avoid;

            if (assessment.TargetDetected)
                return BehaviourState.Approach;

            // Continuity only when the current situation is neutral.
            if (memory.LastVictimSeen.IsValid)
                return BehaviourState.Approach;

            if (memory.LastObstacleSeen.IsValid && memory.LastBehaviour == BehaviourState.Avoid)
                return BehaviourState.Avoid;

            return BehaviourState.Search;
        }
    }
}
