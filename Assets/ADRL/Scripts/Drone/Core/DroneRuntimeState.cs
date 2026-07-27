namespace ADRL.Drone.Core
{
    public enum DroneRuntimeState
    {
        Uninitialized,
        Registered,
        Initializing,
        Idle,
        Active,
        Paused,
        Returning,
        Shutdown,
        Destroyed
    }
}
