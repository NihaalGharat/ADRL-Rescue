namespace ADRL.Sensors.Fusion
{
    using System;
    using System.Collections.Generic;
    using ADRL.Sensors.Interfaces;
    using UnityEngine;

    /// <summary>
    /// Aggregates multiple <see cref="ISensorDataProvider"/> instances into a
    /// single fixed-size reading. The fused dimension is the sum of the provider
    /// dimensions and is stable for a fixed provider set, which makes it safe to
    /// use as the agent's vector observation size.
    /// </summary>
    public sealed class SensorFusionProvider
    {
        private readonly List<ISensorDataProvider> _providers = new(4);

        public int ProviderCount => _providers.Count;

        /// <summary>Total number of float samples produced by all providers.</summary>
        public int DimensionCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _providers.Count; i++)
                    count += _providers[i].DimensionCount;
                return count;
            }
        }

        public void Add(ISensorDataProvider provider)
        {
            if (provider == null)
                return;

            if (!_providers.Contains(provider))
                _providers.Add(provider);
        }

        public void Remove(ISensorDataProvider provider)
        {
            _providers.Remove(provider);
        }

        public void Clear()
        {
            _providers.Clear();
        }

        /// <summary>
        /// Captures a sample from every provider in registration order and
        /// concatenates them into a single buffer of exactly
        /// <see cref="DimensionCount"/> floats.
        /// </summary>
        public ISensorReading Fuse(Transform origin)
        {
            var count = DimensionCount;
            var buffer = new float[count];
            var offset = 0;

            for (var i = 0; i < _providers.Count && offset < count; i++)
            {
                var reading = _providers[i].Read(origin);
                if (reading.Values == null || reading.DimensionCount <= 0)
                    continue;

                var length = Math.Min(reading.DimensionCount, count - offset);
                Array.Copy(reading.Values, 0, buffer, offset, length);
                offset += length;
            }

            return new SensorReading(buffer);
        }

        /// <summary>Resets all registered providers.</summary>
        public void Reset()
        {
            for (var i = 0; i < _providers.Count; i++)
                _providers[i].Reset();
        }
    }
}
