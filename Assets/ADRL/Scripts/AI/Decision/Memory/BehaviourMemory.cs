namespace ADRL.AI.Decision.Memory
{
    using UnityEngine;

    /// <summary>
    /// Bounded, deterministic store for short-term behaviour memory. It owns the
    /// currently-relevant records (last victim seen, last obstacle seen, last
    /// behaviour) and never decides anything itself - it only stores and retrieves
    /// context on behalf of the <see cref="BehaviourMemoryService"/>. The backing
    /// array is fixed at construction to <see cref="Capacity"/>, so memory can never
    /// grow without bound.
    /// </summary>
    /// <remarks>
    /// Records are immutable snapshots; ageing and confidence decay are expressed by
    /// replacing stored records with updated immutable ones during
    /// <see cref="Expire"/>. Intended for use on the Unity main thread only - no
    /// locks are taken, matching the framework's main-thread assumption.
    /// </remarks>
    public sealed class BehaviourMemory
    {
        /// <summary>Shared, read-only empty memory for memory-free selection paths.</summary>
        public static readonly BehaviourMemory Empty = new(0);

        private readonly MemoryRecord[] _records;
        private int _count;

        /// <summary>Most recently selected behaviour, used for continuity decisions.</summary>
        public BehaviourState LastBehaviour { get; private set; } = BehaviourState.Idle;

        /// <summary>The bounded number of record slots.</summary>
        public int Capacity => _records.Length;

        /// <summary>Number of currently stored, valid records.</summary>
        public int Count => _count;

        public BehaviourMemory(int capacity)
        {
            _records = new MemoryRecord[Mathf.Max(1, capacity)];
        }

        /// <summary>Stored victim record, or <see cref="MemoryRecord.Invalid"/> when absent.</summary>
        public MemoryRecord LastVictimSeen => Retrieve(BehaviourState.Approach);

        /// <summary>Stored obstacle record, or <see cref="MemoryRecord.Invalid"/> when absent.</summary>
        public MemoryRecord LastObstacleSeen => Retrieve(BehaviourState.Avoid);

        /// <summary>
        /// Stores (or refreshes) a record for the given behaviour, replacing any
        /// existing record for the same behaviour. When the store is full a new
        /// behaviour evicts the oldest stored record first, keeping the store
        /// bounded.
        /// </summary>
        public void Store(BehaviourState behaviour, float timestamp)
        {
            for (var i = 0; i < _records.Length; i++)
            {
                if (!_records[i].IsValid)
                    continue;

                if (_records[i].Behaviour == behaviour)
                {
                    _records[i] = new MemoryRecord(behaviour, timestamp, 0f, 1f, true);
                    return;
                }
            }

            if (_count < _records.Length)
            {
                for (var i = 0; i < _records.Length; i++)
                {
                    if (_records[i].IsValid)
                        continue;

                    _records[i] = new MemoryRecord(behaviour, timestamp, 0f, 1f, true);
                    _count++;
                    return;
                }
            }

            var oldest = 0;
            for (var i = 1; i < _records.Length; i++)
            {
                if (_records[i].Timestamp < _records[oldest].Timestamp)
                    oldest = i;
            }

            _records[oldest] = new MemoryRecord(behaviour, timestamp, 0f, 1f, true);
        }

        /// <summary>Returns the stored record for the behaviour, or <see cref="MemoryRecord.Invalid"/>.</summary>
        public MemoryRecord Retrieve(BehaviourState behaviour)
        {
            for (var i = 0; i < _records.Length; i++)
            {
                if (_records[i].IsValid && _records[i].Behaviour == behaviour)
                    return _records[i];
            }

            return MemoryRecord.Invalid;
        }

        /// <summary>Drops the stored record for the given behaviour, if any.</summary>
        public void Remove(BehaviourState behaviour)
        {
            for (var i = 0; i < _records.Length; i++)
            {
                if (!_records[i].IsValid || _records[i].Behaviour != behaviour)
                    continue;

                _records[i] = MemoryRecord.Invalid;
                _count--;
                return;
            }
        }

        /// <summary>Removes every stored record and resets the last-behaviour marker.</summary>
        public void Clear()
        {
            for (var i = 0; i < _records.Length; i++)
                _records[i] = MemoryRecord.Invalid;

            _count = 0;
            LastBehaviour = BehaviourState.Idle;
        }

        /// <summary>Records the most recently selected behaviour for continuity.</summary>
        public void RecordLastBehaviour(BehaviourState behaviour)
        {
            LastBehaviour = behaviour;
        }

        /// <summary>
        /// Ages every stored record against <paramref name="now"/>, decaying its
        /// confidence and dropping records whose age exceeds the configured duration
        /// for their behaviour. Expiry is driven by duration and confidence alone;
        /// <see cref="MemoryPolicy.RefreshThreshold"/> governs whether a
        /// re-observation extends or re-bases a memory in the service instead.
        /// </summary>
        public void Expire(float now, MemoryPolicy policy)
        {
            for (var i = 0; i < _records.Length; i++)
            {
                if (!_records[i].IsValid)
                    continue;

                var age = Mathf.Max(0f, now - _records[i].Timestamp);
                var duration = _records[i].Behaviour == BehaviourState.Approach
                    ? policy.VictimMemoryDuration
                    : policy.ObstacleMemoryDuration;
                var confidence = Mathf.Max(0f, 1f - age * policy.ConfidenceDecay);
                var valid = age <= duration && confidence > 0f;

                if (!valid)
                {
                    _records[i] = MemoryRecord.Invalid;
                    _count--;
                    continue;
                }

                _records[i] = new MemoryRecord(
                    _records[i].Behaviour,
                    _records[i].Timestamp,
                    age,
                    confidence,
                    true);
            }
        }
    }
}
