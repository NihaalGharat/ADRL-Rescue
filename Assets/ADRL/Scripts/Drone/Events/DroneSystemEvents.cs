namespace ADRL.Drone.Events
{
    using ADRL.Core.Events;

    public readonly struct DroneInitializingEvent : IEvent
    {
    }

    public readonly struct DroneInitializedEvent : IEvent
    {
    }

    public readonly struct DroneShutdownEvent : IEvent
    {
    }

    public readonly struct DroneRegisteredEvent : IEvent
    {
        public int DroneId { get; }

        public DroneRegisteredEvent(int droneId)
        {
            DroneId = droneId;
        }
    }

    public readonly struct DroneUnregisteredEvent : IEvent
    {
        public int DroneId { get; }

        public DroneUnregisteredEvent(int droneId)
        {
            DroneId = droneId;
        }
    }

    public readonly struct SubsystemStartingEvent : IEvent
    {
    }

    public readonly struct SubsystemReadyEvent : IEvent
    {
    }

    public readonly struct SubsystemValidatedEvent : IEvent
    {
        public bool Passed { get; }

        public SubsystemValidatedEvent(bool passed)
        {
            Passed = passed;
        }
    }

    public readonly struct SubsystemFaultedEvent : IEvent
    {
        public string Reason { get; }

        public SubsystemFaultedEvent(string reason)
        {
            Reason = reason;
        }
    }

    public readonly struct SubsystemShutdownEvent : IEvent
    {
    }
}
