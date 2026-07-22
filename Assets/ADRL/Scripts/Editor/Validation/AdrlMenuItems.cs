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
            var guid = AssetDatabase.FindAssets("NAMESPACE_GUIDE")[0];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        [MenuItem(RootMenu + "Documentation/Developer Handbook", priority = 101)]
        private static void OpenDeveloperHandbook()
        {
            var guids = AssetDatabase.FindAssets("18_DEVELOPER_HANDBOOK");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }
    }
}
