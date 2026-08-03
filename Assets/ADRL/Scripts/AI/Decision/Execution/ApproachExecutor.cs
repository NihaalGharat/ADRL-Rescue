namespace ADRL.AI.Decision.Execution
{
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.DecisionMaking;
    using UnityEngine;

    /// <summary>
    /// Owns movement generation for the <see cref="BehaviourState.Approach"/>
    /// behaviour. Steers toward the assessed target side while driving forward
    /// smoothly. Lateral steering and yaw are proportional to the target side with
    /// configurable gains, and both are clamped so the command stays smooth and
    /// jitter-free. The execution profile scales the forward drive by its speed
    /// multiplier and the steering/yaw gains by its turn-rate multiplier (and
    /// caution); the decision of which side to steer toward is unchanged. Fully
    /// deterministic for a fixed snapshot and profile.
    /// </summary>
    public sealed class ApproachExecutor : IBehaviourExecutor
    {
        private readonly float _forwardSpeed;
        private readonly float _steerGain;
        private readonly float _yawGain;

        public ApproachExecutor(float forwardSpeed, float steerGain, float yawGain)
        {
            _forwardSpeed = forwardSpeed;
            _steerGain = steerGain;
            _yawGain = yawGain;
        }

        public BehaviourState Behaviour => BehaviourState.Approach;

        /// <inheritdoc/>
        public DroneCommand Resolve(SituationSnapshot assessment)
        {
            return Resolve(assessment, BehaviourExecutionProfile.Empty);
        }

        public DroneCommand Resolve(SituationSnapshot assessment, BehaviourExecutionProfile profile)
        {
            var turnGain = profile.TurnRateMultiplier * (1f + profile.CautionLevel);
            var steer = Mathf.Clamp(assessment.TargetSide * _steerGain * turnGain, -1f, 1f);
            var yaw = Mathf.Clamp(assessment.TargetSide * _yawGain * turnGain, -1f, 1f);

            return new DroneCommand(
                new Vector3(steer, 0f, _forwardSpeed * profile.SpeedMultiplier),
                yaw,
                false);
        }
    }
}
