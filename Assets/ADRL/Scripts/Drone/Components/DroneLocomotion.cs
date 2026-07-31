namespace ADRL.Drone.Components
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Resources;
    using ADRL.Drone.Controllers;
    using UnityEngine;

    /// <summary>
    /// Applies the drone's passive <see cref="DroneMotor"/> command output to the
    /// transform. <see cref="DroneMotor"/> is intentionally a data-only motor, so
    /// this component is the bridge that actually moves and rotates the drone
    /// every physics tick while it is in the <see cref="DroneState.Active"/> state.
    /// </summary>
    [RequireComponent(typeof(DroneController))]
    public sealed class DroneLocomotion : MonoBehaviour
    {
        private DroneController _controller;
        private DroneConfig _config;

        private void Awake()
        {
            _controller = GetComponent<DroneController>();
        }

        private void Start()
        {
            _config = ResourceLocator.IsInitialized
                ? ResourceLocator.Configs.Get<DroneConfig>()
                : null;
        }

        private void FixedUpdate()
        {
            if (_controller == null || _controller.CurrentState != DroneState.Active)
                return;

            if (_controller.Motor is not DroneMotor motor)
                return;

            if (motor.CurrentVelocity.sqrMagnitude > Mathf.Epsilon)
            {
                transform.position += motor.CurrentVelocity * Time.fixedDeltaTime;

                if (_config != null && motor.TargetRotation != Quaternion.identity)
                {
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        motor.TargetRotation,
                        _config.RotationSpeed * Time.fixedDeltaTime);
                }
            }
        }
    }
}
