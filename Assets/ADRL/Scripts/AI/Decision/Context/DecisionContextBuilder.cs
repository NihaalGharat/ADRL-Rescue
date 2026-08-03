namespace ADRL.AI.Decision.Context
{
    using System;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// The single owner of <see cref="DecisionContextSnapshot"/> composition. It is a
    /// pure, stateless function: given the same subsystem outputs it always produces
    /// the same snapshot, and it holds no state between calls. It never scores,
    /// arbitrates, selects, transitions or mutates runtime state - it only assembles
    /// the immutable context snapshot and defensively copies the candidate array so
    /// the snapshot never aliases the engine's working buffer.
    /// </summary>
    public sealed class DecisionContextBuilder : IDecisionContextBuilder
    {
        /// <inheritdoc/>
        public DecisionContextSnapshot Build(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask mission,
            TaskCandidate[] candidates,
            TaskPriority winning,
            BehaviourState behaviour,
            DroneCommand command,
            DecisionDiagnostics diagnostics,
            DecisionRuntimeState runtimeState)
        {
            var owned = Array.Empty<TaskCandidate>();
            if (candidates != null)
            {
                owned = new TaskCandidate[candidates.Length];
                Array.Copy(candidates, owned, candidates.Length);
            }

            return new DecisionContextSnapshot(
                assessment,
                memory,
                mission,
                owned,
                winning,
                behaviour,
                command,
                diagnostics,
                runtimeState);
        }
    }
}
