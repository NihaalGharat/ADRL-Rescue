namespace ADRL.Core.Bootstrap
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Core.Services;
    using ADRL.Core.Simulation;

    public static class GameBootstrap
    {
        public static ServiceLocator Services { get; private set; }
        public static EventBus EventBus { get; private set; }
        public static SimulationManager Simulation { get; private set; }

        public static ProjectConfig ProjectConfig { get; private set; }
        public static RuntimeConfig RuntimeConfig { get; private set; }
        public static SimulationConfig SimulationConfig { get; private set; }

        private static bool _initialized;

        public static void Initialize(
            ProjectConfig projectConfig,
            RuntimeConfig runtimeConfig,
            SimulationConfig simulationConfig)
        {
            if (_initialized)
                return;

            ProjectConfig = projectConfig;
            RuntimeConfig = runtimeConfig;
            SimulationConfig = simulationConfig;

            EventBus = new EventBus();
            Services = new ServiceLocator();

            ApplyRuntimeSettings();

            Simulation = SimulationManager.CreateInstance(simulationConfig);
            Simulation.Initialize(EventBus);

            Services.Register<EventBus>(EventBus);

            EventBus.Publish(new BootstrapCompletedEvent());

            _initialized = true;
        }

        private static void ApplyRuntimeSettings()
        {
            if (RuntimeConfig == null)
                return;

            if (RuntimeConfig.TargetFrameRate > 0)
                UnityEngine.Application.targetFrameRate = RuntimeConfig.TargetFrameRate;

            UnityEngine.QualitySettings.SetQualityLevel(RuntimeConfig.QualityLevel, false);

            UnityEngine.Time.timeScale = RuntimeConfig.TimeScale;
        }
    }
}
