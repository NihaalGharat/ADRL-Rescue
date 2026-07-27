namespace ADRL.Drone.Utilities
{
    using System.Collections.Generic;
    using ADRL.Drone.Controllers;
    using ADRL.Drone.Core;

    public readonly struct FleetValidationResult
    {
        public bool IsValid { get; }
        public IReadOnlyList<string> Errors { get; }

        public FleetValidationResult(bool isValid, IReadOnlyList<string> errors)
        {
            IsValid = isValid;
            Errors = errors;
        }
    }

    public static class DroneFleetValidator
    {
        private static readonly List<string> _buffer = new();

        public static FleetValidationResult Validate(
            DroneRegistry registry,
            DroneContext context,
            IReadOnlyDictionary<int, DroneRuntimeInfo> runtimeInfos,
            DroneSystemState systemState,
            int nextAvailableId)
        {
            _buffer.Clear();

            ValidateRuntimeStore(runtimeInfos);
            ValidateRegistryConsistency(registry, runtimeInfos);
            ValidateContextCounters(context, runtimeInfos);
            ValidateSnapshotConsistency(context, systemState, nextAvailableId);

            var errors = _buffer.Count > 0 ? new List<string>(_buffer) : new List<string>();
            return new FleetValidationResult(errors.Count == 0, errors);
        }

        private static void ValidateRuntimeStore(
            IReadOnlyDictionary<int, DroneRuntimeInfo> runtimeInfos)
        {
            foreach (var kvp in runtimeInfos)
            {
                if (kvp.Value.RuntimeState == DroneRuntimeState.Uninitialized)
                {
                    _buffer.Add(
                        $"Drone {kvp.Key} has Uninitialized runtime state after registration.");
                }

                if (kvp.Value.DroneId != kvp.Key)
                {
                    _buffer.Add(
                        $"Runtime info key mismatch: key={kvp.Key}, id={kvp.Value.DroneId}");
                }
            }
        }

        private static void ValidateRegistryConsistency(
            DroneRegistry registry,
            IReadOnlyDictionary<int, DroneRuntimeInfo> runtimeInfos)
        {
            var registryIds = new HashSet<int>();

            foreach (var controller in registry.GetAll())
            {
                registryIds.Add(controller.DroneId);
            }

            foreach (var kvp in runtimeInfos)
            {
                if (!registry.Contains(kvp.Key))
                {
                    _buffer.Add(
                        $"Drone {kvp.Key} has runtime info but is not in registry.");
                }
            }

            foreach (var id in registryIds)
            {
                if (!runtimeInfos.ContainsKey(id))
                {
                    _buffer.Add(
                        $"Drone {id} is in registry but has no runtime info.");
                }
            }
        }

        private static void ValidateContextCounters(
            DroneContext context,
            IReadOnlyDictionary<int, DroneRuntimeInfo> runtimeInfos)
        {
            if (context.RegisteredDroneCount < 0)
                _buffer.Add($"RegisteredDroneCount is negative ({context.RegisteredDroneCount}).");

            if (context.ActiveDroneCount < 0)
                _buffer.Add($"ActiveDroneCount is negative ({context.ActiveDroneCount}).");

            if (context.InactiveCount < 0)
                _buffer.Add($"InactiveCount is negative ({context.InactiveCount}).");

            if (context.DestroyedCount < 0)
                _buffer.Add($"DestroyedCount is negative ({context.DestroyedCount}).");

            if (context.TotalRegistered < 0)
                _buffer.Add($"TotalRegistered is negative ({context.TotalRegistered}).");

            var activeCount = 0;
            var destroyedCount = 0;

            foreach (var info in runtimeInfos.Values)
            {
                switch (info.RuntimeState)
                {
                    case DroneRuntimeState.Active:
                        activeCount++;
                        break;
                    case DroneRuntimeState.Destroyed:
                        destroyedCount++;
                        break;
                }
            }

            if (context.ActiveDroneCount != activeCount)
            {
                _buffer.Add(
                    $"ActiveDroneCount mismatch: context={context.ActiveDroneCount}, computed={activeCount}");
            }

            if (context.DestroyedCount != destroyedCount)
            {
                _buffer.Add(
                    $"DestroyedCount mismatch: context={context.DestroyedCount}, computed={destroyedCount}");
            }
        }

        private static void ValidateSnapshotConsistency(
            DroneContext context,
            DroneSystemState systemState,
            int nextAvailableId)
        {
            if (context.NextAvailableId != nextAvailableId)
            {
                _buffer.Add(
                    $"NextAvailableId mismatch: context={context.NextAvailableId}, manager={nextAvailableId}");
            }

            if (context.FleetState != systemState)
            {
                _buffer.Add(
                    $"FleetState mismatch: context={context.FleetState}, manager={systemState}");
            }
        }
    }
}
