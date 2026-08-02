namespace ADRL.Drone.Components
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Drone.Controllers;
    using UnityEngine;

    /// <summary>
    /// Detects runtime collisions against the drone's trigger collider and forwards
    /// them down the event pipeline. It publishes <see cref="CollisionEvent"/> and
    /// applies the configured collision damage through <see cref="DroneHealth"/>.
    /// It owns no reward, mission, or episode logic; downstream systems (e.g. the
    /// reward evaluator) react to the published event unchanged.
    /// </summary>
    [RequireComponent(typeof(DroneController))]
    public sealed class DroneCollisionDetector : MonoBehaviour
    {
        private DroneController _controller;
        private DroneConfig _config;

        private void Awake()
        {
            _controller = GetComponent<DroneController>();
        }

        private void Start()
        {
            _config = _controller != null ? _controller.Config : null;
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleCollision(other, 1f);
        }

        /// <summary>
        /// Handles a collision contact: publishes the collision event and forwards
        /// the configured damage to the drone's health. Exposed publicly so the
        /// pipeline is testable without entering play mode.
        /// </summary>
        public void HandleCollision(Collider other, float impactForce)
        {
            if (other == null)
                return;

            if (_controller == null)
                _controller = GetComponent<DroneController>();

            if (_controller == null || other.transform.IsChildOf(transform))
                return;

            if (_config == null)
                _config = _controller.Config;

            _controller.EventBus?.Publish(new CollisionEvent(
                _controller.DroneId,
                ResolveCollisionTag(other),
                Mathf.Max(0f, impactForce)));

            if (_config != null && _controller.Health != null && !_controller.Health.IsDestroyed)
                _controller.Health.TakeDamage(_config.CollisionDamage);
        }

        private static string ResolveCollisionTag(Collider other)
        {
            var tag = other.tag;
            return string.IsNullOrEmpty(tag) || tag == "Untagged" ? other.name : tag;
        }
    }
}
