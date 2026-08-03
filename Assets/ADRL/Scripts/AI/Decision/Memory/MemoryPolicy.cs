namespace ADRL.AI.Decision.Memory
{
    /// <summary>
    /// Immutable, single-owner configuration surface for the behaviour memory
    /// layer. Every timing/decay/capacity constant the memory service uses lives
    /// here; no magic numbers appear anywhere else in the memory implementation.
    /// Durations are expressed in decision steps so the whole memory layer stays
    /// deterministic and testable without wall-clock or Unity Time dependency.
    /// </summary>
    public readonly struct MemoryPolicy
    {
        /// <summary>Lifetime (in decision steps) of a stored victim memory.</summary>
        public readonly float VictimMemoryDuration;

        /// <summary>Lifetime (in decision steps) of a stored obstacle memory.</summary>
        public readonly float ObstacleMemoryDuration;

        /// <summary>Confidence lost per decision step of age.</summary>
        public readonly float ConfidenceDecay;

        /// <summary>
        /// Maximum age (in steps) at which re-observing an entity refreshes its
        /// existing memory; a re-observation older than this is re-based as a new
        /// memory instead of extending the old one.
        /// </summary>
        public readonly float RefreshThreshold;

        /// <summary>Hard upper bound on the number of stored records.</summary>
        public readonly int MaxRecords;

        public MemoryPolicy(
            float victimMemoryDuration,
            float obstacleMemoryDuration,
            float confidenceDecay,
            float refreshThreshold,
            int maxRecords)
        {
            VictimMemoryDuration = victimMemoryDuration;
            ObstacleMemoryDuration = obstacleMemoryDuration;
            ConfidenceDecay = confidenceDecay;
            RefreshThreshold = refreshThreshold;
            MaxRecords = maxRecords;
        }

        /// <summary>Sensor-friendly defaults for the rescue sweep.</summary>
        public static MemoryPolicy Default => new(
            victimMemoryDuration: 10f,
            obstacleMemoryDuration: 6f,
            confidenceDecay: 0.15f,
            refreshThreshold: 5f,
            maxRecords: 4);
    }
}
