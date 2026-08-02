namespace ADRL.Environment.Core
{
    using ADRL.Core.Events;
    using ADRL.Environment.Events;
    using UnityEngine;

    /// <summary>
    /// Observes mission progress and publishes <see cref="MissionCompletedEvent"/>
    /// exactly once when every registered victim has been rescued. The tracker owns
    /// no reward, episode, or simulation state: it only reports the completion. The
    /// episode lifecycle remains exclusively owned by the simulation manager.
    /// </summary>
    public sealed class MissionProgressTracker : MonoBehaviour
    {
        private EventBus _eventBus;
        private int _registeredVictims;
        private int _rescuedVictims;
        private bool _missionCompleted;

        public int TotalVictims => _registeredVictims;

        public int RescuedVictims => _rescuedVictims;

        public bool IsMissionCompleted => _missionCompleted;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<VictimRegisteredEvent>(OnVictimRegistered);
            _eventBus?.Subscribe<VictimRescuedEvent>(OnVictimRescued);
            _eventBus?.Subscribe<EnvironmentResetEvent>(OnEnvironmentReset);
        }

        private void OnVictimRegistered(VictimRegisteredEvent evt)
        {
            _registeredVictims++;
            TryCompleteMission();
        }

        private void OnVictimRescued(VictimRescuedEvent evt)
        {
            _rescuedVictims++;
            TryCompleteMission();
        }

        private void TryCompleteMission()
        {
            if (_missionCompleted || _registeredVictims <= 0 || _rescuedVictims < _registeredVictims)
                return;

            _missionCompleted = true;
            _eventBus?.Publish(new MissionCompletedEvent(_rescuedVictims, _registeredVictims));
        }

        private void OnEnvironmentReset(EnvironmentResetEvent evt)
        {
            // Victims reset to Waiting in place (they are not re-registered), so
            // the registered total persists while the progress counters restart.
            _rescuedVictims = 0;
            _missionCompleted = false;
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<VictimRegisteredEvent>(OnVictimRegistered);
            _eventBus?.Unsubscribe<VictimRescuedEvent>(OnVictimRescued);
            _eventBus?.Unsubscribe<EnvironmentResetEvent>(OnEnvironmentReset);
        }
    }
}
