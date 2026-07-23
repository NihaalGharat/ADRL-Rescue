namespace ADRL.Environment.Procedural.Rules
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Resources;
    using ADRL.Environment.Core;
    using ADRL.Environment.Procedural;
    using ADRL.Environment.Victims;
    using UnityEngine;

    public class VictimGenerationRule : GenerationRule
    {
        private readonly EnvironmentManager _environmentManager;
        private GameObject _victimPrefab;

        public VictimGenerationRule(EnvironmentManager environmentManager)
        {
            _environmentManager = environmentManager;
            Name = "Victim Generation Rule";
            Priority = 3;
            LoadPrefab();
        }

        private void LoadPrefab()
        {
            if (ResourceLocator.Prefabs.TryGetPath(PrefabCategory.Victim, "Default", out var path))
                _victimPrefab = AssetProvider.Load<GameObject>(path);
        }

        public override int Generate(IGenerationContext context)
        {
            if (_victimPrefab == null)
                return 0;

            var config = ResourceLocator.Configs.Get<EnvironmentConfig>();

            if (config == null)
                return 0;

            var minCount = config.MinVictims;
            var maxCount = Mathf.Min(config.MaxVictims, MaxCount);
            var target = context.Random.Next(minCount, maxCount + 1);
            var placed = 0;

            for (var i = 0; i < target; i++)
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

                var victimObj = Object.Instantiate(_victimPrefab, position, Quaternion.identity);
                var victim = victimObj.GetComponent<Victim>();

                if (victim == null)
                {
                    Object.Destroy(victimObj);
                    continue;
                }

                _environmentManager.RegisterVictim(victim);
                context.PlacedPositions.Add(position);
                placed++;
                context.TotalGenerated++;
            }

            return placed;
        }
    }
}
