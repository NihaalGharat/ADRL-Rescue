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
        public static FleetValidationResult Validate(
            DroneRegistry registry,
            DroneContext context,
            IReadOnlyDictionary<int, DroneRuntimeInfo> runtimeInfos,
            DroneSystemState systemState,
            int nextAvailableId)
        {
            var buffer = new List<string>();

            ValidateRuntimeStore(runtimeInfos, buffer);
            ValidateRegistryConsistency(registry, runtimeInfos, buffer);
            ValidateContextCounters(context, runtimeInfos, buffer);
            ValidateSnapshotConsistency(context, systemState, nextAvailableId, buffer);

            return new FleetValidationResult(buffer.Count == 0, buffer);
        }

        private static void ValidateRuntimeStore(
            IReadOnlyDictionary<int, DroneRuntimeInfo> runtimeInfos,
            List<string> buffer)
        {
            foreach (var kvp in runtimeInfos)
            {
                if (kvp.Value.RuntimeState == DroneRuntimeState.Uninitialized)
                {
                    buffer.Add(
                        $"Drone {kvp.Key} has Uninitialized runtime state after registration.");
                }

                if (kvp.Value.DroneId != kvp.Key)
                {
                    buffer.Add(
                        $"Runtime info key mismatch: key={kvp.Key}, id={kvp.Value.DroneId}");
                }
            }
        }

        private static void ValidateRegistryConsistency(
            DroneRegistry registry,
            IReadOnlyDictionary<int, DroneRuntimeInfo> runtimeInfos,
            List<string> buffer)
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
                    buffer.Add(
                        $"Drone {kvp.Key} has runtime info but is not in registry.");
                }
            }

            foreach (var id in registryIds)
            {
                if (!runtimeInfos.ContainsKey(id))
                {
                    buffer.Add(
                        $"Drone {id} is in registry but has no runtime info.");
                }
            }
        }

        private static void ValidateContextCounters(
            DroneContext context,
            IReadOnlyDictionary<int, DroneRuntimeInfo> runtimeInfos,
            List<string> buffer)
        {
            if (context.RegisteredDroneCount < 0)
                buffer.Add($"RegisteredDroneCount is negative ({context.RegisteredDroneCount}).");

            if (context.ActiveDroneCount < 0)
                buffer.Add($"ActiveDroneCount is negative ({context.ActiveDroneCount}).");

            if (context.InactiveCount < 0)
                buffer.Add($"InactiveCount is negative ({context.InactiveCount}).");

            if (context.DestroyedCount < 0)
                buffer.Add($"DestroyedCount is negative ({context.DestroyedCount}).");

            if (context.TotalRegistered < 0)
                buffer.Add($"TotalRegistered is negative ({context.TotalRegistered}).");

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
                buffer.Add(
                    $"ActiveDroneCount mismatch: context={context.ActiveDroneCount}, computed={activeCount}");
            }

            if (context.DestroyedCount != destroyedCount)
            {
                buffer.Add(
                    $"DestroyedCount mismatch: context={context.DestroyedCount}, computed={destroyedCount}");
            }
        }

        private static void ValidateSnapshotConsistency(
            DroneContext context,
            DroneSystemState systemState,
            int nextAvailableId,
            List<string> buffer)
        {
            if (context.NextAvailableId != nextAvailableId)
            {
                buffer.Add(
                    $"NextAvailableId mismatch: context={context.NextAvailableId}, manager={nextAvailableId}");
            }

            if (context.FleetState != systemState)
            {
                buffer.Add(
                    $"FleetState mismatch: context={context.FleetState}, manager={systemState}");
            }
        }
    }
}
