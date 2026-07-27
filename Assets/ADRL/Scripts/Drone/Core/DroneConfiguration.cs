namespace ADRL.Drone.Core
{
    public class DroneConfiguration
    {
        public int MaxFleetSize { get; set; } = 10;
        public int DefaultSpawnCount { get; set; } = 3;
        public bool EnableEnergyManagement { get; set; } = true;
    }
}
