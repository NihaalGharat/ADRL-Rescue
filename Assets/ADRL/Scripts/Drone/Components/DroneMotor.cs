namespace ADRL.Drone.Components
{
    using ADRL.Core.Configuration;
    using ADRL.Drone.Interfaces;
    using UnityEngine;

    public class DroneMotor : IMotor
    {
        private DroneConfig _config;
        private Vector3 _currentVelocity;
        private Quaternion _targetRotation;

        public bool IsInitialized { get; private set; }

        public void Initialize(DroneConfig config)
        {
            _config = config;
            _currentVelocity = Vector3.zero;
            _targetRotation = Quaternion.identity;
            IsInitialized = true;
        }

        public void Move(Vector3 direction, float speed)
        {
            if (!IsInitialized)
                return;

            var clampedSpeed = Mathf.Min(speed, _config.MaxSpeed);
            _currentVelocity = direction.normalized * clampedSpeed;
            _targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        public void Rotate(Quaternion targetRotation, float rotationSpeed)
        {
            if (!IsInitialized)
                return;

            _targetRotation = targetRotation;
        }

        public void Stop()
        {
            if (!IsInitialized)
                return;

            _currentVelocity = Vector3.zero;
        }

        public void EmergencyStop()
        {
            _currentVelocity = Vector3.zero;
        }

        public Vector3 CurrentVelocity => _currentVelocity;
        public Quaternion TargetRotation => _targetRotation;
    }
}
