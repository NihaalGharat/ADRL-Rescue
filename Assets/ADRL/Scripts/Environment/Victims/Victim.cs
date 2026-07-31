namespace ADRL.Environment.Victims
{
    using ADRL.Environment.Interfaces;
    using ADRL.Sensors.Interfaces;
    using UnityEngine;

    public class Victim : MonoBehaviour, IEnvironmentObject, IVictimDetectable
    {
        [SerializeField]
        private int _victimId;

        [SerializeField]
        private VictimState _state = VictimState.Waiting;

        public int Id => _victimId;

        public VictimState State => _state;

        public bool IsAlive => _state == VictimState.Waiting || _state == VictimState.Detected;

        public Vector3 VictimPosition => transform.position;

        public void SetId(int victimId)
        {
            _victimId = victimId;
        }

        public void MarkDetected()
        {
            _state = VictimState.Detected;
        }

        public void MarkRescued()
        {
            _state = VictimState.Rescued;
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
