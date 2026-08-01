namespace ADRL.Core.Configuration
{
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Configuration/Reward Config",
        fileName = "RewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        [SerializeField]
        private float _victimFoundReward = 10f;

        [SerializeField]
        private float _victimRescuedReward = 20f;

        [SerializeField]
        private float _collisionPenalty = -5f;

        [SerializeField]
        private float _timePenalty = -0.01f;

        [SerializeField]
        private float _successBonus = 50f;

        [SerializeField]
        private float _outOfBoundsPenalty = -10f;

        [SerializeField]
        private float _energyDepletedPenalty = -15f;

        [SerializeField]
        private float _noveltyBonus = 0.05f;

        [SerializeField]
        private float _noveltyCellSize = 2f;

        [SerializeField]
        private bool _shapingEnabled = true;

        [SerializeField]
        private float _shapingGamma = 0.99f;

        [SerializeField]
        private float _shapingScale = 0.1f;

        [SerializeField]
        private float _rewardScale = 1f;

        [SerializeField]
        private float _minStepReward = -0.1f;

        [SerializeField]
        private float _maxStepReward = 0.1f;

        [SerializeField]
        private float _stuckPenalty = -0.5f;

        [SerializeField]
        private float _stuckDetectionWindow = 2f;

        [SerializeField]
        private float _stuckDistanceThreshold = 0.1f;

        [SerializeField]
        private float _oscillationPenalty = -0.5f;

        [SerializeField]
        private float _oscillationDetectionWindow = 2f;

        [SerializeField]
        private float _oscillationThreshold = 3f;

        [SerializeField]
        private float _damagePenalty = -2f;

        public float VictimFoundReward => _victimFoundReward;
        public float VictimRescuedReward => _victimRescuedReward;
        public float CollisionPenalty => _collisionPenalty;
        public float TimePenalty => _timePenalty;
        public float SuccessBonus => _successBonus;
        public float OutOfBoundsPenalty => _outOfBoundsPenalty;
        public float EnergyDepletedPenalty => _energyDepletedPenalty;
        public float NoveltyBonus => _noveltyBonus;
        public float NoveltyCellSize => _noveltyCellSize;
        public bool ShapingEnabled => _shapingEnabled;
        public float ShapingGamma => _shapingGamma;
        public float ShapingScale => _shapingScale;
        public float RewardScale => _rewardScale;
        public float MinStepReward => _minStepReward;
        public float MaxStepReward => _maxStepReward;
        public float StuckPenalty => _stuckPenalty;
        public float StuckDetectionWindow => _stuckDetectionWindow;
        public float StuckDistanceThreshold => _stuckDistanceThreshold;
        public float OscillationPenalty => _oscillationPenalty;
        public float OscillationDetectionWindow => _oscillationDetectionWindow;
        public float OscillationThreshold => _oscillationThreshold;
        public float DamagePenalty => _damagePenalty;

#if UNITY_EDITOR
        private void OnValidate()
        {
            ClampPenalty(ref _collisionPenalty, "CollisionPenalty");
            ClampPenalty(ref _timePenalty, "TimePenalty");
            ClampPenalty(ref _outOfBoundsPenalty, "OutOfBoundsPenalty");
            ClampPenalty(ref _energyDepletedPenalty, "EnergyDepletedPenalty");
            ClampPenalty(ref _stuckPenalty, "StuckPenalty");
            ClampPenalty(ref _oscillationPenalty, "OscillationPenalty");
            ClampPenalty(ref _damagePenalty, "DamagePenalty");

            ClampBonus(ref _victimFoundReward, "VictimFoundReward");
            ClampBonus(ref _victimRescuedReward, "VictimRescuedReward");
            ClampBonus(ref _noveltyBonus, "NoveltyBonus");
            ClampBonus(ref _successBonus, "SuccessBonus");

            if (_noveltyCellSize <= 0f)
            {
                Debug.LogError(
                    $"[RewardConfig] NoveltyCellSize must be positive (was {_noveltyCellSize}); clamped to 0.01.");
                _noveltyCellSize = 0.01f;
            }

            if (_shapingGamma < 0f || _shapingGamma >= 1f)
            {
                Debug.LogError(
                    $"[RewardConfig] ShapingGamma must be in [0, 1) (was {_shapingGamma}); clamped to 0.99.");
                _shapingGamma = 0.99f;
            }

            if (_rewardScale <= 0f)
            {
                Debug.LogError(
                    $"[RewardConfig] RewardScale must be positive (was {_rewardScale}); clamped to 0.01.");
                _rewardScale = 0.01f;
            }

            if (_minStepReward > _maxStepReward)
            {
                Debug.LogError(
                    $"[RewardConfig] MinStepReward ({_minStepReward}) must not exceed MaxStepReward ({_maxStepReward}); " +
                    "MinStepReward clamped to MaxStepReward.");
                _minStepReward = _maxStepReward;
            }

            ClampWindow(ref _stuckDetectionWindow, "StuckDetectionWindow");
            ClampThreshold(ref _stuckDistanceThreshold, "StuckDistanceThreshold");
            ClampWindow(ref _oscillationDetectionWindow, "OscillationDetectionWindow");
            ClampThreshold(ref _oscillationThreshold, "OscillationThreshold");
        }

        private static void ClampPenalty(ref float value, string name)
        {
            if (value <= 0f)
                return;

            Debug.LogError($"[RewardConfig] {name} must be <= 0 (was {value}); clamped to 0.");
            value = 0f;
        }

        private static void ClampBonus(ref float value, string name)
        {
            if (value >= 0f)
                return;

            Debug.LogError($"[RewardConfig] {name} must be >= 0 (was {value}); clamped to 0.");
            value = 0f;
        }

        private static void ClampWindow(ref float value, string name)
        {
            if (value >= 1f)
                return;

            Debug.LogError($"[RewardConfig] {name} must be >= 1 (was {value}); clamped to 1.");
            value = 1f;
        }

        private static void ClampThreshold(ref float value, string name)
        {
            if (value >= 0f)
                return;

            Debug.LogError($"[RewardConfig] {name} must be >= 0 (was {value}); clamped to 0.");
            value = 0f;
        }
#endif
    }
}
