namespace ADRL.AI.Decision.Context
{
    using System;
    using ADRL.AI.Decision.Knowledge;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// Immutable runtime snapshot that unifies every subsystem's output for one
    /// decision step into a single consistent context: the assessed situation, the
    /// behaviour memory at that step, the mission objective, the generated
    /// candidates, the winning objective, the selected behaviour, the resolved
    /// command, the synchronized diagnostics and the runtime metadata. Produced by
    /// the <see cref="DecisionContextBuilder"/>, the single owner of context
    /// composition; consumed read-only by validation, diagnostics and the smoke
    /// test. Never mutated in place.
    /// </summary>
    /// <remarks>
    /// All fields are readonly and the snapshot is never mutated after construction.
    /// The candidate array is an owned copy of the engine's working buffer, so later
    /// pipeline activity cannot change a built snapshot. The behaviour memory is
    /// stored by reference: the snapshot holds the memory state as of its build step
    /// and never mutates it - only the <see cref="BehaviourMemoryService"/> mutates
    /// memory. <see cref="Candidates"/> and <see cref="Memory"/> are null-safe;
    /// <see cref="Empty"/> is the canonical pre-step snapshot.
    /// </remarks>
    public readonly struct DecisionContextSnapshot
    {
        /// <summary>The situation the last decision reasoned over.</summary>
        public readonly SituationSnapshot Assessment;

        /// <summary>The behaviour memory in effect for the last decision.</summary>
        public readonly BehaviourMemory Memory;

        /// <summary>The mission objective in effect for the last decision.</summary>
        public readonly MissionTask Mission;

        /// <summary>The candidate objectives generated for the last decision.</summary>
        public readonly TaskCandidate[] Candidates;

        /// <summary>The winning objective for the last decision.</summary>
        public readonly TaskPriority Winning;

        /// <summary>The behaviour selected for the last decision.</summary>
        public readonly BehaviourState Behaviour;

        /// <summary>The command resolved for the last decision.</summary>
        public readonly DroneCommand Command;

        /// <summary>The synchronized diagnostics of the last decision.</summary>
        public readonly DecisionDiagnostics Diagnostics;

        /// <summary>The runtime metadata of the last decision.</summary>
        public readonly DecisionRuntimeState RuntimeState;

        /// <summary>
        /// The execution profile the last decision optimized for, or
        /// <see cref="BehaviourExecutionProfile.Empty"/> before any step or for an
        /// idle step. Carried so the snapshot is the complete per-step context -
        /// the optimizer and the executors both consume it.
        /// </summary>
        public readonly BehaviourExecutionProfile ExecutionProfile;

        /// <summary>
        /// The persistent world-knowledge store in effect for the last decision.
        /// Read-only for consumers; the <see cref="KnowledgeUpdater"/> is the only
        /// writer. Mirrors the behaviour-memory contract: the snapshot holds the
        /// store as of its build step and never mutates it.
        /// </summary>
        public readonly WorldKnowledgeStore Knowledge;

        public DecisionContextSnapshot(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask mission,
            TaskCandidate[] candidates,
            TaskPriority winning,
            BehaviourState behaviour,
            DroneCommand command,
            DecisionDiagnostics diagnostics,
            DecisionRuntimeState runtimeState)
            : this(
                assessment,
                memory,
                mission,
                candidates,
                winning,
                behaviour,
                command,
                diagnostics,
                runtimeState,
                BehaviourExecutionProfile.Empty,
                WorldKnowledgeStore.Empty)
        {
        }

        public DecisionContextSnapshot(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask mission,
            TaskCandidate[] candidates,
            TaskPriority winning,
            BehaviourState behaviour,
            DroneCommand command,
            DecisionDiagnostics diagnostics,
            DecisionRuntimeState runtimeState,
            BehaviourExecutionProfile executionProfile)
            : this(
                assessment,
                memory,
                mission,
                candidates,
                winning,
                behaviour,
                command,
                diagnostics,
                runtimeState,
                executionProfile,
                WorldKnowledgeStore.Empty)
        {
        }

        public DecisionContextSnapshot(
            SituationSnapshot assessment,
            BehaviourMemory memory,
            MissionTask mission,
            TaskCandidate[] candidates,
            TaskPriority winning,
            BehaviourState behaviour,
            DroneCommand command,
            DecisionDiagnostics diagnostics,
            DecisionRuntimeState runtimeState,
            BehaviourExecutionProfile executionProfile,
            WorldKnowledgeStore knowledge)
        {
            Assessment = assessment;
            Memory = memory;
            Mission = mission;
            Candidates = candidates;
            Winning = winning;
            Behaviour = behaviour;
            Command = command;
            Diagnostics = diagnostics;
            RuntimeState = runtimeState;
            ExecutionProfile = executionProfile;
            Knowledge = knowledge ?? WorldKnowledgeStore.Empty;
        }

        /// <summary>
        /// Returns a copy of this snapshot with the given execution profile. The
        /// snapshot is immutable, so this never mutates the original - it composes a
        /// fresh value carrying the profile as the final element of the decision
        /// chain, produced by the single owner of context composition.
        /// </summary>
        public DecisionContextSnapshot WithExecutionProfile(BehaviourExecutionProfile executionProfile)
        {
            return new DecisionContextSnapshot(
                Assessment,
                Memory,
                Mission,
                Candidates,
                Winning,
                Behaviour,
                Command,
                Diagnostics,
                RuntimeState,
                executionProfile,
                Knowledge);
        }

        /// <summary>
        /// Returns a copy of this snapshot carrying the given world-knowledge
        /// store. The snapshot is immutable, so this never mutates the original -
        /// it composes a fresh value embedding the store produced by the
        /// <see cref="KnowledgeUpdater"/> as part of the complete per-step context.
        /// </summary>
        public DecisionContextSnapshot WithKnowledge(WorldKnowledgeStore knowledge)
        {
            return new DecisionContextSnapshot(
                Assessment,
                Memory,
                Mission,
                Candidates,
                Winning,
                Behaviour,
                Command,
                Diagnostics,
                RuntimeState,
                ExecutionProfile,
                knowledge);
        }

        /// <summary>An empty, pre-step context snapshot.</summary>
        public static DecisionContextSnapshot Empty => new(
            SituationSnapshot.Invalid,
            BehaviourMemory.Empty,
            MissionTask.Invalid,
            Array.Empty<TaskCandidate>(),
            TaskPriority.Invalid,
            BehaviourState.Idle,
            DroneCommand.Idle,
            DecisionDiagnostics.Empty,
            DecisionRuntimeState.Empty,
            BehaviourExecutionProfile.Empty,
            WorldKnowledgeStore.Empty);
    }
}
