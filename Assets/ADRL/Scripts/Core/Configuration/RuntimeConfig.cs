namespace ADRL.Core.Configuration
{
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Configuration/Runtime Config",
        fileName = "RuntimeConfig")]
    public class RuntimeConfig : ScriptableObject
    {
        [SerializeField]
        [Min(0)]
        private int _targetFrameRate = 60;

        [SerializeField]
        [Range(0f, 10f)]
        private float _timeScale = 1f;

        [SerializeField]
        [Range(0, 6)]
        private int _qualityLevel = 2;

        [SerializeField]
        private bool _vsyncEnabled;

        [SerializeField]
        [Min(0f)]
        private float _fixedTimestep = 0.02f;

        [SerializeField]
        [Min(0f)]
        private float _maxAllowedTimestep = 0.1f;

        [SerializeField]
        private Vector3 _gravity = new Vector3(0f, -9.81f, 0f);

        public int TargetFrameRate => _targetFrameRate;
        public float TimeScale => _timeScale;
        public int QualityLevel => _qualityLevel;
        public bool VsyncEnabled => _vsyncEnabled;
        public float FixedTimestep => _fixedTimestep;
        public float MaxAllowedTimestep => _maxAllowedTimestep;
        public Vector3 Gravity => _gravity;

        public void ApplyPhysicsSettings()
        {
            Time.fixedDeltaTime = _fixedTimestep;
            Time.maximumDeltaTime = _maxAllowedTimestep;
            Physics.gravity = _gravity;
        }
    }
}
