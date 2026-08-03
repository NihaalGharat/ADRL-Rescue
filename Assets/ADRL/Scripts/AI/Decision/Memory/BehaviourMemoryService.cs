namespace ADRL.AI.Decision.Memory
{
    using UnityEngine;

    /// <summary>
    /// The single owner of all runtime behaviour-memory updates. It refreshes
    /// existing memory from the current assessment, expires stale records, decays
    /// confidence, and clears invalid records. It never generates commands and
    /// never selects behaviours - it only stores and retrieves runtime context for
    /// the decision engine. Fully deterministic: given the same assessment history
    /// and step clock, it always produces the same memory state.
    /// </summary>
    /// <remarks>
    /// Intended for the Unity main thread only (no locks), matching the framework's
    /// main-thread assumption. Timestamps come from the caller's deterministic step
    /// clock, never from <see cref="Time"/>, keeping the layer testable.
    /// </remarks>
    public sealed class BehaviourMemoryService
    {
        private readonly MemoryPolicy _policy;
        private readonly BehaviourMemory _memory;

        public BehaviourMemoryService(MemoryPolicy policy)
        {
            _policy = policy;
            _memory = new BehaviourMemory(policy.MaxRecords);
        }

        /// <summary>The bounded store this service owns and updates.</summary>
        public BehaviourMemory Memory => _memory;

        /// <summary>Most recently stored victim record, or <see cref="MemoryRecord.Invalid"/>.</summary>
        public MemoryRecord LastVictimSeen => _memory.LastVictimSeen;

        /// <summary>Most recently stored obstacle record, or <see cref="MemoryRecord.Invalid"/>.</summary>
        public MemoryRecord LastObstacleSeen => _memory.LastObstacleSeen;

        /// <summary>Most recently selected behaviour for continuity.</summary>
        public BehaviourState LastBehaviour => _memory.LastBehaviour;

        /// <summary>
        /// Applies one memory update from the current assessment at the given step
        /// clock. A freshly seen victim refreshes victim memory; a sensed obstacle
        /// refreshes obstacle memory. Then stale records are expired and invalid
        /// records cleared. Re-observing an entity while its memory is still within
        /// <see cref="MemoryPolicy.RefreshThreshold"/> extends that memory; an older
        /// re-observation re-bases it as a fresh memory.
        /// </summary>
        public void Update(SituationSnapshot assessment, float now)
        {
            if (assessment.IsValid && assessment.TargetDetected)
                StoreVictim(assessment, now);

            if (assessment.IsValid && assessment.ObstacleProximity > 0f)
                StoreObstacle(assessment, now);

            _memory.Expire(now, _policy);
        }

        /// <summary>Records the most recently selected behaviour for continuity.</summary>
        public void RecordBehaviour(BehaviourState behaviour)
        {
            _memory.RecordLastBehaviour(behaviour);
        }

        /// <summary>Restores the memory layer to a fresh, empty state.</summary>
        public void Reset()
        {
            _memory.Clear();
        }

        private void StoreVictim(SituationSnapshot assessment, float now)
        {
            var existing = _memory.LastVictimSeen;
            if (existing.IsValid && now - existing.Timestamp <= _policy.RefreshThreshold)
            {
                _memory.Store(BehaviourState.Approach, now);
                return;
            }

            if (existing.IsValid)
                _memory.Remove(BehaviourState.Approach);

            _memory.Store(BehaviourState.Approach, now);
        }

        private void StoreObstacle(SituationSnapshot assessment, float now)
        {
            var existing = _memory.LastObstacleSeen;
            if (existing.IsValid && now - existing.Timestamp <= _policy.RefreshThreshold)
            {
                _memory.Store(BehaviourState.Avoid, now);
                return;
            }

            if (existing.IsValid)
                _memory.Remove(BehaviourState.Avoid);

            _memory.Store(BehaviourState.Avoid, now);
        }
    }
}
