namespace ADRL.AI.Decision.Execution
{
    using ADRL.AI.DecisionMaking;
    using UnityEngine;

    /// <summary>
    /// Owns movement generation for the <see cref="BehaviourState.Search"/>
    /// behaviour. Produces steady forward exploration with a gentle, constant-rate
    /// yaw sweep so the drone gradually scans its surroundings. Fully
    /// deterministic: the command is a pure function of the assessed situation and
    /// the configured constants, with no random numbers, no hidden state, and no
    /// left/right oscillation.
    /// </summary>
    public sealed class SearchExecutor : IBehaviourExecutor
    {
        private readonly float _forwardSpeed;
        private readonly float _yawRate;

        public SearchExecutor(float forwardSpeed, float yawRate)
        {
            _forwardSpeed = forwardSpeed;
            _yawRate = yawRate;
        }

        public BehaviourState Behaviour => BehaviourState.Search;

        /// <summary>
        /// Forward drive plus a smooth, constant yaw sweep. The yaw never changes
        /// sign for a fixed situation, so no oscillation occurs.
        /// </summary>
        public DroneCommand Resolve(SituationSnapshot assessment)
        {
            return new DroneCommand(
                new Vector3(0f, 0f, _forwardSpeed),
                _yawRate,
                false);
        }
    }
}
