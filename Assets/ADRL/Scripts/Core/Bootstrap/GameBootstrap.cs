namespace ADRL.Core.Bootstrap
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Core.Resources;
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
            SimulationConfig simulationConfig,
            DroneConfig droneConfig = null,
            EnvironmentConfig environmentConfig = null,
            SensorConfig sensorConfig = null,
            RewardConfig rewardConfig = null,
            TrainingConfig trainingConfig = null)
        {
            if (_initialized)
                return;

            ProjectConfig = projectConfig;
            RuntimeConfig = runtimeConfig;
            SimulationConfig = simulationConfig;

            EventBus = new EventBus();
            Services = new ServiceLocator();

            ResourceLocator.Initialize();
            RegisterAllConfigs(droneConfig, environmentConfig, sensorConfig, rewardConfig, trainingConfig);

            ApplyRuntimeSettings();

            var validationResult = AssetValidation.ValidateAll(ResourceLocator.Configs, ResourceLocator.Prefabs);
            AssetValidation.LogResults(validationResult);

            Simulation = SimulationManager.CreateInstance(simulationConfig);
            Simulation.Initialize(EventBus);

            Services.Register<EventBus>(EventBus);

            EventBus.Publish(new BootstrapCompletedEvent());

            _initialized = true;
        }

        private static void RegisterAllConfigs(
            DroneConfig droneConfig,
            EnvironmentConfig environmentConfig,
            SensorConfig sensorConfig,
            RewardConfig rewardConfig,
            TrainingConfig trainingConfig)
        {
            var registry = ResourceLocator.Configs;

            registry.Register(ProjectConfig);
            registry.Register(RuntimeConfig);
            registry.Register(SimulationConfig);

            if (droneConfig != null)
                registry.Register(droneConfig);

            if (environmentConfig != null)
                registry.Register(environmentConfig);

            if (sensorConfig != null)
                registry.Register(sensorConfig);

            if (rewardConfig != null)
                registry.Register(rewardConfig);

            if (trainingConfig != null)
                registry.Register(trainingConfig);
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
