namespace ADRL.Drone.Interfaces
{
    using UnityEngine;

    public interface INavigationSystem
    {
        void SetTarget(INavigationTarget target);
        void ClearTarget();
        Vector3 GetCurrentTargetPosition();
        bool HasTarget { get; }
    }
}
