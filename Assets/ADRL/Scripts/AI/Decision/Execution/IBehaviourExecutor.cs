namespace ADRL.AI.Decision.Execution
{
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// The single owner of movement generation for one behaviour. An executor is
    /// a pure, stateless function of the assessed situation: the same
    /// <see cref="SituationSnapshot"/> always yields the same
    /// <see cref="DroneCommand"/>. It never decides <em>what</em> to do (that
    /// belongs to the <c>BehaviourSelector</c>) and never runs the drone (that
    /// belongs to <c>DroneController</c>).
    /// </summary>
    public interface IBehaviourExecutor
    {
        /// <summary>The behaviour this executor owns movement for.</summary>
        BehaviourState Behaviour { get; }

        /// <summary>Generate the command for one assessed situation.</summary>
        DroneCommand Resolve(SituationSnapshot assessment);
    }
}
