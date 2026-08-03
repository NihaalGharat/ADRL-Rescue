namespace ADRL.Editor.Validation
{
    using ADRL.Environment.Victims;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// One-time wiring step for Phase 8.1.3: creates the victim prefab that
    /// <see cref="VictimGenerationRule"/> loads at boot. The prefab carries the
    /// <see cref="Victim"/> lifecycle component and a solid (non-trigger)
    /// capsule collider on the default layer, because
    /// <c>DroneVictimInteraction</c> and the thermal/ray sensors scan with
    /// <c>QueryTriggerInteraction.Ignore</c> over <c>DefaultRaycastLayers</c>.
    /// </summary>
    /// <example>
    /// Unity.exe -batchmode -nographics -projectPath &lt;path&gt; \
    ///   -executeMethod ADRL.Editor.Validation.VictimPrefabWiring.Run
    /// </example>
    public static class VictimPrefabWiring
    {
        private const string PrefabPath = "Assets/ADRL/Resources/Prefabs/Victim/Victim.prefab";

        [MenuItem("ADRL/Phase 8.1.3/Wire Victim Prefab")]
        public static void Run()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                Debug.Log("[VictimPrefabWiring] Victim prefab already exists at '" + PrefabPath + "'.");
                EditorApplication.Exit(0);
                return;
            }

            EnsurePrefabFolder();

            var victim = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            victim.name = "Victim";
            victim.layer = 0;

            var capsule = victim.GetComponent<CapsuleCollider>();
            if (capsule == null)
                capsule = victim.AddComponent<CapsuleCollider>();
            capsule.isTrigger = false;

            if (victim.GetComponent<Victim>() == null)
                victim.AddComponent<Victim>();

            var saved = PrefabUtility.SaveAsPrefabAsset(victim, PrefabPath);
            AssetDatabase.SaveAssets();
            Object.DestroyImmediate(victim);

            if (saved == null)
            {
                Debug.LogError("[VictimPrefabWiring] Failed to save victim prefab at '" + PrefabPath + "'.");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[VictimPrefabWiring] Created victim prefab at '" + PrefabPath + "'.");
            EditorApplication.Exit(0);
        }

        private static void EnsurePrefabFolder()
        {
            var parent = "Assets/ADRL/Resources/Prefabs";
            var folder = parent + "/Victim";

            if (!AssetDatabase.IsValidFolder(parent))
            {
                Debug.LogError("[VictimPrefabWiring] Expected folder '" + parent + "' is missing.");
                EditorApplication.Exit(1);
                return;
            }

            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(parent, "Victim");
        }
    }
}
