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
        private bool _autoGenerate = true;

        public float TerrainWidth => _terrainWidth;
        public float TerrainLength => _terrainLength;
        public int HeightmapResolution => _heightmapResolution;
        public float HeightScale => _heightScale;
        public float NoiseScale => _noiseScale;
        public int SeedOffset => _seedOffset;
        public bool AutoGenerate => _autoGenerate;
    }
}
