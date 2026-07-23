namespace ADRL.Environment.Obstacles
{
    using System.Collections.Generic;
    using ADRL.Environment.WorldObjects;

    public class ObstacleManager
    {
        private WorldObjectRegistry _registry;
        private List<Obstacle> _obstacles;

        public IReadOnlyList<Obstacle> All => _obstacles;

        public void Initialize(WorldObjectRegistry registry)
        {
            _registry = registry;
            _obstacles = new List<Obstacle>();
        }

        public bool RegisterObstacle(Obstacle obstacle)
        {
            if (obstacle == null)
                return false;

            if (!_registry.Register(obstacle))
                return false;

            _obstacles.Add(obstacle);
            return true;
        }

        public bool UnregisterObstacle(int id)
        {
            var obstacle = _registry.GetById(id) as Obstacle;

            if (obstacle == null)
                return false;

            if (!_registry.Unregister(id))
                return false;

            _obstacles.Remove(obstacle);
            return true;
        }

        public Obstacle GetObstacle(int id)
        {
            return _registry.GetById(id) as Obstacle;
        }

        public List<Obstacle> GetObstaclesByType(ObstacleType type)
        {
            var result = new List<Obstacle>();

            for (var i = 0; i < _obstacles.Count; i++)
            {
                if (_obstacles[i].ObstacleType == type)
                    result.Add(_obstacles[i]);
            }

            return result;
        }

        public List<Obstacle> GetActiveObstacles()
        {
            var result = new List<Obstacle>();

            for (var i = 0; i < _obstacles.Count; i++)
            {
                if (_obstacles[i].IsActive)
                    result.Add(_obstacles[i]);
            }

            return result;
        }

        public List<Obstacle> GetDestroyedObstacles()
        {
            var result = new List<Obstacle>();

            for (var i = 0; i < _obstacles.Count; i++)
            {
                if (_obstacles[i].State == ObstacleState.Destroyed)
                    result.Add(_obstacles[i]);
            }

            return result;
        }

        public void ResetAll()
        {
            for (var i = 0; i < _obstacles.Count; i++)
                _obstacles[i].Reset();
        }

        public void Clear()
        {
            for (var i = 0; i < _obstacles.Count; i++)
            {
                _registry.Unregister(_obstacles[i].Id);
            }

            _obstacles.Clear();
            _registry = null;
        }
    }
}
