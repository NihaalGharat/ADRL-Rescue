namespace ADRL.AI.Decision.Memory
{
    /// <summary>
    /// Immutable snapshot of a single piece of short-term runtime memory. It is
    /// produced by the <see cref="BehaviourMemoryService"/> and consumed (read-only)
    /// by behaviour selection. Records are value snapshots - never mutated in place;
    /// ageing and confidence updates replace a record with a fresh immutable one.
    /// </summary>
    public readonly struct MemoryRecord
    {
        /// <summary>The behaviour this memory pertains to (Approach = victim, Avoid = obstacle).</summary>
        public readonly BehaviourState Behaviour;

        /// <summary>The decision step at which this record was (re)stored.</summary>
        public readonly float Timestamp;

        /// <summary>Age in decision steps since <see cref="Timestamp"/> at the time this snapshot was created.</summary>
        public readonly float Age;

        /// <summary>Current confidence in (0, 1], decaying with age via <see cref="MemoryPolicy.ConfidenceDecay"/>.</summary>
        public readonly float Confidence;

        /// <summary>True while this record is usable; false once expired or cleared.</summary>
        public readonly bool IsValid;

        public MemoryRecord(
            BehaviourState behaviour,
            float timestamp,
            float age,
            float confidence,
            bool isValid)
        {
            Behaviour = behaviour;
            Timestamp = timestamp;
            Age = age;
            Confidence = confidence;
            IsValid = isValid;
        }

        /// <summary>An empty, unusable record for a missing memory slot.</summary>
        public static MemoryRecord Invalid => new(BehaviourState.Idle, 0f, 0f, 0f, false);
    }
}
