namespace ADRL.Drone.Controllers
{
    using System.Collections.Generic;
    using ADRL.Core.Events;
    using ADRL.Drone.Events;

    public class DroneStateMachine
    {
        private readonly Dictionary<DroneState, HashSet<DroneState>> _validTransitions;
        private readonly EventBus _eventBus;
        private readonly int _droneId;

        public DroneState CurrentState { get; private set; }

        public DroneStateMachine(EventBus eventBus, int droneId)
        {
            _eventBus = eventBus;
            _droneId = droneId;
            CurrentState = DroneState.Uninitialized;
            _validTransitions = new Dictionary<DroneState, HashSet<DroneState>>
            {
                [DroneState.Uninitialized] = new HashSet<DroneState> { DroneState.Initializing },
                [DroneState.Initializing] = new HashSet<DroneState> { DroneState.Idle, DroneState.Disabled },
                [DroneState.Idle] = new HashSet<DroneState> { DroneState.Active, DroneState.Destroyed, DroneState.Disabled },
                [DroneState.Active] = new HashSet<DroneState> { DroneState.Paused, DroneState.Idle, DroneState.Emergency, DroneState.Destroyed, DroneState.Disabled },
                [DroneState.Paused] = new HashSet<DroneState> { DroneState.Active, DroneState.Destroyed, DroneState.Disabled },
                [DroneState.Emergency] = new HashSet<DroneState> { DroneState.Idle, DroneState.Destroyed, DroneState.Disabled },
                [DroneState.Disabled] = new HashSet<DroneState>(),
                [DroneState.Destroyed] = new HashSet<DroneState>()
            };
        }

        public void Initialize(DroneState initialState)
        {
            CurrentState = initialState;
        }

        public bool TryTransitionTo(DroneState newState)
        {
            if (CurrentState == newState)
                return true;

            if (!_validTransitions.TryGetValue(CurrentState, out var allowed))
                return false;

            if (!allowed.Contains(newState))
                return false;

            var previous = CurrentState;
            CurrentState = newState;

            _eventBus?.Publish(new DroneStateChangedEvent(_droneId, previous, newState));

            return true;
        }

        public bool CanTransitionTo(DroneState newState)
        {
            if (CurrentState == newState)
                return true;

            if (!_validTransitions.TryGetValue(CurrentState, out var allowed))
                return false;

            return allowed.Contains(newState);
        }
    }
}
