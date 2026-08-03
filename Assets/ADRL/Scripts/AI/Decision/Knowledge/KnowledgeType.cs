namespace ADRL.AI.Decision.Knowledge
{
    /// <summary>
    /// Immutable classification of a single piece of world knowledge. The type is
    /// set once at record creation and never changes - records are replaced, never
    /// reclassified in place. <see cref="Unknown"/> is the canonical placeholder
    /// for an empty or unclassified record.
    /// </summary>
    public enum KnowledgeType
    {
        /// <summary>No classification; the placeholder for an empty record.</summary>
        Unknown = 0,

        /// <summary>A living target the drone has detected and confirmed.</summary>
        Victim,

        /// <summary>A static obstacle sensed in the environment.</summary>
        Obstacle,

        /// <summary>A hazard judged dangerous enough to avoid.</summary>
        Hazard,

        /// <summary>A region the drone has explored.</summary>
        ExploredRegion,

        /// <summary>A region known to be safe.</summary>
        SafeRegion,
    }
}
