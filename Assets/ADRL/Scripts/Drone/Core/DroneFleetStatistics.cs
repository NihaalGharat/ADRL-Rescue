namespace ADRL.Drone.Core
{
    public readonly struct DroneFleetStatistics
    {
        public int Registered { get; }
        public int Active { get; }
        public int Idle { get; }
        public int Paused { get; }
        public int Returning { get; }
        public int Destroyed { get; }
        public int Initializing { get; }
        public int TotalRegistered { get; }

        public DroneFleetStatistics(
            int registered,
            int active,
            int idle,
            int paused,
            int returning,
            int destroyed,
            int initializing,
            int totalRegistered)
        {
            Registered = registered;
            Active = active;
            Idle = idle;
            Paused = paused;
            Returning = returning;
            Destroyed = destroyed;
            Initializing = initializing;
            TotalRegistered = totalRegistered;
        }
    }
}
