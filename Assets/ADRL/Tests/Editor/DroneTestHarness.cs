namespace ADRL.Tests.Editor
{
    using System;
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Drone.Components;
    using ADRL.Drone.Controllers;
    using ADRL.Drone.Core;
    using UnityEngine;

    /// <summary>
    /// Builds a fully initialized drone (manager, controller, motor) with a
    /// collision detector so the Phase 8.1.1 collision pipeline can be tested
    /// without entering play mode.
    /// </summary>
    internal sealed class DroneTestHarness : IDisposable
    {
        public readonly EventBus EventBus;
        public readonly DroneConfig Config;
        public readonly DroneController Controller;
        public readonly DroneCollisionDetector Detector;
        public readonly GameObject DroneObject;

        private readonly GameObject _managerObject;

        public DroneTestHarness()
        {
            EventBus = new EventBus();
            EventBus.Initialize();

            Config = ScriptableObject.CreateInstance<DroneConfig>();

            _managerObject = new GameObject("[TestDroneManager]");
            var manager = _managerObject.AddComponent<DroneManager>();
            manager.Initialize(EventBus, new DroneContext(), new DroneRegistry(), new DroneConfiguration());

            DroneObject = new GameObject("[TestDrone]");
            Controller = DroneObject.AddComponent<DroneController>();
            Controller.Initialize(EventBus, Config, new DroneMotor(), manager);

            Detector = DroneObject.AddComponent<DroneCollisionDetector>();
        }

        public void Dispose()
        {
            TestUtilities.DestroySafe(DroneObject);
            TestUtilities.DestroySafe(_managerObject);
            TestUtilities.DestroySafe(Config);
            EventBus.Shutdown();
        }
    }
}
