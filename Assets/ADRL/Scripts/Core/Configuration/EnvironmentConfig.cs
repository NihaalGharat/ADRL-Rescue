namespace ADRL.Core.Configuration
{
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Configuration/Environment Config",
        fileName = "EnvironmentConfig")]
    public class EnvironmentConfig : ScriptableObject
    {
        [SerializeField]
        [Min(1f)]
        private float _terrainSize = 200f;

        [SerializeField]
        [Min(1)]
        private int _minObstacles = 5;

        [SerializeField]
        [Min(1)]
        private int _maxObstacles = 20;

        [SerializeField]
        [Min(1)]
        private int _minVictims = 1;

        [SerializeField]
        [Min(1)]
        private int _maxVictims = 10;

        [SerializeField]
        [Range(0f, 1f)]
        private float _hazardDensity = 0.1f;

        [SerializeField]
        private bool _enableProceduralGeneration = true;

        public float TerrainSize => _terrainSize;
        public int MinObstacles => _minObstacles;
        public int MaxObstacles => _maxObstacles;
        public int MinVictims => _minVictims;
        public int MaxVictims => _maxVictims;
        public float HazardDensity => _hazardDensity;
        public bool EnableProceduralGeneration => _enableProceduralGeneration;
    }
}
