namespace ADRL.AI.Decision.Mission
{
    using ADRL.AI.Decision.Memory;

    /// <summary>
    /// The single owner of the current mission task, its transitions, its priority,
    /// its continuity and its state. It sits between behaviour memory and behaviour
    /// selection, answering "what objective should the drone currently pursue?" -
    /// it never decides how to perform that objective (that is the behaviour
    /// selector's job) and never generates movement, motors, physics, rewards, path
    /// planning or mapping.
    /// </summary>
    /// <remarks>
    /// Deterministic by construction: state transitions run through the pure
    /// <see cref="MissionTransitionRules"/> against a caller-supplied step clock, so
    /// the same assessment history always yields the same mission sequence. No
    /// <see cref="UnityEngine.Time"/> dependency, no hidden mutable state beyond the
    /// explicitly-owned current task. Intended for the Unity main thread only, matching
    /// the framework's main-thread assumption.
    /// </remarks>
    public sealed class MissionCoordinator
    {
        private readonly MissionPolicy _policy;
        private MissionTask _current;

        public MissionCoordinator(MissionPolicy policy)
        {
            _policy = policy;
            _current = new MissionTask(MissionTaskState.Idle, 0f, MissionTaskState.Idle, true);
        }

        /// <summary>The mission objective currently being pursued.</summary>
        public MissionTask CurrentTask => _current;

        /// <summary>
        /// Advances the mission one step from the current assessment and behaviour
        /// memory at the given step clock, returning the updated task. The caller
        /// (decision engine) supplies the deterministic clock and the already-updated
        /// memory.
        /// </summary>
        public MissionTask Update(SituationSnapshot assessment, BehaviourMemory memory, float now)
        {
            _current = MissionTransitionRules.Transition(assessment, memory, _current, now, _policy);
            return _current;
        }

        /// <summary>Restores the mission to its initial idle objective.</summary>
        public void Reset()
        {
            _current = new MissionTask(MissionTaskState.Idle, 0f, MissionTaskState.Idle, true);
        }
    }
}
