namespace ADRL.Drone.Interfaces
{
    using System.Collections.Generic;
    using ADRL.Drone.Core;

    public interface IDroneDiagnostics
    {
        DroneSubsystemHealth Health { get; }
        DroneRuntimeState CurrentRuntimeState { get; }
        DroneSystemState SystemState { get; }
        bool IsServiceProviderReady { get; }
        bool LastValidationPassed { get; }
        int RegisteredDroneCount { get; }
        DroneFleetStatistics FleetStatistics { get; }
        int CurrentEpisode { get; }
        float InitializationDuration { get; }
        float SubsystemUptime { get; }
        long LastValidationTime { get; }
        int PendingSpawnCount { get; }
        int TotalPoolObjects { get; }
        int TotalBorrowCount { get; }
        int TotalReturnCount { get; }
        int TotalPoolMissCount { get; }
        IReadOnlyDictionary<string, PoolStatistics> PoolStatisticsByType { get; }
        DroneHealthReport HealthReport { get; }
    }
}
