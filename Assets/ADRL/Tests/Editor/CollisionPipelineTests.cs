namespace ADRL.Tests.Editor.Interaction
{
    using NUnit.Framework;
    using ADRL.Core.Events;
    using ADRL.Tests.Editor;
    using UnityEngine;

    /// <summary>
    /// Verifies the Phase 8.1.1 collision pipeline: <see cref="ADRL.Drone.Components.DroneCollisionDetector"/>
    /// publishes <see cref="CollisionEvent"/> with the owning drone's id and
    /// forwards the configured damage to the drone's health. The detector owns no
    /// reward or episode logic.
    /// </summary>
    [TestFixture]
    internal sealed class CollisionPipelineTests
    {
        [Test]
        public void Collision_PublishesEvent_ForOwningDrone()
        {
            using var h = new DroneTestHarness();
            var received = default(CollisionEvent);
            var count = 0;
            h.EventBus.Subscribe<CollisionEvent>(e => { received = e; count++; });

            var obstacle = CreateCollider("Obstacle");
            h.Detector.HandleCollision(obstacle, 1f);

            Assert.AreEqual(1, count);
            Assert.AreEqual(h.Controller.DroneId, received.DroneId);
            Assert.AreEqual("Obstacle", received.CollidedTag);
            Assert.AreEqual(1f, received.ImpactForce, 1e-6f);
            Object.DestroyImmediate(obstacle.gameObject);
        }

        [Test]
        public void Collision_ForwardsConfiguredDamage()
        {
            using var h = new DroneTestHarness();
            var startingHealth = h.Controller.Health.CurrentHealth;
            var obstacle = CreateCollider("Obstacle");

            h.Detector.HandleCollision(obstacle, 1f);

            Assert.AreEqual(startingHealth - h.Config.CollisionDamage, h.Controller.Health.CurrentHealth, 1e-6f);
            Assert.IsFalse(h.Controller.Health.IsDestroyed);
            Object.DestroyImmediate(obstacle.gameObject);
        }

        [Test]
        public void Collision_IsIgnored_WhenDamagedDroneIsDestroyed()
        {
            using var h = new DroneTestHarness();
            var obstacle = CreateCollider("Obstacle");

            var requiredHits = Mathf.CeilToInt(h.Config.MaxHealth / h.Config.CollisionDamage);
            for (var i = 0; i < requiredHits + 1; i++)
                h.Detector.HandleCollision(obstacle, 1f);

            Assert.IsTrue(h.Controller.Health.IsDestroyed);
            Assert.AreEqual(0f, h.Controller.Health.CurrentHealth, 1e-6f);
            Object.DestroyImmediate(obstacle.gameObject);
        }

        [Test]
        public void Collision_IgnoresSelfCollider()
        {
            using var h = new DroneTestHarness();
            var count = 0;
            h.EventBus.Subscribe<CollisionEvent>(_ => count++);

            var selfCollider = h.DroneObject.AddComponent<SphereCollider>();
            h.Detector.HandleCollision(selfCollider, 1f);

            Assert.AreEqual(0, count);
            Assert.AreEqual(h.Config.MaxHealth, h.Controller.Health.CurrentHealth, 1e-6f);
        }

        [Test]
        public void Collision_ClampsNegativeImpactForce()
        {
            using var h = new DroneTestHarness();
            CollisionEvent received = default;
            h.EventBus.Subscribe<CollisionEvent>(e => received = e);

            var obstacle = CreateCollider("Obstacle");
            h.Detector.HandleCollision(obstacle, -5f);

            Assert.AreEqual(0f, received.ImpactForce, 1e-6f);
            Object.DestroyImmediate(obstacle.gameObject);
        }

        private static Collider CreateCollider(string name)
        {
            var go = new GameObject(name);
            return go.AddComponent<SphereCollider>();
        }
    }
}
