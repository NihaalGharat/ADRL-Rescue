namespace ADRL.AI.Decision.Telemetry
{
    using System.Globalization;
    using System.Text;
    using ADRL.AI.Decision.Mission;

    /// <summary>
    /// Pure, deterministic formatter that renders a
    /// <see cref="DecisionTelemetrySnapshot"/> into human-readable text describing
    /// the health and performance of the decision pipeline. It is a stateless
    /// function of the snapshot's fields and distributions: the same telemetry
    /// always produces byte-identical output (floats rendered with the invariant
    /// culture). Formatting is observational only - it never mutates the snapshot
    /// and never influences decisions.
    /// </summary>
    /// <example>
    /// === Decision Telemetry ===
    /// Decisions: 9
    /// Average Confidence: 0.99
    /// Average Optimization Confidence: 1
    /// Behaviour Distribution
    ///   Idle: 0
    ///   Avoid: 9
    /// Mission Distribution
    ///   AvoidHazard: 9
    /// Knowledge Records: 1
    /// Memory Records: 1
    /// Average Candidates: 1
    /// Execution Speed: 0.9
    /// Turn Multiplier: 1.29
    /// </example>
    public static class DecisionTelemetryFormatter
    {
        /// <summary>
        /// Renders the telemetry snapshot as a deterministic multi-line string.
        /// </summary>
        public static string Format(DecisionTelemetrySnapshot telemetry)
        {
            var output = new StringBuilder();
            output.AppendLine("=== Decision Telemetry ===");
            output.AppendLine("Decisions: " + telemetry.DecisionCount.ToString(CultureInfo.InvariantCulture));
            output.AppendLine("Average Confidence: " + Number(telemetry.AverageDecisionConfidence));
            output.AppendLine("Average Optimization Confidence: " + Number(telemetry.AverageOptimizationConfidence));
            output.AppendLine("Behaviour Distribution");
            AppendBehaviourDistribution(output, telemetry);
            output.AppendLine("Mission Distribution");
            AppendMissionDistribution(output, telemetry);
            output.AppendLine("Knowledge Records: " + telemetry.KnowledgeRecordCount.ToString(CultureInfo.InvariantCulture));
            output.AppendLine("Memory Records: " + telemetry.MemoryRecordCount.ToString(CultureInfo.InvariantCulture));
            output.AppendLine("Average Candidates: " + Number(telemetry.AverageCandidateCount));
            output.AppendLine("Execution Speed: " + Number(telemetry.AverageExecutionSpeedMultiplier));
            output.AppendLine("Turn Multiplier: " + Number(telemetry.AverageTurnRateMultiplier));
            output.AppendLine("Current Behaviour: " + telemetry.CurrentBehaviour.ToString());
            output.AppendLine("Current Mission: " + telemetry.CurrentMission.ToString());
            output.AppendLine("Current Executor: " + telemetry.CurrentExecutor);
            output.AppendLine("Latest Step: " + telemetry.LatestDecisionStep.ToString(CultureInfo.InvariantCulture));
            output.AppendLine("Latest Timestamp: " + Number(telemetry.LatestDecisionTimestamp));
            return output.ToString();
        }

        private static string Number(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static void AppendBehaviourDistribution(StringBuilder output, DecisionTelemetrySnapshot telemetry)
        {
            if (telemetry.BehaviourDistribution == null)
                return;

            for (var i = 0; i < telemetry.BehaviourDistribution.Length; i++)
            {
                var entry = telemetry.BehaviourDistribution[i];
                output.AppendLine("  " + entry.Behaviour.ToString() + ": "
                    + entry.Count.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void AppendMissionDistribution(StringBuilder output, DecisionTelemetrySnapshot telemetry)
        {
            if (telemetry.MissionDistribution == null)
                return;

            for (var i = 0; i < telemetry.MissionDistribution.Length; i++)
            {
                var entry = telemetry.MissionDistribution[i];
                output.AppendLine("  " + entry.Mission.ToString() + ": "
                    + entry.Count.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
