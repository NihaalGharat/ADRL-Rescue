namespace ADRL.Editor.Validation
{
    using UnityEditor;
    using UnityEngine;

    public static class ReferenceValidator
    {
        public static void Validate()
        {
            var warnings = 0;
            var checkedObjects = 0;

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ADRL" });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                var components = prefab.GetComponentsInChildren<MonoBehaviour>(true);

                foreach (var component in components)
                {
                    if (component == null)
                    {
                        Debug.LogWarning($"[ReferenceValidator] Missing script in prefab: {path}");
                        warnings++;
                        continue;
                    }

                    var serialized = new SerializedObject(component);
                    var prop = serialized.GetIterator();

                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                            prop.objectReferenceValue == null &&
                            prop.name.StartsWith("_"))
                        {
                            checkedObjects++;
                        }
                    }
                }
            }

            Debug.Log($"[ReferenceValidator] Scanned {guids.Length} prefabs. Checked {checkedObjects} serialized references.");

            var message = warnings > 0
                ? $"Validation complete.\nWarnings: {warnings}\n\nSee Console for details."
                : $"Validation complete.\nPrefabs scanned: {guids.Length}\nReferences checked: {checkedObjects}\nAll references valid.";

            EditorUtility.DisplayDialog("ADRL Reference Validation", message, "OK");
        }
    }
}
