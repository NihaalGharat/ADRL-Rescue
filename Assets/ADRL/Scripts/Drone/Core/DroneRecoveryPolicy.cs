namespace ADRL.Drone.Core
{
    using System.Collections.Generic;

    public readonly struct RecoveryValidationResult
    {
        public bool IsRecoverable { get; }
        public IReadOnlyList<string> Errors { get; }
        public IReadOnlyList<string> Warnings { get; }

        public RecoveryValidationResult(
            bool isRecoverable,
            IReadOnlyList<string> errors,
            IReadOnlyList<string> warnings)
        {
            IsRecoverable = isRecoverable;
            Errors = errors;
            Warnings = warnings;
        }
    }

    public static class DroneRecoveryPolicy
    {
        public static bool CanRecover(DroneRuntimeSnapshot snapshot)
        {
            return ValidateSnapshot(snapshot).IsRecoverable;
        }

        public static RecoveryValidationResult ValidateSnapshot(DroneRuntimeSnapshot snapshot)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (snapshot.SnapshotVersion != DroneRuntimeSnapshot.CurrentVersion)
            {
                errors.Add(
                    $"Snapshot version {snapshot.SnapshotVersion} is not supported. " +
                    $"Expected version {DroneRuntimeSnapshot.CurrentVersion}.");
            }

            if (snapshot.Entries == null)
            {
                errors.Add("Snapshot entries collection is null.");
            }
            else
            {
                var seenIds = new HashSet<int>();

                foreach (var entry in snapshot.Entries)
                {
                    if (entry.DroneId <= 0)
                    {
                        errors.Add($"Entry has invalid DroneId ({entry.DroneId}).");
                    }

                    if (entry.RuntimeState == DroneRuntimeState.Uninitialized)
                    {
                        errors.Add(
                            $"Drone {entry.DroneId} has Uninitialized runtime state in snapshot.");
                    }

                    if (!seenIds.Add(entry.DroneId))
                    {
                        errors.Add($"Duplicate drone ID {entry.DroneId} in snapshot.");
                    }

                    if (entry.RegistrationTime < 0f)
                    {
                        warnings.Add(
                            $"Drone {entry.DroneId} has negative RegistrationTime.");
                    }
                }

                if (snapshot.RegisteredDroneCount != snapshot.Entries.Count)
                {
                    errors.Add(
                        $"RegisteredDroneCount ({snapshot.RegisteredDroneCount}) does not " +
                        $"match entry count ({snapshot.Entries.Count}).");
                }
            }

            if (snapshot.EpisodeNumber < 0)
            {
                errors.Add($"EpisodeNumber is negative ({snapshot.EpisodeNumber}).");
            }

            if (snapshot.TotalRegistered < 0)
            {
                errors.Add($"TotalRegistered is negative ({snapshot.TotalRegistered}).");
            }

            if (snapshot.TotalRegistered < snapshot.RegisteredDroneCount)
            {
                errors.Add(
                    $"TotalRegistered ({snapshot.TotalRegistered}) is less than " +
                    $"RegisteredDroneCount ({snapshot.RegisteredDroneCount}).");
            }

            if (snapshot.NextAvailableId <= 0)
            {
                errors.Add($"NextAvailableId is {snapshot.NextAvailableId}. Must be positive.");
            }

            if (snapshot.NextAvailableId < snapshot.RegisteredDroneCount)
            {
                errors.Add(
                    $"NextAvailableId ({snapshot.NextAvailableId}) is less than " +
                    $"RegisteredDroneCount ({snapshot.RegisteredDroneCount}).");
            }

            return new RecoveryValidationResult(errors.Count == 0, errors, warnings);
        }
    }
}
