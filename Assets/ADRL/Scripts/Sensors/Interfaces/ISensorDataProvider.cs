namespace ADRL.Sensors.Interfaces
{
    using UnityEngine;

    /// <summary>
    /// Contract for any sensor that produces a fixed-size sample relative to a
    /// drone transform. Providers are intentionally pure data sources: they
    /// never mutate the drone or publish events, which keeps the sensor layer
    /// deterministic and free of any ML-Agents dependency.
    /// </summary>
    public interface ISensorDataProvider
    {
        /// <summary>Stable identifier used for logging and diagnostics.</summary>
        string SensorId { get; }

        /// <summary>Fixed number of float samples this provider emits per read.</summary>
        int DimensionCount { get; }

        /// <summary>True while the provider is enabled and producing samples.</summary>
        bool IsActive { get; }

        /// <summary>
        /// Captures a sample relative to the supplied origin transform.
        /// Returns an invalid reading when <paramref name="origin"/> is null
        /// or the provider is inactive.
        /// </summary>
        ISensorReading Read(Transform origin);

        /// <summary>Restores the provider to its initial idle state.</summary>
        void Reset();
    }
}
