namespace ADRL.Drone.Core
{
    using ADRL.Drone.Controllers;

    public class DroneRuntimeInfo
    {
        public int DroneId { get; }
        public DroneRuntimeState RuntimeState { get; set; }
        public float RegistrationTime { get; }
        public int ActivationCount { get; set; }
        public int EpisodeNumber { get; }
        public float LastTransitionTime { get; set; }
        public int RuntimeFlags { get; set; }
        public DroneController ControllerReference { get; }

        public DroneRuntimeInfo(
            int droneId,
            DroneRuntimeState state,
            float registrationTime,
            int episodeNumber,
            DroneController controller)
        {
            DroneId = droneId;
            RuntimeState = state;
            RegistrationTime = registrationTime;
            ActivationCount = 0;
            EpisodeNumber = episodeNumber;
            LastTransitionTime = registrationTime;
            RuntimeFlags = 0;
            ControllerReference = controller;
        }
    }
}
