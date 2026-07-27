namespace ADRL.Drone.Core
{
    using System.Collections.Generic;

    public class DronePersistenceManager
    {
        private readonly List<DroneRuntimeSnapshot> _snapshots = new();
        private readonly int _maxSnapshots;

        public DronePersistenceManager(int maxSnapshots = 100)
        {
            _maxSnapshots = maxSnapshots;
        }

        public int SnapshotCount => _snapshots.Count;

        public DroneRuntimeSnapshot CaptureSnapshot(DroneManager manager)
        {
            var snapshot = manager.CreateSnapshot();
            StoreSnapshot(snapshot);
            return snapshot;
        }

        public bool RestoreSnapshot(DroneRuntimeSnapshot snapshot, DroneManager manager)
        {
            return manager.RestoreSnapshot(snapshot);
        }

        public void ClearSnapshots()
        {
            _snapshots.Clear();
        }

        public bool TryGetLatestSnapshot(out DroneRuntimeSnapshot snapshot)
        {
            if (_snapshots.Count > 0)
            {
                snapshot = _snapshots[^1];
                return true;
            }
            snapshot = default;
            return false;
        }

        public IReadOnlyList<DroneRuntimeSnapshot> GetSnapshotHistory()
        {
            return _snapshots.AsReadOnly();
        }

        private void StoreSnapshot(DroneRuntimeSnapshot snapshot)
        {
            _snapshots.Add(snapshot);
            if (_snapshots.Count > _maxSnapshots)
            {
                _snapshots.RemoveAt(0);
            }
        }
    }
}
