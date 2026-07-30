namespace ADRL.Drone.Core
{
    using System.Collections.Generic;
    using ADRL.Drone.Controllers;

    public class DronePool
    {
        private readonly Queue<DroneController> _available = new();
        private readonly HashSet<DroneController> _active = new();
        private readonly string _droneType;
        private readonly PoolPolicy _policy;

        public string DroneType => _droneType;

        public PoolPolicy Policy => _policy;

        public int AvailableCount => _available.Count;

        public int ActiveCount => _active.Count;

        public DronePool(string droneType, PoolPolicy policy)
        {
            _droneType = droneType ?? string.Empty;
            _policy = policy;
        }

        public bool TryBorrow(out DroneController controller)
        {
            if (_available.Count > 0)
            {
                controller = _available.Dequeue();
                _active.Add(controller);
                return true;
            }

            controller = null;
            return false;
        }

        public void Return(DroneController controller)
        {
            if (controller == null)
                return;

            if (_active.Remove(controller))
            {
                _available.Enqueue(controller);
            }
        }

        internal void AddActive(DroneController controller)
        {
            _active.Add(controller);
        }

        internal void AddToAvailable(DroneController controller)
        {
            _available.Enqueue(controller);
        }

        internal bool TryRemoveStale(DroneController controller)
        {
            if (controller == null)
                return false;

            var removed = _active.Remove(controller);

            var snapshot = _available.ToArray();
            _available.Clear();
            foreach (var c in snapshot)
            {
                if (c != null && !ReferenceEquals(c, controller))
                    _available.Enqueue(c);
            }

            return removed;
        }
    }
}
