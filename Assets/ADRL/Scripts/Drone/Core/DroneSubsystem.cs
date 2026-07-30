namespace ADRL.Drone.Core
{
    using System;
    using System.Collections.Generic;
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Core.Resources;
    using ADRL.Drone.Events;
    using ADRL.Drone.Interfaces;
    using ADRL.Drone.Utilities;
    using UnityEngine;

    using PoolStatsDict = System.Collections.Generic.IReadOnlyDictionary<string, ADRL.Drone.Core.PoolStatistics>;

    public class DroneSubsystem : IDroneSubsystem
    {
        private EventBus _eventBus;
        private DroneManager _manager;
        private DroneContext _context;
        private DroneRegistry _registry;
        private DroneConfiguration _configuration;
        private DroneServiceProvider _serviceProvider;
        private DroneSubsystemHealth _health = DroneSubsystemHealth.Shutdown;
        private float _initializationStartTime;
        private float _bootTime;
        private long _lastValidationTime;
        private bool _lastValidationPassed;
        private DronePersistenceManager _persistenceManager;
        private DroneRecoveryValidator _recoveryValidator;

        public DroneSubsystemHealth Health => _health;
        public DroneServiceProvider Services => _serviceProvider;

        public void Boot(EventBus eventBus)
        {
            if (_health != DroneSubsystemHealth.Shutdown)
                return;

            _eventBus = eventBus;
            _health = DroneSubsystemHealth.Initializing;
            _initializationStartTime = Time.realtimeSinceStartup;

            _eventBus.Publish(new SubsystemStartingEvent());

            try
            {
                DroneBootstrap.Boot(eventBus);
            }
            catch
            {
                _health = DroneSubsystemHealth.Faulted;
                _eventBus.Publish(new SubsystemFaultedEvent("Boot failed with exception."));
                return;
            }

            _manager = DroneBootstrap.DroneManager;
            _context = DroneBootstrap.Context;
            _registry = _manager.Registry;
            _configuration = _manager.Configuration;

            var initialDiagnostics = new DroneDiagnostics(
                DroneSubsystemHealth.Initializing,
                DroneRuntimeState.Uninitialized,
                _manager.State,
                false,
                false,
                0,
                new DroneFleetStatistics(0, 0, 0, 0, 0, 0, 0, 0),
                0,
                0f,
                0f,
                0L);

            var validator = new DroneStartupValidator(
                _manager, _context, _registry, _configuration, _health);

            var droneConfig = ResourceLocator.IsInitialized
                ? ResourceLocator.Configs.Get<DroneConfig>()
                : null;

            var spawnManager = new DroneSpawnManager();
            spawnManager.Initialize(_eventBus, _manager, droneConfig);

            var poolManager = new DronePoolManager();
            poolManager.Initialize(spawnManager.PrefabRegistry, _manager.Context.DroneRoot);
            spawnManager.SetAllocator(poolManager);

            _serviceProvider = new DroneServiceProvider(
                _manager, _registry, _context, _configuration,
                initialDiagnostics, validator, spawnManager, poolManager);

            _persistenceManager = new DronePersistenceManager();
            _recoveryValidator = new DroneRecoveryValidator(_manager, _serviceProvider);

            var report = _serviceProvider.Validator.Validate();
            _lastValidationTime = DateTime.UtcNow.Ticks;
            _lastValidationPassed = report.Passed;

            InitializeDebugDrawer();

            if (!report.Passed && report.Errors.Count > 0)
            {
                DroneBootstrap.Shutdown(_eventBus);
                _manager = null;
                _context = null;
                _registry = null;
                _configuration = null;
                _serviceProvider = null;
                _health = DroneSubsystemHealth.Faulted;
                _eventBus.Publish(new SubsystemFaultedEvent(
                    $"Startup validation failed: {string.Join("; ", report.Errors)}"));
                return;
            }

            _health = DroneSubsystemHealth.Healthy;
            _bootTime = Time.realtimeSinceStartup;

            _eventBus.Publish(new SubsystemReadyEvent());
        }

        public void Shutdown()
        {
            if (_health == DroneSubsystemHealth.Shutdown)
                return;

            GetDiagnostics();
            Validate();

            _eventBus?.Publish(new SubsystemShutdownEvent());

            if (_serviceProvider?.Allocator is DronePoolManager poolManager)
                poolManager.Shutdown();

            _serviceProvider?.SpawnManager?.ClearQueue();

            DroneBootstrap.Shutdown(_eventBus);

            _persistenceManager = null;
            _recoveryValidator = null;
            _manager = null;
            _context = null;
            _registry = null;
            _configuration = null;
            _serviceProvider = null;
            _health = DroneSubsystemHealth.Shutdown;
        }

        public void Reset()
        {
            if (_health != DroneSubsystemHealth.Healthy &&
                _health != DroneSubsystemHealth.Degraded)
                return;

            if (_manager != null)
            {
                _manager.ResetFleet();
            }
        }

        public DroneSubsystemValidationReport Validate()
        {
            _lastValidationTime = DateTime.UtcNow.Ticks;

            var errors = new List<string>();
            var warnings = new List<string>();
            var components = new List<string>();

            ValidateDependencies(errors, warnings, components);

            if (_health == DroneSubsystemHealth.Faulted)
                errors.Add("Subsystem is in Faulted state.");
            if (_health == DroneSubsystemHealth.Initializing)
                warnings.Add("Subsystem is still initializing.");
            if (_health == DroneSubsystemHealth.Shutdown)
                errors.Add("Subsystem is not booted.");

            if (_serviceProvider != null)
            {
                var startupReport = _serviceProvider.Validator.Validate();
                foreach (var e in startupReport.Errors)
                    errors.Add(e);
                foreach (var w in startupReport.Warnings)
                    warnings.Add(w);
                foreach (var c in startupReport.ValidatedComponents)
                {
                    if (!components.Contains(c))
                        components.Add(c);
                }

                if (_manager != null &&
                    _manager.State != DroneSystemState.Uninitialized)
                {
                    var fleetReport = _manager.ValidateFleet();
                    if (!fleetReport.IsValid)
                    {
                        foreach (var e in fleetReport.Errors)
                            errors.Add($"[Fleet] {e}");
                    }
                    if (!components.Contains("FleetValidator"))
                        components.Add("FleetValidator");

                    var entityErrors = DroneEntityValidator.ValidateFleetEntities(_manager);
                    foreach (var e in entityErrors)
                        errors.Add(e);
                    if (!components.Contains("EntityValidator"))
                        components.Add("EntityValidator");

                    if (_serviceProvider?.Allocator is DronePoolManager poolManager)
                    {
                        foreach (var type in poolManager.AllPoolTypes)
                        {
                            if (!components.Contains($"Pool:{type}"))
                                components.Add($"Pool:{type}");
                        }
                        if (!components.Contains("PoolManager"))
                            components.Add("PoolManager");
                        if (poolManager.TotalActiveCount > 0 || poolManager.PoolCount > 0)
                        {
                            if (!components.Contains("PoolStorage"))
                                components.Add("PoolStorage");
                        }
                    }
                }
            }

            _lastValidationPassed = errors.Count == 0;

            var report = new DroneSubsystemValidationReport(
                _lastValidationPassed,
                warnings,
                errors,
                0.0,
                components);

            _eventBus?.Publish(new SubsystemValidatedEvent(report.Passed));

            return report;
        }

        private void ValidateDependencies(
            List<string> errors,
            List<string> warnings,
            List<string> components)
        {
            if (_manager != null)
                components.Add("DroneManager");
            else
                errors.Add("DroneManager is null.");

            if (_context != null)
                components.Add("DroneContext");
            else
                errors.Add("DroneContext is null.");

            if (_registry != null)
                components.Add("DroneRegistry");
            else
                errors.Add("DroneRegistry is null.");

            if (_configuration != null)
                components.Add("DroneConfiguration");
            else
                errors.Add("DroneConfiguration is null.");

            if (_serviceProvider != null)
                components.Add("DroneServiceProvider");
            else
                errors.Add("DroneServiceProvider is null.");

            if (_eventBus != null)
                components.Add("EventBus");
            else
                errors.Add("EventBus is null.");
        }

        public DroneDiagnostics GetDiagnostics()
        {
            if (_manager == null || _context == null)
            {
                return new DroneDiagnostics(
                    _health,
                    DroneRuntimeState.Uninitialized,
                    DroneSystemState.Uninitialized,
                    _serviceProvider != null,
                    _lastValidationPassed,
                    0,
                    new DroneFleetStatistics(0, 0, 0, 0, 0, 0, 0, 0),
                    0,
                    0f,
                    0f,
                    _lastValidationTime);
            }

            var fleetStats = _manager.GetFleetStatistics();
            var uptime = _bootTime > 0f ? Time.realtimeSinceStartup - _bootTime : 0f;
            var initDuration = _bootTime > 0f ? _bootTime - _initializationStartTime : 0f;

            int pendingSpawns = 0;
            int totalPoolObjs = 0;
            int borrowCount = 0;
            int returnCount = 0;
            int missCount = 0;
            PoolStatsDict poolStatsByType = null;

            if (_serviceProvider?.Allocator is DronePoolManager poolManager)
            {
                pendingSpawns = _serviceProvider.SpawnManager?.PendingCount ?? 0;
                totalPoolObjs = poolManager.TotalPoolObjectsCount;
                borrowCount = poolManager.TotalBorrowCount;
                returnCount = poolManager.TotalReturnCount;
                missCount = poolManager.TotalPoolMissCount;
                poolStatsByType = poolManager.GetAllStatistics();
            }

            var isOperational = _health == DroneSubsystemHealth.Healthy
                             || _health == DroneSubsystemHealth.Degraded;

            var healthReport = new DroneHealthReport(
                isOperational,
                _context.RegisteredDroneCount,
                fleetStats.Active,
                fleetStats.Idle,
                borrowCount,
                returnCount,
                missCount,
                pendingSpawns,
                totalPoolObjs,
                _health.ToString());

            return new DroneDiagnostics(
                _health,
                DroneRuntimeState.Uninitialized,
                _manager.State,
                _serviceProvider != null,
                _lastValidationPassed,
                _context.RegisteredDroneCount,
                fleetStats,
                _context.EpisodeNumber,
                initDuration,
                uptime,
                _lastValidationTime,
                pendingSpawns,
                totalPoolObjs,
                borrowCount,
                returnCount,
                missCount,
                poolStatsByType,
                healthReport);
        }

        public DroneRuntimeSnapshot CreateSnapshot()
        {
            if (_health != DroneSubsystemHealth.Healthy &&
                _health != DroneSubsystemHealth.Degraded)
            {
                return default;
            }

            var snapshot = _persistenceManager.CaptureSnapshot(_manager);
            _eventBus?.Publish(new RuntimeSnapshotCreatedEvent(
                _persistenceManager.SnapshotCount - 1, snapshot.Entries.Count));
            return snapshot;
        }

        public bool RestoreSnapshot(DroneRuntimeSnapshot snapshot)
        {
            if (_health != DroneSubsystemHealth.Healthy &&
                _health != DroneSubsystemHealth.Degraded)
            {
                return false;
            }

            var policyValidation = DroneRecoveryPolicy.ValidateSnapshot(snapshot);
            if (!policyValidation.IsRecoverable)
            {
                _eventBus?.Publish(new RuntimeRestoreFailedEvent(
                    string.Join("; ", policyValidation.Errors)));
                return false;
            }

            var preSnapshot = _manager.CreateSnapshot();
            _eventBus?.Publish(new RuntimeRestoreStartedEvent());

            _manager.ResetRuntime();

            if (!_persistenceManager.RestoreSnapshot(snapshot, _manager))
            {
                _manager.RestoreSnapshot(preSnapshot);
                _eventBus?.Publish(new RuntimeRestoreFailedEvent("Restore operation failed."));
                _eventBus?.Publish(new RuntimeRestoreCompletedEvent(true));
                return false;
            }

            var recoveryReport = _recoveryValidator.Validate();
            if (!recoveryReport.IsRecoverable)
            {
                _manager.RestoreSnapshot(preSnapshot);
                _eventBus?.Publish(new RuntimeRestoreFailedEvent(
                    string.Join("; ", recoveryReport.Errors)));
                _eventBus?.Publish(new RuntimeRestoreCompletedEvent(true));
                return false;
            }

            _persistenceManager.CaptureSnapshot(_manager);
            _eventBus?.Publish(new RuntimeRestoreCompletedEvent(false));
            return true;
        }

        public void ResetRuntime()
        {
            if (_health != DroneSubsystemHealth.Healthy &&
                _health != DroneSubsystemHealth.Degraded)
            {
                return;
            }

            _manager.ResetRuntime();
        }

        public bool TryGetLatestSnapshot(out DroneRuntimeSnapshot snapshot)
        {
            if (_persistenceManager == null)
            {
                snapshot = default;
                return false;
            }

            return _persistenceManager.TryGetLatestSnapshot(out snapshot);
        }

        public void ClearSnapshots()
        {
            if (_persistenceManager == null)
                return;

            _persistenceManager.ClearSnapshots();
            _eventBus?.Publish(new RuntimeSnapshotClearedEvent());
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void InitializeDebugDrawer()
        {
            if (_manager == null) return;
            var drawer = _manager.gameObject.GetComponent<DroneDebugDrawer>();
            if (drawer == null)
                drawer = _manager.gameObject.AddComponent<DroneDebugDrawer>();
            drawer.Initialize(this);
        }

        public DroneSubsystemValidationReport ValidateRestore()
        {
            if (_health == DroneSubsystemHealth.Shutdown)
            {
                var errors = new List<string> { "Subsystem is not booted." };
                return new DroneSubsystemValidationReport(
                    false, new List<string>(), errors, 0.0, new List<string>());
            }

            if (_recoveryValidator == null)
            {
                var errors = new List<string> { "Recovery validator is not initialized." };
                return new DroneSubsystemValidationReport(
                    false, new List<string>(), errors, 0.0, new List<string>());
            }

            var report = _recoveryValidator.Validate();
            var warnings = new List<string>(report.Warnings);
            var errorList = new List<string>(report.Errors);

            return new DroneSubsystemValidationReport(
                report.IsRecoverable, warnings, errorList, 0.0, new List<string>());
        }
    }
}
