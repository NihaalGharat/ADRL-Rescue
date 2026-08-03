namespace ADRL.AI.Decision.Context
{
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// Immutable snapshot of the decision framework's pure runtime metadata: the
    /// step clocks, the deterministic decision timestamp, and the outcome of the
    /// last step (behaviour, mission objective, command, executor and winner).
    /// It is diagnostic bookkeeping only - it never influences future decisions.
    /// Never mutated in place - every step produces a fresh value.
    /// </summary>
    /// <remarks>
    /// <see cref="DecisionStep"/> counts decisions made; <see cref="EpisodeStep"/>
    /// counts episode steps. In the current runtime every decision is made once per
    /// episode step, so the two clocks advance together; the field is kept separate
    /// for a future runtime in which a decision may be throttled to every N episode
    /// steps. <see cref="DecisionTimestamp"/> is the deterministic step-clock value
    /// the decision was made at (the value of the step clock before the step's own
    /// count advanced), which is what all mission timeouts and memory ages are
    /// measured against.
    /// </remarks>
    public readonly struct DecisionRuntimeState
    {
        /// <summary>Total number of decisions made so far.</summary>
        public readonly int DecisionStep;

        /// <summary>Total number of episode steps carried out so far.</summary>
        public readonly int EpisodeStep;

        /// <summary>The deterministic step-clock value at which the last decision was made.</summary>
        public readonly float DecisionTimestamp;

        /// <summary>The behaviour selected by the last decision.</summary>
        public readonly BehaviourState CurrentBehaviour;

        /// <summary>The mission objective in effect for the last decision.</summary>
        public readonly MissionTask CurrentMission;

        /// <summary>The command resolved by the last decision's executor.</summary>
        public readonly DroneCommand LastCommand;

        /// <summary>
        /// Concrete type name of the executor that produced the last command
        /// (for example <c>AvoidExecutor</c>), or an empty string before any step.
        /// </summary>
        public readonly string CurrentExecutor;

        /// <summary>The prioritizer's winning objective for the last decision.</summary>
        public readonly TaskPriority CurrentWinner;

        /// <summary>
        /// The execution speed multiplier of the last decision's optimized profile
        /// (1 = the executor's configured base), or 0 before any step.
        /// </summary>
        public readonly float ExecutionSpeedMultiplier;

        /// <summary>
        /// The execution turn-rate multiplier of the last decision's optimized
        /// profile (1 = the executor's configured base), or 0 before any step.
        /// </summary>
        public readonly float ExecutionTurnRate;

        /// <summary>
        /// The execution confidence in [0, 1] of the last decision's optimized
        /// profile, or 0 before any step.
        /// </summary>
        public readonly float ExecutionConfidence;

        /// <summary>
        /// The deterministic step-clock value at which the last execution profile
        /// was optimized, or 0 before any step.
        /// </summary>
        public readonly float OptimizationTimestamp;

        /// <summary>Number of world-knowledge records stored at the last step, or 0 before any step.</summary>
        public readonly int KnowledgeRecordCount;

        /// <summary>Number of known-victim records stored at the last step, or 0 before any step.</summary>
        public readonly int KnownVictims;

        /// <summary>Number of known-hazard records stored at the last step, or 0 before any step.</summary>
        public readonly int KnownHazards;

        /// <summary>Number of known-obstacle records stored at the last step, or 0 before any step.</summary>
        public readonly int KnownObstacles;

        /// <summary>The deterministic step-clock value at which world knowledge was last updated, or 0 before any step.</summary>
        public readonly float KnowledgeTimestamp;

        public DecisionRuntimeState(
            int decisionStep,
            int episodeStep,
            float decisionTimestamp,
            BehaviourState currentBehaviour,
            MissionTask currentMission,
            DroneCommand lastCommand,
            string currentExecutor,
            TaskPriority currentWinner)
            : this(
                decisionStep,
                episodeStep,
                decisionTimestamp,
                currentBehaviour,
                currentMission,
                lastCommand,
                currentExecutor,
                currentWinner,
                0f,
                0f,
                0f,
                0f,
                0,
                0,
                0,
                0,
                0f)
        {
        }

        public DecisionRuntimeState(
            int decisionStep,
            int episodeStep,
            float decisionTimestamp,
            BehaviourState currentBehaviour,
            MissionTask currentMission,
            DroneCommand lastCommand,
            string currentExecutor,
            TaskPriority currentWinner,
            float executionSpeedMultiplier,
            float executionTurnRate,
            float executionConfidence,
            float optimizationTimestamp)
            : this(
                decisionStep,
                episodeStep,
                decisionTimestamp,
                currentBehaviour,
                currentMission,
                lastCommand,
                currentExecutor,
                currentWinner,
                executionSpeedMultiplier,
                executionTurnRate,
                executionConfidence,
                optimizationTimestamp,
                0,
                0,
                0,
                0,
                0f)
        {
        }

        private DecisionRuntimeState(
            int decisionStep,
            int episodeStep,
            float decisionTimestamp,
            BehaviourState currentBehaviour,
            MissionTask currentMission,
            DroneCommand lastCommand,
            string currentExecutor,
            TaskPriority currentWinner,
            float executionSpeedMultiplier,
            float executionTurnRate,
            float executionConfidence,
            float optimizationTimestamp,
            int knowledgeRecordCount,
            int knownVictims,
            int knownHazards,
            int knownObstacles,
            float knowledgeTimestamp)
        {
            DecisionStep = decisionStep;
            EpisodeStep = episodeStep;
            DecisionTimestamp = decisionTimestamp;
            CurrentBehaviour = currentBehaviour;
            CurrentMission = currentMission;
            LastCommand = lastCommand;
            CurrentExecutor = currentExecutor;
            CurrentWinner = currentWinner;
            ExecutionSpeedMultiplier = executionSpeedMultiplier;
            ExecutionTurnRate = executionTurnRate;
            ExecutionConfidence = executionConfidence;
            OptimizationTimestamp = optimizationTimestamp;
            KnowledgeRecordCount = knowledgeRecordCount;
            KnownVictims = knownVictims;
            KnownHazards = knownHazards;
            KnownObstacles = knownObstacles;
            KnowledgeTimestamp = knowledgeTimestamp;
        }

        /// <summary>
        /// Returns a copy of this runtime payload carrying the world-knowledge
        /// counts of the last step. The payload is immutable, so this never mutates
        /// the original - it composes a fresh value with the knowledge metadata the
        /// diagnostics projection reads.
        /// </summary>
        public DecisionRuntimeState WithKnowledge(
            int knowledgeRecordCount,
            int knownVictims,
            int knownHazards,
            int knownObstacles,
            float knowledgeTimestamp)
        {
            return new DecisionRuntimeState(
                DecisionStep,
                EpisodeStep,
                DecisionTimestamp,
                CurrentBehaviour,
                CurrentMission,
                LastCommand,
                CurrentExecutor,
                CurrentWinner,
                ExecutionSpeedMultiplier,
                ExecutionTurnRate,
                ExecutionConfidence,
                OptimizationTimestamp,
                knowledgeRecordCount,
                knownVictims,
                knownHazards,
                knownObstacles,
                knowledgeTimestamp);
        }

        /// <summary>A zero-step runtime metadata payload.</summary>
        public static DecisionRuntimeState Empty => new(
            0,
            0,
            0f,
            BehaviourState.Idle,
            MissionTask.Invalid,
            DroneCommand.Idle,
            string.Empty,
            TaskPriority.Invalid);
    }
}
