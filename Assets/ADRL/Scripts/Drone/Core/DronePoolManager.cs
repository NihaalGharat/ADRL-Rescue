namespace ADRL.Drone.Core
{
    using System.Collections.Generic;
    using ADRL.Drone.Controllers;
    using ADRL.Drone.Interfaces;
    using UnityEngine;

    public class DronePoolManager : IDroneAllocator
    {
        private readonly Dictionary<string, DronePool> _pools = new();
        private readonly Dictionary<DroneController, string> _controllerToPool = new();
        private readonly DroneFactory _factory = new();
        private DronePrefabRegistry _prefabRegistry;
        private Transform _droneRoot;

        private int _borrowCount;
        private int _returnCount;
        private int _poolMissCount;

        public void Initialize(DronePrefabRegistry prefabRegistry, Transform droneRoot)
        {
            _prefabRegistry = prefabRegistry;
            _droneRoot = droneRoot;
        }

        public DroneController BorrowOrCreate(
            SpawnRequest request,
            SpawnParameters parameters,
            Transform parent)
        {
            var pool = GetOrCreatePool(request.DroneType);

            if (pool.TryBorrow(out var controller))
            {
                _controllerToPool[controller] = request.DroneType;
                _borrowCount++;
                return controller;
            }

            if (_prefabRegistry == null ||
                !_prefabRegistry.TryGetPrefab(request.DroneType, out var prefab))
            {
                _poolMissCount++;
                return null;
            }

            controller = _factory.Create(prefab, parameters, parent, out _);
            pool.AddActive(controller);
            _controllerToPool[controller] = request.DroneType;
            _borrowCount++;
            return controller;
        }

        public void Return(DroneController controller)
        {
            if (controller == null)
                return;

            if (!_controllerToPool.TryGetValue(controller, out var droneType))
                return;

            if (_pools.TryGetValue(droneType, out var pool))
            {
                pool.Return(controller);
                _controllerToPool.Remove(controller);
                _returnCount++;
            }
        }

        public void Prewarm(string droneType, int count)
        {
            if (_prefabRegistry == null || count <= 0)
                return;

            if (!_prefabRegistry.TryGetPrefab(droneType, out var prefab))
                return;

            var pool = GetOrCreatePool(droneType);

            var parameters = new SpawnParameters(
                Vector3.zero,
                Quaternion.identity,
                100f,
                100f,
                DroneState.Uninitialized);

            for (int i = 0; i < count; i++)
            {
                var controller = _factory.Create(prefab, parameters, _droneRoot, out _);
                controller.gameObject.SetActive(false);
                pool.AddToAvailable(controller);
            }
        }

        public void Shutdown()
        {
            _pools.Clear();
            _controllerToPool.Clear();
            _borrowCount = 0;
            _returnCount = 0;
            _poolMissCount = 0;
        }

        public bool PoolExists(string droneType)
        {
            return !string.IsNullOrEmpty(droneType) && _pools.ContainsKey(droneType);
        }

        public PoolStatistics GetStatistics(string droneType)
        {
            if (!_pools.TryGetValue(droneType, out var pool))
                return PoolStatistics.Empty;

            return new PoolStatistics(
                pool.AvailableCount,
                pool.ActiveCount,
                pool.AvailableCount + pool.ActiveCount,
                _borrowCount,
                _returnCount,
                _poolMissCount);
        }

        private DronePool GetOrCreatePool(string droneType)
        {
            if (!_pools.TryGetValue(droneType, out var pool))
            {
                pool = new DronePool(droneType, PoolPolicy.Default());
                _pools[droneType] = pool;
            }

            return pool;
        }
    }
}
