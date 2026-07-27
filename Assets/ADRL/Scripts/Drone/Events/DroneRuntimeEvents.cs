namespace ADRL.Drone.Events
{
    using ADRL.Core.Events;
    using ADRL.Drone.Core;

    public readonly struct DroneRuntimeStateChangingEvent : IEvent
    {
        public int DroneId { get; }
        public DroneRuntimeState CurrentState { get; }
        public DroneRuntimeState NewState { get; }

        public DroneRuntimeStateChangingEvent(
            int droneId,
            DroneRuntimeState currentState,
            DroneRuntimeState newState)
        {
            DroneId = droneId;
            CurrentState = currentState;
            NewState = newState;
        }
    }

    public readonly struct DroneRuntimeStateChangedEvent : IEvent
    {
        public int DroneId { get; }
        public DroneRuntimeState PreviousState { get; }
        public DroneRuntimeState NewState { get; }

        public DroneRuntimeStateChangedEvent(
            int droneId,
            DroneRuntimeState previousState,
            DroneRuntimeState newState)
        {
            DroneId = droneId;
            PreviousState = previousState;
            NewState = newState;
        }
    }

    public readonly struct FleetStateChangedEvent : IEvent
    {
        public DroneSystemState PreviousState { get; }
        public DroneSystemState NewState { get; }

        public FleetStateChangedEvent(
            DroneSystemState previousState,
            DroneSystemState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    public readonly struct FleetResettingEvent : IEvent
    {
        public int EpisodeNumber { get; }

        public FleetResettingEvent(int episodeNumber)
        {
            EpisodeNumber = episodeNumber;
        }
    }

    public readonly struct FleetResetCompletedEvent : IEvent
    {
        public int EpisodeNumber { get; }

        public FleetResetCompletedEvent(int episodeNumber)
        {
            EpisodeNumber = episodeNumber;
        }
    }
}
