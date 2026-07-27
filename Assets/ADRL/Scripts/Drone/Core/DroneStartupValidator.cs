namespace ADRL.Drone.Core
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using ADRL.Drone.Interfaces;

    public class DroneStartupValidator : IDroneValidator
    {
        private readonly DroneManager _manager;
        private readonly DroneContext _context;
        private readonly DroneRegistry _registry;
        private readonly DroneConfiguration _configuration;
        private readonly DroneSubsystemHealth _health;

        public DroneStartupValidator(
            DroneManager manager,
            DroneContext context,
            DroneRegistry registry,
            DroneConfiguration configuration,
            DroneSubsystemHealth health)
        {
            _manager = manager;
            _context = context;
            _registry = registry;
            _configuration = configuration;
            _health = health;
        }

        public DroneSubsystemValidationReport Validate()
        {
            var sw = Stopwatch.StartNew();
            var warnings = new List<string>();
            var errors = new List<string>();
            var components = new List<string>();

            ValidateManager(errors, warnings, components);
            ValidateContext(errors, warnings, components);
            ValidateRegistry(errors, warnings, components);
            ValidateConfiguration(errors, warnings, components);
            ValidateSubsystemHealth(errors, warnings, components);
            ValidateDependencies(errors, warnings, components);

            sw.Stop();
            return new DroneSubsystemValidationReport(
                errors.Count == 0,
                warnings,
                errors,
                sw.Elapsed.TotalMilliseconds,
                components);
        }

        private void ValidateManager(
            List<string> errors,
            List<string> warnings,
            List<string> components)
        {
            components.Add("DroneManager");

            if (_manager == null)
            {
                errors.Add("DroneManager is null.");
                return;
            }

            if (_manager.State == DroneSystemState.Uninitialized)
            {
                errors.Add("DroneManager is in Uninitialized state. Boot must be called first.");
            }
        }

        private void ValidateContext(
            List<string> errors,
            List<string> warnings,
            List<string> components)
        {
            components.Add("DroneContext");

            if (_context == null)
            {
                errors.Add("DroneContext is null.");
                return;
            }

            if (_context.RegisteredDroneCount < 0)
                errors.Add($"RegisteredDroneCount is negative ({_context.RegisteredDroneCount}).");

            if (_context.ActiveDroneCount < 0)
                errors.Add($"ActiveDroneCount is negative ({_context.ActiveDroneCount}).");

            if (_context.InactiveCount < 0)
                errors.Add($"InactiveCount is negative ({_context.InactiveCount}).");

            if (_context.DestroyedCount < 0)
                errors.Add($"DestroyedCount is negative ({_context.DestroyedCount}).");

            if (_context.TotalRegistered < 0)
                errors.Add($"TotalRegistered is negative ({_context.TotalRegistered}).");
        }

        private void ValidateRegistry(
            List<string> errors,
            List<string> warnings,
            List<string> components)
        {
            components.Add("DroneRegistry");

            if (_registry == null)
            {
                errors.Add("DroneRegistry is null.");
                return;
            }
        }

        private void ValidateConfiguration(
            List<string> errors,
            List<string> warnings,
            List<string> components)
        {
            components.Add("DroneConfiguration");

            if (_configuration == null)
            {
                errors.Add("DroneConfiguration is null.");
                return;
            }

            if (_configuration.MaxFleetSize <= 0)
                errors.Add($"MaxFleetSize must be positive (current: {_configuration.MaxFleetSize}).");

            if (_configuration.DefaultSpawnCount <= 0)
                warnings.Add($"DefaultSpawnCount is {_configuration.DefaultSpawnCount}. No drones will spawn by default.");
        }

        private void ValidateSubsystemHealth(
            List<string> errors,
            List<string> warnings,
            List<string> components)
        {
            components.Add("DroneSubsystem");

            if (_health == DroneSubsystemHealth.Faulted)
                errors.Add("Subsystem is in Faulted state.");

            if (_health == DroneSubsystemHealth.Shutdown)
                errors.Add("Subsystem is Shutdown. Boot must be called.");
        }

        private void ValidateDependencies(
            List<string> errors,
            List<string> warnings,
            List<string> components)
        {
            components.Add("Dependencies");
        }
    }
}
