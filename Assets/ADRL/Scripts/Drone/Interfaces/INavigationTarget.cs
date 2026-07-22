namespace ADRL.Drone.Interfaces
{
    using UnityEngine;

    public interface INavigationTarget
    {
        Vector3 Position { get; }
        string TargetId { get; }
        bool IsValid { get; }
    }
}
