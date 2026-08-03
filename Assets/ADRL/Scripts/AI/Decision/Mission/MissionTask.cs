namespace ADRL.AI.Decision.Mission
{
    /// <summary>
    /// Immutable snapshot of the mission coordinator's current objective. It
    /// carries the active <see cref="State"/>, the deterministic step at which the
    /// state was entered (<see cref="EntryStep"/>), and the state to restore after a
    /// temporary hazard (<see cref="PreviousState"/>). Never mutated in place - every
    /// transition produces a fresh value.
    /// </summary>
    public readonly struct MissionTask
    {
        /// <summary>The current mission objective.</summary>
        public readonly MissionTaskState State;

        /// <summary>The deterministic step clock value at which this state was entered.</summary>
        public readonly float EntryStep;

        /// <summary>
        /// The mission state to restore when <see cref="AvoidHazard"/> clears. Only
        /// meaningful after a hazard override; harmless otherwise.
        /// </summary>
        public readonly MissionTaskState PreviousState;

        /// <summary>True when this snapshot holds a real mission state.</summary>
        public readonly bool IsValid;

        public MissionTask(
            MissionTaskState state,
            float entryStep,
            MissionTaskState previousState,
            bool isValid)
        {
            State = state;
            EntryStep = entryStep;
            PreviousState = previousState;
            IsValid = isValid;
        }

        /// <summary>An empty, unusable mission snapshot.</summary>
        public static MissionTask Invalid =>
            new(MissionTaskState.Idle, 0f, MissionTaskState.Idle, false);
    }
}
