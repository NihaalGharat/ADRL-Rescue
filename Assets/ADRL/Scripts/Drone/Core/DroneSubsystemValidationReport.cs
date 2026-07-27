namespace ADRL.Drone.Core
{
    using System.Collections.Generic;

    public readonly struct DroneSubsystemValidationReport
    {
        public bool Passed { get; }
        public IReadOnlyList<string> Warnings { get; }
        public IReadOnlyList<string> Errors { get; }
        public double ExecutionTimeMs { get; }
        public IReadOnlyList<string> ValidatedComponents { get; }

        public DroneSubsystemValidationReport(
            bool passed,
            IReadOnlyList<string> warnings,
            IReadOnlyList<string> errors,
            double executionTimeMs,
            IReadOnlyList<string> validatedComponents)
        {
            Passed = passed;
            Warnings = warnings;
            Errors = errors;
            ExecutionTimeMs = executionTimeMs;
            ValidatedComponents = validatedComponents;
        }
    }
}
