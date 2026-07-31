namespace ADRL.Sensors.Detection
{
    using ADRL.Core.Configuration;
    using ADRL.Sensors.Interfaces;
    using UnityEngine;

    /// <summary>
    /// Thermal sensor that searches for the nearest living
    /// <see cref="IVictimDetectable"/> inside a sphere of
    /// <see cref="SensorConfig.ThermalSensorRange"/> around the drone.
    /// </summary>
    /// <remarks>
    /// Observation layout (2 samples):
    /// <list type="number">
    /// <item><description>1 when a living victim is in thermal range, otherwise 0.</description></item>
    /// <item><description>Normalized proximity in [0, 1]; 1 = victim at range 0,
    /// 0 = no victim in range.</description></item>
    /// </list>
    /// </remarks>
    public sealed class DroneThermalSensor : ISensorDataProvider
    {
        private readonly SensorConfig _config;
        private readonly float[] _values = new float[2];
        private Collider[] _overlapBuffer;

        public string SensorId => "DroneThermalSensor";
        public int DimensionCount => 2;
        public bool IsActive { get; private set; }

        public DroneThermalSensor(SensorConfig config)
        {
            _config = config ?? throw new System.ArgumentNullException(nameof(config));
            IsActive = true;
        }

        public ISensorReading Read(Transform origin)
        {
            if (!IsActive || origin == null)
                return SensorReading.Empty;

            _values[0] = 0f;
            _values[1] = 0f;

            var range = _config.ThermalSensorRange;
            if (range <= 0f)
                return new SensorReading(_values);

            if (_overlapBuffer == null || _overlapBuffer.Length < 16)
                _overlapBuffer = new Collider[16];

            var count = Physics.OverlapSphereNonAlloc(
                origin.position, range, _overlapBuffer,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            var bestProximity = 0f;
            for (var i = 0; i < count; i++)
            {
                var collider = _overlapBuffer[i];
                if (collider == null || collider.transform.IsChildOf(origin))
                    continue;

                var victim = collider.GetComponentInParent<IVictimDetectable>();
                if (victim == null || !victim.IsAlive)
                    continue;

                var distance = Vector3.Distance(origin.position, victim.VictimPosition);
                var proximity = Mathf.Clamp01(1f - distance / range);
                if (proximity > bestProximity)
                    bestProximity = proximity;
            }

            if (bestProximity > 0f)
            {
                _values[0] = 1f;
                _values[1] = bestProximity;
            }

            return new SensorReading(_values);
        }

        public void Reset()
        {
            _values[0] = 0f;
            _values[1] = 0f;
            IsActive = true;
        }
    }
}
