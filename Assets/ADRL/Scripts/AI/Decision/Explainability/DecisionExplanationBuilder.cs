namespace ADRL.AI.Decision.Explainability
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Memory;
    using ADRL.AI.Decision.Mission;
    using ADRL.AI.Decision.Prioritization;
    using ADRL.AI.DecisionMaking;

    /// <summary>
    /// The single owner of <see cref="DecisionExplanation"/> composition. It is a
    /// pure, stateless function: given the same <see cref="DecisionContextSnapshot"/>
    /// it always produces the same explanation, and it holds no state between calls.
    /// It reads the built snapshot only - it never scores, arbitrates, selects,
    /// transitions or mutates runtime state - and it defensively copies every array
    /// so the explanation never aliases the snapshot's buffers. Explainability is
    /// strictly observational: building an explanation never influences the
    /// decision chain.
    /// </summary>
    public sealed class DecisionExplanationBuilder
    {
        /// <summary>
        /// Builds the deterministic explanation of the given context snapshot.
        /// </summary>
        public DecisionExplanation Build(DecisionContextSnapshot snapshot)
        {
            var candidateTasks = CandidateStates(snapshot);
            var scored = ScoredCandidates(snapshot);
            var knowledge = KnowledgeSummary(snapshot);
            var memory = MemorySummary(snapshot);

            return new DecisionExplanation(
                snapshot.Assessment,
                knowledge,
                memory,
                snapshot.Mission,
                candidateTasks,
                scored,
                snapshot.Winning,
                snapshot.Behaviour,
                snapshot.RuntimeState.CurrentExecutor ?? string.Empty,
                snapshot.ExecutionProfile,
                snapshot.Command,
                snapshot.Diagnostics.DecisionTimestamp,
                snapshot.RuntimeState.DecisionStep,
                BuildReasons(snapshot, candidateTasks, scored));
        }

        /// <summary>
        /// The ordered candidate objective states, as an owned copy. A null or
        /// empty candidate buffer produces an empty array.
        /// </summary>
        private static MissionTaskState[] CandidateStates(DecisionContextSnapshot snapshot)
        {
            var source = snapshot.Candidates;
            if (source == null || source.Length == 0)
                return Array.Empty<MissionTaskState>();

            var result = new MissionTaskState[source.Length];
            for (var i = 0; i < source.Length; i++)
                result[i] = source[i].Task.State;

            return result;
        }

        /// <summary>
        /// The full scored priority list, as an owned copy. A null or empty buffer
        /// produces an empty array.
        /// </summary>
        private static TaskPriority[] ScoredCandidates(DecisionContextSnapshot snapshot)
        {
            var source = snapshot.ScoredCandidates;
            if (source == null || source.Length == 0)
                return Array.Empty<TaskPriority>();

            var result = new TaskPriority[source.Length];
            Array.Copy(source, result, source.Length);
            return result;
        }

        private static DecisionExplanation.KnowledgeSummary KnowledgeSummary(DecisionContextSnapshot snapshot)
        {
            var knowledge = snapshot.Knowledge;
            if (knowledge == null)
                return DecisionExplanation.KnowledgeSummary.Empty;

            return new DecisionExplanation.KnowledgeSummary(
                knowledge.Count,
                knowledge.CountOf(ADRL.AI.Decision.Knowledge.KnowledgeType.Victim),
                knowledge.CountOf(ADRL.AI.Decision.Knowledge.KnowledgeType.Hazard),
                knowledge.CountOf(ADRL.AI.Decision.Knowledge.KnowledgeType.Obstacle),
                snapshot.Diagnostics.NearestVictimDistance,
                snapshot.Diagnostics.NearestHazardDistance);
        }

        private static DecisionExplanation.MemorySummary MemorySummary(DecisionContextSnapshot snapshot)
        {
            var memory = snapshot.Memory;
            if (memory == null)
                return DecisionExplanation.MemorySummary.Empty;

            return new DecisionExplanation.MemorySummary(
                memory.LastBehaviour,
                memory.LastVictimSeen,
                memory.LastObstacleSeen,
                memory.Count);
        }

        /// <summary>
        /// The narrative reasoning entries of the decision, composed in the fixed
        /// pipeline order (Assessment → Knowledge → Mission → Candidates → Priority
        /// → Winner → Behaviour → Executor → Optimization → Command). Numeric text
        /// is formatted with the invariant culture so the output is byte-identical
        /// across locales.
        /// </summary>
        private static DecisionReason[] BuildReasons(
            DecisionContextSnapshot snapshot,
            MissionTaskState[] candidateTasks,
            TaskPriority[] scored)
        {
            var reasons = new List<DecisionReason>();

            if (snapshot.Assessment.TargetDetected)
                reasons.Add(new DecisionReason(
                    "Assessment",
                    "Target detected (" + snapshot.Assessment.TargetProximity.ToString("F2", CultureInfo.InvariantCulture) + ")"));
            else
                reasons.Add(new DecisionReason("Assessment", "No target detected"));

            if (snapshot.Assessment.ObstacleProximity > 0f)
                reasons.Add(new DecisionReason(
                    "Assessment",
                    "Obstacle proximity (" + snapshot.Assessment.ObstacleProximity.ToString("F2", CultureInfo.InvariantCulture) + ")"));

            var knowledge = snapshot.Knowledge;
            if (knowledge != null && knowledge.Count > 0)
            {
                if (knowledge.CountOf(ADRL.AI.Decision.Knowledge.KnowledgeType.Victim) > 0)
                    reasons.Add(new DecisionReason(
                        "Knowledge",
                        "Known Victims: " + knowledge.CountOf(ADRL.AI.Decision.Knowledge.KnowledgeType.Victim)));
                if (knowledge.CountOf(ADRL.AI.Decision.Knowledge.KnowledgeType.Hazard) > 0)
                    reasons.Add(new DecisionReason(
                        "Knowledge",
                        "Known Hazards: " + knowledge.CountOf(ADRL.AI.Decision.Knowledge.KnowledgeType.Hazard)));
                if (knowledge.CountOf(ADRL.AI.Decision.Knowledge.KnowledgeType.Obstacle) > 0)
                    reasons.Add(new DecisionReason(
                        "Knowledge",
                        "Known Obstacles: " + knowledge.CountOf(ADRL.AI.Decision.Knowledge.KnowledgeType.Obstacle)));
            }
            else
            {
                reasons.Add(new DecisionReason("Knowledge", "No records stored"));
            }

            reasons.Add(new DecisionReason("Mission", snapshot.Mission.State.ToString()));

            for (var i = 0; i < candidateTasks.Length; i++)
                reasons.Add(new DecisionReason("Candidates", candidateTasks[i].ToString()));

            for (var i = 0; i < scored.Length; i++)
                reasons.Add(new DecisionReason(
                    "Priority",
                    scored[i].Task.State.ToString() + " : " + scored[i].PriorityScore.ToString("F2", CultureInfo.InvariantCulture)));

            if (snapshot.Winning.IsValid)
                reasons.Add(new DecisionReason("Winner", snapshot.Winning.Task.State.ToString()));
            else
                reasons.Add(new DecisionReason("Winner", "None"));

            reasons.Add(new DecisionReason("Behaviour", snapshot.Behaviour.ToString()));

            var executor = snapshot.RuntimeState.CurrentExecutor;
            if (!string.IsNullOrEmpty(executor))
                reasons.Add(new DecisionReason("Executor", executor));

            if (!snapshot.ExecutionProfile.IsEmpty)
            {
                reasons.Add(new DecisionReason(
                    "Optimization",
                    "Speed x" + snapshot.ExecutionProfile.SpeedMultiplier.ToString("F2", CultureInfo.InvariantCulture)));
                reasons.Add(new DecisionReason(
                    "Optimization",
                    "Turn Rate x" + snapshot.ExecutionProfile.TurnRateMultiplier.ToString("F2", CultureInfo.InvariantCulture)));
            }
            else
            {
                reasons.Add(new DecisionReason("Optimization", "No optimization"));
            }

            reasons.Add(new DecisionReason("Command", DescribeCommand(snapshot.Command)));

            return reasons.ToArray();
        }

        /// <summary>
        /// A deterministic, human-readable description of a resolved command.
        /// </summary>
        private static string DescribeCommand(DroneCommand command)
        {
            if (command.IsIdle)
                return "Idle";

            var parts = new List<string>();
            if (command.MoveDirection.z > 0.01f)
                parts.Add("Move Forward");
            if (command.MoveDirection.z < -0.01f)
                parts.Add("Move Backward");
            if (command.MoveDirection.x > 0.01f)
                parts.Add("Strafe Right");
            if (command.MoveDirection.x < -0.01f)
                parts.Add("Strafe Left");
            if (command.MoveDirection.y > 0.01f)
                parts.Add("Ascend");
            if (command.MoveDirection.y < -0.01f)
                parts.Add("Descend");
            if (command.Yaw > 0.01f)
                parts.Add("Yaw Right");
            if (command.Yaw < -0.01f)
                parts.Add("Yaw Left");

            return parts.Count > 0 ? string.Join(", ", parts.ToArray()) : "Active";
        }
    }
}
