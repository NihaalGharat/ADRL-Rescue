namespace ADRL.Editor.Validation
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class ConfigurationValidator
    {
        private static readonly Dictionary<string, Type> ExpectedConfigs = new Dictionary<string, Type>
        {
            { "ProjectConfig", null },
            { "RuntimeConfig", null },
            { "SimulationConfig", null },
            { "DroneConfig", null },
            { "EnvironmentConfig", null },
            { "SensorConfig", null },
            { "RewardConfig", null },
            { "TrainingConfig", null }
        };

        [MenuItem("Tools/ADRL/Validate Configurations", priority = 2)]
        public static void Validate()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var found = 0;

            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ADRL/ScriptableObjects" });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset == null)
                    continue;

                var name = asset.GetType().Name;

                if (ExpectedConfigs.ContainsKey(name))
                {
                    found++;
                    ExpectedConfigs[name] = asset.GetType();
                }
            }

            foreach (var kvp in ExpectedConfigs)
            {
                if (kvp.Value == null)
                {
                    errors.Add($"Missing configuration: {kvp.Key}. Create it in Assets/ADRL/ScriptableObjects/Configurations/");
                }
            }

            Debug.Log($"[ConfigurationValidator] Found {found}/{ExpectedConfigs.Count} configuration types.");

            foreach (var error in errors)
            {
                Debug.LogError($"[ConfigurationValidator] {error}");
            }

            foreach (var warning in warnings)
            {
                Debug.LogWarning($"[ConfigurationValidator] {warning}");
            }

            var message = errors.Count > 0
                ? $"Validation complete.\nErrors: {errors.Count}\n\nSee Console for details."
                : $"Validation complete.\nAll {ExpectedConfigs.Count} configurations found.";

            EditorUtility.DisplayDialog("ADRL Configuration Validation", message, "OK");
        }
    }
}
