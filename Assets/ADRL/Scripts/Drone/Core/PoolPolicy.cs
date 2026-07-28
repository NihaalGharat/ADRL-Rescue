namespace ADRL.Drone.Core
{
    public readonly struct PoolPolicy
    {
        public int PrewarmCount { get; }

        public int MaxPoolSize { get; }

        public bool AllowGrowthBeyondMax { get; }

        public bool ReturnToPoolOnDestroy { get; }

        public PoolPolicy(
            int prewarmCount,
            int maxPoolSize,
            bool allowGrowthBeyondMax,
            bool returnToPoolOnDestroy)
        {
            PrewarmCount = prewarmCount;
            MaxPoolSize = maxPoolSize;
            AllowGrowthBeyondMax = allowGrowthBeyondMax;
            ReturnToPoolOnDestroy = returnToPoolOnDestroy;
        }

        public static PoolPolicy Default()
        {
            return new PoolPolicy(
                prewarmCount: 5,
                maxPoolSize: 20,
                allowGrowthBeyondMax: false,
                returnToPoolOnDestroy: true);
        }
    }
}
