namespace ADRL.Core.Configuration
{
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Configuration/Sensor Config",
        fileName = "SensorConfig")]
    public class SensorConfig : ScriptableObject
    {
        [SerializeField]
        [Min(1)]
        private int _rayCount = 12;

        [SerializeField]
        [Min(0f)]
        private float _rayRange = 50f;

        [SerializeField]
        [Range(0f, 360f)]
        private float _rayConeAngle = 180f;

        [SerializeField]
        [Min(0f)]
        private float _thermalSensorRange = 30f;

        [SerializeField]
        [Min(1)]
        private int _visionResolution = 128;

        [SerializeField]
        [Min(0f)]
        private float _visionRange = 40f;

        [SerializeField]
        [Min(1)]
        private int _sensorUpdateInterval = 1;

        public int RayCount => _rayCount;
        public float RayRange => _rayRange;
        public float RayConeAngle => _rayConeAngle;
        public float ThermalSensorRange => _thermalSensorRange;
        public int VisionResolution => _visionResolution;
        public float VisionRange => _visionRange;
        public int SensorUpdateInterval => _sensorUpdateInterval;
    }
}
