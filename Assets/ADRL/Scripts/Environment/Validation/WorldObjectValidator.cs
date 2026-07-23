namespace ADRL.Environment.Validation
{
    using ADRL.Environment.Obstacles;
    using ADRL.Environment.WorldObjects;
    using UnityEngine;

    public static class WorldObjectValidator
    {
        public static bool ValidateId(IWorldObject obj, out string message)
        {
            if (obj == null)
            {
                message = "WorldObject is null.";
                return false;
            }

            if (obj.Id <= 0)
            {
                message = $"WorldObject has invalid ID: {obj.Id}.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool ValidateCategory(IWorldObject obj, out string message)
        {
            if (obj == null)
            {
                message = "WorldObject is null.";
                return false;
            }

            if (obj.Category == WorldObjectCategory.Unknown)
            {
                message = $"WorldObject {obj.Id} has Unknown category.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool CheckDuplicateRegistration(WorldObjectRegistry registry, int id, out string message)
        {
            if (registry == null)
            {
                message = "Registry is null.";
                return false;
            }

            if (registry.GetById(id) != null)
            {
                message = $"Duplicate registration: object with ID {id} is already registered.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool CheckMissingReferences(MonoBehaviour component, out string message)
        {
            if (component == null)
            {
                message = "Component reference is null.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool ValidateObstacleType(Obstacle obstacle, out string message)
        {
            if (obstacle == null)
            {
                message = "Obstacle is null.";
                return false;
            }

            if (obstacle.ObstacleType == ObstacleType.Unknown)
            {
                message = $"Obstacle {obstacle.Id} has Unknown type.";
                return false;
            }

            message = string.Empty;
            return true;
        }
    }
}
