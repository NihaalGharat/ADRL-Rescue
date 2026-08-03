namespace ADRL.AI.Decision
{
    using ADRL.AI.Decision.Execution;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors.Interfaces;

    /// <summary>
    /// The decision framework's execution entry point. It owns decision making
    /// only: it takes a fused sensor reading, assesses the situation, updates the
    /// short-term behaviour memory, selects a behaviour (optionally influenced by
    /// that memory), obtains the matching behaviour executor, and returns the
    /// <see cref="DroneCommand"/> the existing actuator pipeline already knows how
    /// to run. It never generates behaviour-specific movement itself - that is
    /// owned by the <see cref="IBehaviourExecutor"/> instances. It never touches
    /// movement, rewards, simulation, mission, or environment state - those stay
    /// with their owners.
    /// </summary>
    /// <remarks>
    /// Deterministic by construction: given the same context and the same fused
    /// reading sequence, the same commands are produced. The behaviour executor is
    /// resolved through the <see cref="BehaviourExecutorFactory"/>, the single
    /// owner of the behaviour-to-executor mapping. Behaviour memory is owned and
    /// updated by the <see cref="BehaviourMemoryService"/>; it never generates
    /// commands and never replaces current perception.
    /// </remarks>
    public sealed class DecisionEngine
    {
        private readonly ISituationAssessor _assessor;
        private readonly IBehaviourSelector _selector;
        private readonly BehaviourExecutorFactory _executorFactory;
        private readonly BehaviourMemoryService _memoryService;

        private int _stepCount;
        private BehaviourState _last;
        private SituationSnapshot _lastAssessment;

        public DecisionEngine(
            DecisionContext context,
            ISituationAssessor assessor,
            IBehaviourSelector selector,
            MemoryPolicy? memoryPolicy = null)
        {
            _assessor = assessor;
            _selector = selector;
            _executorFactory = new BehaviourExecutorFactory(context);
            _memoryService = new BehaviourMemoryService(memoryPolicy ?? MemoryPolicy.Default);
            _last = BehaviourState.Idle;
            _lastAssessment = SituationSnapshot.Invalid;
        }

        /// <summary>The behaviour-memory service backing this engine's decisions.</summary>
        public BehaviourMemoryService MemoryService => _memoryService;

        /// <summary>
        /// Runs one decision step over a fused reading and returns the resolved
        /// command for the existing actuator. The engine first refreshes behaviour
        /// memory from the current assessment (expiring stale context), then selects
        /// and resolves the command. The step clock drives memory timestamps so the
        /// whole pipeline stays deterministic.
        /// </summary>
        public DecisionResult Decide(ISensorReading fused)
        {
            var now = _stepCount;
            var assessment = _assessor.Assess(fused);
            _memoryService.Update(assessment, now);

            var behaviour = _selector.Select(assessment, _memoryService.Memory);
            _memoryService.RecordBehaviour(behaviour);

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
            _memoryService.Reset();
        }
    }
}
