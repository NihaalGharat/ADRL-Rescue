namespace ADRL.AI.Decision
{
    using ADRL.AI.Decision.Memory;

    /// <summary>
    /// Selects a single <see cref="BehaviourState"/> for the current situation.
    /// Implementations own the decision policy and must be deterministic for a
    /// fixed snapshot. The memory-aware overload lets a selector factor short-term
    /// behaviour memory into continuity decisions, with current perception always
    /// taking priority over remembered context.
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
    }
}
