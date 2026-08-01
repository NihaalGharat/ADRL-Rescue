namespace ADRL.Core.Simulation
{
    using ADRL.Core.Bootstrap;
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using UnityEngine;

    public class SimulationManager : MonoBehaviour
    {
        private SimulationState _currentState = SimulationState.Uninitialized;
        private SimulationConfig _config;
        private EventBus _eventBus;
        private float _elapsedTime;
        private int _currentEpisode;
        private bool _episodeFinalized;

        public SimulationState CurrentState => _currentState;

        public float ElapsedTime => _elapsedTime;
        public int CurrentEpisode => _currentEpisode;
        public int MaxEpisodeLength => _config != null ? _config.MaxEpisodeLength : 3000;

        public event System.Action<SimulationState, SimulationState> StateChanged;

        public static SimulationManager CreateInstance(SimulationConfig config)
        {
            var gameObject = new GameObject("[SimulationManager]");
            var manager = gameObject.AddComponent<SimulationManager>();
            manager._config = config;
            return manager;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_currentState != SimulationState.Running)
                return;

            _elapsedTime += Time.deltaTime;
        }

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<AgentEpisodeEndedEvent>(OnAgentEpisodeEnded);
            SetState(SimulationState.Initializing);
            SetState(SimulationState.Ready);

            if (_config != null && _config.AutoStartSimulation)
            {
                StartSimulation();
            }
        }

        public void StartSimulation()
        {
            if (_currentState != SimulationState.Ready &&
                _currentState != SimulationState.Completed)
                return;

            _currentEpisode++;
            _elapsedTime = 0f;
            _episodeFinalized = false;
            SetState(SimulationState.Running);
            _eventBus?.Publish(new SimulationStartedEvent());
            _eventBus?.Publish(new EpisodeStartedEvent(_currentEpisode));
        }

        public void PauseSimulation()
        {
            if (_currentState != SimulationState.Running)
                return;

            SetState(SimulationState.Paused);
            _eventBus?.Publish(new SimulationPausedEvent());
            Time.timeScale = 0f;
        }

        public void ResumeSimulation()
        {
            if (_currentState != SimulationState.Paused)
                return;

            SetState(SimulationState.Running);
            _eventBus?.Publish(new SimulationResumedEvent());

            if (GameBootstrap.RuntimeConfig != null)
            {
                Time.timeScale = GameBootstrap.RuntimeConfig.TimeScale;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        public void StopSimulation()
        {
            if (_currentState != SimulationState.Running &&
                _currentState != SimulationState.Paused)
                return;

            SetState(SimulationState.Completed);
            _eventBus?.Publish(new SimulationStoppedEvent());
        }

        public void ResetSimulation()
        {
            _elapsedTime = 0f;
            _episodeFinalized = false;
            SetState(SimulationState.Ready);
            _eventBus?.Publish(new SimulationResetEvent());
        }

        /// <summary>
        /// Finalizes the current simulation episode with the reward data reported
        /// by the drone agent, so the completed episode carries real totals. Events
        /// reported outside a running episode (e.g. stale or post-completion) are
        /// ignored, and only the first report per episode is honoured.
        /// </summary>
        private void OnAgentEpisodeEnded(AgentEpisodeEndedEvent evt)
        {
            if (_currentState != SimulationState.Running || _episodeFinalized)
                return;

            _episodeFinalized = true;
            _eventBus?.Publish(new EpisodeCompletedEvent(
                _currentEpisode, evt.TotalReward, evt.StepsCompleted));
            SetState(SimulationState.Completed);
            _eventBus?.Publish(new SimulationStoppedEvent());
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<AgentEpisodeEndedEvent>(OnAgentEpisodeEnded);
        }

        private void SetState(SimulationState newState)
        {
            var previous = _currentState;
            _currentState = newState;
            StateChanged?.Invoke(previous, newState);
        }
    }
}
