namespace ADRL.AI.Decision
{
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Execution;
    using ADRL.AI.Decision.Explainability;
    using ADRL.AI.Decision.Knowledge;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.Decision.Trace;
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors.Interfaces;

    /// <summary>
    /// The decision framework's execution entry point. It owns decision making
    /// only: it takes a fused sensor reading, assesses the situation, updates the
    /// short-term behaviour memory, advances the mission coordinator, generates the
    /// currently-available candidate objectives, scores them, lets the task
    /// prioritizer arbitrate them into a single winning objective, selects a
    /// behaviour from that objective, obtains the matching behaviour executor, and
    /// returns the <see cref="DroneCommand"/> the existing actuator pipeline already
    /// knows how to run. It never generates behaviour-specific movement itself -
    /// that is owned by the <see cref="IBehaviourExecutor"/> instances. It never
    /// touches movement, rewards, simulation, mission, or environment state - those
    /// stay with their owners.
    /// </summary>
    /// <remarks>
    /// Deterministic by construction: given the same context and the same fused
    /// reading sequence, the same commands are produced. The behaviour executor is
    /// resolved through the <see cref="BehaviourExecutorFactory"/>, the single
    /// owner of the behaviour-to-executor mapping. Behaviour memory is owned and
    /// updated by the <see cref="BehaviourMemoryService"/>, the mission task is
    /// owned and advanced by the <see cref="MissionCoordinator"/>, candidate
    /// availability is owned by the <see cref="TaskCandidateGenerator"/>, scoring by
    /// the <see cref="PriorityEvaluator"/>, and objective arbitration by the
    /// <see cref="TaskPrioritizer"/>; none of them ever generates commands and none
    /// replaces current perception. After each step the engine captures the whole
    /// decision chain as an immutable <see cref="DecisionContextSnapshot"/> through
    /// the <see cref="IDecisionContextBuilder"/> - the single owner of context
    /// composition - and exposes the synchronized
    /// <see cref="DecisionDiagnostics"/> and the built snapshot for validation and
    /// the smoke test. It then explains the completed decision as an immutable
    /// <see cref="ADRL.AI.Decision.Explainability.DecisionExplanation"/> through the
    /// <see cref="ADRL.AI.Decision.Explainability.DecisionExplanationBuilder"/> - the
    /// single owner of explanation composition - which reads the snapshot only and
    /// never influences the decision chain. Finally it records the completed
    /// decision as an immutable replayable
    /// <see cref="ADRL.AI.Decision.Trace.DecisionTraceFrame"/> through the
    /// <see cref="ADRL.AI.Decision.Trace.DecisionTraceBuilder"/> - the single owner
    /// of trace composition - appended to the bounded
    /// <see cref="ADRL.AI.Decision.Trace.DecisionTraceStore"/> and embedded in the
    /// snapshot; tracing is strictly observational and never influences the
    /// decision chain.
    /// </remarks>
    public sealed class DecisionEngine
    {
        private readonly ISituationAssessor _assessor;
        private readonly IBehaviourSelector _selector;
        private readonly BehaviourExecutorFactory _executorFactory;
        private readonly BehaviourMemoryService _memoryService;
        private readonly MissionCoordinator _mission;
        private readonly MissionPolicy _missionPolicy;
        private readonly TaskCandidateGenerator _generator;
        private readonly PriorityEvaluator _evaluator;
        private readonly PriorityPolicy _priorityPolicy;
        private readonly TaskPrioritizer _prioritizer;
        private readonly IDecisionContextBuilder _builder;
        private readonly IBehaviourOptimizer _optimizer;
        private readonly KnowledgeUpdater _knowledgeUpdater;
        private readonly DecisionExplanationBuilder _explanationBuilder;
        private readonly DecisionTraceBuilder _traceBuilder;
        private readonly DecisionTraceStore _traceStore;

        private int _stepCount;
        private DecisionRuntimeState _runtimeState;
        private DecisionContextSnapshot _lastSnapshot;
        private DecisionExplanation _lastExplanation;
        private DecisionTraceFrame _lastTraceFrame;

        public DecisionEngine(
            DecisionContext context,
            ISituationAssessor assessor,
            IBehaviourSelector selector,
            MemoryPolicy? memoryPolicy = null,
            MissionPolicy? missionPolicy = null,
            PriorityPolicy? priorityPolicy = null,
            OptimizationPolicy? optimizationPolicy = null,
            KnowledgePolicy? knowledgePolicy = null)
        {
            _assessor = assessor;
            _selector = selector;
            _executorFactory = new BehaviourExecutorFactory(context);
            _memoryService = new BehaviourMemoryService(memoryPolicy ?? MemoryPolicy.Default);
            _missionPolicy = missionPolicy ?? MissionPolicy.Default;
            _mission = new MissionCoordinator(_missionPolicy);
            _priorityPolicy = priorityPolicy ?? PriorityPolicy.Default;
            _generator = new TaskCandidateGenerator();
            _evaluator = new PriorityEvaluator();
            _prioritizer = new TaskPrioritizer(_evaluator, _priorityPolicy, _generator);
            _builder = new DecisionContextBuilder();
            _optimizer = new BehaviourOptimizer(optimizationPolicy ?? OptimizationPolicy.Default);
            _knowledgeUpdater = new KnowledgeUpdater(knowledgePolicy ?? KnowledgePolicy.Default);
            _explanationBuilder = new DecisionExplanationBuilder();
            _traceBuilder = new DecisionTraceBuilder();
            _traceStore = new DecisionTraceStore();
            _runtimeState = DecisionRuntimeState.Empty;
            _lastSnapshot = DecisionContextSnapshot.Empty;
            _lastExplanation = DecisionExplanation.Empty;
            _lastTraceFrame = DecisionTraceFrame.Empty;
        }

        /// <summary>The behaviour-memory service backing this engine's decisions.</summary>
        public BehaviourMemoryService MemoryService => _memoryService;

        /// <summary>The mission coordinator backing this engine's decisions.</summary>
        public MissionCoordinator Mission => _mission;

        /// <summary>The persistent world-knowledge store backing this engine's decisions.</summary>
        public WorldKnowledgeStore KnowledgeStore => _knowledgeUpdater.Store;

        /// <summary>
        /// Runs one decision step over a fused reading and returns the resolved
        /// command for the existing actuator. The engine first refreshes behaviour
        /// memory from the current assessment (expiring stale context), then advances
        /// the mission task from that assessment and memory, generates the
        /// currently-available candidate objectives, scores them through the priority
        /// evaluator, lets the prioritizer arbitrate them into a single winning
        /// objective, selects the behaviour from that objective, and resolves the
        /// command. Finally it captures the whole chain as an immutable
        /// <see cref="DecisionContextSnapshot"/> and synchronized diagnostics. The
        /// step clock drives memory timestamps and mission timeouts so the whole
        /// pipeline stays deterministic.
        /// </summary>
        public DecisionResult Decide(ISensorReading fused)
        {
            var now = _stepCount;
            var assessment = _assessor.Assess(fused);

            // Phase 9.0: update world knowledge from the current assessment,
            // corroborated by short-term memory, before the rest of the pipeline
            // runs. Knowledge is write-only through the updater and read-only for
            // every consumer.
            _knowledgeUpdater.Update(assessment, _memoryService.Memory, now);

            _memoryService.Update(assessment, now);

            var mission = _mission.Update(assessment, _memoryService.Memory, now);
            var candidates = _generator.Generate(assessment, _memoryService.Memory, mission, _missionPolicy);
            var scored = _evaluator.Evaluate(candidates, mission, _priorityPolicy);
            var winning = _prioritizer.Select(scored);
            var behaviour = _selector.Select(assessment, winning.Task);
            _memoryService.RecordBehaviour(behaviour);

            var executor = _executorFactory.Get(behaviour);

            var stepCount = _stepCount + 1;

            // Phase 8.9: optimize execution from the current context before the
            // executor runs. The optimizer reads the current mission, winner,
            // behaviour, assessment and memory (the command is not known yet, so the
            // transient context carries the neutral idle command). It never changes
            // the mission, the priority, the behaviour or the command - it only
            // produces the execution profile that refines how the executor moves.
            var contextRuntime = WithKnowledge(
                new DecisionRuntimeState(
                    decisionStep: stepCount,
                    episodeStep: stepCount,
                    decisionTimestamp: now,
                    currentBehaviour: behaviour,
                    currentMission: mission,
                    lastCommand: DroneCommand.Idle,
                    currentExecutor: executor.GetType().Name,
                    currentWinner: winning),
                now);
            var context = _builder.Build(
                assessment,
                _memoryService.Memory,
                mission,
                candidates,
                winning,
                behaviour,
                DroneCommand.Idle,
                DecisionDiagnostics.From(contextRuntime, assessment, candidates.Length, _knowledgeUpdater.Store),
                contextRuntime);

            var profile = _optimizer.Optimize(behaviour, context);
            var command = executor.Resolve(assessment, profile);

            _runtimeState = WithKnowledge(
                new DecisionRuntimeState(
                    decisionStep: stepCount,
                    episodeStep: stepCount,
                    decisionTimestamp: now,
                    currentBehaviour: behaviour,
                    currentMission: mission,
                    lastCommand: command,
                    currentExecutor: executor.GetType().Name,
                    currentWinner: winning,
                    executionSpeedMultiplier: profile.SpeedMultiplier,
                    executionTurnRate: profile.TurnRateMultiplier,
                    executionConfidence: profile.ExecutionConfidence,
                    optimizationTimestamp: now),
                now);

            var diagnostics = DecisionDiagnostics.From(_runtimeState, assessment, candidates.Length, _knowledgeUpdater.Store);
            _lastSnapshot = _builder.Build(
                assessment,
                _memoryService.Memory,
                mission,
                candidates,
                winning,
                behaviour,
                command,
                diagnostics,
                _runtimeState)
                .WithExecutionProfile(profile)
                .WithKnowledge(_knowledgeUpdater.Store)
                .WithScoredCandidates(scored);

            // Phase 9.1: explain the completed decision. The explanation is built
            // strictly from the immutable snapshot by the single owner of
            // explanation composition, so it never influences the decision chain.
            // The diagnostics and the snapshot are recomposed to carry the
            // explanation, keeping diagnostics, snapshot and explanation
            // synchronized.
            _lastExplanation = _explanationBuilder.Build(_lastSnapshot);
            _lastSnapshot = _lastSnapshot.WithDiagnostics(
                _lastSnapshot.Diagnostics.WithExplanation(_lastExplanation));

            // Phase 9.2: trace the completed decision. The trace frame is built
            // strictly from the snapshot and its explanation by the single owner of
            // trace composition, appended to the single owner of trace history and
            // carried by the snapshot, so the whole decision chain is replayable
            // without any behavioural change. Tracing is observational only.
            _lastTraceFrame = _traceBuilder.Build(_lastSnapshot, _lastExplanation);
            _lastSnapshot = _lastSnapshot.WithTrace(_lastTraceFrame);
            _lastSnapshot = _lastSnapshot.WithDiagnostics(
                _lastSnapshot.Diagnostics.WithTraceFrame(_lastTraceFrame));
            _traceStore.Append(_lastTraceFrame);

            _stepCount++;

            return new DecisionResult(behaviour, command, assessment);
        }

        /// <summary>
        /// Attaches the current world-knowledge counts to a fresh runtime payload
        /// at the given step clock, so both the transient context and the final
        /// snapshot carry synchronized knowledge metadata.
        /// </summary>
        private DecisionRuntimeState WithKnowledge(DecisionRuntimeState runtime, float now)
        {
            return runtime.WithKnowledge(
                _knowledgeUpdater.Store.Count,
                _knowledgeUpdater.Store.CountOf(KnowledgeType.Victim),
                _knowledgeUpdater.Store.CountOf(KnowledgeType.Hazard),
                _knowledgeUpdater.Store.CountOf(KnowledgeType.Obstacle),
                now);
        }

        /// <summary>
        /// The immutable context snapshot of the last decision step - the complete
        /// decision chain as a single consistent context. The canonical
        /// <see cref="DecisionContextSnapshot.Empty"/> snapshot before any step.
        /// </summary>
        public DecisionContextSnapshot LastSnapshot => _lastSnapshot;

        /// <summary>
        /// The immutable, deterministic explanation of the last decision step - the
        /// complete decision chain as structured, human-readable reasoning. The
        /// canonical <see cref="DecisionExplanation.Empty"/> explanation before any
        /// step. Observational only: it never influences decisions.
        /// </summary>
        public DecisionExplanation LastExplanation => _lastExplanation;

        /// <summary>
        /// The immutable, replayable trace frame of the last decision step - the
        /// complete decision chain as a structured trace record for replay,
        /// inspection and debugging. The canonical <see cref="DecisionTraceFrame.Empty"/>
        /// frame before any step. Observational only: it never influences decisions.
        /// </summary>
        public DecisionTraceFrame LastTraceFrame => _lastTraceFrame;

        /// <summary>
        /// The single owner of trace history, recording every completed decision as
        /// an immutable frame in deterministic append order (bounded, evicting the
        /// oldest frame at capacity). Read-only for consumers; the engine is the
        /// only writer. Cleared by <see cref="Reset"/>.
        /// </summary>
        public DecisionTraceStore TraceStore => _traceStore;

        /// <summary>
        /// Read-only projection of the framework's running state, synchronized with
        /// the last built context snapshot.
        /// </summary>
        public DecisionDiagnostics GetDiagnostics()
        {
            return _lastSnapshot.Diagnostics;
        }

        /// <summary>Restores the framework to a fresh, zero-step state.</summary>
        public void Reset()
        {
            _stepCount = 0;
            _runtimeState = DecisionRuntimeState.Empty;
            _lastSnapshot = DecisionContextSnapshot.Empty;
            _lastExplanation = DecisionExplanation.Empty;
            _lastTraceFrame = DecisionTraceFrame.Empty;
            _traceStore.Clear();
            _memoryService.Reset();
            _mission.Reset();
            _knowledgeUpdater.Reset();
        }
    }
}
