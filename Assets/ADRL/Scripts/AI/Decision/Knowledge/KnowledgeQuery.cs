namespace ADRL.AI.Decision.Knowledge
{
    using UnityEngine;

    /// <summary>
    /// Pure retrieval facade over a <see cref="WorldKnowledgeStore"/>. It is
    /// stateless (it holds only a read-only store reference) and implements
    /// retrieval only - none of its members ever writes to the store. Consumers
    /// read the world through this facade; writers write exclusively through the
    /// <see cref="KnowledgeUpdater"/>.
    /// </summary>
    public sealed class KnowledgeQuery : IKnowledgeQuery
    {
        private readonly WorldKnowledgeStore _store;

        public KnowledgeQuery(WorldKnowledgeStore store)
        {
            _store = store ?? WorldKnowledgeStore.Empty;
        }

        /// <inheritdoc/>
        public WorldKnowledgeRecord NearestVictim(Vector3 origin) => _store.QueryNearest(KnowledgeType.Victim, origin);

        /// <inheritdoc/>
        public WorldKnowledgeRecord NearestHazard(Vector3 origin) => _store.QueryNearest(KnowledgeType.Hazard, origin);

        /// <inheritdoc/>
        public WorldKnowledgeRecord NearestObstacle(Vector3 origin) => _store.QueryNearest(KnowledgeType.Obstacle, origin);

        /// <inheritdoc/>
        public WorldKnowledgeRecord[] KnownVictims() => _store.QueryAll(KnowledgeType.Victim);

        /// <inheritdoc/>
        public WorldKnowledgeRecord[] KnownHazards() => _store.QueryAll(KnowledgeType.Hazard);

        /// <inheritdoc/>
        public WorldKnowledgeRecord[] KnownObstacles() => _store.QueryAll(KnowledgeType.Obstacle);

        /// <inheritdoc/>
        public WorldKnowledgeRecord[] KnownRegions()
        {
            var explored = _store.QueryAll(KnowledgeType.ExploredRegion);
            var safe = _store.QueryAll(KnowledgeType.SafeRegion);
            var result = new WorldKnowledgeRecord[explored.Length + safe.Length];
            System.Array.Copy(explored, 0, result, 0, explored.Length);
            System.Array.Copy(safe, 0, result, explored.Length, safe.Length);
            return result;
        }
    }
}
