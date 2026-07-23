namespace ADRL.Environment.Procedural
{
    using System.Collections.Generic;
    using ADRL.Environment.WorldObjects;
    using UnityEngine;

    public static class PlacementUtility
    {
        public static Vector3 GetRandomPositionInBounds(Bounds bounds, System.Random random)
        {
            var x = (float)(random.NextDouble() * 2.0 - 1.0) * bounds.extents.x + bounds.center.x;
            var z = (float)(random.NextDouble() * 2.0 - 1.0) * bounds.extents.z + bounds.center.z;
            return new Vector3(x, bounds.center.y, z);
        }

        public static bool HasMinimumSpacing(
            Vector3 position,
            float minSpacing,
            WorldObjectRegistry registry,
            IReadOnlyList<Vector3> placedPositions)
        {
            var sqrMinSpacing = minSpacing * minSpacing;

            for (var i = 0; i < placedPositions.Count; i++)
            {
                if ((placedPositions[i] - position).sqrMagnitude < sqrMinSpacing)
                    return false;
            }

            var allObjects = registry.GetByCategory(WorldObjectCategory.Obstacle);

            for (var i = 0; i < allObjects.Count; i++)
            {
                if (allObjects[i] is MonoBehaviour mb)
                {
                    if ((mb.transform.position - position).sqrMagnitude < sqrMinSpacing)
                        return false;
                }
            }

            return true;
        }

        public static bool TryFindValidPosition(
            Bounds bounds,
            float minSpacing,
            int maxAttempts,
            System.Random random,
            WorldObjectRegistry registry,
            List<Vector3> placedPositions,
            out Vector3 position)
        {
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                position = GetRandomPositionInBounds(bounds, random);

                if (HasMinimumSpacing(position, minSpacing, registry, placedPositions))
                    return true;
            }

            position = Vector3.zero;
            return false;
        }
    }
}
