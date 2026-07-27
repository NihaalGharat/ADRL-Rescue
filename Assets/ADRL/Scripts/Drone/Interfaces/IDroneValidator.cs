namespace ADRL.Drone.Interfaces
{
    using ADRL.Drone.Core;

    public interface IDroneValidator
    {
        DroneSubsystemValidationReport Validate();
    }
}
