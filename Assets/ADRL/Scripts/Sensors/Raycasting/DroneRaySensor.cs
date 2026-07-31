namespace ADRL.Sensors.Raycasting
{
    using ADRL.Core.Configuration;
    using ADRL.Sensors.Interfaces;
    using UnityEngine;

    /// <summary>
    /// Spherical ray sensor that emits a fixed number of rays spread horizontally
    /// across the configured cone centered on the drone's forward direction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Observation layout per ray (2 samples):
    /// <list type="number">
    /// <item><description>Normalized proximity in [0, 1]; 1 = collision at range 0,
    /// 0 = no hit within <see cref="SensorConfig.RayRange"/>.</description></item>
    /// <item><description>1 when the hit collider belongs to an
    /// <see cref="IVictimDetectable"/>, otherwise 0.</description></item>
    /// </list>
    /// Total dimension is <c>RayCount * 2</c>.
    /// </para>
    /// </remarks>
    public sealed class DroneRaySensor : ISensorDataProvider
    {
        private readonly SensorConfig _config;
        private readonly int _rayCount;
        private readonly float[] _values;
        private readonly Vector3 _rayOriginOffset = new(0f, 0.5f, 0f);

        public string SensorId => "DroneRaySensor";
        public int DimensionCount => _rayCount * 2;
        public bool IsActive { get; private set; }

        public DroneRaySensor(SensorConfig config)
        {
            _config = config ?? throw new System.ArgumentNullException(nameof(config));
            _rayCount = Mathf.Max(1, config.RayCount);
            _values = new float[DimensionCount];
            IsActive = true;
        }

        public ISensorReading Read(Transform origin)
        {
            if (!IsActive || origin == null)
                return SensorReading.Empty;

            var rayOrigin = origin.position + _rayOriginOffset;
            var baseYaw = Mathf.Atan2(origin.forward.x, origin.forward.z);
            var halfCone = _config.RayConeAngle * 0.5f * Mathf.Deg2Rad;
            var coneSpan = _config.RayConeAngle * Mathf.Deg2Rad;

            for (var i = 0; i < _rayCount; i++)
            {
                var t = _rayCount == 1 ? 0.5f : i / (float)(_rayCount - 1);
                var angle = baseYaw - halfCone + t * coneSpan;
                var direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));

                if (Physics.Raycast(rayOrigin, direction, out var hit, _config.RayRange,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                    && !hit.collider.transform.IsChildOf(origin))
                {
                    _values[i * 2] = Mathf.Clamp01(1f - hit.distance / _config.RayRange);
                    _values[i * 2 + 1] = HasVictim(hit) ? 1f : 0f;
                }
                else
                {
                    _values[i * 2] = 0f;
                    _values[i * 2 + 1] = 0f;
                }
            }

            return new SensorReading(_values);
        }

        public void Reset()
        {
            System.Array.Clear(_values, 0, _values.Length);
            IsActive = true;
        }

        private static bool HasVictim(RaycastHit hit)
        {
            return hit.collider != null &&
                   hit.collider.GetComponentInParent<IVictimDetectable>() != null;
        }
    }
}
