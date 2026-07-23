namespace ADRL.Environment.Scenarios
{
    using System.Collections.Generic;

    public static class ScenarioValidator
    {
        private static bool IsValid(ScenarioProfile profile, out string message)
        {
            if (profile == null)
            {
                message = "ScenarioProfile is null.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool ValidateProfile(ScenarioProfile profile, out string message)
        {
            if (!IsValid(profile, out message))
                return false;

            if (string.IsNullOrWhiteSpace(profile.ScenarioName))
            {
                message = "ScenarioProfile has no name.";
                return false;
            }

            if (profile.Seed < 0 && profile.OverrideSeed)
            {
                message = $"ScenarioProfile seed ({profile.Seed}) is negative.";
                return false;
            }

            return true;
        }

        public static bool ValidateSeed(int seed, out string message)
        {
            if (seed < 0)
            {
                message = $"Seed ({seed}) is negative. A non-negative seed is required for deterministic generation.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool ValidateGenerationSettings(ScenarioProfile profile, out string message)
        {
            if (!IsValid(profile, out message))
                return false;

            if (!profile.EnableProceduralGeneration)
            {
                return true;
            }

            if (profile.GenerationSettings == null)
            {
                message = "ScenarioProfile has EnableProceduralGeneration enabled but no GenerationSettings assigned.";
                return false;
            }

            var settings = profile.GenerationSettings;
            var valid = true;
            var errors = new List<string>();

            if (settings.MinSpacing <= 0f)
            {
                errors.Add($"MinSpacing ({settings.MinSpacing}) must be greater than zero.");
                valid = false;
            }

            if (settings.MaxPlacementAttempts <= 0)
            {
                errors.Add($"MaxPlacementAttempts ({settings.MaxPlacementAttempts}) must be greater than zero.");
                valid = false;
            }

            if (settings.UseEnvironmentConfigBounds && settings.EnvironmentConfig == null)
            {
                errors.Add("UseEnvironmentConfigBounds is enabled but no EnvironmentConfig is assigned.");
                valid = false;
            }

            message = valid ? string.Empty : string.Join("; ", errors);
            return valid;
        }

        public static bool ValidateEnvironmentConfig(ScenarioProfile profile, out string message)
        {
            if (!IsValid(profile, out message))
                return false;

            if (profile.EnvironmentConfig == null)
            {
                message = "ScenarioProfile has no EnvironmentConfig assigned.";
                return false;
            }

            var config = profile.EnvironmentConfig;

            if (config.TerrainSize <= 0f)
            {
                message = $"EnvironmentConfig TerrainSize ({config.TerrainSize}) must be greater than zero.";
                return false;
            }

            if (config.MinVictims > config.MaxVictims)
            {
                message = $"EnvironmentConfig MinVictims ({config.MinVictims}) exceeds MaxVictims ({config.MaxVictims}).";
                return false;
            }

            if (config.MinObstacles > config.MaxObstacles)
            {
                message = $"EnvironmentConfig MinObstacles ({config.MinObstacles}) exceeds MaxObstacles ({config.MaxObstacles}).";
                return false;
            }

            return true;
        }

        public static bool ValidateMissionSettings(ScenarioProfile profile, out string message)
        {
            if (!IsValid(profile, out message))
                return false;

            if (profile.MissionDuration <= 0f)
            {
                message = $"MissionDuration ({profile.MissionDuration}) must be greater than zero.";
                return false;
            }

            if (profile.SuccessThreshold <= 0f || profile.SuccessThreshold > 1f)
            {
                message = $"SuccessThreshold ({profile.SuccessThreshold}) must be between 0 and 1.";
                return false;
            }

            if (profile.FailureThreshold < 0f || profile.FailureThreshold >= profile.SuccessThreshold)
            {
                message = $"FailureThreshold ({profile.FailureThreshold}) must be non-negative and less than SuccessThreshold ({profile.SuccessThreshold}).";
                return false;
            }

            return true;
        }

        public static bool ValidateAll(ScenarioProfile profile, out string message)
        {
            if (!ValidateProfile(profile, out message))
                return false;

            if (!ValidateSeed(profile.GetEffectiveSeed(), out message))
                return false;

            if (!ValidateGenerationSettings(profile, out message))
                return false;

            if (!ValidateEnvironmentConfig(profile, out message))
                return false;

            if (!ValidateMissionSettings(profile, out message))
                return false;

            return true;
        }
    }
}
