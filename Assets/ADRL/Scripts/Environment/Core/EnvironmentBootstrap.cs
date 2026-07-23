namespace ADRL.Environment.Core
{
    using ADRL.Core.Events;
    using ADRL.Environment.Events;
    using UnityEngine;

    public static class EnvironmentBootstrap
    {
        private static EnvironmentManager _environmentManager;
        private static EnvironmentContext _context;

        public static EnvironmentManager EnvironmentManager => _environmentManager;
        public static EnvironmentContext Context => _context;

        private static bool _initialized;

        public static void Boot(
            WorldSettings worldSettings,
            EventBus eventBus)
        {
            if (_initialized)
                return;

            var environmentGo = new GameObject("[EnvironmentSystem]");
            Object.DontDestroyOnLoad(environmentGo);

            _environmentManager = environmentGo.AddComponent<EnvironmentManager>();

            eventBus.Publish(new EnvironmentInitializingEvent());

            _context = new EnvironmentContext
            {
                WorldSettings = worldSettings,
                ActiveSeed = worldSettings != null ? SeedManager.GenerateSeed(worldSettings) : SeedManager.GenerateSeed(),
                RuntimeState = EnvironmentState.Uninitialized
            };

            if (worldSettings != null && worldSettings.EnableDebugLogs)
                Debug.Log($"[EnvironmentBootstrap] Boot complete\nEpisode: {_context.CurrentEpisode}\nSeed: {_context.ActiveSeed}\nSeed Mode: {worldSettings.SeedMode}");

            _environmentManager.Initialize(eventBus);

            if (_environmentManager.TerrainGenerator != null && _environmentManager.TerrainGenerator.IsGenerated)
            {
                _context.GeneratedTerrain = _environmentManager.TerrainGenerator.TerrainData;
                _context.TerrainSize = new Vector2(
                    _environmentManager.TerrainGenerator.Settings.TerrainWidth,
                    _environmentManager.TerrainGenerator.Settings.TerrainLength);
            }

            _context.RuntimeState = EnvironmentState.Ready;

            _initialized = true;
        }

        public static void Shutdown(EventBus eventBus)
        {
            if (!_initialized)
                return;

            if (_environmentManager != null)
                Object.Destroy(_environmentManager.gameObject);

            _context?.Reset();
            _context = null;
            SeedManager.Reset();
            _environmentManager = null;
            _initialized = false;

            eventBus?.Publish(new EnvironmentShutdownEvent());
        }

        public static bool IsInitialized => _initialized;
    }
}
