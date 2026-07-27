namespace ADRL.Drone.Core
{
    using ADRL.Drone.Controllers;
    using UnityEngine;

    public readonly struct SpawnParameters
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public float InitialHealth { get; }
        public float InitialBattery { get; }
        public DroneState InitialState { get; }

        public SpawnParameters(
            Vector3 position,
            Quaternion rotation,
            float initialHealth,
            float initialBattery,
            DroneState initialState)
        {
            Position = position;
            Rotation = rotation;
            InitialHealth = initialHealth;
            InitialBattery = initialBattery;
            InitialState = initialState;
        }
    }
}
