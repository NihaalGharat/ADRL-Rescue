namespace ADRL.Environment.Core
{
    using System.Collections.Generic;
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Core.Resources;
    using ADRL.Environment.Events;
    using ADRL.Environment.Hazards;
    using ADRL.Environment.Spawning;
    using ADRL.Environment.Victims;
    using ADRL.Environment.Obstacles;
    using ADRL.Environment.WorldObjects;
    using UnityEngine;

    public class EnvironmentManager : MonoBehaviour
    {
        [SerializeField]
        private SpawnManager _spawnManager;

        private HazardManager _hazardManager;
        private WorldObjectRegistry _worldObjectRegistry;
        private ObstacleManager _obstacleManager;
        private EventBus _eventBus;
        private EnvironmentConfig _config;
        private EnvironmentState _state = EnvironmentState.Uninitialized;
        private readonly List<Victim> _victims = new();
        private int _nextVictimId = 1;
        private int _nextHazardId = 1;
        private int _nextObstacleId = 1;

        public EnvironmentState State => _state;

        public IReadOnlyList<Victim> Victims => _victims;

        public HazardManager HazardManager => _hazardManager;

        public SpawnManager SpawnManager => _spawnManager;

        public ObstacleManager ObstacleManager => _obstacleManager;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _config = ResourceLocator.Configs.Get<EnvironmentConfig>();
            _state = EnvironmentState.Initializing;

            if (_spawnManager == null)
                _spawnManager = FindAnyObjectByType<SpawnManager>();

            if (_hazardManager == null)
                _hazardManager = new HazardManager();

            InitializeWorldObjectRegistry();
            InitializeObstacleManager();
            RegisterExistingObjects();
            _state = EnvironmentState.Ready;
            _eventBus.Publish(new EnvironmentInitializedEvent());
        }

        public int RegisterVictim(Victim victim)
        {
            if (victim == null || _victims.Contains(victim))
                return -1;

            var victimId = _nextVictimId++;
            victim.SetId(victimId);
            victim.Initialize();
            _victims.Add(victim);
            _eventBus.Publish(new VictimRegisteredEvent(victimId));
            return victimId;
        }

        public int RegisterHazard(Hazard hazard)
        {
            if (hazard == null)
                return -1;

            var hazardId = _nextHazardId++;
            hazard.SetId(hazardId);
            _hazardManager.RegisterHazard(hazard);
            _eventBus.Publish(new HazardRegisteredEvent(hazardId));
            return hazardId;
        }

        public void StartEnvironment()
        {
            if (_state != EnvironmentState.Ready)
                return;

            _state = EnvironmentState.Running;
        }

        public void ResetEnvironment()
        {
            _state = EnvironmentState.Resetting;

            for (var i = 0; i < _victims.Count; i++)
                _victims[i].Reset();

            _hazardManager.ResetAll();
            _obstacleManager?.ResetAll();
            _worldObjectRegistry?.ResetAll();

            _nextVictimId = 1;
            _nextHazardId = 1;
            _nextObstacleId = 1;
            _state = EnvironmentState.Ready;
            _eventBus.Publish(new EnvironmentResetEvent());
        }

        public void CompleteEnvironment()
        {
            if (_state != EnvironmentState.Running)
                return;

            _state = EnvironmentState.Completed;
        }

        public void FailEnvironment()
        {
            _state = EnvironmentState.Failed;
        }

        private void InitializeWorldObjectRegistry()
        {
            _worldObjectRegistry = new WorldObjectRegistry();
            _worldObjectRegistry.Initialize();
        }

        private void CleanupWorldObjectRegistry()
        {
            _worldObjectRegistry?.Clear();
            _worldObjectRegistry = null;
        }

        private void InitializeObstacleManager()
        {
            _obstacleManager = new ObstacleManager();
            _obstacleManager.Initialize(_worldObjectRegistry);
        }

        private void CleanupObstacleManager()
        {
            _obstacleManager?.Clear();
            _obstacleManager = null;
        }

        private void OnDestroy()
        {
            CleanupObstacleManager();
            CleanupWorldObjectRegistry();
        }

        private void RegisterExistingObjects()
        {
            var existingVictims = FindObjectsByType<Victim>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (var i = 0; i < existingVictims.Length; i++)
            {
                if (!_victims.Contains(existingVictims[i]))
                    RegisterVictim(existingVictims[i]);
            }

            var existingHazards = FindObjectsByType<Hazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (var i = 0; i < existingHazards.Length; i++)
            {
                RegisterHazard(existingHazards[i]);
            }

            var spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (var i = 0; i < spawnPoints.Length; i++)
            {
                _spawnManager?.RegisterSpawnPoint(spawnPoints[i]);
            }

            var existingObstacles = FindObjectsByType<Obstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (var i = 0; i < existingObstacles.Length; i++)
            {
                RegisterObstacle(existingObstacles[i]);
            }
        }

        private int RegisterObstacle(Obstacle obstacle)
        {
            if (obstacle == null)
                return -1;

            var obstacleId = _nextObstacleId++;
            obstacle.SetId(obstacleId);
            _obstacleManager?.RegisterObstacle(obstacle);
            _eventBus.Publish(new ObstacleRegisteredEvent(obstacleId));
            return obstacleId;
        }
    }
}
