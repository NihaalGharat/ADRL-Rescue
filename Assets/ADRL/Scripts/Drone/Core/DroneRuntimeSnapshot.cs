namespace ADRL.Drone.Core
{
    using System.Collections.Generic;

    public readonly struct DroneRuntimeSnapshot
    {
        public const int CurrentVersion = 1;

        public int SnapshotVersion { get; }
        public long Timestamp { get; }
        public int EpisodeNumber { get; }
        public DroneSystemState FleetRuntimeState { get; }
        public int RegisteredDroneCount { get; }
        public int TotalRegistered { get; }
        public int NextAvailableId { get; }
        public IReadOnlyList<DroneSnapshotEntry> Entries { get; }
        public DroneFleetStatistics FleetStatistics { get; }

        public DroneRuntimeSnapshot(
            int snapshotVersion,
            long timestamp,
            int episodeNumber,
            DroneSystemState fleetRuntimeState,
            int registeredDroneCount,
            int totalRegistered,
            int nextAvailableId,
            IReadOnlyList<DroneSnapshotEntry> entries,
            DroneFleetStatistics fleetStatistics)
        {
            SnapshotVersion = snapshotVersion;
            Timestamp = timestamp;
            EpisodeNumber = episodeNumber;
            FleetRuntimeState = fleetRuntimeState;
            RegisteredDroneCount = registeredDroneCount;
            TotalRegistered = totalRegistered;
            NextAvailableId = nextAvailableId;
            Entries = entries;
            FleetStatistics = fleetStatistics;
        }
    }
}
