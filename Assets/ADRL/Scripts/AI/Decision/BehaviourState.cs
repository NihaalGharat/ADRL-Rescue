namespace ADRL.AI.Decision
{
    /// <summary>
    /// The discrete behavioural state selected by the decision framework. It
    /// names <em>what the drone intends to do</em>; it never executes movement
    /// itself - the existing <c>DroneActionResolver</c>/<c>DroneController</c>
    /// own execution.
    /// </summary>
    public enum BehaviourState
    {
        /// <summary>Nothing to engage with; hold position and command idle.</summary>
        Idle,

        /// <summary>Sweep the environment looking for an unassessed target.</summary>
        Search,

        /// <summary>A target was assessed; steer toward it.</summary>
        Approach,

        /// <summary>An obstacle closing ahead must be evaded first.</summary>
        Avoid,
    }
}