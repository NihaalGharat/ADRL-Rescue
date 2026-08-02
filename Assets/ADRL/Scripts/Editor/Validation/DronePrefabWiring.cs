namespace ADRL.Editor.Validation
{
    using ADRL.AI.Interaction;
    using ADRL.Drone.Components;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// One-time wiring step for Phase 8.1.1: adds the collision/victim pipeline
    /// components to the drone prefab and configures its physics surface. The
    /// existing capsule collider becomes a trigger and a kinematic rigidbody is
    /// added so <see cref="DroneCollisionDetector.OnTriggerEnter"/> fires while the
    /// transform-driven drone remains physically inert.
    /// </summary>
    /// <example>
    /// Unity.exe -batchmode -nographics -projectPath &lt;path&gt; \
    ///   -executeMethod ADRL.Editor.Validation.DronePrefabWiring.Run
    /// </example>
    public static class DronePrefabWiring
    {
        private const string PrefabPath = "Assets/ADRL/Resources/Prefabs/Drone/Drone.prefab";

        [MenuItem("ADRL/Phase 8.1.1/Wire Drone Prefab")]
        public static void Run()
        {
            var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
            if (contents == null)
            {
                Debug.LogError($"[DronePrefabWiring] Prefab not found at '{PrefabPath}'.");
                EditorApplication.Exit(1);
                return;
            }

            try
            {
                var rigidbody = contents.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    rigidbody = contents.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;

                var capsule = contents.GetComponent<CapsuleCollider>();
                if (capsule == null)
                    capsule = contents.AddComponent<CapsuleCollider>();
                capsule.isTrigger = true;

                if (contents.GetComponent<DroneCollisionDetector>() == null)
                    contents.AddComponent<DroneCollisionDetector>();

                if (contents.GetComponent<DroneVictimInteraction>() == null)
                    contents.AddComponent<DroneVictimInteraction>();

                PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
                AssetDatabase.SaveAssets();

                Debug.Log(
                    "[DronePrefabWiring] Wired drone prefab: kinematic Rigidbody, " +
                    "trigger capsule collider, DroneCollisionDetector, DroneVictimInteraction.");
                EditorApplication.Exit(0);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
    }
}
