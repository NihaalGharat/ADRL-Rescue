namespace ADRL.Drone.Utilities
{
    using System.Collections.Generic;
    using ADRL.Drone.Controllers;
    using ADRL.Drone.Core;
    using ADRL.Drone.Interfaces;

    public readonly struct EntityValidationResult
    {
        public bool IsValid { get; }
        public IReadOnlyList<string> Errors { get; }

        public EntityValidationResult(bool isValid, List<string> errors)
        {
            IsValid = isValid;
            Errors = errors;
        }
    }

    public static class DroneEntityValidator
    {
        public static EntityValidationResult ValidateEntity(DroneController controller)
        {
            var errors = new List<string>();

            if (controller == null)
            {
                errors.Add("Controller is null.");
                return new EntityValidationResult(false, errors);
            }

            var identity = controller.GetComponent<DroneIdentity>();

            if (identity == null)
            {
                errors.Add($"Drone {controller.DroneId} is missing DroneIdentity component.");
            }
            else
            {
                if (!identity.IsAssigned)
                    errors.Add($"DroneIdentity on drone {controller.DroneId} is not assigned.");

                if (identity.DroneId != controller.DroneId)
                    errors.Add($"DroneIdentity ID mismatch: identity={identity.DroneId}, controller={controller.DroneId}");
            }

            if (!controller.Motor.IsInitialized)
                errors.Add($"Drone {controller.DroneId} motor is not initialized.");

            if (controller.CurrentState == DroneState.Uninitialized)
                errors.Add($"Drone {controller.DroneId} controller is not initialized.");

            return new EntityValidationResult(errors.Count == 0, errors);
        }

        public static List<string> ValidateFleetEntities(DroneManager manager)
        {
            var errors = new List<string>();

            if (manager == null)
            {
                errors.Add("DroneManager is null.");
                return errors;
            }

            if (manager.State == DroneSystemState.Uninitialized)
                return errors;

            foreach (var controller in manager.Registry.GetAll())
            {
                var result = ValidateEntity(controller);
                if (!result.IsValid)
                {
                    foreach (var e in result.Errors)
                        errors.Add($"[Entity] {e}");
                }
            }

            return errors;
        }
    }
}
