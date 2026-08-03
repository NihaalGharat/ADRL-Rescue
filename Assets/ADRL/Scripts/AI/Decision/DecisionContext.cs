namespace ADRL.AI.Decision
{
    /// <summary>
    /// Immutable context that parameterises the decision framework. It owns the
    /// tuning thresholds consumed by the behaviour selector and the executor
    /// movement-generation constants consumed by the behaviour executors; the
    /// decision engine holds the runtime state. This keeps all decision behaviour
    /// config-driven and fully deterministic for a fixed input.
    /// </summary>
    public readonly struct DecisionContext
    {
        /// <summary>Proximity above which a target is considered detected.</summary>
        public readonly float DetectionProximity;

        /// <summary>Proximity above which an obstacle forces an avoid state.</summary>
        public readonly float AvoidThreshold;

        /// <summary>
        /// Fraction of the reading used as a "contacts alive" gate. When the
        /// proportion of actively-reported channels is below this value the
        /// snapshot is treated as not-yet-viable and search is preferred.
        /// </summary>
        public readonly float MinimumActiveRatio;

        /// <summary>
        /// Gain applied to how strongly a detected target steers the drone
        /// toward it (lateral + forward weight).
        /// </summary>
        public readonly float ApproachGain;

        /// <summary>
        /// Gain applied to how strongly an obstacle steers the drone away.
        /// </summary>
        public readonly float EvadeGain;

        /// <summary>Forward drive speed used by the search executor.</summary>
        public readonly float SearchForwardSpeed;

        /// <summary>Constant-rate yaw sweep used by the search executor.</summary>
        public readonly float SearchYawRate;

        /// <summary>Forward drive speed used by the approach executor.</summary>
        public readonly float ApproachForwardSpeed;

        /// <summary>Lateral steering gain used by the approach executor.</summary>
        public readonly float ApproachSteerGain;

        /// <summary>Yaw gain used by the approach executor to smooth steering.</summary>
        public readonly float ApproachYawGain;

        /// <summary>Backoff drive speed used by the avoid executor.</summary>
        public readonly float AvoidBackoffSpeed;

        /// <summary>Lateral steering gain used by the avoid executor.</summary>
        public readonly float AvoidSteerGain;

        /// <summary>Yaw gain used by the avoid executor to turn away smoothly.</summary>
        public readonly float AvoidYawGain;

        public DecisionContext(
            float detectionProximity,
            float avoidThreshold,
            float minimumActiveRatio,
            float approachGain,
            float evadeGain)
            : this(
                detectionProximity,
                avoidThreshold,
                minimumActiveRatio,
                approachGain,
                evadeGain,
                0.6f,
                0.15f,
                0.8f,
                0.8f,
                0.2f,
                0.3f,
                0.9f,
                0.25f)
        {
        }

        public DecisionContext(
            float detectionProximity,
            float avoidThreshold,
            float minimumActiveRatio,
            float approachGain,
            float evadeGain,
            float searchForwardSpeed,
            float searchYawRate,
            float approachForwardSpeed,
            float approachSteerGain,
            float approachYawGain,
            float avoidBackoffSpeed,
            float avoidSteerGain,
            float avoidYawGain)
        {
            DetectionProximity = detectionProximity;
            AvoidThreshold = avoidThreshold;
            MinimumActiveRatio = minimumActiveRatio;
            ApproachGain = approachGain;
            EvadeGain = evadeGain;
            SearchForwardSpeed = searchForwardSpeed;
            SearchYawRate = searchYawRate;
            ApproachForwardSpeed = approachForwardSpeed;
            ApproachSteerGain = approachSteerGain;
            ApproachYawGain = approachYawGain;
            AvoidBackoffSpeed = avoidBackoffSpeed;
            AvoidSteerGain = avoidSteerGain;
            AvoidYawGain = avoidYawGain;
        }

        /// <summary>Sensor-friendly default tuning for a six-band ray sweep.</summary>
        public static DecisionContext Default => new(
            detectionProximity: 0.35f,
            avoidThreshold: 0.65f,
            minimumActiveRatio: 0.1f,
            approachGain: 0.8f,
            evadeGain: 0.9f);
    }
}
