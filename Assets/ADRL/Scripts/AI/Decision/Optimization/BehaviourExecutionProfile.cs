namespace ADRL.AI.Decision.Optimization
{
    /// <summary>
    /// Immutable runtime parameters describing <em>how</em> a selected behaviour
    /// should be executed. Produced by the <see cref="IBehaviourOptimizer"/> and
    /// consumed read-only by the behaviour executors, which scale their base
    /// movement constants by the profile multipliers. The profile never decides
    /// what to do - the mission, the priority and the behaviour are chosen before
    /// it exists. All fields are readonly; never mutated in place.
    /// </summary>
    /// <remarks>
    /// Multipliers are expressed relative to neutral 1.0, so a value of 1 leaves
    /// the executor's configured base unchanged. This makes the legacy executor
    /// path (no profile) and the optimized path agree exactly whenever the profile
    /// is empty. <see cref="PreferredDistance"/> and <see cref="Smoothness"/> are
    /// carried in the profile for the deterministic optimization surface; the
    /// current executors consume speed, turn rate and caution only.
    /// </remarks>
    public readonly struct BehaviourExecutionProfile
    {
        /// <summary>
        /// Speed multiplier applied to the executor's base forward/backoff drive;
        /// 1 = the executor's configured base speed.
        /// </summary>
        public readonly float SpeedMultiplier;

        /// <summary>
        /// Turn-rate multiplier applied to the executor's base steering and yaw
        /// gains; 1 = the executor's configured base turning.
        /// </summary>
        public readonly float TurnRateMultiplier;

        /// <summary>
        /// Caution level in [0, 1]; higher values make execution more evasive.
        /// 0 = no additional caution.
        /// </summary>
        public readonly float CautionLevel;

        /// <summary>
        /// Preferred target distance for the behaviour, or 0 when none applies.
        /// </summary>
        public readonly float PreferredDistance;

        /// <summary>
        /// Smoothing factor in [0, 1], or 0 when no smoothing applies.
        /// </summary>
        public readonly float Smoothness;

        /// <summary>
        /// Execution confidence in [0, 1] backing this profile; 0 marks the
        /// neutral no-op profile (<see cref="Empty"/>).
        /// </summary>
        public readonly float ExecutionConfidence;

        public BehaviourExecutionProfile(
            float speedMultiplier,
            float turnRateMultiplier,
            float cautionLevel,
            float preferredDistance,
            float smoothness,
            float executionConfidence)
        {
            SpeedMultiplier = speedMultiplier;
            TurnRateMultiplier = turnRateMultiplier;
            CautionLevel = cautionLevel;
            PreferredDistance = preferredDistance;
            Smoothness = smoothness;
            ExecutionConfidence = executionConfidence;
        }

        /// <summary>
        /// The neutral no-op profile: multipliers of 1, no caution, no preferred
        /// distance, no smoothing and no confidence. Returns the executor's
        /// configured base behaviour byte-for-byte when applied.
        /// </summary>
        public static BehaviourExecutionProfile Empty =>
            new(1f, 1f, 0f, 0f, 0f, 0f);

        /// <summary>True when this is the neutral no-op profile.</summary>
        public bool IsEmpty =>
            ExecutionConfidence <= 0f && SpeedMultiplier == 1f && TurnRateMultiplier == 1f;
    }
}
