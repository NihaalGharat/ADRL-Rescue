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
            GameBootstrap.Initialize(_projectConfig, _runtimeConfig, _simulationConfig);
        }
    }
}
