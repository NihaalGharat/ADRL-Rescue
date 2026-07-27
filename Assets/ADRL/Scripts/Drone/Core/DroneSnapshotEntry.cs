namespace ADRL.Drone.Core
{
    public readonly struct DroneSnapshotEntry
    {
        public int DroneId { get; }
        public DroneRuntimeState RuntimeState { get; }
        public int ActivationCount { get; }
        public float RegistrationTime { get; }
        public float LastTransitionTime { get; }
        public int RuntimeFlags { get; }

        public DroneSnapshotEntry(
            int droneId,
            DroneRuntimeState runtimeState,
            int activationCount,
            float registrationTime,
            float lastTransitionTime,
            int runtimeFlags)
        {
            DroneId = droneId;
            RuntimeState = runtimeState;
            ActivationCount = activationCount;
            RegistrationTime = registrationTime;
            LastTransitionTime = lastTransitionTime;
            RuntimeFlags = runtimeFlags;
        }
    }
}
