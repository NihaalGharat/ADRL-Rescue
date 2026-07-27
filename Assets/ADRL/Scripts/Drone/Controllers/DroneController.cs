namespace ADRL.Drone.Controllers
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using ADRL.Drone.Components;
    using ADRL.Drone.Core;
    using ADRL.Drone.Events;
    using ADRL.Drone.Interfaces;
    using UnityEngine;

    [RequireComponent(typeof(DroneIdentity))]
    public class DroneController : MonoBehaviour
    {
        private int _droneId;
        private EventBus _eventBus;
        private DroneConfig _config;
        private IMotor _motor;
        private DroneHealth _health;
        private DroneEnergy _energy;
        private DroneStateMachine _stateMachine;
        private DroneManager _droneManager;

        public int DroneId => _droneId;
        public DroneState CurrentState => _stateMachine.CurrentState;
        public DroneHealth Health => _health;
        public DroneEnergy Energy => _energy;
        public IMotor Motor => _motor;
        public DroneStateMachine StateMachine => _stateMachine;

        public void Initialize(EventBus eventBus, DroneConfig config, IMotor motor, DroneManager droneManager)
        {
            _eventBus = eventBus;
            _config = config;
            _motor = motor;
            _droneManager = droneManager;

            _droneId = _droneManager.RegisterDrone(this);

            var identity = GetComponent<DroneIdentity>();
            if (identity != null)
                identity.AssignId(_droneId);

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
            _droneManager.TransitionRuntimeState(_droneId, DroneRuntimeState.Initializing);

            _stateMachine.TryTransitionTo(DroneState.Idle);
            _droneManager.TransitionRuntimeState(_droneId, DroneRuntimeState.Idle);

            _eventBus?.Publish(new DroneSpawnedEvent(_droneId));
        }

        public void Activate()
        {
            if (_stateMachine.TryTransitionTo(DroneState.Active))
            {
                _droneManager.TransitionRuntimeState(_droneId, DroneRuntimeState.Active);
                _eventBus?.Publish(new DroneActivatedEvent(_droneId));
            }
        }

        public void Deactivate()
        {
            _motor.Stop();
            if (_stateMachine.TryTransitionTo(DroneState.Idle))
            {
                _droneManager.TransitionRuntimeState(_droneId, DroneRuntimeState.Idle);
            }
        }

        public void Pause()
        {
            if (_stateMachine.TryTransitionTo(DroneState.Paused))
            {
                _droneManager.TransitionRuntimeState(_droneId, DroneRuntimeState.Paused);
            }
        }

        public void Resume()
        {
            if (_stateMachine.TryTransitionTo(DroneState.Active))
            {
                _droneManager.TransitionRuntimeState(_droneId, DroneRuntimeState.Active);
            }
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
            if (_stateMachine.TryTransitionTo(DroneState.Destroyed))
            {
                _droneManager.TransitionRuntimeState(_droneId, DroneRuntimeState.Destroyed);
            }
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
    }
}
