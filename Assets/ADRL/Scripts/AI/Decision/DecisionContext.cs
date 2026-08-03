namespace ADRL.AI.Decision
{
    /// <summary>
    /// Immutable context that parameterises the decision framework. It owns the
    /// tuning thresholds consumed by the behaviour selector; the decision engine
    /// holds the runtime state. This keeps all decision behaviour config-driven
    /// and fully deterministic for a fixed input.
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

        public DecisionContext(
            float detectionProximity,
            float avoidThreshold,
            float minimumActiveRatio,
            float approachGain,
            float evadeGain)
        {
            DetectionProximity = detectionProximity;
            AvoidThreshold = avoidThreshold;
            MinimumActiveRatio = minimumActiveRatio;
            ApproachGain = approachGain;
            EvadeGain = evadeGain;
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