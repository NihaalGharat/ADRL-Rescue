namespace ADRL.Drone.Core
{
    using System;
    using System.Collections.Generic;
    using ADRL.Core.Events;
    using ADRL.Drone.Controllers;
    using ADRL.Drone.Events;
    using ADRL.Drone.Utilities;
    using UnityEngine;

    public class DroneManager : MonoBehaviour
    {
        private EventBus _eventBus;
        private DroneContext _context;
        private DroneRegistry _registry;
        private DroneConfiguration _configuration;
        private DroneSystemState _state = DroneSystemState.Uninitialized;

        private int _nextDroneId = 1;
        private Dictionary<int, DroneRuntimeInfo> _runtimeInfos = new();

        public DroneContext Context => _context;
        public DroneRegistry Registry => _registry;
        public DroneConfiguration Configuration => _configuration;
        public DroneSystemState State => _state;

        public void Initialize(
            EventBus eventBus,
            DroneContext context,
            DroneRegistry registry,
            DroneConfiguration configuration)
        {
            _eventBus = eventBus;
            _context = context;
            _registry = registry;
            _configuration = configuration;

            DroneLifecyclePolicy.Validate(_state, DroneSystemState.Initializing);
            _state = DroneSystemState.Initializing;
            _context.FleetState = _state;

            DroneLifecyclePolicy.Validate(_state, DroneSystemState.Ready);
            _state = DroneSystemState.Ready;
            _context.FleetState = _state;

            _eventBus?.Publish(new FleetStateChangedEvent(DroneSystemState.Initializing, DroneSystemState.Ready));
            _eventBus?.Publish(new DroneInitializedEvent());
        }

        public void Shutdown()
        {
            DroneLifecyclePolicy.Validate(_state, DroneSystemState.Uninitialized);

            _registry?.Clear();
            _runtimeInfos?.Clear();
            _context?.Reset();
            _eventBus = null;
            _context = null;
            _registry = null;
            _configuration = null;
            _runtimeInfos = null;
            _state = DroneSystemState.Uninitialized;
        }

        public int RegisterDrone(DroneController controller)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            if (_state != DroneSystemState.Ready && _state != DroneSystemState.Running)
                throw new InvalidOperationException($"Cannot register drone in state {_state}.");

            var droneId = AllocateDroneId();
            return RegisterDrone(droneId, controller);
        }

        public int RegisterDrone(int droneId, DroneController controller)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));

            if (_registry.Contains(droneId))
                throw new InvalidOperationException($"Drone ID {droneId} is already registered.");

            if (_state != DroneSystemState.Ready && _state != DroneSystemState.Running)
                throw new InvalidOperationException($"Cannot register drone in state {_state}.");

            _registry.Register(droneId, controller);

            var runtimeInfo = new DroneRuntimeInfo(
                droneId,
                DroneRuntimeState.Registered,
                Time.time,
                _context.EpisodeNumber,
                controller);
            _runtimeInfos[droneId] = runtimeInfo;

            _context.RegisteredDroneCount = _registry.Count;
            _context.TotalRegistered++;
            _context.NextAvailableId = _nextDroneId;

            _eventBus?.Publish(new DroneRuntimeStateChangedEvent(
                droneId, DroneRuntimeState.Uninitialized, DroneRuntimeState.Registered));
            _eventBus?.Publish(new DroneRegisteredEvent(droneId));
            return droneId;
        }

        public bool UnregisterDrone(int droneId)
        {
            if (!_registry.Contains(droneId))
                throw new InvalidOperationException($"Drone ID {droneId} is not registered.");

            if (_state != DroneSystemState.Ready && _state != DroneSystemState.Running)
                throw new InvalidOperationException($"Cannot unregister drone in state {_state}.");

            if (_runtimeInfos.TryGetValue(droneId, out var info))
            {
                UpdateStateCounter(info.RuntimeState, -1);
            }

            _registry.Unregister(droneId);
            _runtimeInfos.Remove(droneId);
            _context.RegisteredDroneCount = _registry.Count;
            _eventBus?.Publish(new DroneUnregisteredEvent(droneId));
            return true;
        }

        public int AllocateDroneId()
        {
            if (_state != DroneSystemState.Ready && _state != DroneSystemState.Running)
                throw new InvalidOperationException($"Cannot allocate ID in state {_state}.");

            var id = _nextDroneId;
            _nextDroneId++;
            return id;
        }

        public void TransitionRuntimeState(int droneId, DroneRuntimeState newState)
        {
            if (_state != DroneSystemState.Ready && _state != DroneSystemState.Running)
                throw new InvalidOperationException(
                    $"Cannot transition drone in system state {_state}.");

            if (!_runtimeInfos.TryGetValue(droneId, out var info))
                throw new InvalidOperationException(
                    $"Drone {droneId} not found in runtime store.");

            var oldState = info.RuntimeState;
            DroneStateTransitionValidator.Validate(oldState, newState);

            _eventBus?.Publish(new DroneRuntimeStateChangingEvent(droneId, oldState, newState));

            info.RuntimeState = newState;
            info.LastTransitionTime = Time.time;
            if (newState == DroneRuntimeState.Active)
                info.ActivationCount++;

            UpdateStateCounter(oldState, -1);
            UpdateStateCounter(newState, 1);

            _eventBus?.Publish(new DroneRuntimeStateChangedEvent(droneId, oldState, newState));
        }

        private void UpdateStateCounter(DroneRuntimeState state, int delta)
        {
            switch (state)
            {
                case DroneRuntimeState.Active:
                    _context.ActiveDroneCount = Math.Max(0, _context.ActiveDroneCount + delta);
                    break;
                case DroneRuntimeState.Destroyed:
                    _context.DestroyedCount = Math.Max(0, _context.DestroyedCount + delta);
                    break;
                case DroneRuntimeState.Registered:
                case DroneRuntimeState.Idle:
                case DroneRuntimeState.Paused:
                case DroneRuntimeState.Returning:
                case DroneRuntimeState.Shutdown:
                case DroneRuntimeState.Initializing:
                    _context.InactiveCount = Math.Max(0, _context.InactiveCount + delta);
                    break;
            }
        }

        public DroneRuntimeInfo GetRuntimeInfo(int droneId)
        {
            _runtimeInfos.TryGetValue(droneId, out var info);
            return info;
        }

        public DroneFleetStatistics GetFleetStatistics()
        {
            var registered = 0;
            var active = 0;
            var idle = 0;
            var paused = 0;
            var returning = 0;
            var destroyed = 0;
            var initializing = 0;

            foreach (var info in _runtimeInfos.Values)
            {
                registered++;
                switch (info.RuntimeState)
                {
                    case DroneRuntimeState.Active: active++; break;
                    case DroneRuntimeState.Idle: idle++; break;
                    case DroneRuntimeState.Paused: paused++; break;
                    case DroneRuntimeState.Returning: returning++; break;
                    case DroneRuntimeState.Destroyed: destroyed++; break;
                    case DroneRuntimeState.Initializing: initializing++; break;
                }
            }

            return new DroneFleetStatistics(
                registered, active, idle, paused, returning, destroyed, initializing,
                _context.TotalRegistered);
        }

        public FleetValidationResult ValidateFleet()
        {
            return DroneFleetValidator.Validate(
                _registry, _context, _runtimeInfos, _state, _nextDroneId);
        }

        public void ResetFleet()
        {
            if (!DroneLifecyclePolicy.CanReset(_state))
                return;

            var previousState = _state;
            DroneLifecyclePolicy.Validate(previousState, DroneSystemState.Resetting);
            _eventBus?.Publish(new FleetResettingEvent(_context.EpisodeNumber));
            _eventBus?.Publish(new FleetStateChangedEvent(previousState, DroneSystemState.Resetting));

            _state = DroneSystemState.Resetting;
            _context.FleetState = _state;

            _registry.Clear();
            _runtimeInfos.Clear();
            _nextDroneId = 1;
            _context.RegisteredDroneCount = 0;
            _context.ActiveDroneCount = 0;
            _context.TotalRegistered = 0;
            _context.InactiveCount = 0;
            _context.DestroyedCount = 0;
            _context.EpisodeNumber = 0;
            _context.NextAvailableId = 1;

            DroneLifecyclePolicy.Validate(_state, DroneSystemState.Ready);
            _state = DroneSystemState.Ready;
            _context.FleetState = _state;

            _eventBus?.Publish(new FleetStateChangedEvent(DroneSystemState.Resetting, DroneSystemState.Ready));
            _eventBus?.Publish(new FleetResetCompletedEvent(_context.EpisodeNumber));
        }

        public DroneController GetDrone(int droneId)
        {
            return _registry.Get(droneId);
        }

        public bool ContainsDrone(int droneId)
        {
            return _registry.Contains(droneId);
        }

        public int GetRegisteredDroneCount()
        {
            return _registry.Count;
        }

        public int GetActiveDroneCount()
        {
            return _context.ActiveDroneCount;
        }

        public int GetInactiveDroneCount()
        {
            return _context.InactiveCount;
        }

        public FleetSnapshot GetFleetSnapshot()
        {
            int active = 0, idle = 0, paused = 0, returning = 0,
                destroyed = 0, initializing = 0, shutdown = 0;

            foreach (var info in _runtimeInfos.Values)
            {
                switch (info.RuntimeState)
                {
                    case DroneRuntimeState.Active: active++; break;
                    case DroneRuntimeState.Idle: idle++; break;
                    case DroneRuntimeState.Paused: paused++; break;
                    case DroneRuntimeState.Returning: returning++; break;
                    case DroneRuntimeState.Destroyed: destroyed++; break;
                    case DroneRuntimeState.Initializing: initializing++; break;
                    case DroneRuntimeState.Shutdown: shutdown++; break;
                }
            }

            return new FleetSnapshot(
                _registry.Count,
                _context.TotalRegistered,
                _context.ActiveDroneCount,
                _context.InactiveCount,
                _context.DestroyedCount,
                _nextDroneId,
                _state,
                idle,
                paused,
                returning,
                initializing,
                shutdown);
        }

        public DroneRuntimeSnapshot CreateSnapshot()
        {
            if (_runtimeInfos == null)
            {
                return new DroneRuntimeSnapshot(
                    0, 0L, 0, _state, 0, 0, 1,
                    new List<DroneSnapshotEntry>().AsReadOnly(),
                    new DroneFleetStatistics(0, 0, 0, 0, 0, 0, 0, 0));
            }

            var entries = new List<DroneSnapshotEntry>(_runtimeInfos.Count);

            foreach (var info in _runtimeInfos.Values)
            {
                entries.Add(new DroneSnapshotEntry(
                    info.DroneId,
                    info.RuntimeState,
                    info.ActivationCount,
                    info.RegistrationTime,
                    info.LastTransitionTime,
                    info.RuntimeFlags));
            }

            return new DroneRuntimeSnapshot(
                DroneRuntimeSnapshot.CurrentVersion,
                DateTime.UtcNow.Ticks,
                _context?.EpisodeNumber ?? 0,
                _state,
                _context?.RegisteredDroneCount ?? 0,
                _context?.TotalRegistered ?? 0,
                _nextDroneId,
                entries.AsReadOnly(),
                GetFleetStatistics());
        }

        public bool RestoreSnapshot(DroneRuntimeSnapshot snapshot)
        {
            if (_state != DroneSystemState.Ready && _state != DroneSystemState.Running)
                return false;

            if (_runtimeInfos == null || _context == null)
                return false;

            _runtimeInfos.Clear();
            _registry.Clear();

            foreach (var entry in snapshot.Entries)
            {
                var runtimeInfo = new DroneRuntimeInfo(
                    entry.DroneId,
                    entry.RuntimeState,
                    entry.RegistrationTime,
                    snapshot.EpisodeNumber,
                    null);
                runtimeInfo.ActivationCount = entry.ActivationCount;
                runtimeInfo.LastTransitionTime = entry.LastTransitionTime;
                runtimeInfo.RuntimeFlags = entry.RuntimeFlags;
                _runtimeInfos[entry.DroneId] = runtimeInfo;
            }

            _context.RegisteredDroneCount = snapshot.RegisteredDroneCount;
            _context.TotalRegistered = snapshot.TotalRegistered;
            _context.NextAvailableId = snapshot.NextAvailableId;
            _context.EpisodeNumber = snapshot.EpisodeNumber;
            _context.FleetState = snapshot.FleetRuntimeState;
            _nextDroneId = snapshot.NextAvailableId;

            RecalculateStateCounters();
            return true;
        }

        public void ResetRuntime()
        {
            if (_runtimeInfos == null || _context == null)
                return;

            _runtimeInfos.Clear();
            _context.RegisteredDroneCount = 0;
            _context.ActiveDroneCount = 0;
            _context.InactiveCount = 0;
            _context.DestroyedCount = 0;
            _context.TotalRegistered = 0;
        }

        public void ClearRuntime()
        {
            ResetRuntime();
            _nextDroneId = 1;
            _context.NextAvailableId = 1;
            _context.EpisodeNumber = 0;
        }

        private void RecalculateStateCounters()
        {
            int active = 0, inactive = 0, destroyed = 0;

            foreach (var info in _runtimeInfos.Values)
            {
                switch (info.RuntimeState)
                {
                    case DroneRuntimeState.Active:
                        active++;
                        break;
                    case DroneRuntimeState.Destroyed:
                        destroyed++;
                        break;
                    default:
                        inactive++;
                        break;
                }
            }

            _context.ActiveDroneCount = active;
            _context.InactiveCount = inactive;
            _context.DestroyedCount = destroyed;
        }

        private void OnDestroy()
        {
            _registry?.Clear();
            _runtimeInfos?.Clear();
            _context?.Reset();
            _state = DroneSystemState.Uninitialized;
        }
    }

    public readonly struct FleetSnapshot
    {
        public int RegisteredCount { get; }
        public int TotalRegistered { get; }
        public int ActiveCount { get; }
        public int InactiveCount { get; }
        public int DestroyedCount { get; }
        public int NextAvailableId { get; }
        public DroneSystemState SystemState { get; }

        public int IdleCount { get; }
        public int PausedCount { get; }
        public int ReturningCount { get; }
        public int InitializingCount { get; }
        public int ShutdownCount { get; }

        public FleetSnapshot(
            int registeredCount,
            int totalRegistered,
            int activeCount,
            int inactiveCount,
            int destroyedCount,
            int nextAvailableId,
            DroneSystemState systemState,
            int idleCount,
            int pausedCount,
            int returningCount,
            int initializingCount,
            int shutdownCount)
        {
            RegisteredCount = registeredCount;
            TotalRegistered = totalRegistered;
            ActiveCount = activeCount;
            InactiveCount = inactiveCount;
            DestroyedCount = destroyedCount;
            NextAvailableId = nextAvailableId;
            SystemState = systemState;
            IdleCount = idleCount;
            PausedCount = pausedCount;
            ReturningCount = returningCount;
            InitializingCount = initializingCount;
            ShutdownCount = shutdownCount;
        }
    }
}
