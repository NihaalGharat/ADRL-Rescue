namespace ADRL.Drone.Events
{
    using ADRL.Core.Events;
    using ADRL.Drone.Core;

    public readonly struct DroneSpawningEvent : IEvent
    {
        public SpawnRequest Request { get; }
        public SpawnParameters Parameters { get; }

        public DroneSpawningEvent(SpawnRequest request, SpawnParameters parameters)
        {
            Request = request;
            Parameters = parameters;
        }
    }

    public readonly struct DroneSpawnCompletedEvent : IEvent
    {
        public SpawnResult Result { get; }

        public DroneSpawnCompletedEvent(SpawnResult result)
        {
            Result = result;
        }
    }
}
