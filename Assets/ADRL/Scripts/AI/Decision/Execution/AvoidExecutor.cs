namespace ADRL.AI.Decision.Execution
{
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.DecisionMaking;
    using UnityEngine;

    /// <summary>
    /// Owns movement generation for the <see cref="BehaviourState.Avoid"/>
    /// behaviour. Evades the imminent obstacle on a deterministic side: the drone
    /// strafes away from the obstacle while backing off slightly and turning away,
    /// which both avoids the collision and begins recovering a clean forward
    /// heading. The avoidance side is derived solely from the assessed obstacle
    /// side, so there is no left/right oscillation. The execution profile scales
    /// the backoff by its speed multiplier, the steering/yaw by its turn-rate
    /// multiplier, and raises the evasion response by its caution level - the
    /// closer the obstacle, the more cautiously the optimizer executes. The
    /// direction of evasion is unchanged. Configurable gains and speeds;
    /// deterministic for a fixed snapshot and profile.
    /// </summary>
    public sealed class AvoidExecutor : IBehaviourExecutor
    {
        private readonly float _backoffSpeed;
        private readonly float _steerGain;
        private readonly float _yawGain;

        public AvoidExecutor(float backoffSpeed, float steerGain, float yawGain)
        {
            _backoffSpeed = backoffSpeed;
            _steerGain = steerGain;
            _yawGain = yawGain;
        }

        public BehaviourState Behaviour => BehaviourState.Avoid;

        /// <inheritdoc/>
        public DroneCommand Resolve(SituationSnapshot assessment)
        {
            return Resolve(assessment, BehaviourExecutionProfile.Empty);
        }

        public DroneCommand Resolve(SituationSnapshot assessment, BehaviourExecutionProfile profile)
        {
            // Obstacle on the right (positive) -> evade left (negative strafe).
            var evasionGain = 1f + profile.CautionLevel;
            var steer = Mathf.Clamp(-assessment.ObstacleSide * _steerGain * profile.TurnRateMultiplier * evasionGain, -1f, 1f);
            var yaw = Mathf.Clamp(-assessment.ObstacleSide * _yawGain * profile.TurnRateMultiplier * evasionGain, -1f, 1f);

            return new DroneCommand(
                new Vector3(steer, 0f, -_backoffSpeed * profile.SpeedMultiplier),
                yaw,
                false);
        }
    }
}
