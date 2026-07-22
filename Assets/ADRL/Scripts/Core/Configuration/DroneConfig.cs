namespace ADRL.Core.Configuration
{
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Configuration/Drone Config",
        fileName = "DroneConfig")]
    public class DroneConfig : ScriptableObject
    {
        [SerializeField]
        [Min(0f)]
        private float _maxSpeed = 20f;

        [SerializeField]
        [Min(0f)]
        private float _acceleration = 10f;

        [SerializeField]
        [Min(0f)]
        private float _rotationSpeed = 90f;

        [SerializeField]
        [Min(0f)]
        private float _maxHealth = 100f;

        [SerializeField]
        [Min(0f)]
        private float _collisionDamage = 10f;

        [SerializeField]
        [Min(0f)]
        private float _maxEnergy = 100f;

        [SerializeField]
        [Min(0f)]
        private float _energyDrainRate = 1f;

        [SerializeField]
        [Min(0f)]
        private float _takeoffHeight = 5f;

        [SerializeField]
        private Vector3 _spawnOffset = new Vector3(0f, 3f, 0f);

        public float MaxSpeed => _maxSpeed;
        public float Acceleration => _acceleration;
        public float RotationSpeed => _rotationSpeed;
        public float MaxHealth => _maxHealth;
        public float CollisionDamage => _collisionDamage;
        public float MaxEnergy => _maxEnergy;
        public float EnergyDrainRate => _energyDrainRate;
        public float TakeoffHeight => _takeoffHeight;
        public Vector3 SpawnOffset => _spawnOffset;
    }
}
