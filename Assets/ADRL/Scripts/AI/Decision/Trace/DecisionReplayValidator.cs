namespace ADRL.AI.Decision.Trace
{
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;
    using UnityEngine;

    /// <summary>
    /// Pure validation of decision-trace replay consistency. It confirms that a
    /// replayed <see cref="DecisionTraceFrame"/> is internally synchronized - the
    /// behaviour, mission, command, explanation, diagnostics, knowledge and
    /// optimization payloads all describe the same completed decision. Every check
    /// returns a plain bool and never throws, so validation is a read-only, side
    /// effect-free observer of replay correctness; it never modifies runtime state.
    /// </summary>
    /// <remarks>
    /// A frame is <see cref="IsValid"/> only when every cross-payload check holds:
    /// the behaviour and mission match their copies inside the explanation, the
    /// explanation's step/timestamp and command match the frame, the synchronized
    /// diagnostics projection matches the step, behaviour, command, mission and
    /// winner, the diagnostics knowledge count matches the frame's knowledge count,
    /// and the optimization confidence is identical across the frame, its profile
    /// and the explanation. The canonical <see cref="DecisionTraceFrame.Empty"/>
    /// frame is valid.
    /// </remarks>
    public static class DecisionReplayValidator
    {
        private const float ConfidenceEpsilon = 1e-6f;

        /// <summary>
        /// True when the frame's reference payloads are present and its clocks are
        /// non-negative. This is the baseline completeness check - the empty frame
        /// satisfies it.
        /// </summary>
        public static bool FrameValid(DecisionTraceFrame frame)
        {
            return frame != null
                && frame.ExecutorName != null
                && frame.Reasons != null
                && frame.Explanation.IsValid
                && frame.DecisionStep >= 0
                && frame.DecisionTimestamp >= 0f;
        }

        /// <summary>True when the frame's behaviour matches the explanation's behaviour.</summary>
        public static bool BehaviourMatchesTrace(DecisionTraceFrame frame)
        {
            return frame != null && frame.Behaviour == frame.Explanation.Behaviour;
        }

        /// <summary>True when the frame's mission matches the explanation's mission.</summary>
        public static bool MissionMatchesTrace(DecisionTraceFrame frame)
        {
            return frame != null && SameMission(frame.Mission, frame.Explanation.Mission);
        }

        /// <summary>
        /// True when the explanation's step clock, timestamp and command match the
        /// frame's own records.
        /// </summary>
        public static bool ExplanationMatchesTrace(DecisionTraceFrame frame)
        {
            return frame != null
                && frame.Explanation.DecisionStep == frame.DecisionStep
                && Mathf.Abs(frame.Explanation.DecisionTimestamp - frame.DecisionTimestamp) <= ConfidenceEpsilon
                && SameCommand(frame.Command, frame.Explanation.Command);
        }

        /// <summary>
        /// True when the synchronized diagnostics projection describes the same
        /// decision as the frame: same step, behaviour, command, mission and winner.
        /// </summary>
        public static bool DiagnosticsSynchronized(DecisionTraceFrame frame)
        {
            return frame != null
                && frame.Diagnostics.StepCount == frame.DecisionStep
                && frame.Diagnostics.LastBehaviour == frame.Behaviour
                && SameCommand(frame.Diagnostics.LastCommand, frame.Command)
                && SameMission(frame.Diagnostics.LastMissionTask, frame.Mission)
                && SamePriority(frame.Diagnostics.LastWinning, frame.WinningTask);
        }

        /// <summary>True when the frame's command matches the command in the explanation.</summary>
        public static bool CommandSynchronized(DecisionTraceFrame frame)
        {
            return frame != null && SameCommand(frame.Command, frame.Explanation.Command);
        }

        /// <summary>True when the synchronized diagnostics knowledge count matches the frame's knowledge count.</summary>
        public static bool KnowledgeSynchronized(DecisionTraceFrame frame)
        {
            return frame != null && frame.Diagnostics.KnowledgeRecordCount == frame.KnowledgeCount;
        }

        /// <summary>
        /// True when the optimization confidence is identical across the frame, its
        /// optimized profile and the explanation's profile.
        /// </summary>
        public static bool OptimizationSynchronized(DecisionTraceFrame frame)
        {
            if (frame == null)
                return false;

            return Mathf.Abs(frame.OptimizationConfidence - frame.OptimizationProfile.ExecutionConfidence) <= ConfidenceEpsilon
                && Mathf.Abs(frame.OptimizationConfidence - frame.Explanation.OptimizationProfile.ExecutionConfidence) <= ConfidenceEpsilon
                && Mathf.Abs(frame.OptimizationConfidence - frame.Diagnostics.LastExplanation.OptimizationProfile.ExecutionConfidence) <= ConfidenceEpsilon;
        }

        /// <summary>
        /// True when the frame passes every consistency check: complete, behaviour
        /// and mission match the explanation, explanation matches the trace, the
        /// diagnostics are synchronized, and command, knowledge and optimization are
        /// all synchronized. No exceptions are ever thrown.
        /// </summary>
        public static bool IsValid(DecisionTraceFrame frame)
        {
            return FrameValid(frame)
                && BehaviourMatchesTrace(frame)
                && MissionMatchesTrace(frame)
                && ExplanationMatchesTrace(frame)
                && DiagnosticsSynchronized(frame)
                && CommandSynchronized(frame)
                && KnowledgeSynchronized(frame)
                && OptimizationSynchronized(frame);
        }

        private static bool SameMission(MissionTask a, MissionTask b)
        {
            return a.State == b.State
                && a.EntryStep == b.EntryStep
                && a.PreviousState == b.PreviousState
                && a.IsValid == b.IsValid;
        }

        private static bool SamePriority(TaskPriority a, TaskPriority b)
        {
            return SameMission(a.Task, b.Task)
                && a.PriorityScore == b.PriorityScore
                && a.Confidence == b.Confidence
                && a.IsValid == b.IsValid;
        }

        private static bool SameCommand(DroneCommand a, DroneCommand b)
        {
            return a.IsIdle == b.IsIdle
                && a.Yaw == b.Yaw
                && a.MoveDirection == b.MoveDirection;
        }
    }
}
