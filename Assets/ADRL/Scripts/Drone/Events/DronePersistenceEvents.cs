namespace ADRL.Drone.Events
{
    using ADRL.Core.Events;

    public readonly struct RuntimeSnapshotCreatedEvent : IEvent
    {
        public int SnapshotIndex { get; }
        public int EntryCount { get; }

        public RuntimeSnapshotCreatedEvent(int snapshotIndex, int entryCount)
        {
            SnapshotIndex = snapshotIndex;
            EntryCount = entryCount;
        }
    }

    public readonly struct RuntimeRestoreStartedEvent : IEvent
    {
    }

    public readonly struct RuntimeRestoreCompletedEvent : IEvent
    {
        public bool RolledBack { get; }

        public RuntimeRestoreCompletedEvent(bool rolledBack)
        {
            RolledBack = rolledBack;
        }
    }

    public readonly struct RuntimeRestoreFailedEvent : IEvent
    {
        public string Reason { get; }

        public RuntimeRestoreFailedEvent(string reason)
        {
            Reason = reason;
        }
    }

    public readonly struct RuntimeSnapshotClearedEvent : IEvent
    {
    }
}
