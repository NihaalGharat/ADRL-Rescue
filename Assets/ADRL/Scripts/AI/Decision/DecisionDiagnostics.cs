namespace ADRL.AI.Decision
{
    using ADRL.AI.DecisionMaking;
    using ADRL.AI.Decision.Advisory;
    using ADRL.AI.Decision.Analytics;
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Evaluation;
    using ADRL.AI.Decision.Explainability;
    using ADRL.AI.Decision.Knowledge;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.Decision.Telemetry;
    using ADRL.AI.Decision.Trace;
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

        /// <summary>
        /// The deterministic explanation of the last decision step, or
        /// <see cref="DecisionExplanation.Empty"/> before any step. Carried so the
        /// diagnostics projection also exposes the human-readable explanation
        /// (winner, behaviour, executor, command and the narrative reasons) - it is
        /// read-only and never influences decisions.
        /// </summary>
        public readonly DecisionExplanation LastExplanation;

        /// <summary>
        /// The immutable, replayable trace frame of the last decision step, or
        /// <see cref="DecisionTraceFrame.Empty"/> before any step. Carried so the
        /// diagnostics projection also exposes the structured trace record of the
        /// step for replay and inspection - it is read-only and never influences
        /// decisions.
        /// </summary>
        public readonly DecisionTraceFrame LastTraceFrame;

        /// <summary>
        /// The immutable telemetry snapshot of all decision steps observed so far,
        /// or <see cref="DecisionTelemetrySnapshot.Empty"/> before any step.
        /// Carried so the diagnostics projection also exposes the health and
        /// performance telemetry of the decision pipeline - it is read-only and
        /// never influences decisions.
        /// </summary>
        public readonly DecisionTelemetrySnapshot LastTelemetry;

        /// <summary>
        /// The immutable analytics snapshot computed from the decision telemetry,
        /// or <see cref="DecisionAnalyticsSnapshot.Empty"/> before any step.
        /// Carried so the diagnostics projection also exposes the engineering
        /// analytics (health, balance, entropy, utilization) of the decision
        /// pipeline - it is read-only and never influences decisions.
        /// </summary>
        public readonly DecisionAnalyticsSnapshot LastAnalytics;

        /// <summary>
        /// The immutable quality-evaluation snapshot computed from the decision
        /// analytics, telemetry, trace and explanation, or
        /// <see cref="DecisionEvaluationSnapshot.Empty"/> before any step.
        /// Carried so the diagnostics projection also exposes the decision quality
        /// evaluation (overall score, grade, suitability, confidence, optimization,
        /// knowledge and consistency) of the decision pipeline - it is read-only
        /// and never influences decisions.
        /// </summary>
        public readonly DecisionEvaluationSnapshot LastEvaluation;

        /// <summary>
        /// The immutable advisory snapshot computed from the decision evaluation,
        /// analytics, telemetry, trace and explanation, or
        /// <see cref="DecisionAdvisorySnapshot.Empty"/> before any step. Carried
        /// so the diagnostics projection also exposes the recommended actions
        /// (overall recommendation, ordered recommendations, confidence,
        /// requires-attention and status) of the decision pipeline - it is
        /// read-only and never influences decisions.
        /// </summary>
        public readonly DecisionAdvisorySnapshot LastAdvisory;

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
            float knowledgeTimestamp = 0f,
            DecisionExplanation lastExplanation = default,
            DecisionTraceFrame lastTraceFrame = null,
            DecisionTelemetrySnapshot lastTelemetry = default,
            DecisionAnalyticsSnapshot lastAnalytics = default,
            DecisionEvaluationSnapshot lastEvaluation = default,
            DecisionAdvisorySnapshot lastAdvisory = default)
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
            LastExplanation = lastExplanation;
            LastTraceFrame = lastTraceFrame ?? DecisionTraceFrame.Empty;
            LastTelemetry = lastTelemetry.BehaviourDistribution == null
                || lastTelemetry.MissionDistribution == null
                ? DecisionTelemetrySnapshot.Empty
                : lastTelemetry;
            LastAnalytics = lastAnalytics.BehaviourAnalytics == null
                || lastAnalytics.MissionAnalytics == null
                ? DecisionAnalyticsSnapshot.Empty
                : lastAnalytics;
            LastEvaluation = lastEvaluation.EvaluationGrade == null
                ? DecisionEvaluationSnapshot.Empty
                : lastEvaluation;
            LastAdvisory = lastAdvisory.Recommendations == null
                ? DecisionAdvisorySnapshot.Empty
                : lastAdvisory;
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
            0,
            0,
            0f,
            0f,
            0f,
            DecisionExplanation.Empty,
            DecisionTraceFrame.Empty,
            DecisionTelemetrySnapshot.Empty,
            DecisionAnalyticsSnapshot.Empty,
            DecisionEvaluationSnapshot.Empty,
            DecisionAdvisorySnapshot.Empty);

        /// <summary>
        /// Returns a copy of this diagnostics payload carrying the given decision
        /// explanation. The payload is immutable, so this never mutates the
        /// original - it composes a fresh value with the explanation the engine
        /// builds from the last snapshot, so diagnostics and explanation stay
        /// synchronized without changing any decision behaviour.
        /// </summary>
        public DecisionDiagnostics WithExplanation(DecisionExplanation lastExplanation)
        {
            return new DecisionDiagnostics(
                StepCount,
                LastBehaviour,
                LastAssessment,
                LastMissionTask,
                LastWinning,
                SelectedExecutor,
                LastCommand,
                DecisionTimestamp,
                CandidateCount,
                KnowledgeRecordCount,
                NearestVictimDistance,
                NearestHazardDistance,
                KnowledgeTimestamp,
                lastExplanation,
                LastTraceFrame,
                LastTelemetry,
                LastAnalytics,
                LastEvaluation,
                LastAdvisory);
        }

        /// <summary>
        /// Returns a copy of this diagnostics payload carrying the given trace
        /// frame. The payload is immutable, so this never mutates the original - it
        /// composes a fresh value with the trace frame the engine builds from the
        /// last snapshot, so diagnostics and trace stay synchronized without
        /// changing any decision behaviour.
        /// </summary>
        public DecisionDiagnostics WithTraceFrame(DecisionTraceFrame lastTraceFrame)
        {
            return new DecisionDiagnostics(
                StepCount,
                LastBehaviour,
                LastAssessment,
                LastMissionTask,
                LastWinning,
                SelectedExecutor,
                LastCommand,
                DecisionTimestamp,
                CandidateCount,
                KnowledgeRecordCount,
                NearestVictimDistance,
                NearestHazardDistance,
                KnowledgeTimestamp,
                LastExplanation,
                lastTraceFrame,
                LastTelemetry,
                LastAnalytics,
                LastEvaluation,
                LastAdvisory);
        }

        /// <summary>
        /// Returns a copy of this diagnostics payload carrying the given telemetry
        /// snapshot. The payload is immutable, so this never mutates the original -
        /// it composes a fresh value with the telemetry snapshot the engine builds
        /// from the last trace frame, so diagnostics and telemetry stay
        /// synchronized without changing any decision behaviour.
        /// </summary>
        public DecisionDiagnostics WithTelemetry(DecisionTelemetrySnapshot lastTelemetry)
        {
            return new DecisionDiagnostics(
                StepCount,
                LastBehaviour,
                LastAssessment,
                LastMissionTask,
                LastWinning,
                SelectedExecutor,
                LastCommand,
                DecisionTimestamp,
                CandidateCount,
                KnowledgeRecordCount,
                NearestVictimDistance,
                NearestHazardDistance,
                KnowledgeTimestamp,
                LastExplanation,
                LastTraceFrame,
                lastTelemetry,
                LastAnalytics,
                LastEvaluation);
        }

        /// <summary>
        /// Returns a copy of this diagnostics payload carrying the given analytics
        /// snapshot. The payload is immutable, so this never mutates the original -
        /// it composes a fresh value with the analytics snapshot the engine
        /// computes from the last telemetry snapshot, so diagnostics and analytics
        /// stay synchronized without changing any decision behaviour.
        /// </summary>
        public DecisionDiagnostics WithAnalytics(DecisionAnalyticsSnapshot lastAnalytics)
        {
            return new DecisionDiagnostics(
                StepCount,
                LastBehaviour,
                LastAssessment,
                LastMissionTask,
                LastWinning,
                SelectedExecutor,
                LastCommand,
                DecisionTimestamp,
                CandidateCount,
                KnowledgeRecordCount,
                NearestVictimDistance,
                NearestHazardDistance,
                KnowledgeTimestamp,
                LastExplanation,
                LastTraceFrame,
                LastTelemetry,
                lastAnalytics,
                LastEvaluation,
                LastAdvisory);
        }

        /// <summary>
        /// Returns a copy of this diagnostics payload carrying the given quality
        /// evaluation snapshot. The payload is immutable, so this never mutates the
        /// original - it composes a fresh value with the evaluation snapshot the
        /// engine computes from the last analytics, telemetry, trace and
        /// explanation, so diagnostics and evaluation stay synchronized without
        /// changing any decision behaviour.
        /// </summary>
        public DecisionDiagnostics WithEvaluation(DecisionEvaluationSnapshot lastEvaluation)
        {
            return new DecisionDiagnostics(
                StepCount,
                LastBehaviour,
                LastAssessment,
                LastMissionTask,
                LastWinning,
                SelectedExecutor,
                LastCommand,
                DecisionTimestamp,
                CandidateCount,
                KnowledgeRecordCount,
                NearestVictimDistance,
                NearestHazardDistance,
                KnowledgeTimestamp,
                LastExplanation,
                LastTraceFrame,
                LastTelemetry,
                LastAnalytics,
                lastEvaluation,
                LastAdvisory);
        }

        /// <summary>
        /// Returns a copy of this diagnostics payload carrying the given advisory
        /// snapshot. The payload is immutable, so this never mutates the original -
        /// it composes a fresh value with the advisory snapshot the engine computes
        /// from the last evaluation, analytics, telemetry, trace and explanation,
        /// so diagnostics and advisory stay synchronized without changing any
        /// decision behaviour.
        /// </summary>
        public DecisionDiagnostics WithAdvisory(DecisionAdvisorySnapshot lastAdvisory)
        {
            return new DecisionDiagnostics(
                StepCount,
                LastBehaviour,
                LastAssessment,
                LastMissionTask,
                LastWinning,
                SelectedExecutor,
                LastCommand,
                DecisionTimestamp,
                CandidateCount,
                KnowledgeRecordCount,
                NearestVictimDistance,
                NearestHazardDistance,
                KnowledgeTimestamp,
                LastExplanation,
                LastTraceFrame,
                LastTelemetry,
                LastAnalytics,
                LastEvaluation,
                lastAdvisory);
        }

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
