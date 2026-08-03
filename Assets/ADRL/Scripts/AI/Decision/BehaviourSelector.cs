namespace ADRL.AI.Decision
{
    /// <summary>
    /// Deterministic behaviour-selection policy. It ranks the assessed situation
    /// against the <see cref="DecisionContext"/> thresholds and picks the highest
    /// priority state that applies. Safety (avoiding an imminent obstacle) is
    /// ranked above approaching a target; a detected target beats searching; a
    /// non-viable or empty assessment falls back to search/idle.
    /// </summary>
    public sealed class BehaviourSelector : IBehaviourSelector
    {
        private readonly DecisionContext _context;

        public BehaviourSelector(DecisionContext context)
        {
            _context = context;
        }

        public BehaviourState Select(SituationSnapshot assessment)
        {
            if (!assessment.IsValid)
                return BehaviourState.Idle;

            var obstacleImminent = assessment.ObstacleProximity >= _context.AvoidThreshold;

            if (obstacleImminent)
                return BehaviourState.Avoid;

            if (assessment.TargetDetected)
                return BehaviourState.Approach;

            return BehaviourState.Search;
        }
    }
}