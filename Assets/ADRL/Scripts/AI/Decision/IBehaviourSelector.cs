namespace ADRL.AI.Decision
{
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// Selects a single <see cref="BehaviourState"/> for the current situation.
    /// Implementations own the decision policy and must be deterministic for a
    /// fixed snapshot. The memory-aware overload lets a selector factor short-term
    /// behaviour memory into continuity decisions, and the mission-aware overload
    /// maps the coordinator's current <see cref="MissionTask"/> onto a behaviour -
    /// in both cases current perception always takes priority over remembered or
    /// mission context.
    /// </summary>
    public interface IBehaviourSelector
    {
        /// <summary>Choose the behaviour to execute for the given situation.</summary>
        BehaviourState Select(SituationSnapshot assessment);

        /// <summary>
        /// Choose the behaviour to execute, optionally influenced by remembered
        /// context. Current sensor data always has priority over memory.
        /// </summary>
        BehaviourState Select(SituationSnapshot assessment, BehaviourMemory memory);

        /// <summary>
        /// Choose the behaviour to execute, optionally influenced by the current
        /// mission task. Current sensor data always has priority over the mission;
        /// the mission only resolves neutral current situations.
        /// </summary>
        BehaviourState Select(SituationSnapshot assessment, MissionTask mission);
    }
}
