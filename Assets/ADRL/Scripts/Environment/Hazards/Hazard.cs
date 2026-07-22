namespace ADRL.Environment.Hazards
{
    using ADRL.Environment.Interfaces;
    using UnityEngine;

    public class Hazard : MonoBehaviour, IEnvironmentObject
    {
        [SerializeField]
        private int _hazardId;

        [SerializeField]
        private HazardType _hazardType;

        [SerializeField]
        private bool _isActive = true;

        public int Id => _hazardId;

        public HazardType HazardType => _hazardType;

        public bool IsActive => _isActive;

        public void SetId(int hazardId)
        {
            _hazardId = hazardId;
        }

        public void SetHazardType(HazardType hazardType)
        {
            _hazardType = hazardType;
        }

        public void Activate()
        {
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        public void Initialize()
        {
            _isActive = true;
        }

        public void Reset()
        {
            _isActive = true;
        }

        public void Cleanup()
        {
            _isActive = false;
        }
    }
}
