namespace ADRL.Environment.Terrain
{
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Environment/Terrain Settings",
        fileName = "TerrainSettings")]
    public class TerrainSettings : ScriptableObject
    {
        [SerializeField]
        [Min(10f)]
        private float _terrainWidth = 200f;

        [SerializeField]
        [Min(10f)]
        private float _terrainLength = 200f;

        [SerializeField]
        [Min(3)]
        private int _heightmapResolution = 513;

        [SerializeField]
        [Min(0f)]
        private float _heightScale = 50f;

        [SerializeField]
        [Min(0.001f)]
        private float _noiseScale = 0.02f;

        [SerializeField]
        [Min(0)]
        private int _seedOffset;

        [SerializeField]
        [Min(1)]
        private int _octaves = 4;

        [SerializeField]
        [Min(0.1f)]
        private float _persistence = 0.5f;

        [SerializeField]
        [Min(1f)]
        private float _lacunarity = 2f;

        [SerializeField]
        [Min(0.1f)]
        private float _heightMultiplier = 1f;

        [SerializeField]
        [Min(0f)]
        private float _warpStrength = 4f;

        [SerializeField]
        [Min(0.001f)]
        private float _warpScale = 0.02f;

        [SerializeField]
        [Min(0.1f)]
        private float _voronoiCellSize = 8f;

        [SerializeField]
        [Min(0f)]
        private float _fbmWeight = 0.40f;

        [SerializeField]
        [Min(0f)]
        private float _ridgedWeight = 0.25f;

        [SerializeField]
        [Min(0f)]
        private float _domainWarpWeight = 0.20f;

        [SerializeField]
        [Min(0f)]
        private float _voronoiWeight = 0.15f;

        [SerializeField]
        private TerrainAlgorithm _terrainAlgorithm = TerrainAlgorithm.FBM;

        [SerializeField]
        private bool _autoGenerate = true;

        public float TerrainWidth => _terrainWidth;
        public float TerrainLength => _terrainLength;
        public int HeightmapResolution => _heightmapResolution;
        public float HeightScale => _heightScale;
        public float NoiseScale => _noiseScale;
        public int SeedOffset => _seedOffset;
        public int Octaves => _octaves;
        public float Persistence => _persistence;
        public float Lacunarity => _lacunarity;
        public float HeightMultiplier => _heightMultiplier;
        public float WarpStrength => _warpStrength;
        public float WarpScale => _warpScale;
        public float VoronoiCellSize => _voronoiCellSize;
        public float FbmWeight => _fbmWeight;
        public float RidgedWeight => _ridgedWeight;
        public float DomainWarpWeight => _domainWarpWeight;
        public float VoronoiWeight => _voronoiWeight;
        public TerrainAlgorithm TerrainAlgorithm => _terrainAlgorithm;
        public bool UseFractalNoise => _terrainAlgorithm == TerrainAlgorithm.FBM;
        public bool AutoGenerate => _autoGenerate;
    }
}
