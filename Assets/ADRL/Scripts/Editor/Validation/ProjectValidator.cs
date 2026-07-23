namespace ADRL.Editor.Validation
{
    using UnityEditor;
    using UnityEngine;

    public static class ProjectValidator
    {
        public static void Validate()
        {
            var errors = 0;
            var warnings = 0;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[ProjectValidator] Cannot validate while in Play Mode.");
                EditorUtility.DisplayDialog("ADRL Project Validation",
                    "Cannot validate while in Play Mode.", "OK");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ADRL/ScriptableObjects" });
            if (guids.Length == 0)
            {
                Debug.LogWarning("[ProjectValidator] No ScriptableObject configs found in Assets/ADRL/ScriptableObjects/.");
                warnings++;
            }
            else
            {
                Debug.Log($"[ProjectValidator] Found {guids.Length} ScriptableObject assets in config directory.");
            }

            guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { "Assets/ADRL/Scripts" });
            if (guids.Length == 0)
            {
                Debug.LogError("[ProjectValidator] No Assembly Definitions found in Assets/ADRL/Scripts/.");
                errors++;
            }
            else
            {
                Debug.Log($"[ProjectValidator] Found {guids.Length} Assembly Definitions.");
            }

            if (errors > 0)
            {
                EditorUtility.DisplayDialog("ADRL Project Validation",
                    $"Validation complete.\nErrors: {errors}\nWarnings: {warnings}\n\nSee Console for details.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("ADRL Project Validation",
                    $"Validation complete.\nErrors: {errors}\nWarnings: {warnings}\n\nAll checks passed.", "OK");
            }
        }
    }
}
