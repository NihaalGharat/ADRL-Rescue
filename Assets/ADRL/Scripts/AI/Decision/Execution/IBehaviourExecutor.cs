namespace ADRL.AI.Decision.Execution
{
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// The single owner of movement generation for one behaviour. An executor is
    /// a pure, stateless function of the assessed situation and the execution
    /// profile: the same <see cref="SituationSnapshot"/> and
    /// <see cref="BehaviourExecutionProfile"/> always yield the same
    /// <see cref="DroneCommand"/>. The profile only scales the executor's base
    /// movement constants (speed and turning), so the movement <em>decision</em>
    /// - direction, side, pattern - is unchanged; only its parameters improve. It
    /// never decides <em>what</em> to do (that belongs to the
    /// <c>BehaviourSelector</c>) and never runs the drone (that belongs to
    /// <c>DroneController</c>).
    /// </summary>
    public interface IBehaviourExecutor
    {
        /// <summary>The behaviour this executor owns movement for.</summary>
        BehaviourState Behaviour { get; }

        /// <summary>
        /// Generate the command for one assessed situation using the neutral,
        /// no-op execution profile (the executor's configured base behaviour).
        /// Kept for backward compatibility; equivalent to resolving with
        /// <see cref="BehaviourExecutionProfile.Empty"/>.
        /// </summary>
        DroneCommand Resolve(SituationSnapshot assessment);

        /// <summary>Generate the command for one assessed situation and execution profile.</summary>
        DroneCommand Resolve(SituationSnapshot assessment, BehaviourExecutionProfile profile);
    }
}
