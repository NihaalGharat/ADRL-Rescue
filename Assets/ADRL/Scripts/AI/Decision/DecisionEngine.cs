namespace ADRL.AI.Decision
{
    using ADRL.AI.DecisionMaking;
    using ADRL.Sensors.Interfaces;
    using UnityEngine;

    /// <summary>
    /// The decision framework's execution entry point. It owns decision making
    /// only: it takes a fused sensor reading, assesses the situation, selects a
    /// behaviour, and produces the <see cref="DroneCommand"/> the existing action
    /// pipeline already knows how to run. It never touches movement, rewards,
    /// simulation, mission, or environment state - those stay with their owners.
    /// </summary>
    /// <remarks>
    /// Deterministic by construction: given the same context and the same fused
    /// reading, the same command is produced.
    /// </remarks>
    public sealed class DecisionEngine
    {
        private readonly DecisionContext _context;
        private readonly ISituationAssessor _assessor;
        private readonly IBehaviourSelector _selector;

        private int _stepCount;
        private BehaviourState _last;
        private SituationSnapshot _lastAssessment;

        public DecisionEngine(DecisionContext context, ISituationAssessor assessor, IBehaviourSelector selector)
        {
            _context = context;
            _assessor = assessor;
            _selector = selector;
            _last = BehaviourState.Idle;
            _lastAssessment = SituationSnapshot.Invalid;
        }

        /// <summary>
        /// Runs one decision step over a fused reading and returns the resolved
        /// command for the existing actuator.
        /// </summary>
        public DecisionResult Decide(ISensorReading fused)
        {
            var assessment = _assessor.Assess(fused);
            var behaviour = _selector.Select(assessment);
            var command = ResolveCommand(behaviour, assessment);

            _stepCount++;
            _last = behaviour;
            _lastAssessment = assessment;

            return new DecisionResult(behaviour, command, assessment);
        }

        /// <summary>Read-only projection of the framework's running state.</summary>
        public DecisionDiagnostics GetDiagnostics()
        {
            return new DecisionDiagnostics(_stepCount, _last, _lastAssessment);
        }

        /// <summary>Restores the framework to a fresh, zero-step state.</summary>
        public void Reset()
        {
            _stepCount = 0;
            _last = BehaviourState.Idle;
            _lastAssessment = SituationSnapshot.Invalid;
        }

        private DroneCommand ResolveCommand(BehaviourState behaviour, SituationSnapshot assessment)
        {
            switch (behaviour)
            {
                case BehaviourState.Idle:
                    return DroneCommand.Idle;

                case BehaviourState.Search:
                    // A gentle, steady forward sweep with no lateral bias.
                    return new DroneCommand(new Vector3(0f, 0f, 0.6f), 0f, false);

                case BehaviourState.Approach:
                    // Steer laterally toward the target side, driven forward.
                    var steer = Mathf.Clamp(assessment.TargetSide * _context.ApproachGain, -1f, 1f);
                    return new DroneCommand(new Vector3(steer, 0f, 0.8f), 0f, false);

                case BehaviourState.Avoid:
                    // Push away from the imminent obstacle while backing off slightly.
                    var evade = Mathf.Clamp(-assessment.ObstacleSide * _context.EvadeGain, -1f, 1f);
                    return new DroneCommand(new Vector3(evade, 0f, -0.3f), 0f, false);

                default:
                    return DroneCommand.Idle;
            }
        }
    }
}