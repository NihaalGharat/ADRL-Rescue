namespace ADRL.Drone.Core
{
    using System.Collections.Generic;

    public class DroneRecoveryValidator
    {
        private readonly DroneManager _manager;
        private readonly DroneServiceProvider _serviceProvider;

        public DroneRecoveryValidator(
            DroneManager manager,
            DroneServiceProvider serviceProvider)
        {
            _manager = manager;
            _serviceProvider = serviceProvider;
        }

        public RecoveryValidationResult Validate()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (_manager != null)
            {
                var fleetReport = _manager.ValidateFleet();
                if (!fleetReport.IsValid)
                {
                    foreach (var e in fleetReport.Errors)
                    {
                        errors.Add($"[Fleet] {e}");
                    }
                }
            }
            else
            {
                errors.Add("DroneManager is null. Cannot validate restored runtime.");
            }

            if (_serviceProvider?.Validator != null)
            {
                var startupReport = _serviceProvider.Validator.Validate();
                foreach (var e in startupReport.Errors)
                {
                    errors.Add($"[Startup] {e}");
                }
                foreach (var w in startupReport.Warnings)
                {
                    warnings.Add($"[Startup] {w}");
                }
            }
            else
            {
                warnings.Add("DroneServiceProvider or Validator is null. Startup validation skipped.");
            }

            return new RecoveryValidationResult(errors.Count == 0, errors, warnings);
        }
    }
}
