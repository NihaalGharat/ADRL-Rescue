namespace ADRL.Sensors.Interfaces
{
    /// <summary>
    /// Immutable snapshot of a single sensor sample. The <see cref="Values"/>
    /// array is always written into the observation vector in the order the
    /// provider reports <see cref="DimensionCount"/>.
    /// </summary>
    public interface ISensorReading
    {
        /// <summary>The raw, ordered sample values.</summary>
        float[] Values { get; }

        /// <summary>Number of float samples contained in <see cref="Values"/>.</summary>
        int DimensionCount { get; }

        /// <summary>Simulation time at which the sample was captured.</summary>
        float Timestamp { get; }

        /// <summary>False when the provider could not produce a valid sample.</summary>
        bool IsValid { get; }
    }
}
