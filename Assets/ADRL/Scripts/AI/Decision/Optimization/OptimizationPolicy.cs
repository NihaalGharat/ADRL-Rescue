namespace ADRL.AI.Decision.Optimization
{
    /// <summary>
    /// The single configuration owner of behaviour-execution optimization. It
    /// holds only the deterministic tuning constants consumed by the
    /// <see cref="BehaviourOptimizer"/>: the per-behaviour speed targets (as
    /// multipliers relative to neutral 1.0), the turn-rate multiplier bounds, the
    /// obstacle caution, the preferred victim approach distance, the confidence
    /// scaling and the smoothing factor. Configuration only - it contains no logic.
    /// </summary>
    /// <remarks>
    /// The speed targets are multipliers relative to the executor's configured base
    /// speed (1.0 = unchanged). The optimizer pushes the multiplier toward the
    /// target as execution confidence rises, so higher-confidence situations execute
    /// closer to the configured target. All values are bounded by the
    /// <see cref="OptimizationValidator"/> contract when the optimizer clamps them.
    /// </remarks>
    public readonly struct OptimizationPolicy
    {
        /// <summary>Speed target (multiplier over the base) for the search behaviour.</summary>
        public readonly float SearchSpeed;

        /// <summary>Speed target (multiplier over the base) for the approach behaviour.</summary>
        public readonly float ApproachSpeed;

        /// <summary>Speed target (multiplier over the base) for the avoid behaviour.</summary>
        public readonly float AvoidSpeed;

        /// <summary>Speed target (multiplier over the base) for the rescue behaviour.</summary>
        public readonly float RescueSpeed;

        /// <summary>Upper bound for the turn-rate multiplier.</summary>
        public readonly float MaximumTurnRate;

        /// <summary>Lower bound for the turn-rate multiplier.</summary>
        public readonly float MinimumTurnRate;

        /// <summary>How strongly obstacle proximity slows execution and raises caution, in (0, 1].</summary>
        public readonly float ObstacleCaution;

        /// <summary>Preferred approach distance for a confirmed victim.</summary>
        public readonly float VictimApproachDistance;

        /// <summary>Multiplier applied to the combined confidence before it shapes the profile.</summary>
        public readonly float ConfidenceScaling;

        /// <summary>Base smoothing factor carried in every optimized execution profile.</summary>
        public readonly float SmoothingFactor;

        public OptimizationPolicy(
            float searchSpeed,
            float approachSpeed,
            float avoidSpeed,
            float rescueSpeed,
            float maximumTurnRate,
            float minimumTurnRate,
            float obstacleCaution,
            float victimApproachDistance,
            float confidenceScaling,
            float smoothingFactor)
        {
            SearchSpeed = searchSpeed;
            ApproachSpeed = approachSpeed;
            AvoidSpeed = avoidSpeed;
            RescueSpeed = rescueSpeed;
            MaximumTurnRate = maximumTurnRate;
            MinimumTurnRate = minimumTurnRate;
            ObstacleCaution = obstacleCaution;
            VictimApproachDistance = victimApproachDistance;
            ConfidenceScaling = confidenceScaling;
            SmoothingFactor = smoothingFactor;
        }

        /// <summary>Default tuning: confident execution, cautious avoidance.</summary>
        public static OptimizationPolicy Default => new(
            searchSpeed: 1.15f,
            approachSpeed: 1.20f,
            avoidSpeed: 0.90f,
            rescueSpeed: 1.10f,
            maximumTurnRate: 1.30f,
            minimumTurnRate: 0.85f,
            obstacleCaution: 0.60f,
            victimApproachDistance: 1.5f,
            confidenceScaling: 1.0f,
            smoothingFactor: 0.5f);
    }
}
