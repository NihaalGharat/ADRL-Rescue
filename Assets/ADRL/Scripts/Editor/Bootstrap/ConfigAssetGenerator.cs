namespace ADRL.Editor.Bootstrap
{
    using System;
    using System.IO;
    using ADRL.Core.Configuration;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class ConfigAssetGenerator
    {
        private const string ConfigFolder = "Assets/ADRL/ScriptableObjects";

        private static readonly (Type type, string subfolder, string fileName)[] Configs =
        {
            (typeof(ProjectConfig), "Configurations", "ProjectConfig"),
            (typeof(RuntimeConfig), "Configurations", "RuntimeConfig"),
            (typeof(SimulationConfig), "Configurations", "SimulationConfig"),
            (typeof(DroneConfig), "Drone", "DroneConfig"),
            (typeof(EnvironmentConfig), "Environment", "EnvironmentConfig"),
            (typeof(SensorConfig), "Sensors", "SensorConfig"),
            (typeof(RewardConfig), "Rewards", "RewardConfig"),
            (typeof(TrainingConfig), "Training", "TrainingConfig"),
        };

        [MenuItem("Tools/ADRL/Generate Config Assets", priority = 0)]
        private static void GenerateConfigAssets()
        {
            var created = 0;
            var skipped = 0;

            foreach (var (type, subfolder, fileName) in Configs)
            {
                var path = Path.Combine(ConfigFolder, subfolder, $"{fileName}.asset").Replace("\\", "/");

                var existing = AssetDatabase.LoadAssetAtPath(path, type);
                if (existing != null)
                {
                    Debug.Log($"[ConfigAssetGenerator] Already exists: {path}");
                    skipped++;
                    continue;
                }

                var instance = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(instance, path);
                Debug.Log($"[ConfigAssetGenerator] Created: {path}");
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ConfigAssetGenerator] Done. Created {created}, skipped {skipped}.");
            EditorUtility.DisplayDialog("ADRL Config Asset Generator",
                $"Created: {created}\nSkipped (already exist): {skipped}", "OK");
        }

        [MenuItem("Tools/ADRL/Assign Configs to Bootstrapper", priority = 0)]
        private static void AssignConfigsToBootstrapper()
        {
            var scenePath = "Assets/ADRL/Scenes/Main.unity";
            var scene = SceneManager.GetSceneByPath(scenePath);

            var needsReload = false;
            if (!scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                needsReload = true;
            }

            var rootObjects = scene.GetRootGameObjects();
            GameObject bootstrapGo = null;

            foreach (var go in rootObjects)
            {
                if (go.name == "Bootstrap")
                {
                    bootstrapGo = go;
                    break;
                }
            }

            if (bootstrapGo == null)
            {
                EditorUtility.DisplayDialog("ADRL Config Assigner",
                    "Bootstrap GameObject not found in Main.unity.", "OK");

                if (needsReload)
                    EditorSceneManager.CloseScene(scene, true);

                return;
            }

            var bootstrapper = bootstrapGo.GetComponent<ADRL.Core.Bootstrap.Bootstrapper>();
            if (bootstrapper == null)
            {
                EditorUtility.DisplayDialog("ADRL Config Assigner",
                    "Bootstrapper component not found on Bootstrap GameObject.", "OK");

                if (needsReload)
                    EditorSceneManager.CloseScene(scene, true);

                return;
            }

            var serialized = new SerializedObject(bootstrapper);
            var assigned = 0;
            var missing = 0;

            foreach (var (type, subfolder, fileName) in Configs)
            {
                var path = Path.Combine(ConfigFolder, subfolder, $"{fileName}.asset").Replace("\\", "/");
                var asset = AssetDatabase.LoadAssetAtPath(path, type);

                var fieldName = $"_{char.ToLower(fileName[0])}{fileName.Substring(1)}";
                var prop = serialized.FindProperty(fieldName);

                if (prop == null)
                {
                    Debug.LogError($"[ConfigAssetGenerator] Field '{fieldName}' not found on Bootstrapper.");
                    missing++;
                    continue;
                }

                if (asset != null)
                {
                    prop.objectReferenceValue = asset;
                    assigned++;
                }
                else
                {
                    Debug.LogWarning($"[ConfigAssetGenerator] Asset not found: {path}. Generate configs first.");
                    missing++;
                }
            }

            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            if (needsReload)
                EditorSceneManager.CloseScene(scene, true);

            var msg = missing > 0
                ? $"Assigned: {assigned}\nMissing: {missing}\n\nGenerate config assets first if any are missing."
                : $"All {assigned} configs assigned successfully.";

            EditorUtility.DisplayDialog("ADRL Config Assigner", msg, "OK");
        }
    }
}
