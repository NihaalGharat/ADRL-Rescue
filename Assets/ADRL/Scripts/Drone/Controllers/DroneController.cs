namespace ADRL.Drone.Controllers
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Drone.Components;
    using ADRL.Drone.Events;
    using ADRL.Drone.Interfaces;
    using UnityEngine;

    public class DroneController : MonoBehaviour
    {
        private int _droneId;
        private EventBus _eventBus;
        private DroneConfig _config;
        private IMotor _motor;
        private DroneHealth _health;
        private DroneEnergy _energy;
        private DroneStateMachine _stateMachine;

        private static int _nextId;

        public int DroneId => _droneId;
        public DroneState CurrentState => _stateMachine.CurrentState;
        public DroneHealth Health => _health;
        public DroneEnergy Energy => _energy;
        public IMotor Motor => _motor;
        public DroneStateMachine StateMachine => _stateMachine;

        public void Initialize(EventBus eventBus, DroneConfig config, IMotor motor)
        {
            _droneId = InterlockedIncrement(ref _nextId);
            _eventBus = eventBus;
            _config = config;
            _motor = motor;
            _health = new DroneHealth(eventBus);
            _energy = new DroneEnergy(eventBus);
            _stateMachine = new DroneStateMachine(eventBus, _droneId);

            _health.SetDroneId(_droneId);
            _energy.SetDroneId(_droneId);

            _health.Initialize(config);
            _energy.Initialize(config);
            _motor.Initialize(config);
            _stateMachine.Initialize(DroneState.Uninitialized);

            _stateMachine.TryTransitionTo(DroneState.Initializing);
            _stateMachine.TryTransitionTo(DroneState.Idle);

            _eventBus?.Publish(new DroneSpawnedEvent(_droneId));
        }

        public void Activate()
        {
            if (_stateMachine.TryTransitionTo(DroneState.Active))
            {
                _eventBus?.Publish(new DroneActivatedEvent(_droneId));
            }
        }

        public void Deactivate()
        {
            _motor.Stop();
            _stateMachine.TryTransitionTo(DroneState.Idle);
        }

        public void Pause()
        {
            _stateMachine.TryTransitionTo(DroneState.Paused);
        }

        public void Resume()
        {
            _stateMachine.TryTransitionTo(DroneState.Active);
        }

        public void EmergencyStop()
        {
            _motor.EmergencyStop();
            if (_stateMachine.TryTransitionTo(DroneState.Emergency))
            {
                _eventBus?.Publish(new EmergencyStopEvent(_droneId));
            }
        }

        public void DestroyDrone()
        {
            _motor.EmergencyStop();
            _stateMachine.TryTransitionTo(DroneState.Destroyed);
        }

        public void Disable()
        {
            _motor.EmergencyStop();
            _stateMachine.TryTransitionTo(DroneState.Disabled);
        }

        private void Update()
        {
            if (CurrentState != DroneState.Active)
                return;

            _energy.Consume(_config.EnergyDrainRate * Time.deltaTime);

            if (_energy.IsDepleted)
            {
                EmergencyStop();
            }
        }

        private static int InterlockedIncrement(ref int location)
        {
            return System.Threading.Interlocked.Increment(ref location);
        }
    }
}
