namespace ADRL.Drone.Interfaces
{
    using ADRL.Drone.Core;

    public interface IDroneContext
    {
        DroneSystemState FleetState { get; }
        int EpisodeNumber { get; }
        int RegisteredDroneCount { get; }
        int ActiveDroneCount { get; }
        int TotalRegistered { get; }
        int InactiveCount { get; }
        int DestroyedCount { get; }
        int NextAvailableId { get; }
    }
}
