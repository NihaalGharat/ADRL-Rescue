namespace ADRL.Drone.Core
{
    using System;
    using System.Collections.Generic;

    public static class DroneStateTransitionValidator
    {
        private static readonly HashSet<(DroneRuntimeState, DroneRuntimeState)> _allowedTransitions = new()
        {
            (DroneRuntimeState.Registered, DroneRuntimeState.Initializing),
            (DroneRuntimeState.Initializing, DroneRuntimeState.Idle),
            (DroneRuntimeState.Idle, DroneRuntimeState.Active),
            (DroneRuntimeState.Active, DroneRuntimeState.Idle),
            (DroneRuntimeState.Active, DroneRuntimeState.Paused),
            (DroneRuntimeState.Paused, DroneRuntimeState.Active),
            (DroneRuntimeState.Active, DroneRuntimeState.Returning),
            (DroneRuntimeState.Returning, DroneRuntimeState.Shutdown),
            (DroneRuntimeState.Shutdown, DroneRuntimeState.Destroyed),
            (DroneRuntimeState.Active, DroneRuntimeState.Destroyed),
            (DroneRuntimeState.Idle, DroneRuntimeState.Destroyed),
            (DroneRuntimeState.Paused, DroneRuntimeState.Destroyed),
            (DroneRuntimeState.Returning, DroneRuntimeState.Destroyed),
            (DroneRuntimeState.Initializing, DroneRuntimeState.Destroyed),
            (DroneRuntimeState.Active, DroneRuntimeState.Registered),
            (DroneRuntimeState.Idle, DroneRuntimeState.Registered),
            (DroneRuntimeState.Paused, DroneRuntimeState.Registered),
        };

        public static bool IsValid(DroneRuntimeState from, DroneRuntimeState to)
        {
            return _allowedTransitions.Contains((from, to));
        }

        public static void Validate(DroneRuntimeState from, DroneRuntimeState to)
        {
            if (!IsValid(from, to))
            {
                throw new InvalidOperationException(
                    $"Invalid runtime state transition: {from} → {to}");
            }
        }
    }
}
