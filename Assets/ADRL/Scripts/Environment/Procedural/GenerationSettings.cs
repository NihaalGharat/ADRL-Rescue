namespace ADRL.Environment.Procedural
{
    using ADRL.Core.Configuration;
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Environment/Generation Settings",
        fileName = "GenerationSettings")]
    public class GenerationSettings : ScriptableObject
    {
        [SerializeField]
        private EnvironmentConfig _environmentConfig;

        [SerializeField]
        private bool _useFixedSeed;

        [SerializeField]
        private int _fixedSeed;

        [SerializeField]
        private Vector3 _boundsCenter = Vector3.zero;

        [SerializeField]
        private Vector3 _boundsSize = new Vector3(200f, 10f, 200f);

        [SerializeField]
        private bool _useEnvironmentConfigBounds = true;

        [SerializeField]
        [Min(0f)]
        private float _minSpacing = 2f;

        [SerializeField]
        [Min(1)]
        private int _maxPlacementAttempts = 50;

        public EnvironmentConfig EnvironmentConfig => _environmentConfig;

        public bool UseFixedSeed => _useFixedSeed;

        public int FixedSeed => _fixedSeed;

        public Vector3 BoundsCenter => _boundsCenter;

        public Vector3 BoundsSize => _boundsSize;

        public bool UseEnvironmentConfigBounds => _useEnvironmentConfigBounds;

        public float MinSpacing => _minSpacing;

        public int MaxPlacementAttempts => _maxPlacementAttempts;

        public int GetEffectiveSeed()
        {
            if (_useFixedSeed)
                return _fixedSeed;

            return System.Environment.TickCount;
        }

        public Bounds GetGenerationBounds()
        {
            if (_useEnvironmentConfigBounds && _environmentConfig != null)
            {
                var size = _environmentConfig.TerrainSize;
                return new Bounds(Vector3.zero, new Vector3(size, 10f, size));
            }

            return new Bounds(_boundsCenter, _boundsSize);
        }
    }
}
