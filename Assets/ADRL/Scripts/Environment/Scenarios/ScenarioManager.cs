namespace ADRL.Environment.Scenarios
{
    using ADRL.Core.Events;
    using ADRL.Core.Resources;
    using ADRL.Environment.Core;
    using ADRL.Environment.Events;
    using UnityEngine;

    public class ScenarioManager : MonoBehaviour
    {
        private ScenarioProfile _currentProfile;
        private EnvironmentManager _environmentManager;
        private EventBus _eventBus;
        private ScenarioState _state = ScenarioState.Unloaded;
        private float _elapsedTime;
        private int _currentScenarioId;
        private int _nextScenarioId = 1;

        public ScenarioProfile CurrentProfile => _currentProfile;

        public ScenarioState State => _state;

        public float ElapsedTime => _elapsedTime;

        public bool IsInitialized =>
            _environmentManager != null && _eventBus != null;

        public void Initialize(EnvironmentManager environmentManager, EventBus eventBus)
        {
            _environmentManager = environmentManager;
            _eventBus = eventBus;
            _state = ScenarioState.Unloaded;
        }

        public void LoadScenario(ScenarioProfile profile)
        {
            if (profile == null)
            {
                Debug.LogError("[ScenarioManager] Cannot load null ScenarioProfile.");
                return;
            }

            if (!ScenarioValidator.ValidateProfile(profile, out var message))
            {
                Debug.LogError($"[ScenarioManager] ScenarioProfile validation failed: {message}");
                return;
            }

            _currentProfile = profile;
            _currentScenarioId = _nextScenarioId++;

            RegisterScenarioConfigs(profile);

            _state = ScenarioState.Loaded;
            _eventBus?.Publish(new ScenarioLoadedEvent(
                _currentScenarioId,
                profile.ScenarioName,
                profile.ScenarioType));
        }

        public void UnloadScenario()
        {
            if (_currentProfile == null)
                return;

            _state = ScenarioState.Unloading;

            _currentProfile = null;
            _currentScenarioId = 0;
            _elapsedTime = 0f;
            _state = ScenarioState.Unloaded;
        }

        public void StartScenario()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[ScenarioManager] Not initialized. Call Initialize() first.");
                return;
            }

            if (_state != ScenarioState.Loaded)
            {
                Debug.LogWarning($"[ScenarioManager] Cannot start scenario in state {_state}. Expected Loaded.");
                return;
            }

            if (_currentProfile == null)
            {
                Debug.LogError("[ScenarioManager] No scenario profile loaded.");
                return;
            }

            if (!ScenarioValidator.ValidateAll(_currentProfile, out var message))
            {
                Debug.LogError($"[ScenarioManager] Scenario validation failed: {message}");
                return;
            }

            _state = ScenarioState.Starting;

            if (_currentProfile.EnableProceduralGeneration)
            {
                _environmentManager.Initialize(_eventBus);

                if (_environmentManager.State == EnvironmentState.Ready)
                    _environmentManager.StartEnvironment();
            }

            _elapsedTime = 0f;
            _state = ScenarioState.Running;
            _eventBus?.Publish(new ScenarioStartedEvent(_currentScenarioId));
        }

        public void ResetScenario()
        {
            if (_currentProfile == null)
            {
                Debug.LogWarning("[ScenarioManager] No scenario profile loaded. Cannot reset.");
                return;
            }

            _state = ScenarioState.Resetting;
            _elapsedTime = 0f;

            _environmentManager?.ResetEnvironment();

            _state = ScenarioState.Loaded;
            _eventBus?.Publish(new ScenarioResetEvent(_currentScenarioId));
        }

        public void RestartScenario()
        {
            ResetScenario();

            if (_state == ScenarioState.Loaded)
                StartScenario();
        }

        public void CompleteScenario()
        {
            if (_state != ScenarioState.Running)
                return;

            _state = ScenarioState.Completed;
            _environmentManager?.CompleteEnvironment();
            _eventBus?.Publish(new ScenarioCompletedEvent(
                _currentScenarioId,
                _elapsedTime));
        }

        public void FailScenario(string reason)
        {
            if (_state != ScenarioState.Running)
                return;

            _state = ScenarioState.Failed;
            _environmentManager?.FailEnvironment();
            _eventBus?.Publish(new ScenarioFailedEvent(
                _currentScenarioId,
                reason));
        }

        public bool ValidateScenario(out string message)
        {
            if (_currentProfile == null)
            {
                message = "No scenario profile loaded.";
                return false;
            }

            return ScenarioValidator.ValidateAll(_currentProfile, out message);
        }

        private void RegisterScenarioConfigs(ScenarioProfile profile)
        {
            if (profile.EnvironmentConfig != null)
            {
                if (ResourceLocator.IsInitialized)
                    ResourceLocator.Configs.Register(profile.EnvironmentConfig);
            }

            if (profile.GenerationSettings != null)
            {
                if (ResourceLocator.IsInitialized)
                    ResourceLocator.Configs.Register(profile.GenerationSettings);
            }
        }

        private void Update()
        {
            if (_state != ScenarioState.Running)
                return;

            _elapsedTime += Time.deltaTime;

            if (_currentProfile != null && _elapsedTime >= _currentProfile.MissionDuration)
            {
                FailScenario("Mission time expired.");
            }
        }
    }
}
