namespace ADRL.Drone.Utilities
{
    using ADRL.Core.Configuration;
    using ADRL.Drone.Controllers;
    using ADRL.Drone.Core;
    using UnityEngine;

    public readonly struct ValidationResult
    {
        public bool IsValid { get; }
        public string Error { get; }

        private ValidationResult(bool isValid, string error)
        {
            IsValid = isValid;
            Error = error;
        }

        public static ValidationResult Valid()
        {
            return new ValidationResult(true, null);
        }

        public static ValidationResult Invalid(string error)
        {
            return new ValidationResult(
                false, error ?? "Unknown validation error.");
        }
    }

    public static class SpawnValidator
    {
        public static ValidationResult ValidateRequest(SpawnRequest request)
        {
            if (string.IsNullOrEmpty(request.DroneType))
                return ValidationResult.Invalid(
                    "Spawn request has null or empty drone type.");

            return ValidationResult.Valid();
        }

        public static ValidationResult ValidatePrefab(GameObject prefab)
        {
            if (prefab == null)
                return ValidationResult.Invalid("Prefab reference is null.");

            if (prefab.GetComponent<DroneController>() == null)
                return ValidationResult.Invalid(
                    "Prefab is missing required DroneController component.");

            if (prefab.GetComponent<DroneIdentity>() == null)
                return ValidationResult.Invalid(
                    "Prefab is missing required DroneIdentity component.");

            return ValidationResult.Valid();
        }

        public static ValidationResult ValidateSpawnLocation(Vector3 position)
        {
            if (float.IsNaN(position.x) ||
                float.IsNaN(position.y) ||
                float.IsNaN(position.z))
            {
                return ValidationResult.Invalid(
                    "Spawn position contains NaN values.");
            }

            if (float.IsInfinity(position.x) ||
                float.IsInfinity(position.y) ||
                float.IsInfinity(position.z))
            {
                return ValidationResult.Invalid(
                    "Spawn position contains infinite values.");
            }

            return ValidationResult.Valid();
        }

        public static ValidationResult ValidateConfiguration(DroneConfig config)
        {
            if (config == null)
                return ValidationResult.Invalid("DroneConfig is null.");

            return ValidationResult.Valid();
        }

        public static ValidationResult ValidateSpawnParent(Transform parent)
        {
            if (parent == null)
                return ValidationResult.Invalid("Spawn parent transform is null.");

            return ValidationResult.Valid();
        }
    }
}
