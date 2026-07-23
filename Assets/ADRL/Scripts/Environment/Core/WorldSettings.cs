namespace ADRL.Environment.Core
{
    using UnityEngine;

    public enum SeedMode
    {
        Fixed,
        Random,
        TimeBased
    }

    [CreateAssetMenu(
        menuName = "ADRL/Environment/World Settings",
        fileName = "WorldSettings")]
    public class WorldSettings : ScriptableObject
    {
        [SerializeField]
        [Min(10f)]
        private float _worldSize = 200f;

        [SerializeField]
        [Min(10f)]
        private float _playArea = 180f;

        [SerializeField]
        private SeedMode _seedMode = SeedMode.Random;

        [SerializeField]
        private int _fixedSeed;

        [SerializeField]
        private bool _overrideGravity;

        [SerializeField]
        private Vector3 _gravityOverride = new Vector3(0f, -9.81f, 0f);

        [SerializeField]
        private bool _enableDebugLogs;

        [SerializeField]
        private bool _showGizmos = true;

        [SerializeField]
        private bool _skipEnvironmentValidation;

        public float WorldSize => _worldSize;
        public float PlayArea => _playArea;
        public SeedMode SeedMode => _seedMode;
        public int FixedSeed => _fixedSeed;
        public bool OverrideGravity => _overrideGravity;
        public Vector3 GravityOverride => _gravityOverride;
        public bool EnableDebugLogs => _enableDebugLogs;
        public bool ShowGizmos => _showGizmos;
        public bool SkipEnvironmentValidation => _skipEnvironmentValidation;

        public void ApplyGravityOverride()
        {
            if (_overrideGravity)
                Physics.gravity = _gravityOverride;
        }
    }
}
