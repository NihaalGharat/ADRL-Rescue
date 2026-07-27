namespace ADRL.Drone.Interfaces
{
    using System.Collections.Generic;
    using ADRL.Drone.Controllers;

    public interface IDroneRegistry
    {
        int Count { get; }
        void Register(int droneId, DroneController controller);
        bool Unregister(int droneId);
        bool Contains(int droneId);
        DroneController Get(int droneId);
        IReadOnlyCollection<DroneController> GetAll();
        void Clear();
    }
}
