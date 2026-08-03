namespace ADRL.AI.Decision.Knowledge
{
    using UnityEngine;

    /// <summary>
    /// Immutable snapshot of a single piece of world knowledge the drone holds
    /// about its environment. Records are value snapshots produced by the
    /// <see cref="KnowledgeUpdater"/> and stored by the
    /// <see cref="WorldKnowledgeStore"/>; consumers read them and never mutate
    /// them. Ageing, confidence decay and re-observation all replace a record with
    /// a fresh immutable value - there is no mutable state anywhere in this type.
    /// </summary>
    /// <remarks>
    /// <see cref="Position"/> is expressed in the drone's local observation frame
    /// (origin at the drone, forward = +Z) as deterministically projected from the
    /// assessed situation; the decision layer has no world-space access, so world
    /// anchoring is out of scope for this phase. Timestamps are deterministic
    /// decision-step integers from the caller's step clock - never wall-clock time.
    /// </remarks>
    public readonly struct WorldKnowledgeRecord
    {
        /// <summary>What kind of world knowledge this record holds.</summary>
        public readonly KnowledgeType Type;

        /// <summary>Where the record's subject is, in the drone's local frame.</summary>
        public readonly Vector3 Position;

        /// <summary>Current confidence in (0, 1], decaying with age and re-raised by re-observation.</summary>
        public readonly float Confidence;

        /// <summary>The decision step at which this record was (re)stored.</summary>
        public readonly int Timestamp;

        /// <summary>Age in decision steps since <see cref="Timestamp"/> at the time this snapshot was created.</summary>
        public readonly int Age;

        /// <summary>The source that produced the observation (for example <c>Assessment</c>).</summary>
        public readonly string Source;

        /// <summary>True while this record is usable; false once expired or cleared.</summary>
        public readonly bool IsValid;

        public WorldKnowledgeRecord(
            KnowledgeType type,
            Vector3 position,
            float confidence,
            int timestamp,
            int age,
            string source,
            bool isValid)
        {
            Type = type;
            Position = position;
            Confidence = confidence;
            Timestamp = timestamp;
            Age = age;
            Source = source;
            IsValid = isValid;
        }

        /// <summary>An empty, unusable record for a missing knowledge slot.</summary>
        public static WorldKnowledgeRecord Invalid => new(
            KnowledgeType.Unknown, Vector3.zero, 0f, 0, 0, string.Empty, false);
    }
}
