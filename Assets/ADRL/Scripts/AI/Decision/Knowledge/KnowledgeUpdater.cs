namespace ADRL.AI.Decision.Knowledge
{
    using ADRL.AI.Decision.Memory;
    using UnityEngine;

    /// <summary>
    /// The single owner of all world-knowledge writes. It turns the current
    /// situation assessment - corroborated by the short-term behaviour memory -
    /// into updates of the persistent <see cref="WorldKnowledgeStore"/>: inserting
    /// new observations, refreshing and merging re-observations, decaying
    /// confidence, and expiring stale records. It never performs decisions, never
    /// scores and never selects; it only records what the drone currently knows.
    /// Fully deterministic: given the same assessment history and step clock it
    /// always produces the same knowledge state.
    /// </summary>
    /// <remarks>
    /// A detected target becomes a <c>Victim</c> record; a sensed obstacle at or
    /// above the policy hazard threshold becomes a <c>Hazard</c> record (aligned
    /// with the mission coordinator's hazard threshold), otherwise an
    /// <c>Obstacle</c> record. Positions are projected deterministically into the
    /// drone's local frame from the assessed proximity and side. When the current
    /// assessment lacks a detection but short-term memory still holds a fresh
    /// record, the updater corroborates the closest known record of that type
    /// (raising its confidence), so knowledge persists through a transiently
    /// occluded sensor frame. Timestamps come from the caller's deterministic step
    /// clock, never from <see cref="Time"/>. Intended for the Unity main thread
    /// only, matching the framework's main-thread assumption.
    /// </remarks>
    public sealed class KnowledgeUpdater
    {
        private readonly KnowledgePolicy _policy;
        private readonly WorldKnowledgeStore _store;

        public KnowledgeUpdater(KnowledgePolicy policy)
        {
            _policy = policy;
            _store = new WorldKnowledgeStore(policy);
        }

        /// <summary>The persistent store this service owns and updates.</summary>
        public WorldKnowledgeStore Store => _store;

        /// <summary>
        /// Applies one knowledge update from the current assessment and short-term
        /// memory at the given step clock, then expires stale records. The
        /// assessment is the primary source; the memory corroborates records the
        /// assessment can no longer see.
        /// </summary>
        public void Update(SituationSnapshot assessment, BehaviourMemory memory, float now)
        {
            if (assessment.IsValid && assessment.TargetDetected)
            {
                InsertOrRefresh(
                    KnowledgeType.Victim,
                    Project(assessment.TargetProximity, assessment.TargetSide),
                    now,
                    "Assessment");
            }
            else
            {
                Corroborate(KnowledgeType.Victim, now, memory.LastVictimSeen);
            }

            if (assessment.IsValid && assessment.ObstacleProximity > 0f)
            {
                var type = assessment.ObstacleProximity >= _policy.HazardProximityThreshold
                    ? KnowledgeType.Hazard
                    : KnowledgeType.Obstacle;
                InsertOrRefresh(type, Project(assessment.ObstacleProximity, assessment.ObstacleSide), now, "Assessment");
            }
            else
            {
                Corroborate(KnowledgeType.Obstacle, now, memory.LastObstacleSeen);
            }

            _store.Expire(now);
        }

        /// <summary>Restores the knowledge layer to a fresh, empty state.</summary>
        public void Reset()
        {
            _store.Clear();
        }

        /// <summary>
        /// Inserts a new observation, or refreshes the closest existing record of
        /// the same type within the merge distance. A re-observation fully
        /// re-confirms the record: confidence is restored to 1 and the age resets.
        /// </summary>
        private void InsertOrRefresh(KnowledgeType type, Vector3 position, float now, string source)
        {
            var record = new WorldKnowledgeRecord(type, position, 1f, (int)now, 0, source, true);
            if (!_store.Update(record))
                _store.Store(record);
        }

        /// <summary>
        /// When the current assessment cannot see an entity but short-term memory
        /// still holds a fresh record of it, the closest known record of that type
        /// is re-confirmed: its confidence rises by the corroboration boost and its
        /// age resets, while its stored position is kept.
        /// </summary>
        private void Corroborate(KnowledgeType type, float now, MemoryRecord memory)
        {
            if (!memory.IsValid || memory.Age > _policy.CorroborationWindow)
                return;

            var nearest = _store.QueryNearest(type, Vector3.zero);
            if (!nearest.IsValid)
                return;

            var confidence = Mathf.Min(1f, nearest.Confidence + _policy.CorroborationBoost);
            _store.Update(new WorldKnowledgeRecord(
                type, nearest.Position, confidence, (int)now, 0, "MemoryCorroboration", true));
        }

        /// <summary>
        /// Projects a normalized proximity/side assessment into the drone's local
        /// observation frame (origin at the drone, forward = +Z).
        /// </summary>
        private Vector3 Project(float proximity, float side)
        {
            var forward = proximity * _policy.DetectionDistance;
            var lateral = side * _policy.SweepSpread;
            return new Vector3(lateral, 0f, forward);
        }
    }
}
