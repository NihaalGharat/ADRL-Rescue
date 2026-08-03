namespace ADRL.AI.Decision.Execution
{
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// Owns movement generation for the <see cref="BehaviourState.Idle"/>
    /// behaviour. There is nothing to engage with, so it always commands
    /// <see cref="DroneCommand.Idle"/>; no parameters are required.
    /// </summary>
    public sealed class IdleExecutor : IBehaviourExecutor
    {
        public BehaviourState Behaviour => BehaviourState.Idle;

        /// <summary>Idle never generates movement or rotation.</summary>
        public DroneCommand Resolve(SituationSnapshot assessment) => DroneCommand.Idle;
    }
}
