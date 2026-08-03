namespace ADRL.AI.Decision.Execution
{
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// Owns movement generation for the <see cref="BehaviourState.Idle"/>
    /// behaviour. There is nothing to engage with, so it always commands
    /// <see cref="DroneCommand.Idle"/>; no parameters are required and the
    /// execution profile is ignored.
    /// </summary>
    public sealed class IdleExecutor : IBehaviourExecutor
    {
        public BehaviourState Behaviour => BehaviourState.Idle;

        /// <inheritdoc/>
        public DroneCommand Resolve(SituationSnapshot assessment) => DroneCommand.Idle;

        /// <summary>Idle never generates movement or rotation; the profile is ignored.</summary>
        public DroneCommand Resolve(SituationSnapshot assessment, BehaviourExecutionProfile profile) => DroneCommand.Idle;
    }
}
