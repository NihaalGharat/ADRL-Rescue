namespace ADRL.Drone.Core
{
    using ADRL.Core.Events;
    using ADRL.Drone.Events;
    using UnityEngine;

    public static class DroneBootstrap
    {
        private static DroneManager _droneManager;
        private static DroneContext _context;

        public static DroneManager DroneManager => _droneManager;
        public static DroneContext Context => _context;

        private static bool _initialized;

        public static void Boot(EventBus eventBus)
        {
            if (_initialized)
                return;

            var droneGo = new GameObject("[DroneSystem]");
            Object.DontDestroyOnLoad(droneGo);

            _droneManager = droneGo.AddComponent<DroneManager>();

            eventBus.Publish(new DroneInitializingEvent());

            _context = new DroneContext
            {
                FleetState = DroneSystemState.Initializing,
                EpisodeNumber = 0
            };

            var registry = new DroneRegistry();
            var configuration = new DroneConfiguration();

            _droneManager.Initialize(eventBus, _context, registry, configuration);

            _context.FleetState = DroneSystemState.Ready;

            _initialized = true;
        }

        public static void Shutdown(EventBus eventBus)
        {
            if (!_initialized)
                return;

            if (_droneManager != null)
            {
                _droneManager.Shutdown();
                Object.Destroy(_droneManager.gameObject);
            }

            _context?.Reset();
            _context = null;
            _droneManager = null;
            _initialized = false;

            eventBus?.Publish(new DroneShutdownEvent());
        }

        public static bool IsInitialized => _initialized;
    }
}
