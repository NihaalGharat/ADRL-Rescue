namespace ADRL.AI.Decision
{
    /// <summary>
    /// Selects a single <see cref="BehaviourState"/> for the current situation.
    /// Implementations own the decision policy and must be deterministic for a
    /// fixed snapshot.
    /// </summary>
    public interface IBehaviourSelector
    {
        /// <summary>Choose the behaviour to execute for the given situation.</summary>
        BehaviourState Select(SituationSnapshot assessment);
    }
}