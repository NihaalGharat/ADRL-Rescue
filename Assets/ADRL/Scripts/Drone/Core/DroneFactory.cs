namespace ADRL.Drone.Core
{
    using ADRL.Drone.Components;
    using ADRL.Drone.Controllers;
    using ADRL.Drone.Interfaces;
    using UnityEngine;

    public class DroneFactory
    {
        public DroneController Create(
            GameObject prefab,
            SpawnParameters parameters,
            Transform parent,
            out IMotor motor)
        {
            if (prefab == null)
                throw new System.ArgumentNullException(nameof(prefab));

            var go = Object.Instantiate(
                prefab, parameters.Position, parameters.Rotation, parent);
            go.name = prefab.name;

            var controller = go.GetComponent<DroneController>();
            if (controller == null)
            {
                Object.Destroy(go);
                motor = null;
                throw new System.InvalidOperationException(
                    $"Prefab '{prefab.name}' is missing DroneController component.");
            }

            motor = new DroneMotor();
            return controller;
        }
    }
}
