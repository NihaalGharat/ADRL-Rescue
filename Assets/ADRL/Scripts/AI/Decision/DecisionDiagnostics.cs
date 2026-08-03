namespace ADRL.AI.Decision
{
    using ADRL.AI.DecisionMaking;
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Knowledge;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using UnityEngine;

    /// <summary>
    /// Immutable, allocation-free snapshot of the decision framework's running
    /// state. Diagnostic only - it never influences future decisions. Together the
    /// fields describe the complete runtime decision chain of the last step, so the
    /// step can be replayed: the step clock, the selected behaviour, the executor
    /// that produced the command, the command itself, the assessed situation, the
    /// mission objective, the prioritizer's winning objective and how many candidate
    /// objectives were generated. Only immutable snapshots are exposed - never
    /// mutable state or internal collections.
    /// </summary>
    public readonly struct DecisionDiagnostics
    {
        /// <summary>Total number of steps carried out so far.</summary>
        public readonly int StepCount;

        /// <summary>Most recently selected behaviour.</summary>
        public readonly BehaviourState LastBehaviour;

        /// <summary>Most recent assessment the selector reasoned over.</summary>
        public readonly SituationSnapshot LastAssessment;

        /// <summary>The current mission objective the coordinator is pursuing.</summary>
        public readonly MissionTask LastMissionTask;

        /// <summary>The prioritizer's winning objective for the last step.</summary>
        public readonly TaskPriority LastWinning;

        /// <summary>
        /// Concrete type name of the executor that produced the last command
        /// (for example <c>AvoidExecutor</c>), or an empty string before any step.
        /// </summary>
        public readonly string SelectedExecutor;

        /// <summary>The resolved command of the last decision step.</summary>
        public readonly DroneCommand LastCommand;

        /// <summary>The deterministic step-clock value at which the last decision was made.</summary>
        public readonly float DecisionTimestamp;

        /// <summary>Number of candidate objectives generated for the last step.</summary>
        public readonly int CandidateCount;

        /// <summary>The deterministic score of the winning objective.</summary>
        public float WinningPriorityScore => LastWinning.PriorityScore;

        /// <summary>Number of world-knowledge records stored at the last step.</summary>
        public readonly int KnowledgeRecordCount;

        /// <summary>Distance to the nearest known-victim record, or 0 when none is known.</summary>
        public readonly float NearestVictimDistance;

        /// <summary>Distance to the nearest known-hazard record, or 0 when none is known.</summary>
        public readonly float NearestHazardDistance;

        /// <summary>The deterministic step-clock value at which world knowledge was last updated.</summary>
        public readonly float KnowledgeTimestamp;

        public DecisionDiagnostics(
            int stepCount,
            BehaviourState lastBehaviour,
            SituationSnapshot lastAssessment,
            MissionTask lastMissionTask = default,
            TaskPriority lastWinning = default,
            string selectedExecutor = "",
            DroneCommand lastCommand = default,
            float decisionTimestamp = 0f,
            int candidateCount = 0,
            int knowledgeRecordCount = 0,
            float nearestVictimDistance = 0f,
            float nearestHazardDistance = 0f,
            float knowledgeTimestamp = 0f)
        {
            StepCount = stepCount;
            LastBehaviour = lastBehaviour;
            LastAssessment = lastAssessment;
            LastMissionTask = lastMissionTask;
            LastWinning = lastWinning;
            SelectedExecutor = selectedExecutor;
            LastCommand = lastCommand;
            DecisionTimestamp = decisionTimestamp;
            CandidateCount = candidateCount;
            KnowledgeRecordCount = knowledgeRecordCount;
            NearestVictimDistance = nearestVictimDistance;
            NearestHazardDistance = nearestHazardDistance;
            KnowledgeTimestamp = knowledgeTimestamp;
        }

        /// <summary>An idle, zero-step diagnostics payload.</summary>
        public static DecisionDiagnostics Empty => new(
            0,
            BehaviourState.Idle,
            SituationSnapshot.Invalid,
            MissionTask.Invalid,
            TaskPriority.Invalid,
            string.Empty,
            DroneCommand.Idle,
            0f,
            0);

        /// <summary>
        /// Projects the diagnostics of the last decision step from the runtime
        /// metadata, the assessed situation, the generated candidate count and the
        /// current world-knowledge store. This is the projection path used by the
        /// Phase 9.0 runtime, so the embedded knowledge fields are synchronized
        /// with the knowledge store by construction.
        /// </summary>
        public static DecisionDiagnostics From(
            DecisionRuntimeState runtime,
            SituationSnapshot assessment,
            int candidateCount,
            WorldKnowledgeStore knowledge)
        {
            var nearestVictim = knowledge.QueryNearest(KnowledgeType.Victim, Vector3.zero);
            var nearestHazard = knowledge.QueryNearest(KnowledgeType.Hazard, Vector3.zero);

            return new DecisionDiagnostics(
                runtime.DecisionStep,
                runtime.CurrentBehaviour,
                assessment,
                runtime.CurrentMission,
                runtime.CurrentWinner,
                runtime.CurrentExecutor,
                runtime.LastCommand,
                runtime.DecisionTimestamp,
                candidateCount,
                knowledge.Count,
                DistanceOf(nearestVictim),
                DistanceOf(nearestHazard),
                runtime.DecisionTimestamp);
        }

        /// <summary>
        /// The legacy projection path (Phase 8.8 contract): projects against an
        /// empty knowledge store, so the knowledge fields are zero.
        /// </summary>
        public static DecisionDiagnostics From(
            DecisionRuntimeState runtime,
            SituationSnapshot assessment,
            int candidateCount)
        {
            return From(runtime, assessment, candidateCount, WorldKnowledgeStore.Empty);
        }

        private static float DistanceOf(WorldKnowledgeRecord record)
        {
            return record.IsValid ? Vector3.Distance(record.Position, Vector3.zero) : 0f;
        }
    }
}
