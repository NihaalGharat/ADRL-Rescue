namespace ADRL.Editor.Validation
{
    using UnityEditor;
    using UnityEngine;

    public static class AdrlMenuItems
    {
        private const string RootMenu = "Tools/ADRL/";

        [MenuItem(RootMenu + "Validate Project", priority = 1)]
        private static void ValidateProject()
        {
            ProjectValidator.Validate();
        }

        [MenuItem(RootMenu + "Validate Configurations", priority = 2)]
        private static void ValidateConfigurations()
        {
            ConfigurationValidator.Validate();
        }

        [MenuItem(RootMenu + "Validate References", priority = 3)]
        private static void ValidateReferences()
        {
            ReferenceValidator.Validate();
        }

        [MenuItem(RootMenu + "Documentation/Namespace Guide", priority = 100)]
        private static void OpenNamespaceGuide()
        {
            PingDocumentation("NAMESPACE_GUIDE");
        }

        [MenuItem(RootMenu + "Documentation/Developer Handbook", priority = 101)]
        private static void OpenDeveloperHandbook()
        {
            PingDocumentation("18_DEVELOPER_HANDBOOK");
        }

        private static void PingDocumentation(string searchFilter)
        {
            var guids = AssetDatabase.FindAssets(searchFilter);

            if (guids.Length == 0)
            {
                Debug.LogWarning($"[AdrlMenuItems] Documentation file \"{searchFilter}\" not found in project.");
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var obj = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (obj == null)
            {
                Debug.LogWarning($"[AdrlMenuItems] Could not load documentation asset at: {path}");
                return;
            }

            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
    }
}
