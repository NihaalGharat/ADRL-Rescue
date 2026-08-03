namespace ADRL.AI.Decision.Knowledge
{
    /// <summary>
    /// Immutable, single-owner configuration surface for the world knowledge
    /// layer. Every timing, decay, capacity and projection constant the knowledge
    /// service consumes lives here; no magic numbers appear anywhere else in the
    /// knowledge implementation. Lifetimes and timestamps are expressed in decision
    /// steps so the whole layer stays deterministic and testable without wall-clock
    /// or Unity Time dependency.
    /// </summary>
    /// <remarks>
    /// <see cref="HazardProximityThreshold"/> is aligned with the mission
    /// coordinator's hazard threshold so knowledge and mission agree on what counts
    /// as dangerous. <see cref="DetectionDistance"/> and <see cref="SweepSpread"/>
    /// are the projection constants that map a normalized proximity/side assessment
    /// into the drone's local observation frame.
    /// </remarks>
    public readonly struct KnowledgePolicy
    {
        /// <summary>Hard upper bound on the number of stored records.</summary>
        public readonly int MaximumRecords;

        /// <summary>Lifetime (in decision steps) of a stored victim record.</summary>
        public readonly int VictimLifetime;

        /// <summary>Lifetime (in decision steps) of a stored obstacle record.</summary>
        public readonly int ObstacleLifetime;

        /// <summary>Lifetime (in decision steps) of a stored hazard record.</summary>
        public readonly int HazardLifetime;

        /// <summary>Lifetime (in decision steps) of a stored region record.</summary>
        public readonly int ExploredLifetime;

        /// <summary>Distance below which two records of the same type are the same known thing.</summary>
        public readonly float MergeDistance;

        /// <summary>Confidence floor below which a decaying record is dropped.</summary>
        public readonly float MinimumConfidence;

        /// <summary>Confidence lost per decision step of age.</summary>
        public readonly float ConfidenceDecay;

        /// <summary>Maximum sensor range used to project proximity into a forward distance.</summary>
        public readonly float DetectionDistance;

        /// <summary>Lateral half-width of the sensor sweep used to project side into a lateral offset.</summary>
        public readonly float SweepSpread;

        /// <summary>Obstacle proximity at or above which a sensed obstacle is classified a hazard.</summary>
        public readonly float HazardProximityThreshold;

        /// <summary>Maximum age of a short-term memory that can still corroborate knowledge.</summary>
        public readonly float CorroborationWindow;

        /// <summary>Confidence gained per corroboration by a fresh short-term memory.</summary>
        public readonly float CorroborationBoost;

        public KnowledgePolicy(
            int maximumRecords,
            int victimLifetime,
            int obstacleLifetime,
            int hazardLifetime,
            int exploredLifetime,
            float mergeDistance,
            float minimumConfidence,
            float confidenceDecay,
            float detectionDistance,
            float sweepSpread,
            float hazardProximityThreshold,
            float corroborationWindow,
            float corroborationBoost)
        {
            MaximumRecords = maximumRecords;
            VictimLifetime = victimLifetime;
            ObstacleLifetime = obstacleLifetime;
            HazardLifetime = hazardLifetime;
            ExploredLifetime = exploredLifetime;
            MergeDistance = mergeDistance;
            MinimumConfidence = minimumConfidence;
            ConfidenceDecay = confidenceDecay;
            DetectionDistance = detectionDistance;
            SweepSpread = sweepSpread;
            HazardProximityThreshold = hazardProximityThreshold;
            CorroborationWindow = corroborationWindow;
            CorroborationBoost = corroborationBoost;
        }

        /// <summary>
        /// Default tuning for the rescue sweep. The hazard threshold matches the
        /// mission coordinator's default so the drone's knowledge and its mission
        /// classify the same obstacle as a hazard.
        /// </summary>
        public static KnowledgePolicy Default => new(
            maximumRecords: 16,
            victimLifetime: 12,
            obstacleLifetime: 8,
            hazardLifetime: 8,
            exploredLifetime: 20,
            mergeDistance: 2f,
            minimumConfidence: 0.10f,
            confidenceDecay: 0.10f,
            detectionDistance: 50f,
            sweepSpread: 10f,
            hazardProximityThreshold: 0.65f,
            corroborationWindow: 3f,
            corroborationBoost: 0.25f);

        /// <summary>
        /// The canonical zero configuration: no records, no lifetimes, no geometry.
        /// Used only to back the shared empty <see cref="WorldKnowledgeStore"/>.
        /// </summary>
        public static KnowledgePolicy Empty => new(
            0, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
    }
}
