namespace ADRL.AI.Decision.Knowledge
{
    using UnityEngine;

    /// <summary>
    /// Bounded, deterministic store of persistent world knowledge. It is the single
    /// owner of knowledge storage: it holds the fixed-capacity record array and
    /// exposes the primitive store/update/remove/expire/query operations. It never
    /// decides anything - it never scores, selects, classifies or interprets. All
    /// write policy (insert vs refresh vs merge, confidence updates, expiry timing)
    /// lives in the <see cref="KnowledgeUpdater"/>, the single owner of knowledge
    /// writes.
    /// </summary>
    /// <remarks>
    /// Records are immutable value snapshots; ageing, decay and refresh are
    /// expressed by replacing a stored record with a fresh one. The backing array
    /// is fixed at construction to the policy capacity, so knowledge can never grow
    /// without bound. Query results are returned as owned copies so a caller can
    /// never mutate the store through a query result. Deterministic: tie-breaking
    /// and eviction always resolve to the lowest stored slot / oldest timestamp,
    /// so identical operation sequences always produce identical stores. Intended
    /// for the Unity main thread only, matching the framework's main-thread
    /// assumption.
    /// </remarks>
    public sealed class WorldKnowledgeStore
    {
        /// <summary>Shared, read-only empty store for knowledge-free paths.</summary>
        public static readonly WorldKnowledgeStore Empty = new(KnowledgePolicy.Empty);

        private readonly WorldKnowledgeRecord[] _records;
        private readonly KnowledgePolicy _policy;
        private int _count;

        public WorldKnowledgeStore(KnowledgePolicy policy)
        {
            _policy = policy;
            _records = new WorldKnowledgeRecord[Mathf.Max(0, policy.MaximumRecords)];
        }

        /// <summary>The bounded number of record slots.</summary>
        public int Capacity => _records.Length;

        /// <summary>Number of currently stored, valid records.</summary>
        public int Count => _count;

        /// <summary>True when at least one valid record of the given type is stored.</summary>
        public bool Contains(KnowledgeType type)
        {
            for (var i = 0; i < _records.Length; i++)
            {
                if (_records[i].IsValid && _records[i].Type == type)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Number of currently stored, valid records of the given type. A
        /// non-allocating companion to <see cref="QueryAll(KnowledgeType)"/> for
        /// consumers that only need a count.
        /// </summary>
        public int CountOf(KnowledgeType type)
        {
            var count = 0;
            for (var i = 0; i < _records.Length; i++)
            {
                if (_records[i].IsValid && _records[i].Type == type)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Inserts a new record into the first free slot; when the store is full it
        /// evicts the oldest stored record first, keeping the store bounded. This is
        /// a pure insert - merging is the <see cref="KnowledgeUpdater"/>'s
        /// responsibility, expressed through <see cref="Update"/>.
        /// </summary>
        public void Store(WorldKnowledgeRecord record)
        {
            if (_records.Length == 0 || !record.IsValid)
                return;

            for (var i = 0; i < _records.Length; i++)
            {
                if (_records[i].IsValid)
                    continue;

                _records[i] = record;
                _count++;
                return;
            }

            var oldest = 0;
            for (var i = 1; i < _records.Length; i++)
            {
                if (_records[i].Timestamp < _records[oldest].Timestamp)
                    oldest = i;
            }

            _records[oldest] = record;
        }

        /// <summary>
        /// Replaces the first record of the same type within the policy merge
        /// distance of <paramref name="record"/>. Refreshing, merging and confidence
        /// updates all funnel through this primitive. Returns true when a matching
        /// record was replaced, false otherwise (the caller inserts instead).
        /// </summary>
        public bool Update(WorldKnowledgeRecord record)
        {
            var index = FindIndex(record.Type, record.Position);
            if (index < 0 || !record.IsValid)
                return false;

            _records[index] = record;
            return true;
        }

        /// <summary>
        /// Removes the first record of the given type within the policy merge
        /// distance of <paramref name="position"/>. Returns true when a record was
        /// removed.
        /// </summary>
        public bool Remove(KnowledgeType type, Vector3 position)
        {
            var index = FindIndex(type, position);
            if (index < 0)
                return false;

            _records[index] = WorldKnowledgeRecord.Invalid;
            _count--;
            return true;
        }

        /// <summary>Removes every stored record.</summary>
        public void Clear()
        {
            for (var i = 0; i < _records.Length; i++)
                _records[i] = WorldKnowledgeRecord.Invalid;

            _count = 0;
        }

        /// <summary>
        /// Ages and decays every stored record against <paramref name="now"/>, and
        /// drops records whose age exceeds their type lifetime or whose confidence
        /// decays below the policy minimum. Ageing is expressed by replacing a
        /// record with a fresh snapshot carrying the current age and confidence.
        /// </summary>
        public void Expire(float now)
        {
            for (var i = 0; i < _records.Length; i++)
            {
                if (!_records[i].IsValid)
                    continue;

                var age = Mathf.Max(0, (int)now - _records[i].Timestamp);
                var lifetime = LifetimeFor(_records[i].Type);
                var confidence = age > 0
                    ? Mathf.Max(0f, _records[i].Confidence - _policy.ConfidenceDecay)
                    : _records[i].Confidence;
                var valid = age <= lifetime && confidence >= _policy.MinimumConfidence;

                if (!valid)
                {
                    _records[i] = WorldKnowledgeRecord.Invalid;
                    _count--;
                    continue;
                }

                _records[i] = new WorldKnowledgeRecord(
                    _records[i].Type,
                    _records[i].Position,
                    confidence,
                    _records[i].Timestamp,
                    age,
                    _records[i].Source,
                    true);
            }
        }

        /// <summary>
        /// The nearest stored record of the given type to <paramref name="origin"/>,
        /// or <see cref="WorldKnowledgeRecord.Invalid"/> when none exists. Ties
        /// resolve to the first stored record (lowest slot index), keeping the query
        /// deterministic.
        /// </summary>
        public WorldKnowledgeRecord QueryNearest(KnowledgeType type, Vector3 origin)
        {
            var best = -1;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < _records.Length; i++)
            {
                if (!_records[i].IsValid || _records[i].Type != type)
                    continue;

                var distance = (_records[i].Position - origin).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return best < 0 ? WorldKnowledgeRecord.Invalid : _records[best];
        }

        /// <summary>All stored records as an owned copy, in deterministic storage order.</summary>
        public WorldKnowledgeRecord[] QueryAll()
        {
            var result = new WorldKnowledgeRecord[_count];
            var index = 0;
            for (var i = 0; i < _records.Length; i++)
            {
                if (_records[i].IsValid)
                    result[index++] = _records[i];
            }

            return result;
        }

        /// <summary>All stored records of the given type as an owned copy, in deterministic storage order.</summary>
        public WorldKnowledgeRecord[] QueryAll(KnowledgeType type)
        {
            var result = new WorldKnowledgeRecord[CountOf(type)];
            var index = 0;
            for (var i = 0; i < _records.Length; i++)
            {
                if (_records[i].IsValid && _records[i].Type == type)
                    result[index++] = _records[i];
            }

            return result;
        }

        private int FindIndex(KnowledgeType type, Vector3 position)
        {
            var mergeDistanceSquared = _policy.MergeDistance * _policy.MergeDistance;
            for (var i = 0; i < _records.Length; i++)
            {
                if (!_records[i].IsValid || _records[i].Type != type)
                    continue;

                if ((_records[i].Position - position).sqrMagnitude <= mergeDistanceSquared)
                    return i;
            }

            return -1;
        }

        private int LifetimeFor(KnowledgeType type)
        {
            switch (type)
            {
                case KnowledgeType.Victim:
                    return _policy.VictimLifetime;
                case KnowledgeType.Obstacle:
                    return _policy.ObstacleLifetime;
                case KnowledgeType.Hazard:
                    return _policy.HazardLifetime;
                case KnowledgeType.ExploredRegion:
                case KnowledgeType.SafeRegion:
                    return _policy.ExploredLifetime;
                default:
                    return 0;
            }
        }
    }
}
