namespace ADRL.AI.Decision.Mission
{
    /// <summary>
    /// The discrete objective a mission coordinator can currently pursue. It names
    /// <em>what the drone should be doing</em>; it never executes movement itself -
    /// the <see cref="BehaviourSelector"/> maps it to a behaviour and the existing
    /// executor pipeline owns execution. Immutable by construction.
    /// </summary>
    public enum MissionTaskState
    {
        /// <summary>No objective; hold position until a viable assessment arrives.</summary>
        Idle,

        /// <summary>Sweep the area for an unassessed target.</summary>
        SearchArea,

        /// <summary>A target was perceived; investigate it.</summary>
        InvestigateTarget,

        /// <summary>A victim was confirmed at close range; complete the rescue.</summary>
        RescueVictim,

        /// <summary>An imminent hazard must be cleared before any objective continues.</summary>
        AvoidHazard,

        /// <summary>Rescue completed; recover before sweeping the area again.</summary>
        ResumeSearch,
    }
}
