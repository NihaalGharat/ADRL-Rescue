namespace ADRL.Drone.Core
{
    using System;
    using System.Collections.Generic;
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Drone.Controllers;
    using ADRL.Drone.Events;
    using ADRL.Drone.Interfaces;
    using ADRL.Drone.Utilities;
    using UnityEngine;

    public class DroneSpawnManager
    {
        private readonly Queue<SpawnRequest> _spawnQueue = new(64);
        private readonly DroneFactory _factory = new();
        private readonly DronePrefabRegistry _prefabRegistry = new();
        private IDroneAllocator _allocator;
        private EventBus _eventBus;
        private DroneManager _droneManager;
        private DroneConfig _droneConfig;
        private Transform _droneRoot;

        public DronePrefabRegistry PrefabRegistry => _prefabRegistry;
        public int PendingCount => _spawnQueue.Count;

        public void SetAllocator(IDroneAllocator allocator)
        {
            _allocator = allocator;
        }

        public void Initialize(
            EventBus eventBus,
            DroneManager droneManager,
            DroneConfig droneConfig)
        {
            _eventBus = eventBus;
            _droneManager = droneManager;
            _droneConfig = droneConfig;

            if (droneManager != null && droneManager.Context.DroneRoot == null)
            {
                var rootGo = new GameObject("[DroneRuntime]");
                rootGo.transform.SetParent(droneManager.transform);
                droneManager.Context.DroneRoot = rootGo.transform;
            }

            _droneRoot = droneManager?.Context.DroneRoot;
        }

        public SpawnResult Spawn(SpawnRequest request)
        {
            var result = ExecuteSpawn(request);
            _eventBus?.Publish(new DroneSpawnCompletedEvent(result));
            return result;
        }

        public SpawnResult EnqueueSpawn(SpawnRequest request)
        {
            _spawnQueue.Enqueue(request);
            return ProcessQueue();
        }

        public SpawnResult ProcessQueue()
        {
            var lastResult = SpawnResult.CreateFailure("No spawns in queue.");

            while (_spawnQueue.Count > 0)
            {
                var request = _spawnQueue.Dequeue();
                lastResult = ExecuteSpawn(request);
                _eventBus?.Publish(new DroneSpawnCompletedEvent(lastResult));
            }

            return lastResult;
        }

        public void ClearQueue()
        {
            _spawnQueue.Clear();
        }

        public void Return(DroneController controller)
        {
            if (controller == null)
                return;

            controller.ResetDrone();

            if (_allocator != null)
                _allocator.Return(controller);
        }

        private SpawnResult ExecuteSpawn(SpawnRequest request)
        {
            var requestValidation = SpawnValidator.ValidateRequest(request);
            if (!requestValidation.IsValid)
                return SpawnResult.CreateFailure(requestValidation.Error);

            var configValidation = SpawnValidator.ValidateConfiguration(_droneConfig);
            if (!configValidation.IsValid)
                return SpawnResult.CreateFailure(configValidation.Error);

            var parentValidation = SpawnValidator.ValidateSpawnParent(_droneRoot);
            if (!parentValidation.IsValid)
                return SpawnResult.CreateFailure(parentValidation.Error);

            if (!_prefabRegistry.TryGetPrefab(request.DroneType, out var prefab))
                return SpawnResult.CreateFailure(
                    $"No prefab registered for drone type: '{request.DroneType}'.");

            var prefabValidation = SpawnValidator.ValidatePrefab(prefab);
            if (!prefabValidation.IsValid)
                return SpawnResult.CreateFailure(prefabValidation.Error);

            var spawnParams = CalculateSpawnParameters(request);

            var locationValidation =
                SpawnValidator.ValidateSpawnLocation(spawnParams.Position);
            if (!locationValidation.IsValid)
                return SpawnResult.CreateFailure(locationValidation.Error);

            _eventBus?.Publish(new DroneSpawningEvent(request, spawnParams));

            DroneController controller;
            IMotor motor = null;
            try
            {
                if (_allocator != null)
                {
                    controller = _allocator.BorrowOrCreate(
                        request, spawnParams, _droneRoot);
                    motor = controller?.Motor;
                }
                else
                {
                    controller = _factory.Create(
                        prefab, spawnParams, _droneRoot, out motor);
                }
            }
            catch (Exception ex)
            {
                return SpawnResult.CreateFailure(
                    $"Allocation failed: {ex.Message}");
            }

            if (controller == null)
                return SpawnResult.CreateFailure("Allocator returned null.");

            if (motor == null)
                return SpawnResult.CreateFailure(
                    "Allocator returned a drone with no motor.");

            try
            {
                controller.Initialize(
                    _eventBus, _droneConfig, motor, _droneManager);
            }
            catch (Exception ex)
            {
                if (controller != null)
                    GameObject.Destroy(controller.gameObject);
                return SpawnResult.CreateFailure(
                    $"Controller initialization failed: {ex.Message}");
            }

            return SpawnResult.CreateSuccess(
                controller, controller.DroneId, Time.time);
        }

        private SpawnParameters CalculateSpawnParameters(SpawnRequest request)
        {
            var position = _droneConfig.SpawnOffset;
            position.y += _droneConfig.TakeoffHeight;

            return new SpawnParameters(
                position,
                Quaternion.identity,
                _droneConfig.MaxHealth,
                _droneConfig.MaxEnergy,
                DroneState.Uninitialized);
        }
    }
}
