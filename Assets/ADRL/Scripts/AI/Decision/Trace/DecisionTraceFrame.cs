namespace ADRL.AI.Decision.Trace
{
    using System;
    using ADRL.AI.Decision.Explainability;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// Immutable, deterministic trace of one completed decision step. Produced by
    /// the <see cref="DecisionTraceBuilder"/> - the single owner of trace
    /// composition - after the engine has captured the per-step
    /// <see cref="ADRL.AI.Decision.Context.DecisionContextSnapshot"/> and built its
    /// explanation. It is a structured, replayable record of the whole decision
    /// chain (situation, mission, behaviour, optimization, command, explanation,
    /// diagnostics, winner, counts and reasons) for inspection, regression testing
    /// and debugging. It exists only for observation and replay - it never
    /// influences decision making, prioritization, mission selection, optimization
    /// or execution.
    /// </summary>
    /// <remarks>
    /// This type is a sealed immutable class rather than a readonly struct because
    /// it embeds the step's <see cref="DecisionDiagnostics"/> and
    /// <see cref="DecisionExplanation"/> as value members while the diagnostics
    /// projection itself exposes the last trace frame; if both were value types the
    /// two would form an infinitely sized layout (struct A containing struct B
    /// containing struct A). As a class the frame is a stable reference whose
    /// payload can never change: every field is readonly, the reference payloads
    /// (<see cref="Reasons"/>, <see cref="ExecutorName"/>) are defensively copied or
    /// null-coalesced at construction and never exposed for mutation, and
    /// <see cref="Empty"/> is the canonical shared pre-step frame.
    /// </remarks>
    public sealed class DecisionTraceFrame
    {
        /// <summary>The total number of decisions made up to and including this one.</summary>
        public readonly int DecisionStep;

        /// <summary>The deterministic step-clock value at which the decision was made.</summary>
        public readonly float DecisionTimestamp;

        /// <summary>The situation the decision reasoned over.</summary>
        public readonly SituationSnapshot Assessment;

        /// <summary>The mission objective in effect for the decision.</summary>
        public readonly MissionTask Mission;

        /// <summary>The behaviour selected for the decision.</summary>
        public readonly BehaviourState Behaviour;

        /// <summary>The optimized execution profile of the decision.</summary>
        public readonly BehaviourExecutionProfile OptimizationProfile;

        /// <summary>The command resolved for the decision.</summary>
        public readonly DroneCommand Command;

        /// <summary>The immutable explanation of the decision.</summary>
        public readonly DecisionExplanation Explanation;

        /// <summary>The synchronized diagnostics of the decision.</summary>
        public readonly DecisionDiagnostics Diagnostics;

        /// <summary>The winning objective for the decision.</summary>
        public readonly TaskPriority WinningTask;

        /// <summary>Number of candidate objectives generated for the decision.</summary>
        public readonly int CandidateCount;

        /// <summary>Number of world-knowledge records stored at the decision.</summary>
        public readonly int KnowledgeCount;

        /// <summary>Number of behaviour-memory records stored at the decision.</summary>
        public readonly int MemoryCount;

        /// <summary>
        /// Concrete executor type name that produced the command (for example
        /// <c>AvoidExecutor</c>), or an empty string before any step.
        /// </summary>
        public readonly string ExecutorName;

        /// <summary>Execution confidence of the optimized profile, in [0, 1].</summary>
        public readonly float OptimizationConfidence;

        /// <summary>The narrative reasoning entries of the decision, in deterministic order. Owned copy.</summary>
        public readonly DecisionReason[] Reasons;

        public DecisionTraceFrame(
            int decisionStep,
            float decisionTimestamp,
            SituationSnapshot assessment,
            MissionTask mission,
            BehaviourState behaviour,
            BehaviourExecutionProfile optimizationProfile,
            DroneCommand command,
            DecisionExplanation explanation,
            DecisionDiagnostics diagnostics,
            TaskPriority winningTask,
            int candidateCount,
            int knowledgeCount,
            int memoryCount,
            string executorName,
            float optimizationConfidence,
            DecisionReason[] reasons)
        {
            DecisionStep = decisionStep;
            DecisionTimestamp = decisionTimestamp;
            Assessment = assessment;
            Mission = mission;
            Behaviour = behaviour;
            OptimizationProfile = optimizationProfile;
            Command = command;
            Explanation = explanation;
            Diagnostics = diagnostics;
            WinningTask = winningTask;
            CandidateCount = candidateCount;
            KnowledgeCount = knowledgeCount;
            MemoryCount = memoryCount;
            ExecutorName = executorName ?? string.Empty;
            OptimizationConfidence = optimizationConfidence;
            Reasons = reasons == null
                ? new DecisionReason[0]
                : CopyOf(reasons);
        }

        /// <summary>
        /// True when the frame holds a complete, consistent decision record: the
        /// reference payloads are present, the explanation is valid and the step and
        /// timestamp clocks are non-negative. The canonical
        /// <see cref="Empty"/> frame is valid.
        /// </summary>
        public bool IsValid =>
            ExecutorName != null
            && Reasons != null
            && Explanation.IsValid
            && DecisionStep >= 0
            && DecisionTimestamp >= 0f;

        /// <summary>An empty, pre-step decision trace frame.</summary>
        public static DecisionTraceFrame Empty => _empty;

        private static readonly DecisionTraceFrame _empty = new(
            0,
            0f,
            SituationSnapshot.Invalid,
            MissionTask.Invalid,
            BehaviourState.Idle,
            BehaviourExecutionProfile.Empty,
            DroneCommand.Idle,
            DecisionExplanation.Empty,
            DecisionDiagnostics.Empty,
            TaskPriority.Invalid,
            0,
            0,
            0,
            string.Empty,
            0f,
            new DecisionReason[0]);

        private static DecisionReason[] CopyOf(DecisionReason[] source)
        {
            var owned = new DecisionReason[source.Length];
            Array.Copy(source, owned, source.Length);
            return owned;
        }
    }
}
