namespace ADRL.Editor.Validation
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class ConfigurationValidator
    {
        private static readonly string[] ExpectedConfigNames = new[]
        {
            "ProjectConfig",
            "RuntimeConfig",
            "SimulationConfig",
            "DroneConfig",
            "EnvironmentConfig",
            "SensorConfig",
            "RewardConfig",
            "TrainingConfig"
        };

        public static void Validate()
        {
            var errors = new List<string>();
            var expectedTypes = new Dictionary<string, bool>();

            foreach (var name in ExpectedConfigNames)
                expectedTypes[name] = false;

            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ADRL/ScriptableObjects" });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset == null)
                    continue;

                var name = asset.GetType().Name;

                if (expectedTypes.ContainsKey(name))
                    expectedTypes[name] = true;
            }

            var found = 0;

            foreach (var kvp in expectedTypes)
            {
                if (kvp.Value)
                {
                    found++;
                }
                else
                {
                    errors.Add($"Missing configuration: {kvp.Key}. Create it in Assets/ADRL/ScriptableObjects/Configurations/");
                }
            }

            Debug.Log($"[ConfigurationValidator] Found {found}/{ExpectedConfigNames.Length} configuration types.");

            foreach (var error in errors)
                Debug.LogError($"[ConfigurationValidator] {error}");

            var message = errors.Count > 0
                ? $"Validation complete.\nErrors: {errors.Count}\n\nSee Console for details."
                : $"Validation complete.\nAll {ExpectedConfigNames.Length} configurations found.";

            EditorUtility.DisplayDialog("ADRL Configuration Validation", message, "OK");
        }
    }
}
