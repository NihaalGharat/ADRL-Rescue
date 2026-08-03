namespace ADRL.AI.Decision
{
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// Immutable result of a single decision step. It couples the chosen
    /// <see cref="BehaviourState"/> with the <see cref="DroneCommand"/> that the
    /// existing action pipeline (<c>DroneActionResolver</c>/<c>DroneController</c>)
    /// already knows how to execute. The framework produces this; it never runs
    /// the drone.
    /// </summary>
    public readonly struct DecisionResult
    {
        /// <summary>The behaviour selected for this step.</summary>
        public readonly BehaviourState Behaviour;

        /// <summary>The resolved command handed to the existing actuator.</summary>
        public readonly DroneCommand Command;

        /// <summary>The assessment the selector reasoned over.</summary>
        public readonly SituationSnapshot Assessment;

        public DecisionResult(BehaviourState behaviour, DroneCommand command, SituationSnapshot assessment)
        {
            Behaviour = behaviour;
            Command = command;
            Assessment = assessment;
        }
    }
}