namespace ADRL.Drone.Interfaces
{
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
    }
}
