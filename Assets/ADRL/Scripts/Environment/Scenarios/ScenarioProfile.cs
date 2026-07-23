namespace ADRL.Environment.Scenarios
{
    using ADRL.Core.Configuration;
    using ADRL.Environment.Procedural;
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Environment/Scenario Profile",
        fileName = "ScenarioProfile")]
    public class ScenarioProfile : ScriptableObject
    {
        [Header("Metadata")]
        [SerializeField]
        private string _scenarioName = "New Scenario";

        [SerializeField]
        private ScenarioType _scenarioType = ScenarioType.Custom;

        [SerializeField]
        private ScenarioDifficulty _difficulty = ScenarioDifficulty.Medium;

        [SerializeField]
        [TextArea(3, 10)]
        private string _description;

        [SerializeField]
        private string _author;

        [SerializeField]
        private int _version = 1;

        [Header("Configuration")]
        [SerializeField]
        private EnvironmentConfig _environmentConfig;

        [SerializeField]
        private GenerationSettings _generationSettings;

        [Header("Generation")]
        [SerializeField]
        private int _seed;

        [SerializeField]
        private bool _overrideSeed;

        [SerializeField]
        private bool _enableProceduralGeneration = true;

        [Header("Overrides")]
        [SerializeField]
        [Min(0)]
        private int _victimCount;

        [SerializeField]
        [Min(0f)]
        private float _hazardDensity;

        [SerializeField]
        [Min(0)]
        private int _obstacleCount;

        [Header("Mission")]
        [SerializeField]
        [Min(0f)]
        private float _missionDuration = 300f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _successThreshold = 0.8f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _failureThreshold = 0.3f;

        [Header("Reserved — Future Use")]
        [SerializeField]
        private string _weather;

        [SerializeField]
        private string _lighting;

        [SerializeField]
        [Range(0f, 24f)]
        private float _timeOfDay = 12f;

        [SerializeField]
        private string _wind;

        [SerializeField]
        private bool _difficultyScaling;

        public string ScenarioName => _scenarioName;

        public ScenarioType ScenarioType => _scenarioType;

        public ScenarioDifficulty Difficulty => _difficulty;

        public string Description => _description;

        public string Author => _author;

        public int ProfileVersion => _version;

        public EnvironmentConfig EnvironmentConfig => _environmentConfig;

        public GenerationSettings GenerationSettings => _generationSettings;

        public int Seed => _seed;

        public bool OverrideSeed => _overrideSeed;

        public bool EnableProceduralGeneration => _enableProceduralGeneration;

        public int VictimCount => _victimCount;

        public float HazardDensity => _hazardDensity;

        public int ObstacleCount => _obstacleCount;

        public float MissionDuration => _missionDuration;

        public float SuccessThreshold => _successThreshold;

        public float FailureThreshold => _failureThreshold;

        public string Weather => _weather;

        public string Lighting => _lighting;

        public float TimeOfDay => _timeOfDay;

        public string Wind => _wind;

        public bool DifficultyScaling => _difficultyScaling;

        public bool HasVictimOverride => _victimCount > 0;

        public bool HasHazardDensityOverride => _hazardDensity > 0f;

        public bool HasObstacleCountOverride => _obstacleCount > 0;

        public int GetEffectiveSeed()
        {
            if (_overrideSeed)
                return _seed;

            return _generationSettings != null
                ? _generationSettings.GetEffectiveSeed()
                : System.Environment.TickCount;
        }

        public int GetVictimCount()
        {
            if (HasVictimOverride)
                return _victimCount;

            return _environmentConfig != null ? _environmentConfig.MaxVictims : 5;
        }

        public float GetHazardDensity()
        {
            if (HasHazardDensityOverride)
                return _hazardDensity;

            return _environmentConfig != null ? _environmentConfig.HazardDensity : 0.1f;
        }

        public int GetObstacleCount()
        {
            if (HasObstacleCountOverride)
                return _obstacleCount;

            return _environmentConfig != null ? _environmentConfig.MaxObstacles : 10;
        }
    }
}