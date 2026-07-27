namespace ADRL.Drone.Core
{
    using UnityEngine;

    public class DroneContext
    {
        public DroneSystemState FleetState { get; set; } = DroneSystemState.Uninitialized;

        public int EpisodeNumber { get; set; }

        public Transform DroneRoot { get; set; }

        public DroneConfiguration CurrentConfiguration { get; set; }

        public int RegisteredDroneCount { get; set; }

        public int ActiveDroneCount { get; set; }

        public int TotalRegistered { get; set; }

        public int InactiveCount { get; set; }

        public int DestroyedCount { get; set; }

        public int NextAvailableId { get; set; }

        public void Reset()
        {
            FleetState = DroneSystemState.Uninitialized;
            EpisodeNumber = 0;
            DroneRoot = null;
            CurrentConfiguration = null;
            RegisteredDroneCount = 0;
            ActiveDroneCount = 0;
            TotalRegistered = 0;
            InactiveCount = 0;
            DestroyedCount = 0;
            NextAvailableId = 0;
        }
    }
}
