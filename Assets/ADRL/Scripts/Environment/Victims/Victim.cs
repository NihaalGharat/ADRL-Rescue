namespace ADRL.Environment.Victims
{
    using ADRL.Core.Events;
    using ADRL.Environment.Interfaces;
    using ADRL.Sensors.Interfaces;
    using UnityEngine;

    public class Victim : MonoBehaviour, IEnvironmentObject, IVictimDetectable
    {
        [SerializeField]
        private int _victimId;

        [SerializeField]
        private VictimState _state = VictimState.Waiting;

        private EventBus _eventBus;

        public int Id => _victimId;

        public VictimState State => _state;

        public bool IsAlive => _state == VictimState.Waiting || _state == VictimState.Detected;

        public Vector3 VictimPosition => transform.position;

        public void SetId(int victimId)
        {
            _victimId = victimId;
        }

        /// <summary>
        /// Binds the event bus used to publish lifecycle events. Registration
        /// happens before any drone can interact with the victim.
        /// </summary>
        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            Initialize();
        }

        public void MarkDetected()
        {
            if (_state != VictimState.Waiting)
                return;

            _state = VictimState.Detected;
            _eventBus?.Publish(new VictimFoundEvent(_victimId));
        }

        public void MarkRescued()
        {
            if (_state != VictimState.Detected)
                return;

            _state = VictimState.Rescued;
            _eventBus?.Publish(new VictimRescuedEvent(_victimId));
        }

        public void MarkLost()
        {
            _state = VictimState.Lost;
        }

        public void Initialize()
        {
            _state = VictimState.Waiting;
        }

        public void Reset()
        {
            _state = VictimState.Waiting;
        }

        public void Cleanup()
        {
            _state = VictimState.Unknown;
        }
    }
}
