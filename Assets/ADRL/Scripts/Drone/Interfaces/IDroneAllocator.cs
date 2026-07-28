namespace ADRL.Drone.Interfaces
{
    using ADRL.Drone.Controllers;
    using ADRL.Drone.Core;
    using UnityEngine;

    public interface IDroneAllocator
    {
        DroneController BorrowOrCreate(
            SpawnRequest request,
            SpawnParameters parameters,
            Transform parent);

        void Return(DroneController controller);
    }
}
