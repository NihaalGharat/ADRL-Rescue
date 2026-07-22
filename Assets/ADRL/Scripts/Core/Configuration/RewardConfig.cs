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
        private float _explorationBonus = 0.1f;

        [SerializeField]
        private float _successBonus = 50f;

        [SerializeField]
        private float _outOfBoundsPenalty = -10f;

        [SerializeField]
        private float _energyDepletedPenalty = -15f;

        public float VictimFoundReward => _victimFoundReward;
        public float VictimRescuedReward => _victimRescuedReward;
        public float CollisionPenalty => _collisionPenalty;
        public float TimePenalty => _timePenalty;
        public float ExplorationBonus => _explorationBonus;
        public float SuccessBonus => _successBonus;
        public float OutOfBoundsPenalty => _outOfBoundsPenalty;
        public float EnergyDepletedPenalty => _energyDepletedPenalty;
    }
}
