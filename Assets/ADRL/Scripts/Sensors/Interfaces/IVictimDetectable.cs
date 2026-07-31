namespace ADRL.Sensors.Interfaces
{
    using UnityEngine;

    /// <summary>
    /// Marker implemented by environment objects that sensors can recognize as
    /// victims. Defined in the sensor assembly so the sensor layer never has to
    /// reference the environment assembly; the environment's <c>Victim</c>
    /// component implements it, and any sensor can locate victims generically.
    /// </summary>
    public interface IVictimDetectable
    {
        /// <summary>True while the object is still a valid rescue target.</summary>
        bool IsAlive { get; }

        /// <summary>World-space position used to compute proximity readings.</summary>
        Vector3 VictimPosition { get; }
    }
}
