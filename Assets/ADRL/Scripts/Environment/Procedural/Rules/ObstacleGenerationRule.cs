namespace ADRL.Environment.Procedural.Rules
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Resources;
    using ADRL.Environment.Obstacles;
    using ADRL.Environment.Procedural;
    using UnityEngine;

    public class ObstacleGenerationRule : GenerationRule
    {
        private readonly ObstacleManager _obstacleManager;
        private GameObject _obstaclePrefab;

        public ObstacleGenerationRule(ObstacleManager obstacleManager)
        {
            _obstacleManager = obstacleManager;
            Name = "Obstacle Generation Rule";
            Priority = 1;
            LoadPrefab();
        }

        private void LoadPrefab()
        {
            if (ResourceLocator.Prefabs.TryGetPath(PrefabCategory.Obstacle, "Default", out var path))
                _obstaclePrefab = AssetProvider.Load<GameObject>(path);
        }

        public override int Generate(IGenerationContext context)
        {
            if (_obstaclePrefab == null)
                return 0;

            var config = ResourceLocator.Configs.Get<EnvironmentConfig>();

            if (config == null)
                return 0;

            var minCount = config.MinObstacles;
            var maxCount = Mathf.Min(config.MaxObstacles, MaxCount);
            var count = context.Random.Next(minCount, maxCount + 1);
            var obstacleTypes = System.Enum.GetValues(typeof(ObstacleType));
            var placed = 0;

            for (var i = 0; i < count; i++)
            {
                if (!PlacementUtility.TryFindValidPosition(
                        context.Bounds,
                        context.MinSpacing,
                        context.MaxPlacementAttempts,
                        context.Random,
                        context.Registry,
                        context.PlacedPositions,
                        out var position))
                    break;

                var obstacleObj = Object.Instantiate(_obstaclePrefab, position, Quaternion.identity);
                var obstacle = obstacleObj.GetComponent<Obstacle>();

                if (obstacle == null)
                {
                    Object.Destroy(obstacleObj);
                    continue;
                }

                var typeIndex = context.Random.Next(1, obstacleTypes.Length);
                obstacle.SetObstacleType((ObstacleType)obstacleTypes.GetValue(typeIndex));
                _obstacleManager.RegisterObstacle(obstacle);
                context.PlacedPositions.Add(position);
                placed++;
                context.TotalGenerated++;
            }

            return placed;
        }
    }
}
