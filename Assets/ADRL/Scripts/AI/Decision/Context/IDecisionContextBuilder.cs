namespace ADRL.AI.Decision.Context
{
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// The contract for the single owner of <see cref="DecisionContextSnapshot"/>
    /// composition. It assembles the immutable runtime context from every subsystem
    /// output of one decision step. Pure composition only - no decision logic, no
    /// scoring, no transitions, no state mutation.
    /// </summary>
    public interface IDecisionContextBuilder
    {
        /// <summary>
        /// Assembles an immutable <see cref="DecisionContextSnapshot"/> from the
        /// given subsystem outputs. The candidate array is defensively copied so the
        /// snapshot never aliases the engine's working buffer.
        /// </summary>
        DecisionContextSnapshot Build(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask mission,
            TaskCandidate[] candidates,
            TaskPriority winning,
            BehaviourState behaviour,
            DroneCommand command,
            DecisionDiagnostics diagnostics,
            DecisionRuntimeState runtimeState);
    }
}
