namespace ADRL.AI.Decision.Optimization
{
    using ADRL.AI.Decision.Context;

    /// <summary>
    /// The contract for the single owner of behaviour-execution optimization.
    /// Given the selected <see cref="BehaviourState"/> and the current immutable
    /// <see cref="DecisionContextSnapshot"/>, it returns the
    /// <see cref="BehaviourExecutionProfile"/> describing <em>how</em> that
    /// behaviour should execute. Pure and stateless: the same inputs always
    /// produce the same profile, and it never changes the mission objective, the
    /// priority decision or the selected behaviour - those are decided before it
    /// runs. It generates no movement and holds no state between calls.
    /// </summary>
    public interface IBehaviourOptimizer
    {
        /// <summary>Returns the execution profile for the given behaviour and current context.</summary>
        BehaviourExecutionProfile Optimize(BehaviourState behaviour, DecisionContextSnapshot snapshot);
    }
}
