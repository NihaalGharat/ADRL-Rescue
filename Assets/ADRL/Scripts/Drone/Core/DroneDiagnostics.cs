namespace ADRL.Drone.Core
{
    using ADRL.Drone.Interfaces;

    public class DroneDiagnostics : IDroneDiagnostics
    {
        public DroneSubsystemHealth Health { get; }
        public DroneRuntimeState CurrentRuntimeState { get; }
        public DroneSystemState SystemState { get; }
        public bool IsServiceProviderReady { get; }
        public bool LastValidationPassed { get; }
        public int RegisteredDroneCount { get; }
        public DroneFleetStatistics FleetStatistics { get; }
        public int CurrentEpisode { get; }
        public float InitializationDuration { get; }
        public float SubsystemUptime { get; }
        public long LastValidationTime { get; }

        public DroneDiagnostics(
            DroneSubsystemHealth health,
            DroneRuntimeState currentRuntimeState,
            DroneSystemState systemState,
            bool isServiceProviderReady,
            bool lastValidationPassed,
            int registeredDroneCount,
            DroneFleetStatistics fleetStatistics,
            int currentEpisode,
            float initializationDuration,
            float subsystemUptime,
            long lastValidationTime)
        {
            Health = health;
            CurrentRuntimeState = currentRuntimeState;
            SystemState = systemState;
            IsServiceProviderReady = isServiceProviderReady;
            LastValidationPassed = lastValidationPassed;
            RegisteredDroneCount = registeredDroneCount;
            FleetStatistics = fleetStatistics;
            CurrentEpisode = currentEpisode;
            InitializationDuration = initializationDuration;
            SubsystemUptime = subsystemUptime;
            LastValidationTime = lastValidationTime;
        }
    }
}
