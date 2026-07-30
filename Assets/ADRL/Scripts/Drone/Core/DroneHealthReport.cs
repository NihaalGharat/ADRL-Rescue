namespace ADRL.Drone.Core
{
    public readonly struct DroneHealthReport
    {
        public bool IsOperational { get; }
        public int RegisteredDroneCount { get; }
        public int ActiveDroneCount { get; }
        public int IdleDroneCount { get; }
        public int PoolBorrowCount { get; }
        public int PoolReturnCount { get; }
        public int PoolMissCount { get; }
        public int PendingSpawnCount { get; }
        public int TotalPoolObjects { get; }
        public string SubsystemStatus { get; }

        public DroneHealthReport(
            bool isOperational,
            int registeredDroneCount,
            int activeDroneCount,
            int idleDroneCount,
            int poolBorrowCount,
            int poolReturnCount,
            int poolMissCount,
            int pendingSpawnCount,
            int totalPoolObjects,
            string subsystemStatus)
        {
            IsOperational = isOperational;
            RegisteredDroneCount = registeredDroneCount;
            ActiveDroneCount = activeDroneCount;
            IdleDroneCount = idleDroneCount;
            PoolBorrowCount = poolBorrowCount;
            PoolReturnCount = poolReturnCount;
            PoolMissCount = poolMissCount;
            PendingSpawnCount = pendingSpawnCount;
            TotalPoolObjects = totalPoolObjects;
            SubsystemStatus = subsystemStatus ?? string.Empty;
        }
    }
}
