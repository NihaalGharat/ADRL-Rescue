namespace ADRL.AI.Decision.Prioritization
{
    /// <summary>
    /// Immutable, single-owner configuration surface for the task-prioritization
    /// layer. Every base priority, bonus, penalty and tie tolerance the
    /// prioritizer consumes lives here; no magic numbers appear anywhere else in
    /// the prioritization implementation. All weights are expressed as plain
    /// deterministic scores so the layer stays testable without wall-clock or
    /// Unity Time dependency.
    /// </summary>
    public readonly struct PriorityPolicy
    {
        /// <summary>Base priority of a confirmed-victim (rescue) objective.</summary>
        public readonly float VictimPriority;

        /// <summary>Base priority of an imminent-hazard (avoid) objective.</summary>
        public readonly float HazardPriority;

        /// <summary>Base priority of an area-sweep (search) objective.</summary>
        public readonly float SearchPriority;

        /// <summary>Base priority of a post-rescue recovery (resume) objective.</summary>
        public readonly float ResumePriority;

        /// <summary>Base priority of the no-objective (idle) fallback.</summary>
        public readonly float IdlePriority;

        /// <summary>Bonus applied to an objective carried by behaviour memory.</summary>
        public readonly float MemoryBonus;

        /// <summary>Bonus applied to an objective backed by current perception.</summary>
        public readonly float ConfidenceBonus;

        /// <summary>Penalty scaled by objective distance; closer objectives rank higher.</summary>
        public readonly float DistancePenalty;

        /// <summary>
        /// Penalty applied to a re-scored hazard candidate while the mission is
        /// already committed to avoiding, preventing avoidance thrash.
        /// </summary>
        public readonly float CooldownPenalty;

        /// <summary>
        /// Scores within this tolerance of the best are treated as tied; the first
        /// in deterministic generation order wins, keeping arbitration stable.
        /// </summary>
        public readonly float PriorityTieTolerance;

        public PriorityPolicy(
            float victimPriority,
            float hazardPriority,
            float searchPriority,
            float resumePriority,
            float idlePriority,
            float memoryBonus,
            float confidenceBonus,
            float distancePenalty,
            float cooldownPenalty,
            float priorityTieTolerance)
        {
            VictimPriority = victimPriority;
            HazardPriority = hazardPriority;
            SearchPriority = searchPriority;
            ResumePriority = resumePriority;
            IdlePriority = idlePriority;
            MemoryBonus = memoryBonus;
            ConfidenceBonus = confidenceBonus;
            DistancePenalty = distancePenalty;
            CooldownPenalty = cooldownPenalty;
            PriorityTieTolerance = priorityTieTolerance;
        }

        /// <summary>
        /// Safety-first defaults for the rescue sweep: hazard handling outranks
        /// victim pursuit, investigation, recovery, sweeping and idle, matching the
        /// mission coordinator's default stance.
        /// </summary>
        public static PriorityPolicy Default => new(
            victimPriority: 0.8f,
            hazardPriority: 1f,
            searchPriority: 0.4f,
            resumePriority: 0.5f,
            idlePriority: 0f,
            memoryBonus: 0.15f,
            confidenceBonus: 0.2f,
            distancePenalty: 0.25f,
            cooldownPenalty: 0.2f,
            priorityTieTolerance: 0.001f);
    }
}
