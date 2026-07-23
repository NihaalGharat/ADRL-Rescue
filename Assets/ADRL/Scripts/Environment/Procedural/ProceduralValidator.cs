namespace ADRL.Environment.Procedural
{
    using UnityEngine;

    public static class ProceduralValidator
    {
        private static bool IsValid(GenerationSettings settings, out string message)
        {
            if (settings == null)
            {
                message = "GenerationSettings is null.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool ValidateMinSpacing(GenerationSettings settings, out string message)
        {
            if (!IsValid(settings, out message))
                return false;

            if (settings.MinSpacing <= 0f)
            {
                message = $"GenerationSettings MinSpacing ({settings.MinSpacing}) must be greater than zero.";
                return false;
            }

            return true;
        }

        public static bool ValidateMaxAttempts(GenerationSettings settings, out string message)
        {
            if (!IsValid(settings, out message))
                return false;

            if (settings.MaxPlacementAttempts <= 0)
            {
                message = $"GenerationSettings MaxPlacementAttempts ({settings.MaxPlacementAttempts}) must be greater than zero.";
                return false;
            }

            return true;
        }

        public static bool ValidateBounds(GenerationSettings settings, out string message)
        {
            if (!IsValid(settings, out message))
                return false;

            var bounds = settings.GetGenerationBounds();

            if (bounds.size == Vector3.zero)
            {
                message = "GenerationSettings Bounds size is zero. At least one axis must have non-zero size.";
                return false;
            }

            return true;
        }

        public static bool ValidateEnvironmentConfig(GenerationSettings settings, out string message)
        {
            if (!IsValid(settings, out message))
                return false;

            if (settings.UseEnvironmentConfigBounds && settings.EnvironmentConfig == null)
            {
                message = "GenerationSettings has UseEnvironmentConfigBounds enabled but no EnvironmentConfig assigned.";
                return false;
            }

            return true;
        }

        public static bool ValidateAll(GenerationSettings settings, out string message)
        {
            if (!ValidateMinSpacing(settings, out message))
                return false;

            if (!ValidateMaxAttempts(settings, out message))
                return false;

            if (!ValidateBounds(settings, out message))
                return false;

            if (!ValidateEnvironmentConfig(settings, out message))
                return false;

            return true;
        }
    }
}
