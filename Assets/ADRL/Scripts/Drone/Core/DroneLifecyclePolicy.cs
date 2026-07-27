namespace ADRL.Drone.Core
{
    using System;
    using System.Collections.Generic;

    public static class DroneLifecyclePolicy
    {
        private static readonly HashSet<(DroneSystemState, DroneSystemState)> AllowedTransitions = new()
        {
            (DroneSystemState.Uninitialized, DroneSystemState.Initializing),
            (DroneSystemState.Initializing, DroneSystemState.Ready),
            (DroneSystemState.Ready, DroneSystemState.Running),
            (DroneSystemState.Running, DroneSystemState.Completed),
            (DroneSystemState.Running, DroneSystemState.Failed),
            (DroneSystemState.Ready, DroneSystemState.Resetting),
            (DroneSystemState.Running, DroneSystemState.Resetting),
            (DroneSystemState.Resetting, DroneSystemState.Ready),
            (DroneSystemState.Initializing, DroneSystemState.Uninitialized),
            (DroneSystemState.Ready, DroneSystemState.Uninitialized),
            (DroneSystemState.Running, DroneSystemState.Uninitialized),
            (DroneSystemState.Resetting, DroneSystemState.Uninitialized),
            (DroneSystemState.Completed, DroneSystemState.Uninitialized),
            (DroneSystemState.Failed, DroneSystemState.Uninitialized),
        };

        public static bool IsAllowed(DroneSystemState from, DroneSystemState to)
        {
            return AllowedTransitions.Contains((from, to));
        }

        public static void Validate(DroneSystemState from, DroneSystemState to)
        {
            if (!IsAllowed(from, to))
            {
                throw new InvalidOperationException(
                    $"Invalid system lifecycle transition: {from} → {to}");
            }
        }

        public static bool CanBoot(DroneSystemState state)
        {
            return state == DroneSystemState.Uninitialized;
        }

        public static bool CanInitialize(DroneSystemState state)
        {
            return state == DroneSystemState.Initializing;
        }

        public static bool CanRun(DroneSystemState state)
        {
            return state == DroneSystemState.Ready;
        }

        public static bool CanReset(DroneSystemState state)
        {
            return state == DroneSystemState.Ready || state == DroneSystemState.Running;
        }

        public static bool CanShutdown(DroneSystemState state)
        {
            return state != DroneSystemState.Uninitialized;
        }
    }
}
