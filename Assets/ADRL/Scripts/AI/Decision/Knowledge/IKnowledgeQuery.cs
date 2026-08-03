namespace ADRL.AI.Decision.Knowledge
{
    using UnityEngine;

    /// <summary>
    /// Pure retrieval contract over a <see cref="WorldKnowledgeStore"/>. Queries
    /// only ever read the store - they never update, merge, expire or remove
    /// records. The store remains the single owner of storage and the
    /// <see cref="KnowledgeUpdater"/> remains the single owner of writes; this
    /// interface is the read-only view every consumer goes through.
    /// </summary>
    public interface IKnowledgeQuery
    {
        /// <summary>The nearest known victim to the given origin, or <see cref="WorldKnowledgeRecord.Invalid"/>.</summary>
        WorldKnowledgeRecord NearestVictim(Vector3 origin);

        /// <summary>The nearest known hazard to the given origin, or <see cref="WorldKnowledgeRecord.Invalid"/>.</summary>
        WorldKnowledgeRecord NearestHazard(Vector3 origin);

        /// <summary>The nearest known obstacle to the given origin, or <see cref="WorldKnowledgeRecord.Invalid"/>.</summary>
        WorldKnowledgeRecord NearestObstacle(Vector3 origin);

        /// <summary>All known victims in deterministic storage order.</summary>
        WorldKnowledgeRecord[] KnownVictims();

        /// <summary>All known hazards in deterministic storage order.</summary>
        WorldKnowledgeRecord[] KnownHazards();

        /// <summary>All known obstacles in deterministic storage order.</summary>
        WorldKnowledgeRecord[] KnownObstacles();

        /// <summary>All known regions (explored and safe) in deterministic storage order.</summary>
        WorldKnowledgeRecord[] KnownRegions();
    }
}
