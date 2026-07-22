namespace ADRL.Drone.Components
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;

    public class DroneHealth
    {
        private readonly EventBus _eventBus;
        private float _currentHealth;
        private float _maxHealth;
        private int _droneId;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDestroyed => _currentHealth <= 0f;
        public float HealthPercentage => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;

        public DroneHealth(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Initialize(DroneConfig config)
        {
            _maxHealth = config.MaxHealth;
            _currentHealth = _maxHealth;
        }

        public void SetDroneId(int droneId)
        {
            _droneId = droneId;
        }

        public void TakeDamage(float amount)
        {
            if (IsDestroyed)
                return;

            _currentHealth = UnityEngine.Mathf.Max(0f, _currentHealth - amount);

            _eventBus?.Publish(new DroneDamagedEvent(_droneId, amount, _currentHealth));

            if (_currentHealth <= 0f)
            {
                _eventBus?.Publish(new DroneDestroyedEvent(_droneId));
            }
        }

        public void Repair(float amount)
        {
            if (IsDestroyed)
                return;

            _currentHealth = UnityEngine.Mathf.Min(_maxHealth, _currentHealth + amount);
        }

        public void Reset()
        {
            _currentHealth = _maxHealth;
        }
    }
}
