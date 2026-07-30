namespace ADRL.Drone.Core
{
    public readonly struct PoolStatistics
    {
        public int AvailableCount { get; }

        public int ActiveCount { get; }

        public int TotalObjects { get; }

        public int BorrowCount { get; }

        public int ReturnCount { get; }

        public int PoolMissCount { get; }

        public PoolStatistics(
            int availableCount,
            int activeCount,
            int totalObjects,
            int borrowCount,
            int returnCount,
            int poolMissCount)
        {
            AvailableCount = availableCount;
            ActiveCount = activeCount;
            TotalObjects = totalObjects;
            BorrowCount = borrowCount;
            ReturnCount = returnCount;
            PoolMissCount = poolMissCount;
        }

        public static PoolStatistics Empty => new PoolStatistics(0, 0, 0, 0, 0, 0);
    }
}
