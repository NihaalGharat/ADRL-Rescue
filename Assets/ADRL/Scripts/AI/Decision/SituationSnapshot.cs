namespace ADRL.AI.Decision
{
    /// <summary>
    /// Immutable, allocation-free projection of a single sensed situation. It is
    /// a <em>read-only assessment</em> of the environment as seen through the
    /// fused sensor reading - it never drives sensing, movement, or rewards.
    /// </summary>
    /// <remarks>
    /// The situational fields are interpreted by the behaviour selector and the
    /// decision engine only; no other system reads this type.
    /// </remarks>
    public readonly struct SituationSnapshot
    {
        /// <summary>
        /// True when a living target was confirmed by a proximity/victim segment
        /// of the fused reading. Interpretation is owned by the situation
        /// assessor, not by this struct.
        /// </summary>
        public readonly bool TargetDetected;

        /// <summary>
        /// Normalised proximity estimate of the nearest assessed target in
        /// (0, 1]. 1 = immediately present, 0 = no target.
        /// </summary>
        public readonly float TargetProximity;

        /// <summary>
        /// Side of the forward axis on which the nearest target sits, in
        /// (-1, 1). Negative = left, positive = right, 0 = ahead.
        /// </summary>
        public readonly float TargetSide;

        /// <summary>
        /// Normalised proximity of the closest obstacle, in (0, 1]. 1 = very
        /// close, 0 = none sensed.
        /// </summary>
        public readonly float ObstacleProximity;

        /// <summary>
        /// Side of the forward axis on which the nearest obstacle lies, in
        /// (-1, 1). Negative = left, positive = right, 0 = straight ahead.
        /// </summary>
        public readonly float ObstacleSide;

        /// <summary>True when the reading was viable enough to reason about.</summary>
        public readonly bool IsValid;

        public SituationSnapshot(
            bool targetDetected,
            float targetProximity,
            float targetSide,
            float obstacleProximity,
            float obstacleSide,
            bool isValid)
        {
            TargetDetected = targetDetected;
            TargetProximity = targetProximity;
            TargetSide = targetSide;
            ObstacleProximity = obstacleProximity;
            ObstacleSide = obstacleSide;
            IsValid = isValid;
        }

        /// <summary>An empty, invalid snapshot for an unviable reading.</summary>
        public static SituationSnapshot Invalid =>
            new(false, 0f, 0f, 0f, 0f, false);
    }
}