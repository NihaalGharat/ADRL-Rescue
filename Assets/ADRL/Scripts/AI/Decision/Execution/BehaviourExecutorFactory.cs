namespace ADRL.AI.Decision.Execution
{
    /// <summary>
    /// The single owner of behaviour-to-executor resolution. Given a
    /// <see cref="BehaviourState"/> it returns the matching
    /// <see cref="IBehaviourExecutor"/>; no other system switches on behaviour to
    /// generate movement. Executors are built once from the
    /// <see cref="DecisionContext"/> and reused, keeping generation config-driven
    /// and deterministic.
    /// </summary>
    public sealed class BehaviourExecutorFactory
    {
        private readonly IBehaviourExecutor _idle;
        private readonly IBehaviourExecutor _search;
        private readonly IBehaviourExecutor _approach;
        private readonly IBehaviourExecutor _avoid;

        public BehaviourExecutorFactory(DecisionContext context)
        {
            _idle = new IdleExecutor();
            _search = new SearchExecutor(
                context.SearchForwardSpeed,
                context.SearchYawRate);
            _approach = new ApproachExecutor(
                context.ApproachForwardSpeed,
                context.ApproachSteerGain,
                context.ApproachYawGain);
            _avoid = new AvoidExecutor(
                context.AvoidBackoffSpeed,
                context.AvoidSteerGain,
                context.AvoidYawGain);
        }

        /// <summary>
        /// Returns the executor that owns the given behaviour. Unknown states fall
        /// back to idle so no command is ever generated outside a known behaviour.
        /// </summary>
        public IBehaviourExecutor Get(BehaviourState behaviour)
        {
            switch (behaviour)
            {
                case BehaviourState.Idle:
                    return _idle;
                case BehaviourState.Search:
                    return _search;
                case BehaviourState.Approach:
                    return _approach;
                case BehaviourState.Avoid:
                    return _avoid;
                default:
                    return _idle;
            }
        }
    }
}
