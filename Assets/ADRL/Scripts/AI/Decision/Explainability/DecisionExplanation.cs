namespace ADRL.AI.Decision.Explainability
{
    using System;
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Optimization;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// Immutable, deterministic explanation of one completed decision step. It is
    /// a read-only projection of the per-step
    /// <see cref="ADRL.AI.Decision.Context.DecisionContextSnapshot"/> produced by
    /// the <see cref="DecisionExplanationBuilder"/> after the engine captures the
    /// snapshot - it never influences runtime behaviour, scoring, selection,
    /// optimization or execution. Every member is readonly and every array is an
    /// owned copy, so a built explanation can never alias or mutate the snapshot it
    /// was derived from.
    /// </summary>
    /// <remarks>
    /// The explanation carries the assessed situation, the knowledge and memory
    /// summaries, the mission objective, the ordered candidate objectives, the
    /// full scored priority list, the winning objective, the selected behaviour,
    /// the executor that produced the command, the optimized execution profile,
    /// the final command and the decision's deterministic timestamp and step.
    /// <see cref="Reasons"/> holds the narrative reasoning entries that the
    /// <see cref="DecisionExplanationFormatter"/> renders into the deterministic,
    /// human-readable form. <see cref="Empty"/> is the canonical pre-step
    /// explanation.
    /// </remarks>
    public readonly struct DecisionExplanation
    {
        /// <summary>The situation the last decision reasoned over.</summary>
        public readonly SituationSnapshot Assessment;

        /// <summary>The world-knowledge summary in effect for the last decision.</summary>
        public readonly KnowledgeSummary Knowledge;

        /// <summary>The behaviour-memory summary in effect for the last decision.</summary>
        public readonly MemorySummary Memory;

        /// <summary>The mission objective in effect for the last decision.</summary>
        public readonly MissionTask Mission;

        /// <summary>
        /// The ordered candidate objectives generated for the last decision, as
        /// their mission states. Owned copy - never a shared reference.
        /// </summary>
        public readonly MissionTaskState[] CandidateTasks;

        /// <summary>
        /// The full scored priority list of the last decision, in evaluation
        /// order. Owned copy - never a shared reference.
        /// </summary>
        public readonly TaskPriority[] ScoredCandidates;

        /// <summary>The winning objective for the last decision.</summary>
        public readonly TaskPriority Winning;

        /// <summary>The behaviour selected for the last decision.</summary>
        public readonly BehaviourState Behaviour;

        /// <summary>
        /// Concrete type name of the executor that produced the last command
        /// (for example <c>AvoidExecutor</c>), or an empty string before any step.
        /// </summary>
        public readonly string Executor;

        /// <summary>The optimized execution profile of the last decision.</summary>
        public readonly BehaviourExecutionProfile OptimizationProfile;

        /// <summary>The command resolved for the last decision.</summary>
        public readonly DroneCommand Command;

        /// <summary>The deterministic step-clock value at which the decision was made.</summary>
        public readonly float DecisionTimestamp;

        /// <summary>The total number of decisions made so far.</summary>
        public readonly int DecisionStep;

        /// <summary>The narrative reasoning entries of the last decision, in deterministic order.</summary>
        public readonly DecisionReason[] Reasons;

        public DecisionExplanation(
            SituationSnapshot assessment,
            KnowledgeSummary knowledge,
            MemorySummary memory,
            MissionTask mission,
            MissionTaskState[] candidateTasks,
            TaskPriority[] scoredCandidates,
            TaskPriority winning,
            BehaviourState behaviour,
            string executor,
            BehaviourExecutionProfile optimizationProfile,
            DroneCommand command,
            float decisionTimestamp,
            int decisionStep,
            DecisionReason[] reasons)
        {
            Assessment = assessment;
            Knowledge = knowledge;
            Memory = memory;
            Mission = mission;
            CandidateTasks = candidateTasks ?? Array.Empty<MissionTaskState>();
            ScoredCandidates = scoredCandidates ?? Array.Empty<TaskPriority>();
            Winning = winning;
            Behaviour = behaviour;
            Executor = executor ?? string.Empty;
            OptimizationProfile = optimizationProfile;
            Command = command;
            DecisionTimestamp = decisionTimestamp;
            DecisionStep = decisionStep;
            Reasons = reasons ?? Array.Empty<DecisionReason>();
        }

        /// <summary>True when every component payload of the explanation is present.</summary>
        public bool IsValid =>
            CandidateTasks != null
            && ScoredCandidates != null
            && Reasons != null
            && Executor != null
            && DecisionStep >= 0
            && DecisionTimestamp >= 0f;

        /// <summary>An empty, pre-step decision explanation.</summary>
        public static DecisionExplanation Empty => new(
            SituationSnapshot.Invalid,
            KnowledgeSummary.Empty,
            MemorySummary.Empty,
            MissionTask.Invalid,
            Array.Empty<MissionTaskState>(),
            Array.Empty<TaskPriority>(),
            TaskPriority.Invalid,
            BehaviourState.Idle,
            string.Empty,
            BehaviourExecutionProfile.Empty,
            DroneCommand.Idle,
            0f,
            0,
            Array.Empty<DecisionReason>());

        /// <summary>
        /// Immutable summary of the world-knowledge store in effect for a decision:
        /// the total record count, the per-type counts and the nearest known
        /// victim/hazard distances. Read-only diagnostic data - never influences
        /// decisions.
        /// </summary>
        public readonly struct KnowledgeSummary
        {
            /// <summary>Number of world-knowledge records stored at the decision.</summary>
            public readonly int RecordCount;

            /// <summary>Number of known-victim records stored at the decision.</summary>
            public readonly int KnownVictims;

            /// <summary>Number of known-hazard records stored at the decision.</summary>
            public readonly int KnownHazards;

            /// <summary>Number of known-obstacle records stored at the decision.</summary>
            public readonly int KnownObstacles;

            /// <summary>Distance to the nearest known-victim record, or 0 when none is known.</summary>
            public readonly float NearestVictimDistance;

            /// <summary>Distance to the nearest known-hazard record, or 0 when none is known.</summary>
            public readonly float NearestHazardDistance;

            public KnowledgeSummary(
                int recordCount,
                int knownVictims,
                int knownHazards,
                int knownObstacles,
                float nearestVictimDistance,
                float nearestHazardDistance)
            {
                RecordCount = recordCount;
                KnownVictims = knownVictims;
                KnownHazards = knownHazards;
                KnownObstacles = knownObstacles;
                NearestVictimDistance = nearestVictimDistance;
                NearestHazardDistance = nearestHazardDistance;
            }

            /// <summary>An empty knowledge summary.</summary>
            public static KnowledgeSummary Empty => new(0, 0, 0, 0, 0f, 0f);
        }

        /// <summary>
        /// Immutable summary of the behaviour-memory in effect for a decision: the
        /// last behaviour and the stored victim/obstacle records, plus the total
        /// record count. Read-only diagnostic data - never influences decisions.
        /// </summary>
        public readonly struct MemorySummary
        {
            /// <summary>The most recently recorded behaviour.</summary>
            public readonly BehaviourState LastBehaviour;

            /// <summary>The stored victim record, or <see cref="MemoryRecord.Invalid"/> when absent.</summary>
            public readonly MemoryRecord LastVictimSeen;

            /// <summary>The stored obstacle record, or <see cref="MemoryRecord.Invalid"/> when absent.</summary>
            public readonly MemoryRecord LastObstacleSeen;

            /// <summary>Number of currently stored, valid memory records.</summary>
            public readonly int RecordCount;

            public MemorySummary(
                BehaviourState lastBehaviour,
                MemoryRecord lastVictimSeen,
                MemoryRecord lastObstacleSeen,
                int recordCount)
            {
                LastBehaviour = lastBehaviour;
                LastVictimSeen = lastVictimSeen;
                LastObstacleSeen = lastObstacleSeen;
                RecordCount = recordCount;
            }

            /// <summary>An empty memory summary.</summary>
            public static MemorySummary Empty =>
                new(BehaviourState.Idle, MemoryRecord.Invalid, MemoryRecord.Invalid, 0);
        }
    }
}
