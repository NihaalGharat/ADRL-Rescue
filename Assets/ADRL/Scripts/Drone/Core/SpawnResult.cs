namespace ADRL.Drone.Core
{
    using ADRL.Drone.Controllers;

    public readonly struct SpawnResult
    {
        public bool Success { get; }
        public DroneController Drone { get; }
        public int AssignedDroneId { get; }
        public string FailureReason { get; }
        public float SpawnTimestamp { get; }

        private SpawnResult(
            bool success,
            DroneController drone,
            int assignedDroneId,
            string failureReason,
            float spawnTimestamp)
        {
            Success = success;
            Drone = drone;
            AssignedDroneId = assignedDroneId;
            FailureReason = failureReason;
            SpawnTimestamp = spawnTimestamp;
        }

        public static SpawnResult CreateSuccess(
            DroneController drone, int droneId, float timestamp)
        {
            return new SpawnResult(true, drone, droneId, null, timestamp);
        }

        public static SpawnResult CreateFailure(string reason)
        {
            return new SpawnResult(false, null, -1, reason ?? "Unknown error.", 0f);
        }
    }
}
