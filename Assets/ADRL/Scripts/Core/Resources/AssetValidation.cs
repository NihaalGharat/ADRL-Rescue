namespace ADRL.Core.Resources
{
    using System.Collections.Generic;
    using UnityEngine;

    public readonly struct ValidationResult
    {
        public bool HasErrors { get; }
        public bool HasWarnings { get; }
        public IReadOnlyList<string> Errors { get; }
        public IReadOnlyList<string> Warnings { get; }

        public ValidationResult(List<string> errors, List<string> warnings)
        {
            Errors = errors.AsReadOnly();
            Warnings = warnings.AsReadOnly();
            HasErrors = errors.Count > 0;
            HasWarnings = warnings.Count > 0;
        }
    }

    public static class AssetValidation
    {
        public static ValidationResult ValidateConfigs(ConfigRegistry registry)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (registry == null)
            {
                errors.Add("ConfigRegistry is null.");
                return new ValidationResult(errors, warnings);
            }

            if (registry.Count == 0)
            {
                warnings.Add("ConfigRegistry is empty. No configurations registered.");
            }

            return new ValidationResult(errors, warnings);
        }

        public static ValidationResult ValidatePrefabs(PrefabRegistry registry)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (registry == null)
            {
                errors.Add("PrefabRegistry is null.");
                return new ValidationResult(errors, warnings);
            }

            if (registry.Count == 0)
            {
                warnings.Add("PrefabRegistry is empty. No prefabs registered.");
            }

            return new ValidationResult(errors, warnings);
        }

        public static ValidationResult ValidateAll(ConfigRegistry configs, PrefabRegistry prefabs)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            var configResult = ValidateConfigs(configs);
            errors.AddRange(configResult.Errors);
            warnings.AddRange(configResult.Warnings);

            var prefabResult = ValidatePrefabs(prefabs);
            errors.AddRange(prefabResult.Errors);
            warnings.AddRange(prefabResult.Warnings);

            return new ValidationResult(errors, warnings);
        }

        public static void LogResults(ValidationResult result)
        {
            foreach (var error in result.Errors)
            {
                Debug.LogError($"[AssetValidation] {error}");
            }

            foreach (var warning in result.Warnings)
            {
                Debug.LogWarning($"[AssetValidation] {warning}");
            }

            if (!result.HasErrors && !result.HasWarnings)
            {
                Debug.Log("[AssetValidation] All validations passed.");
            }
        }
    }
}
