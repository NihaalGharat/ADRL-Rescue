namespace ADRL.Environment.Core
{
    using System;

    public static class SeedManager
    {
        private static int _currentSeed;

        public static int CurrentSeed => _currentSeed;

        public static int GenerateSeed()
        {
            _currentSeed = Environment.TickCount;
            return _currentSeed;
        }

        public static int GenerateSeed(int fixedSeed)
        {
            _currentSeed = fixedSeed;
            return _currentSeed;
        }

        public static int GenerateSeed(WorldSettings worldSettings)
        {
            if (worldSettings == null)
                return GenerateSeed();

            _currentSeed = worldSettings.SeedMode switch
            {
                SeedMode.Fixed => worldSettings.FixedSeed,
                SeedMode.TimeBased => (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                _ => Environment.TickCount
            };

            return _currentSeed;
        }

        public static void SetSeed(int seed)
        {
            _currentSeed = seed;
        }

        public static void Reset()
        {
            _currentSeed = 0;
        }
    }
}
