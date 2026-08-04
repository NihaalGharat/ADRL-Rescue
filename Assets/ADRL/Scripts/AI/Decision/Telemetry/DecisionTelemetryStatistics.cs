namespace ADRL.AI.Decision.Telemetry
{
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Trace;

    /// <summary>
    /// Internal running accumulator backing the <see cref="DecisionTelemetryCollector"/>.
    /// It owns the running totals, running averages, behaviour/mission counters and
    /// the latest-value projection of the observed decision frames. It is never
    /// exposed publicly - the collector is the single public owner of telemetry and
    /// builds an immutable <see cref="DecisionTelemetrySnapshot"/> from this state.
    /// </summary>
    /// <remarks>
    /// Updates are O(1) and allocation-free: every decision only advances scalar
    /// sums and increments pre-sized counter arrays (indexed by enum value), so
    /// observing a frame never allocates and never uses LINQ. Distribution arrays
    /// are sized once from the enum value counts and cleared on reset; the only
    /// allocations happen when a snapshot is materialized for export.
    /// </remarks>
    internal sealed class DecisionTelemetryStatistics
    {
        private static readonly BehaviourState[] BehaviourValues =
            (BehaviourState[])System.Enum.GetValues(typeof(BehaviourState));

        private static readonly MissionTaskState[] MissionValues =
            (MissionTaskState[])System.Enum.GetValues(typeof(MissionTaskState));

        private int _decisionCount;
        private float _confidenceSum;
        private float _optimizationConfidenceSum;
        private float _candidateCountSum;
        private float _speedSum;
        private float _turnSum;
        private int _knowledgeCount;
        private int _memoryCount;
        private BehaviourState _latestBehaviour;
        private MissionTaskState _latestMission;
        private string _latestExecutor;
        private int _latestStep;
        private float _latestTimestamp;

        private readonly int[] _behaviourCounters;
        private readonly int[] _missionCounters;

        public DecisionTelemetryStatistics()
        {
            _behaviourCounters = new int[BehaviourValues.Length];
            _missionCounters = new int[MissionValues.Length];
            _latestExecutor = string.Empty;
        }

        /// <summary>
        /// Records one completed decision frame. O(1) and allocation-free. Null
        /// frames are ignored so a defensive consumer can never corrupt telemetry.
        /// </summary>
        public void Observe(DecisionTraceFrame frame)
        {
            if (frame == null)
                return;

            _decisionCount++;
            _confidenceSum += frame.WinningTask.Confidence;
            _optimizationConfidenceSum += frame.OptimizationConfidence;
            _candidateCountSum += frame.CandidateCount;
            _speedSum += frame.OptimizationProfile.SpeedMultiplier;
            _turnSum += frame.OptimizationProfile.TurnRateMultiplier;
            _knowledgeCount = frame.KnowledgeCount;
            _memoryCount = frame.MemoryCount;
            _latestBehaviour = frame.Behaviour;
            _latestMission = frame.Mission.State;
            _latestExecutor = frame.ExecutorName ?? string.Empty;
            _latestStep = frame.DecisionStep;
            _latestTimestamp = frame.DecisionTimestamp;

            Increment(_behaviourCounters, (int)frame.Behaviour);
            Increment(_missionCounters, (int)frame.Mission.State);
        }

        /// <summary>Restores a zeroed, fresh state.</summary>
        public void Reset()
        {
            _decisionCount = 0;
            _confidenceSum = 0f;
            _optimizationConfidenceSum = 0f;
            _candidateCountSum = 0f;
            _speedSum = 0f;
            _turnSum = 0f;
            _knowledgeCount = 0;
            _memoryCount = 0;
            _latestBehaviour = BehaviourState.Idle;
            _latestMission = MissionTaskState.Idle;
            _latestExecutor = string.Empty;
            _latestStep = 0;
            _latestTimestamp = 0f;
            System.Array.Clear(_behaviourCounters, 0, _behaviourCounters.Length);
            System.Array.Clear(_missionCounters, 0, _missionCounters.Length);
        }

        public int DecisionCount => _decisionCount;

        public float AverageConfidence => Average(_confidenceSum);

        public float AverageOptimizationConfidence => Average(_optimizationConfidenceSum);

        public float AverageCandidateCount => Average(_candidateCountSum);

        public float AverageSpeedMultiplier => Average(_speedSum);

        public float AverageTurnMultiplier => Average(_turnSum);

        public int KnowledgeCount => _knowledgeCount;

        public int MemoryCount => _memoryCount;

        public BehaviourState LatestBehaviour => _latestBehaviour;

        public MissionTaskState LatestMission => _latestMission;

        public string LatestExecutor => _latestExecutor;

        public int LatestStep => _latestStep;

        public float LatestTimestamp => _latestTimestamp;

        /// <summary>
        /// Builds the behaviour distribution as one owned entry per
        /// <see cref="BehaviourState"/> value in enum order.
        /// </summary>
        public DecisionBehaviourCount[] BuildBehaviourDistribution()
        {
            var result = new DecisionBehaviourCount[BehaviourValues.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = new DecisionBehaviourCount(BehaviourValues[i], _behaviourCounters[(int)BehaviourValues[i]]);
            return result;
        }

        /// <summary>
        /// Builds the mission distribution as one owned entry per
        /// <see cref="MissionTaskState"/> value in enum order.
        /// </summary>
        public DecisionMissionCount[] BuildMissionDistribution()
        {
            var result = new DecisionMissionCount[MissionValues.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = new DecisionMissionCount(MissionValues[i], _missionCounters[(int)MissionValues[i]]);
            return result;
        }

        private float Average(float sum)
        {
            return _decisionCount == 0 ? 0f : sum / _decisionCount;
        }

        private static void Increment(int[] counters, int index)
        {
            if (index >= 0 && index < counters.Length)
                counters[index]++;
        }
    }
}
