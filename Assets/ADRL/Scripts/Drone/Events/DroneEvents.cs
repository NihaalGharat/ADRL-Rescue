namespace ADRL.Drone.Events
{
    using ADRL.Core.Events;
    using ADRL.Drone.Controllers;

    public readonly struct DroneActivatedEvent : IEvent
    {
        public int DroneId { get; }

        public DroneActivatedEvent(int droneId)
        {
            DroneId = droneId;
        }
    }

    public readonly struct DroneStateChangedEvent : IEvent
    {
        public int DroneId { get; }
        public DroneState PreviousState { get; }
        public DroneState NewState { get; }

        public DroneStateChangedEvent(int droneId, DroneState previousState, DroneState newState)
        {
            DroneId = droneId;
            PreviousState = previousState;
            NewState = newState;
        }
    }

    public readonly struct BatteryLowEvent : IEvent
    {
        public int DroneId { get; }
        public float CurrentEnergy { get; }

        public BatteryLowEvent(int droneId, float currentEnergy)
        {
            DroneId = droneId;
            CurrentEnergy = currentEnergy;
        }
    }

    public readonly struct BatteryCriticalEvent : IEvent
    {
        public int DroneId { get; }
        public float CurrentEnergy { get; }

        public BatteryCriticalEvent(int droneId, float currentEnergy)
        {
            DroneId = droneId;
            CurrentEnergy = currentEnergy;
        }
    }

    public readonly struct EmergencyStopEvent : IEvent
    {
        public int DroneId { get; }

        public EmergencyStopEvent(int droneId)
        {
            DroneId = droneId;
        }
    }
}
