namespace ADRL.Drone.Interfaces
{
    using ADRL.Core.Events;
    using ADRL.Drone.Core;

    public interface IDroneSubsystem
    {
        DroneSubsystemHealth Health { get; }
        void Boot(EventBus eventBus);
        void Shutdown();
        void Reset();
        DroneSubsystemValidationReport Validate();
        DroneDiagnostics GetDiagnostics();
    }
}
