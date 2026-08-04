namespace ADRL.AI.Decision.Telemetry
{
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// Immutable, deterministic snapshot of the decision pipeline's telemetry: the
    /// running counts, running averages, latest values and behaviour/mission
    /// distributions of every decision observed so far. It is a pure projection of
    /// the <see cref="DecisionTelemetryCollector"/>'s running statistics - it
    /// contains runtime telemetry only and never influences decision making,
    /// prioritization, mission selection, optimization or execution. All members
    /// are readonly; the distribution arrays are fresh, owned arrays built at
    /// snapshot time and never shared for mutation.
    /// </summary>
    /// <remarks>
    /// Averages are running totals divided by the observed decision count, so the
    /// same observed sequence always yields the same snapshot. The distributions
    /// always contain one entry per <see cref="BehaviourState"/> and per
    /// <see cref="MissionTaskState"/> value in deterministic enum order (zero-count
    /// entries included), so two snapshots over identical sequences compare
    /// byte-for-byte. <see cref="Empty"/> is the canonical pre-observation
    /// snapshot.
    /// </remarks>
    public readonly struct DecisionTelemetrySnapshot
    {
        /// <summary>Total number of decision frames observed so far.</summary>
        public readonly int DecisionCount;

        /// <summary>Running average of the winning objective's confidence, in [0, 1].</summary>
        public readonly float AverageDecisionConfidence;

        /// <summary>Running average of the optimized profile's execution confidence, in [0, 1].</summary>
        public readonly float AverageOptimizationConfidence;

        /// <summary>Running average number of candidate objectives generated per decision.</summary>
        public readonly float AverageCandidateCount;

        /// <summary>Running average execution speed multiplier.</summary>
        public readonly float AverageExecutionSpeedMultiplier;

        /// <summary>Running average execution turn-rate multiplier.</summary>
        public readonly float AverageTurnRateMultiplier;

        /// <summary>Number of world-knowledge records at the latest observed decision.</summary>
        public readonly int KnowledgeRecordCount;

        /// <summary>Number of behaviour-memory records at the latest observed decision.</summary>
        public readonly int MemoryRecordCount;

        /// <summary>The behaviour selected by the latest observed decision.</summary>
        public readonly BehaviourState CurrentBehaviour;

        /// <summary>The mission objective of the latest observed decision.</summary>
        public readonly MissionTaskState CurrentMission;

        /// <summary>
        /// Concrete executor type name of the latest observed decision, or an
        /// empty string before any decision.
        /// </summary>
        public readonly string CurrentExecutor;

        /// <summary>The deterministic step clock of the latest observed decision.</summary>
        public readonly int LatestDecisionStep;

        /// <summary>The deterministic timestamp of the latest observed decision.</summary>
        public readonly float LatestDecisionTimestamp;

        /// <summary>
        /// Behaviour distribution of all observed decisions, one entry per
        /// <see cref="BehaviourState"/> value in enum order. Owned copy; immutable.
        /// </summary>
        public readonly DecisionBehaviourCount[] BehaviourDistribution;

        /// <summary>
        /// Mission distribution of all observed decisions, one entry per
        /// <see cref="MissionTaskState"/> value in enum order. Owned copy; immutable.
        /// </summary>
        public readonly DecisionMissionCount[] MissionDistribution;

        public DecisionTelemetrySnapshot(
            int decisionCount,
            float averageDecisionConfidence,
            float averageOptimizationConfidence,
            float averageCandidateCount,
            float averageExecutionSpeedMultiplier,
            float averageTurnRateMultiplier,
            int knowledgeRecordCount,
            int memoryRecordCount,
            BehaviourState currentBehaviour,
            MissionTaskState currentMission,
            string currentExecutor,
            int latestDecisionStep,
            float latestDecisionTimestamp,
            DecisionBehaviourCount[] behaviourDistribution,
            DecisionMissionCount[] missionDistribution)
        {
            DecisionCount = decisionCount;
            AverageDecisionConfidence = averageDecisionConfidence;
            AverageOptimizationConfidence = averageOptimizationConfidence;
            AverageCandidateCount = averageCandidateCount;
            AverageExecutionSpeedMultiplier = averageExecutionSpeedMultiplier;
            AverageTurnRateMultiplier = averageTurnRateMultiplier;
            KnowledgeRecordCount = knowledgeRecordCount;
            MemoryRecordCount = memoryRecordCount;
            CurrentBehaviour = currentBehaviour;
            CurrentMission = currentMission;
            CurrentExecutor = currentExecutor ?? string.Empty;
            LatestDecisionStep = latestDecisionStep;
            LatestDecisionTimestamp = latestDecisionTimestamp;
            BehaviourDistribution = behaviourDistribution ?? new DecisionBehaviourCount[0];
            MissionDistribution = missionDistribution ?? new DecisionMissionCount[0];
        }

        /// <summary>
        /// True when the snapshot is complete and structurally sound: the reference
        /// payloads are present and every count and average is non-negative. The
        /// canonical <see cref="Empty"/> snapshot is valid.
        /// </summary>
        public bool IsValid =>
            CurrentExecutor != null
            && BehaviourDistribution != null
            && MissionDistribution != null
            && DecisionCount >= 0
            && LatestDecisionStep >= 0
            && LatestDecisionTimestamp >= 0f
            && AverageDecisionConfidence >= 0f
            && AverageOptimizationConfidence >= 0f
            && AverageCandidateCount >= 0f
            && AverageExecutionSpeedMultiplier >= 0f
            && AverageTurnRateMultiplier >= 0f
            && KnowledgeRecordCount >= 0
            && MemoryRecordCount >= 0;

        /// <summary>An empty, pre-observation telemetry snapshot.</summary>
        public static DecisionTelemetrySnapshot Empty => _empty;

        private static readonly DecisionTelemetrySnapshot _empty = new(
            0,
            0f,
            0f,
            0f,
            0f,
            0f,
            0,
            0,
            BehaviourState.Idle,
            MissionTaskState.Idle,
            string.Empty,
            0,
            0f,
            new DecisionBehaviourCount[0],
            new DecisionMissionCount[0]);
    }

    /// <summary>
    /// Immutable telemetry count of one <see cref="BehaviourState"/> value across
    /// all observed decisions.
    /// </summary>
    public readonly struct DecisionBehaviourCount
    {
        /// <summary>The behaviour this entry counts.</summary>
        public readonly BehaviourState Behaviour;

        /// <summary>Number of observed decisions that selected this behaviour.</summary>
        public readonly int Count;

        public DecisionBehaviourCount(BehaviourState behaviour, int count)
        {
            Behaviour = behaviour;
            Count = count;
        }
    }

    /// <summary>
    /// Immutable telemetry count of one <see cref="MissionTaskState"/> value across
    /// all observed decisions.
    /// </summary>
    public readonly struct DecisionMissionCount
    {
        /// <summary>The mission objective this entry counts.</summary>
        public readonly MissionTaskState Mission;

        /// <summary>Number of observed decisions that pursued this mission objective.</summary>
        public readonly int Count;

        public DecisionMissionCount(MissionTaskState mission, int count)
        {
            Mission = mission;
            Count = count;
        }
    }
}
