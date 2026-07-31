namespace ADRL.Sensors
{
    using System;
    using ADRL.Sensors.Interfaces;
    using UnityEngine;

    /// <summary>
    /// Default <see cref="ISensorReading"/> implementation. The <see cref="Values"/>
    /// array is owned by the producing provider and reused between reads to avoid
    /// per-step allocations.
    /// </summary>
    public readonly struct SensorReading : ISensorReading
    {
        private readonly float[] _values;

        public float[] Values => _values;
        public int DimensionCount => _values?.Length ?? 0;
        public float Timestamp { get; }
        public bool IsValid => _values is { Length: > 0 };

        public SensorReading(float[] values)
        {
            _values = values ?? Array.Empty<float>();
            Timestamp = Time.time;
        }

        /// <summary>A shared invalid reading returned when a sample cannot be captured.</summary>
        public static SensorReading Empty => new(Array.Empty<float>());
    }
}
