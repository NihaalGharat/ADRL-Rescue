namespace ADRL.Environment.Events
{
    using ADRL.Core.Events;

    public readonly struct EnvironmentInitializedEvent : IEvent
    {
    }

    public readonly struct EnvironmentResetEvent : IEvent
    {
    }

    public readonly struct VictimRegisteredEvent : IEvent
    {
        public int VictimId { get; }

        public VictimRegisteredEvent(int victimId)
        {
            VictimId = victimId;
        }
    }

    public readonly struct HazardRegisteredEvent : IEvent
    {
        public int HazardId { get; }

        public HazardRegisteredEvent(int hazardId)
        {
            HazardId = hazardId;
        }
    }
}
