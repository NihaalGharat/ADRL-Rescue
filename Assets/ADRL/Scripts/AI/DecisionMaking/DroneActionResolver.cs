namespace ADRL.AI.DecisionMaking
{
    using UnityEngine;

    /// <summary>
    /// A resolved, normalized command for the drone's motor derived from the
    /// raw agent action vector. Kept independent of ML-Agents so it can be
    /// exercised by tests and reused by non-RL controllers.
    /// </summary>
    public readonly struct DroneCommand
    {
        /// <summary>
        /// Movement direction in drone-local space. Components are clamped to
        /// [-1, 1]: x = strafe, y = altitude, z = forward.
        /// </summary>
        public Vector3 MoveDirection { get; }

        /// <summary>Yaw command in [-1, 1]; negative turns left, positive right.</summary>
        public float Yaw { get; }

        /// <summary>True when no movement or rotation was requested.</summary>
        public bool IsIdle { get; }

        public DroneCommand(Vector3 moveDirection, float yaw, bool isIdle)
        {
            MoveDirection = moveDirection;
            Yaw = yaw;
            IsIdle = isIdle;
        }

        public static DroneCommand Idle => new(Vector3.zero, 0f, true);
    }

    /// <summary>
    /// Maps a 4-dimensional continuous action vector to a <see cref="DroneCommand"/>.
    /// </summary>
    /// <remarks>
    /// Action layout (indices are also exposed as constants):
    /// <list type="number">
    /// <item><description><see cref="IndexStrafe"/>: lateral movement, [-1, 1].</description></item>
    /// <item><description><see cref="IndexForward"/>: forward movement, [-1, 1].</description></item>
    /// <item><description><see cref="IndexYaw"/>: rotation, [-1, 1].</description></item>
    /// <item><description><see cref="IndexAltitude"/>: vertical movement, [-1, 1].</description></item>
    /// </list>
    /// </remarks>
    public sealed class DroneActionResolver
    {
        public const int ActionCount = 4;
        public const int IndexStrafe = 0;
        public const int IndexForward = 1;
        public const int IndexYaw = 2;
        public const int IndexAltitude = 3;

        private const float IdleEpsilon = 0.01f;

        /// <summary>
        /// Resolves a raw action buffer into a <see cref="DroneCommand"/>. Returns
        /// <see cref="DroneCommand.Idle"/> when the buffer is too short.
        /// </summary>
        public DroneCommand Resolve(Unity.MLAgents.Actuators.ActionBuffers actions)
        {
            if (actions.ContinuousActions.Length < ActionCount)
                return DroneCommand.Idle;

            var strafe = Mathf.Clamp(actions.ContinuousActions[IndexStrafe], -1f, 1f);
            var forward = Mathf.Clamp(actions.ContinuousActions[IndexForward], -1f, 1f);
            var yaw = Mathf.Clamp(actions.ContinuousActions[IndexYaw], -1f, 1f);
            var altitude = Mathf.Clamp(actions.ContinuousActions[IndexAltitude], -1f, 1f);

            var direction = new Vector3(strafe, altitude, forward);

            if (direction.sqrMagnitude < IdleEpsilon * IdleEpsilon && Mathf.Abs(yaw) < IdleEpsilon)
                return DroneCommand.Idle;

            return new DroneCommand(direction, yaw, false);
        }
    }
}
