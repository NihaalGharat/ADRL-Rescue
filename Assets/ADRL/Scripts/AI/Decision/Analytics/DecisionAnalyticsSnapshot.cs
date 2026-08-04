namespace ADRL.AI.Decision.Analytics
{
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// Immutable, deterministic snapshot of the decision pipeline's engineering
    /// analytics: a pure projection of a
    /// <see cref="ADRL.AI.Decision.Telemetry.DecisionTelemetrySnapshot"/> that
    /// answers "how healthy is the decision system?". It contains health, balance,
    /// entropy and utilization metrics plus the per-category analytics arrays. It
    /// is strictly read-only - it observes telemetry only and never influences
    /// decision making, prioritization, mission selection, optimization or
    /// execution. All members are readonly; the analytics arrays are fresh, owned
    /// arrays built at snapshot time and never shared for mutation.
    /// </summary>
    /// <remarks>
    /// Ratio metrics are expressed as percentages in [0, 100]: the confidence
    /// averages are the telemetry averages scaled by 100, utilization and balance
    /// are clamped to [0, 100] and the health score is the weighted combination
    /// defined by <see cref="DecisionHealthCalculator"/>. Entropy is the Shannon
    /// entropy (natural logarithm) of the category distribution, always finite and
    /// non-negative. The same telemetry always yields the same analytics snapshot.
    /// <see cref="Empty"/> is the canonical pre-observation snapshot.
    /// </remarks>
    public readonly struct DecisionAnalyticsSnapshot
    {
        /// <summary>Total number of decisions the analytics cover.</summary>
        public readonly int DecisionCount;

        /// <summary>Average winning-objective confidence, in [0, 100].</summary>
        public readonly float AverageDecisionConfidence;

        /// <summary>Average execution confidence of the optimized profiles, in [0, 100].</summary>
        public readonly float AverageExecutionConfidence;

        /// <summary>Average optimization-layer confidence, in [0, 100].</summary>
        public readonly float AverageOptimizationConfidence;

        /// <summary>Knowledge-store utilization rate, in [0, 100].</summary>
        public readonly float KnowledgeUtilizationRate;

        /// <summary>Behaviour-memory utilization rate, in [0, 100].</summary>
        public readonly float MemoryUtilizationRate;

        /// <summary>Behaviour distribution balance (evenness), in [0, 100].</summary>
        public readonly float BehaviourBalanceScore;

        /// <summary>Mission distribution balance (evenness), in [0, 100].</summary>
        public readonly float MissionBalanceScore;

        /// <summary>Shannon entropy of the behaviour distribution; non-negative and finite.</summary>
        public readonly float BehaviourEntropy;

        /// <summary>Shannon entropy of the mission distribution; non-negative and finite.</summary>
        public readonly float MissionEntropy;

        /// <summary>Overall decision health score, in [0, 100].</summary>
        public readonly float DecisionHealthScore;

        /// <summary>The deterministic step clock of the latest observed decision.</summary>
        public readonly int DecisionStep;

        /// <summary>The deterministic timestamp of the latest observed decision.</summary>
        public readonly float DecisionTimestamp;

        /// <summary>
        /// Per-behaviour analytics, one entry per <see cref="BehaviourState"/> value
        /// in enum order. Owned copy; immutable.
        /// </summary>
        public readonly BehaviourAnalytics[] BehaviourAnalytics;

        /// <summary>
        /// Per-mission analytics, one entry per <see cref="MissionTaskState"/> value
        /// in enum order. Owned copy; immutable.
        /// </summary>
        public readonly MissionAnalytics[] MissionAnalytics;

        public DecisionAnalyticsSnapshot(
            int decisionCount,
            float averageDecisionConfidence,
            float averageExecutionConfidence,
            float averageOptimizationConfidence,
            float knowledgeUtilizationRate,
            float memoryUtilizationRate,
            float behaviourBalanceScore,
            float missionBalanceScore,
            float behaviourEntropy,
            float missionEntropy,
            float decisionHealthScore,
            int decisionStep,
            float decisionTimestamp,
            BehaviourAnalytics[] behaviourAnalytics,
            MissionAnalytics[] missionAnalytics)
        {
            DecisionCount = decisionCount;
            AverageDecisionConfidence = averageDecisionConfidence;
            AverageExecutionConfidence = averageExecutionConfidence;
            AverageOptimizationConfidence = averageOptimizationConfidence;
            KnowledgeUtilizationRate = knowledgeUtilizationRate;
            MemoryUtilizationRate = memoryUtilizationRate;
            BehaviourBalanceScore = behaviourBalanceScore;
            MissionBalanceScore = missionBalanceScore;
            BehaviourEntropy = behaviourEntropy;
            MissionEntropy = missionEntropy;
            DecisionHealthScore = decisionHealthScore;
            DecisionStep = decisionStep;
            DecisionTimestamp = decisionTimestamp;
            BehaviourAnalytics = behaviourAnalytics ?? new BehaviourAnalytics[0];
            MissionAnalytics = missionAnalytics ?? new MissionAnalytics[0];
        }

        /// <summary>
        /// True when the snapshot is complete and structurally sound: the reference
        /// payloads are present and every count, score, rate, entropy and the
        /// health score is non-negative. The canonical <see cref="Empty"/> snapshot
        /// is valid.
        /// </summary>
        public bool IsValid =>
            BehaviourAnalytics != null
            && MissionAnalytics != null
            && DecisionCount >= 0
            && DecisionStep >= 0
            && DecisionTimestamp >= 0f
            && AverageDecisionConfidence >= 0f
            && AverageExecutionConfidence >= 0f
            && AverageOptimizationConfidence >= 0f
            && KnowledgeUtilizationRate >= 0f
            && MemoryUtilizationRate >= 0f
            && BehaviourBalanceScore >= 0f
            && MissionBalanceScore >= 0f
            && BehaviourEntropy >= 0f
            && MissionEntropy >= 0f
            && DecisionHealthScore >= 0f;

        /// <summary>An empty, pre-observation analytics snapshot.</summary>
        public static DecisionAnalyticsSnapshot Empty => _empty;

        private static readonly DecisionAnalyticsSnapshot _empty = new(
            0,
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            0,
            0f,
            new BehaviourAnalytics[]
            {
                new(BehaviourState.Idle, 0, 0f),
                new(BehaviourState.Search, 0, 0f),
                new(BehaviourState.Approach, 0, 0f),
                new(BehaviourState.Avoid, 0, 0f),
            },
            new MissionAnalytics[]
            {
                new(MissionTaskState.Idle, 0, 0f),
                new(MissionTaskState.SearchArea, 0, 0f),
                new(MissionTaskState.InvestigateTarget, 0, 0f),
                new(MissionTaskState.RescueVictim, 0, 0f),
                new(MissionTaskState.AvoidHazard, 0, 0f),
                new(MissionTaskState.ResumeSearch, 0, 0f),
            });
    }

    /// <summary>
    /// Immutable analytics of one <see cref="BehaviourState"/> value across all
    /// covered decisions: its count and its share of the total, in [0, 100].
    /// </summary>
    public readonly struct BehaviourAnalytics
    {
        /// <summary>The behaviour this entry describes.</summary>
        public readonly BehaviourState Behaviour;

        /// <summary>Number of covered decisions that selected this behaviour.</summary>
        public readonly int Count;

        /// <summary>Share of the covered decisions that selected this behaviour, in [0, 100].</summary>
        public readonly float Share;

        public BehaviourAnalytics(BehaviourState behaviour, int count, float share)
        {
            Behaviour = behaviour;
            Count = count;
            Share = share;
        }
    }

    /// <summary>
    /// Immutable analytics of one <see cref="MissionTaskState"/> value across all
    /// covered decisions: its count and its share of the total, in [0, 100].
    /// </summary>
    public readonly struct MissionAnalytics
    {
        /// <summary>The mission objective this entry describes.</summary>
        public readonly MissionTaskState Mission;

        /// <summary>Number of covered decisions that pursued this mission objective.</summary>
        public readonly int Count;

        /// <summary>Share of the covered decisions that pursued this objective, in [0, 100].</summary>
        public readonly float Share;

        public MissionAnalytics(MissionTaskState mission, int count, float share)
        {
            Mission = mission;
            Count = count;
            Share = share;
        }
    }
}
