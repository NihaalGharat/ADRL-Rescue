namespace ADRL.Drone.Core
{
    using System.Collections.Generic;
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
        public int PendingSpawnCount { get; }
        public int TotalPoolObjects { get; }
        public int TotalBorrowCount { get; }
        public int TotalReturnCount { get; }
        public int TotalPoolMissCount { get; }
        public IReadOnlyDictionary<string, PoolStatistics> PoolStatisticsByType { get; }
        public DroneHealthReport HealthReport { get; }

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
            long lastValidationTime,
            int pendingSpawnCount = 0,
            int totalPoolObjects = 0,
            int totalBorrowCount = 0,
            int totalReturnCount = 0,
            int totalPoolMissCount = 0,
            IReadOnlyDictionary<string, PoolStatistics> poolStatisticsByType = null,
            DroneHealthReport healthReport = default)
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
            PendingSpawnCount = pendingSpawnCount;
            TotalPoolObjects = totalPoolObjects;
            TotalBorrowCount = totalBorrowCount;
            TotalReturnCount = totalReturnCount;
            TotalPoolMissCount = totalPoolMissCount;
            PoolStatisticsByType = poolStatisticsByType;
            HealthReport = healthReport;
        }
    }
}
