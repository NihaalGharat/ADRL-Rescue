namespace ADRL.Tests.Editor.Environment
{
    using NUnit.Framework;
    using UnityEditor;
    using ADRL.Environment.Victims;
    using UnityEngine;

    /// <summary>
    /// Verifies the Phase 8.1.3 victim prefab asset: it exists under a Resources
    /// path that <see cref="ADRL.Core.Resources.PrefabRegistry"/> /
    /// <see cref="ADRL.Core.Resources.AssetProvider"/> can resolve, carries the
    /// <see cref="Victim"/> lifecycle component, and exposes a solid (non-trigger)
    /// collider on the default layer so <c>DroneVictimInteraction</c> and the
    /// sensors (which scan with <c>QueryTriggerInteraction.Ignore</c> over
    /// <c>DefaultRaycastLayers</c>) can detect it.
    /// </summary>
    [TestFixture]
    internal sealed class VictimPrefabIntegrityTests
    {
        private const string PrefabAssetPath = "Assets/ADRL/Resources/Prefabs/Victim/Victim.prefab";
        private const string PrefabResourcePath = "Prefabs/Victim/Victim";

        [Test]
        public void VictimPrefab_Exists()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
            Assert.IsNotNull(prefab, "Victim prefab missing at " + PrefabAssetPath);
        }

        [Test]
        public void VictimPrefab_IsLoadableThroughResources()
        {
            var prefab = Resources.Load<GameObject>(PrefabResourcePath);
            Assert.IsNotNull(prefab, "Victim prefab not loadable via Resources path '" + PrefabResourcePath + "'.");
        }

        [Test]
        public void VictimPrefab_HasVictimComponent()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
            Assert.IsNotNull(prefab, "Victim prefab missing.");
            Assert.IsNotNull(prefab.GetComponent<Victim>(), "Prefab must carry the Victim component.");
        }

        [Test]
        public void VictimPrefab_HasSolidCollider()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
            Assert.IsNotNull(prefab, "Victim prefab missing.");

            var collider = prefab.GetComponent<Collider>();
            Assert.IsNotNull(collider, "Prefab must have a collider so sensors/interaction can detect it.");
            Assert.IsFalse(collider.isTrigger,
                "Victim collider must be solid (non-trigger); interaction scans with QueryTriggerInteraction.Ignore.");
        }

        [Test]
        public void VictimPrefab_IsOnDefaultLayer()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
            Assert.IsNotNull(prefab, "Victim prefab missing.");
            Assert.AreEqual(0, prefab.layer,
                "Victim must be on the Default layer (included in Physics.DefaultRaycastLayers).");
        }

        [Test]
        public void VictimPrefab_StateStartsWaiting()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
            Assert.IsNotNull(prefab, "Victim prefab missing.");

            var victim = prefab.GetComponent<Victim>();
            Assert.IsNotNull(victim, "Prefab must carry the Victim component.");
            Assert.AreEqual(VictimState.Waiting, victim.State, "Newly instantiated victims must start Waiting.");
        }
    }
}
