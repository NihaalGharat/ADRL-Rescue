namespace ADRL.AI.Decision.Prioritization
{
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// Immutable snapshot of a scored candidate objective. It couples the candidate
    /// <see cref="Task"/> with the deterministic <see cref="PriorityScore"/> the
    /// prioritizer assigned and the <see cref="Confidence"/> backing that score.
    /// Never mutated in place - every evaluation produces a fresh value.
    /// </summary>
    public readonly struct TaskPriority
    {
        /// <summary>The candidate objective being scored.</summary>
        public readonly MissionTask Task;

        /// <summary>Deterministic priority score; higher wins.</summary>
        public readonly float PriorityScore;

        /// <summary>Confidence in (0, 1] backing the score; 1 = current perception.</summary>
        public readonly float Confidence;

        /// <summary>True while this snapshot holds a real scored candidate.</summary>
        public readonly bool IsValid;

        public TaskPriority(MissionTask task, float priorityScore, float confidence, bool isValid)
        {
            Task = task;
            PriorityScore = priorityScore;
            Confidence = confidence;
            IsValid = isValid;
        }

        /// <summary>An empty, unusable scored candidate.</summary>
        public static TaskPriority Invalid => new(MissionTask.Invalid, 0f, 0f, false);
    }
}
