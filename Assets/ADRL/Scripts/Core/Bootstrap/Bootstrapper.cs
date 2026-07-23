namespace ADRL.Core.Bootstrap
{
    using ADRL.Core.Configuration;
    using UnityEngine;

    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField]
        private ProjectConfig _projectConfig;

        [SerializeField]
        private RuntimeConfig _runtimeConfig;

        [SerializeField]
        private SimulationConfig _simulationConfig;

        [SerializeField]
        private DroneConfig _droneConfig;

        [SerializeField]
        private EnvironmentConfig _environmentConfig;

        [SerializeField]
        private SensorConfig _sensorConfig;

        [SerializeField]
        private RewardConfig _rewardConfig;

        [SerializeField]
        private TrainingConfig _trainingConfig;

        [SerializeField]
        private bool _initializeOnAwake = true;

        [SerializeField]
        private bool _dontDestroyOnLoad = true;

        private void Awake()
        {
            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            if (_initializeOnAwake)
                Bootstrap();
        }

        public void Bootstrap()
        {
            if (_projectConfig == null)
            {
                Debug.LogError("[Bootstrapper] ProjectConfig is not assigned. Bootstrap aborted.");
                return;
            }

            if (_runtimeConfig == null)
            {
                Debug.LogError("[Bootstrapper] RuntimeConfig is not assigned. Bootstrap aborted.");
                return;
            }

            if (_simulationConfig == null)
            {
                Debug.LogError("[Bootstrapper] SimulationConfig is not assigned. Bootstrap aborted.");
                return;
            }

            GameBootstrap.Initialize(
                _projectConfig,
                _runtimeConfig,
                _simulationConfig,
                _droneConfig,
                _environmentConfig,
                _sensorConfig,
                _rewardConfig,
                _trainingConfig);
        }
    }
}
