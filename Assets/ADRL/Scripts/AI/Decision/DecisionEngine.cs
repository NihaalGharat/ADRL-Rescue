namespace ADRL.AI.Decision
{
    using ADRL.AI.Decision.Execution;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors.Interfaces;

    /// <summary>
    /// The decision framework's execution entry point. It owns decision making
    /// only: it takes a fused sensor reading, assesses the situation, selects a
    /// behaviour, obtains the matching behaviour executor, and returns the
    /// <see cref="DroneCommand"/> the existing actuator pipeline already knows how
    /// to run. It never generates behaviour-specific movement itself - that is
    /// owned by the <see cref="IBehaviourExecutor"/> instances. It never touches
    /// movement, rewards, simulation, mission, or environment state - those stay
    /// with their owners.
    /// </summary>
    /// <remarks>
    /// Deterministic by construction: given the same context and the same fused
    /// reading, the same command is produced. The behaviour executor is resolved
    /// through the <see cref="BehaviourExecutorFactory"/>, the single owner of the
    /// behaviour-to-executor mapping.
    /// </remarks>
    public sealed class DecisionEngine
    {
        private readonly ISituationAssessor _assessor;
        private readonly IBehaviourSelector _selector;
        private readonly BehaviourExecutorFactory _executorFactory;

        private int _stepCount;
        private BehaviourState _last;
        private SituationSnapshot _lastAssessment;

        public DecisionEngine(DecisionContext context, ISituationAssessor assessor, IBehaviourSelector selector)
        {
            _assessor = assessor;
            _selector = selector;
            _executorFactory = new BehaviourExecutorFactory(context);
            _last = BehaviourState.Idle;
            _lastAssessment = SituationSnapshot.Invalid;
        }

        /// <summary>
        /// Runs one decision step over a fused reading and returns the resolved
        /// command for the existing actuator.
        /// </summary>
        public DecisionResult Decide(ISensorReading fused)
        {
            var assessment = _assessor.Assess(fused);
            var behaviour = _selector.Select(assessment);
            var executor = _executorFactory.Get(behaviour);
            var command = executor.Resolve(assessment);

            _stepCount++;
            _last = behaviour;
            _lastAssessment = assessment;

            return new DecisionResult(behaviour, command, assessment);
        }

        /// <summary>Read-only projection of the framework's running state.</summary>
        public DecisionDiagnostics GetDiagnostics()
        {
            return new DecisionDiagnostics(_stepCount, _last, _lastAssessment);
        }

        /// <summary>Restores the framework to a fresh, zero-step state.</summary>
        public void Reset()
        {
            _stepCount = 0;
            _last = BehaviourState.Idle;
            _lastAssessment = SituationSnapshot.Invalid;
        }
    }
}
