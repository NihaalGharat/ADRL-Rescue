namespace ADRL.Environment.Events
{
    using ADRL.Core.Events;

    public readonly struct TerrainGenerationStartedEvent : IEvent
    {
    }

    public readonly struct TerrainGeneratedEvent : IEvent
    {
    }

    public readonly struct TerrainGenerationFailedEvent : IEvent
    {
        public string Reason { get; }

        public TerrainGenerationFailedEvent(string reason)
        {
            Reason = reason;
        }
    }
}
