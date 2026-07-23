namespace ADRL.Drone.Components
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Drone.Events;

    public class DroneEnergy
    {
        private const float LowThreshold = 0.25f;
        private const float CriticalThreshold = 0.10f;

        private readonly EventBus _eventBus;
        private float _currentEnergy;
        private float _maxEnergy;
        private int _droneId;
        private bool _lowBatteryTriggered;
        private bool _criticalBatteryTriggered;

        public float CurrentEnergy => _currentEnergy;
        public float MaxEnergy => _maxEnergy;
        public bool IsDepleted => _currentEnergy <= 0f;
        public bool IsLow => !IsDepleted && _currentEnergy / _maxEnergy <= LowThreshold;
        public bool IsCritical => !IsDepleted && _currentEnergy / _maxEnergy <= CriticalThreshold;
        public float EnergyPercentage => _maxEnergy > 0f ? _currentEnergy / _maxEnergy : 0f;

        public DroneEnergy(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Initialize(DroneConfig config)
        {
            _maxEnergy = config.MaxEnergy;
            _currentEnergy = _maxEnergy;
            _lowBatteryTriggered = false;
            _criticalBatteryTriggered = false;
        }

        public void SetDroneId(int droneId)
        {
            _droneId = droneId;
        }

        public void Consume(float amount)
        {
            if (IsDepleted)
                return;

            _currentEnergy = UnityEngine.Mathf.Max(0f, _currentEnergy - amount);

            CheckThresholds();
        }

        public void Recharge(float amount)
        {
            _currentEnergy = UnityEngine.Mathf.Min(_maxEnergy, _currentEnergy + amount);

            if (_currentEnergy > _maxEnergy * LowThreshold)
                _lowBatteryTriggered = false;

            if (_currentEnergy > _maxEnergy * CriticalThreshold)
                _criticalBatteryTriggered = false;
        }

        public void Reset()
        {
            _currentEnergy = _maxEnergy;
            _lowBatteryTriggered = false;
            _criticalBatteryTriggered = false;
        }

        private void CheckThresholds()
        {
            if (IsCritical && !_criticalBatteryTriggered)
            {
                _criticalBatteryTriggered = true;
                _eventBus?.Publish(new BatteryCriticalEvent(_droneId, _currentEnergy));
                return;
            }

            if (IsLow && !_lowBatteryTriggered)
            {
                _lowBatteryTriggered = true;
                _eventBus?.Publish(new BatteryLowEvent(_droneId, _currentEnergy));
            }
        }
    }
}
