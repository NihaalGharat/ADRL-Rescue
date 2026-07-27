namespace ADRL.Drone.Core
{
    public readonly struct SpawnRequest
    {
        public string DroneType { get; }
        public string SpawnPointId { get; }
        public string Team { get; }
        public string MissionContext { get; }

        public SpawnRequest(
            string droneType,
            string spawnPointId,
            string team,
            string missionContext)
        {
            DroneType = droneType ?? string.Empty;
            SpawnPointId = spawnPointId ?? string.Empty;
            Team = team ?? string.Empty;
            MissionContext = missionContext ?? string.Empty;
        }
    }
}
