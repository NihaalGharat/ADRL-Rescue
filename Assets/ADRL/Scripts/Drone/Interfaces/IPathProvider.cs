namespace ADRL.Drone.Interfaces
{
    using UnityEngine;

    public interface IPathProvider
    {
        Vector3[] GetPath(Vector3 start, Vector3 end);
        bool HasPathTo(Vector3 destination);
    }
}
