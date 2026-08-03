namespace ADRL.AI.Decision.Optimization
{
    using ADRL.AI.Decision.Context;
    using UnityEngine;

    /// <summary>
    /// Pure validation utilities for the behaviour-execution optimization layer.
    /// Each check verifies one invariant of a <see cref="BehaviourExecutionProfile"/>
    /// or of the <see cref="IBehaviourOptimizer"/> itself; <see cref="IsValid"/>
    /// requires every field to satisfy the profile contract. Validation never
    /// mutates a profile and never influences decisions.
    /// </summary>
    /// <remarks>
    /// The multiplier bounds are the single source of truth for the valid execution
    /// profile contract: the <see cref="BehaviourOptimizer"/> clamps to exactly
    /// these bounds and the validator confirms profiles stay inside them, so a
    /// produced profile is valid by construction. The canonical empty profile is
    /// always valid (it represents the neutral no-op execution path).
    /// </remarks>
    public static class OptimizationValidator
    {
        /// <summary>Minimum legal speed multiplier for an optimized profile.</summary>
        public const float SpeedMultiplierMin = 0.35f;

        /// <summary>Maximum legal speed multiplier for an optimized profile.</summary>
        public const float SpeedMultiplierMax = 2.0f;

        /// <summary>Minimum legal turn-rate multiplier for an optimized profile.</summary>
        public const float TurnRateMultiplierMin = 0.35f;

        /// <summary>Maximum legal turn-rate multiplier for an optimized profile.</summary>
        public const float TurnRateMultiplierMax = 2.0f;

        /// <summary>True when the profile's execution confidence is in [0, 1].</summary>
        public static bool ConfidenceInRange(BehaviourExecutionProfile profile)
        {
            return profile.ExecutionConfidence >= 0f && profile.ExecutionConfidence <= 1f;
        }

        /// <summary>True when the profile's speed multiplier is within the valid bounds.</summary>
        public static bool SpeedMultiplierInRange(BehaviourExecutionProfile profile)
        {
            return profile.SpeedMultiplier >= SpeedMultiplierMin && profile.SpeedMultiplier <= SpeedMultiplierMax;
        }

        /// <summary>True when the profile's turn-rate multiplier is within the valid bounds.</summary>
        public static bool TurnRateMultiplierInRange(BehaviourExecutionProfile profile)
        {
            return profile.TurnRateMultiplier >= TurnRateMultiplierMin && profile.TurnRateMultiplier <= TurnRateMultiplierMax;
        }

        /// <summary>True when the profile's caution level is in [0, 1].</summary>
        public static bool CautionInRange(BehaviourExecutionProfile profile)
        {
            return profile.CautionLevel >= 0f && profile.CautionLevel <= 1f;
        }

        /// <summary>True when every field of the profile satisfies the execution contract.</summary>
        public static bool IsValid(BehaviourExecutionProfile profile)
        {
            if (profile.IsEmpty)
                return true;

            return ConfidenceInRange(profile)
                && SpeedMultiplierInRange(profile)
                && TurnRateMultiplierInRange(profile)
                && CautionInRange(profile)
                && profile.PreferredDistance >= 0f
                && profile.Smoothness >= 0f;
        }

        /// <summary>
        /// True when the optimizer is deterministic for the given behaviour and
        /// snapshot: two identical calls produce the same profile.
        /// </summary>
        public static bool IsDeterministic(
            IBehaviourOptimizer optimizer,
            BehaviourState behaviour,
            DecisionContextSnapshot snapshot)
        {
            var a = optimizer.Optimize(behaviour, snapshot);
            var b = optimizer.Optimize(behaviour, snapshot);
            return ProfilesEqual(a, b);
        }

        private static bool ProfilesEqual(BehaviourExecutionProfile a, BehaviourExecutionProfile b)
        {
            return Mathf.Approximately(a.SpeedMultiplier, b.SpeedMultiplier)
                && Mathf.Approximately(a.TurnRateMultiplier, b.TurnRateMultiplier)
                && Mathf.Approximately(a.CautionLevel, b.CautionLevel)
                && Mathf.Approximately(a.PreferredDistance, b.PreferredDistance)
                && Mathf.Approximately(a.Smoothness, b.Smoothness)
                && Mathf.Approximately(a.ExecutionConfidence, b.ExecutionConfidence);
        }
    }
}
