namespace ADRL.Drone.Interfaces
{
    using ADRL.Core.Configuration;
    using UnityEngine;

    public interface IMotor
    {
        void Initialize(DroneConfig config);
        void Move(Vector3 direction, float speed);
        void Rotate(Quaternion targetRotation, float rotationSpeed);
        void Stop();
        void EmergencyStop();
        bool IsInitialized { get; }
    }
}
