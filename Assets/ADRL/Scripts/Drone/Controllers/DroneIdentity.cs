namespace ADRL.Drone.Controllers
{
    using UnityEngine;

    [DisallowMultipleComponent]
    public class DroneIdentity : MonoBehaviour
    {
        [SerializeField] private int _droneId = -1;
        [SerializeField] private bool _isAssigned;

        public int DroneId => _droneId;
        public bool IsAssigned => _isAssigned;

        public void AssignId(int droneId)
        {
            _droneId = droneId;
            _isAssigned = true;
        }

        public void ClearId()
        {
            _droneId = -1;
            _isAssigned = false;
        }
    }
}
