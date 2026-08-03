namespace ADRL.AI.Decision.Mission
{
    /// <summary>
    /// Immutable, single-owner configuration surface for the mission task
    /// coordination layer. Every timing/threshold/priority constant the mission
    /// coordinator consumes lives here; no magic numbers appear anywhere else in
    /// the mission implementation. Durations are expressed in decision steps so the
    /// whole layer stays deterministic and testable without wall-clock or Unity Time
    /// dependency.
    /// </summary>
    public readonly struct MissionPolicy
    {
        /// <summary>Steps spent in <see cref="MissionTaskState.ResumeSearch"/> before sweeping again.</summary>
        public readonly float ResumeTimeout;

        /// <summary>
        /// Minimum steps a hazard override must dwell in <see cref="MissionTaskState.AvoidHazard"/>
        /// before the previous task is restored, preventing avoidance thrash.
        /// </summary>
        public readonly float TransitionCooldown;

        /// <summary>
        /// Obstacle proximity at or above which the current perception is treated as an
        /// imminent hazard. Aligned with the behaviour avoid threshold so mission and
        /// behaviour agree on what counts as dangerous.
        /// </summary>
        public readonly float HazardProximityThreshold;

        /// <summary>
        /// Target proximity at or above which a perceived target is treated as a confirmed
        /// victim, promoting <see cref="MissionTaskState.InvestigateTarget"/> to
        /// <see cref="MissionTaskState.RescueVictim"/>.
        /// </summary>
        public readonly float VictimConfirmationProximity;

        /// <summary>Relative priority of hazard handling vs victim pursuit.</summary>
        public readonly float HazardPriority;

        /// <summary>Relative priority of victim pursuit vs hazard handling.</summary>
        public readonly float VictimPriority;

        /// <summary>
        /// Minimum confidence for a remembered victim to keep the current objective alive
        /// when it leaves the current perception.
        /// </summary>
        public readonly float ConfidenceThreshold;

        public MissionPolicy(
            float resumeTimeout,
            float transitionCooldown,
            float hazardProximityThreshold,
            float victimConfirmationProximity,
            float hazardPriority,
            float victimPriority,
            float confidenceThreshold)
        {
            ResumeTimeout = resumeTimeout;
            TransitionCooldown = transitionCooldown;
            HazardProximityThreshold = hazardProximityThreshold;
            VictimConfirmationProximity = victimConfirmationProximity;
            HazardPriority = hazardPriority;
            VictimPriority = victimPriority;
            ConfidenceThreshold = confidenceThreshold;
        }

        /// <summary>Sensor-friendly defaults for the rescue sweep.</summary>
        public static MissionPolicy Default => new(
            resumeTimeout: 5f,
            transitionCooldown: 3f,
            hazardProximityThreshold: 0.65f,
            victimConfirmationProximity: 0.75f,
            hazardPriority: 1f,
            victimPriority: 0.8f,
            confidenceThreshold: 0.25f);
    }
}
