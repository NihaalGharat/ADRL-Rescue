namespace ADRL.Core.Configuration
{
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Configuration/Simulation Config",
        fileName = "SimulationConfig")]
    public class SimulationConfig : ScriptableObject
    {
        [SerializeField]
        [Min(1)]
        private int _maxEpisodeLength = 3000;

        [SerializeField]
        [Min(1)]
        private int _agentCount = 1;

        [SerializeField]
        [Min(0f)]
        private float _episodeResetDelay = 1f;

        [SerializeField]
        private bool _autoStartSimulation = true;

        [SerializeField]
        private bool _enableRandomSeed;

        [SerializeField]
        private int _randomSeed;

        public int MaxEpisodeLength => _maxEpisodeLength;
        public int AgentCount => _agentCount;
        public float EpisodeResetDelay => _episodeResetDelay;
        public bool AutoStartSimulation => _autoStartSimulation;
        public bool EnableRandomSeed => _enableRandomSeed;
        public int RandomSeed => _randomSeed;
    }
}
