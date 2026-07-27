namespace ADRL.Drone.Core
{
    using System.Collections.Generic;
    using ADRL.Drone.Controllers;

    public class DroneRegistry
    {
        private readonly Dictionary<int, DroneController> _drones = new();

        public int Count => _drones.Count;

        public void Register(int droneId, DroneController controller)
        {
            _drones[droneId] = controller;
        }

        public bool Unregister(int droneId)
        {
            return _drones.Remove(droneId);
        }

        public bool Contains(int droneId)
        {
            return _drones.ContainsKey(droneId);
        }

        public DroneController Get(int droneId)
        {
            _drones.TryGetValue(droneId, out var controller);
            return controller;
        }

        public IReadOnlyCollection<DroneController> GetAll()
        {
            return _drones.Values;
        }

        public void Clear()
        {
            _drones.Clear();
        }
    }
}
