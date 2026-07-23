namespace ADRL.Environment.Procedural.Rules
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Resources;
    using ADRL.Environment.Hazards;
    using ADRL.Environment.Procedural;
    using UnityEngine;

    public class HazardGenerationRule : GenerationRule
    {
        private readonly HazardManager _hazardManager;
        private GameObject _hazardPrefab;

        public HazardGenerationRule(HazardManager hazardManager)
        {
            _hazardManager = hazardManager;
            Name = "Hazard Generation Rule";
            Priority = 2;
            LoadPrefab();
        }

        private void LoadPrefab()
        {
            if (ResourceLocator.Prefabs.TryGetPath(PrefabCategory.Hazard, "Default", out var path))
                _hazardPrefab = AssetProvider.Load<GameObject>(path);
        }

        public override int Generate(IGenerationContext context)
        {
            if (_hazardPrefab == null)
                return 0;

            var config = ResourceLocator.Configs.Get<EnvironmentConfig>();

            if (config == null)
                return 0;

            var area = context.Bounds.size.x * context.Bounds.size.z;
            var spacing = Mathf.Max(context.MinSpacing, 1f);
            var maxPossible = Mathf.FloorToInt(area / (spacing * spacing));
            var count = Mathf.FloorToInt(maxPossible * config.HazardDensity);
            count = Mathf.Clamp(count, 1, Mathf.Min(MaxCount, maxPossible));
            var hazardTypes = System.Enum.GetValues(typeof(HazardType));
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

                var hazardObj = Object.Instantiate(_hazardPrefab, position, Quaternion.identity);
                var hazard = hazardObj.GetComponent<Hazard>();

                if (hazard == null)
                {
                    Object.Destroy(hazardObj);
                    continue;
                }

                var typeIndex = context.Random.Next(hazardTypes.Length);
                hazard.SetHazardType((HazardType)hazardTypes.GetValue(typeIndex));
                _hazardManager.RegisterHazard(hazard);
                context.PlacedPositions.Add(position);
                placed++;
                context.TotalGenerated++;
            }

            return placed;
        }
    }
}
