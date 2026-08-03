namespace ADRL.AI.Decision
{
    using ADRL.Sensors.Interfaces;

    /// <summary>
    /// Translates a fused <see cref="ISensorReading"/> into an immutable
    /// <see cref="SituationSnapshot"/>. Decoupled from the sensor layer so the
    /// decision framework never depends on a concrete provider. An implementation
    /// must be deterministic: the same reading always yields the same snapshot.
    /// </summary>
    public interface ISituationAssessor
    {
        /// <summary>Assess a single fused sensor reading.</summary>
        SituationSnapshot Assess(ISensorReading fused);
    }
}