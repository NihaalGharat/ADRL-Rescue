namespace ADRL.Environment.Obstacles
{
    using ADRL.Environment.WorldObjects;
    using UnityEngine;

    public class Obstacle : WorldObjectBase
    {
        [SerializeField]
        private ObstacleType _obstacleType;

        private ObstacleState _state;

        public ObstacleType ObstacleType => _obstacleType;

        public ObstacleState State => _state;

        public void SetObstacleType(ObstacleType type)
        {
            _obstacleType = type;
        }

        protected override void OnInitialize()
        {
            SetCategory(WorldObjectCategory.Obstacle);
            _state = ObstacleState.Active;
        }

        protected override void OnReset()
        {
            _state = ObstacleState.Active;
        }

        protected override void OnCleanup()
        {
            _state = ObstacleState.Inactive;
        }

        public void MarkDestroyed()
        {
            _state = ObstacleState.Destroyed;
        }
    }
}
