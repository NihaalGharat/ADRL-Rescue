namespace ADRL.AI.Decision.Optimization
{
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Mission;
    using UnityEngine;

    /// <summary>
    /// The single owner of behaviour-execution optimization. It answers one
    /// question - "how should the already-selected behaviour execute most
    /// efficiently?" - by reading the current immutable
    /// <see cref="DecisionContextSnapshot"/> (the mission objective, the winning
    /// objective's priority score and confidence, the behaviour memory confidence
    /// and the current obstacle proximity) and producing a deterministic
    /// <see cref="BehaviourExecutionProfile"/>. It adjusts speed, turn rate,
    /// caution, smoothing and execution confidence only. It never changes the
    /// mission objective, the priority decision, the behaviour or the command -
    /// those are owned by the coordinator, the prioritizer, the selector and the
    /// executors respectively, and all run around it.
    /// </summary>
    /// <remarks>
    /// Deterministic by construction: every output is a pure, clamped function of
    /// the policy and the snapshot, with no random numbers, no hidden state and no
    /// ordering effects. Speed multipliers are pushed from a cautious baseline
    /// toward the policy's per-behaviour target as combined confidence rises;
    /// avoid execution slows and turns harder as obstacle proximity rises. The
    /// neutral <see cref="BehaviourExecutionProfile.Empty"/> profile is returned
    /// only for idle, where no execution parameters apply.
    /// </remarks>
    public sealed class BehaviourOptimizer : IBehaviourOptimizer
    {
        private readonly OptimizationPolicy _policy;

        public BehaviourOptimizer(OptimizationPolicy policy)
        {
            _policy = policy;
        }

        /// <inheritdoc/>
        public BehaviourExecutionProfile Optimize(BehaviourState behaviour, DecisionContextSnapshot snapshot)
        {
            switch (behaviour)
            {
                case BehaviourState.Search:
                    return OptimizeSearch(snapshot);
                case BehaviourState.Approach:
                    return snapshot.Mission.State == MissionTaskState.RescueVictim
                        ? OptimizeRescue(snapshot)
                        : OptimizeApproach(snapshot);
                case BehaviourState.Avoid:
                    return OptimizeAvoid(snapshot);
                default:
                    return BehaviourExecutionProfile.Empty;
            }
        }

        private BehaviourExecutionProfile OptimizeSearch(DecisionContextSnapshot snapshot)
        {
            var confidence = CombinedConfidence(snapshot);
            return Profile(
                speed: Mathf.Lerp(0.85f, _policy.SearchSpeed, confidence),
                turn: Mathf.Lerp(_policy.MinimumTurnRate, _policy.MaximumTurnRate, confidence),
                caution: 0.05f,
                preferredDistance: 0f,
                confidence);
        }

        private BehaviourExecutionProfile OptimizeApproach(DecisionContextSnapshot snapshot)
        {
            var confidence = CombinedConfidence(snapshot);
            return Profile(
                speed: Mathf.Lerp(0.85f, _policy.ApproachSpeed, confidence),
                turn: Mathf.Lerp(_policy.MinimumTurnRate, _policy.MaximumTurnRate, confidence),
                caution: 0.1f,
                preferredDistance: 0f,
                confidence);
        }

        private BehaviourExecutionProfile OptimizeRescue(DecisionContextSnapshot snapshot)
        {
            var confidence = CombinedConfidence(snapshot);
            return Profile(
                speed: Mathf.Lerp(0.85f, _policy.RescueSpeed, confidence),
                turn: Mathf.Lerp(_policy.MinimumTurnRate, _policy.MaximumTurnRate, confidence),
                caution: 0.25f,
                preferredDistance: _policy.VictimApproachDistance,
                confidence);
        }

        private BehaviourExecutionProfile OptimizeAvoid(DecisionContextSnapshot snapshot)
        {
            // Avoid executes at a near-neutral speed (only slightly cautious) so the
            // drone still escapes and recovers quickly; the proximity response is
            // carried by the caution and turn-rate multipliers, which rise as the
            // obstacle closes. This preserves the movement decisions - the direction
            // of evasion is unchanged - while making the evasion harder and sharper.
            var proximity = Mathf.Clamp01(snapshot.Assessment.ObstacleProximity);
            return Profile(
                speed: _policy.AvoidSpeed,
                turn: Mathf.Lerp(_policy.MinimumTurnRate, _policy.MaximumTurnRate, 0.5f + 0.5f * proximity),
                caution: _policy.ObstacleCaution * (0.5f + 0.5f * proximity),
                preferredDistance: 0f,
                CombinedConfidence(snapshot));
        }

        private BehaviourExecutionProfile Profile(
            float speed,
            float turn,
            float caution,
            float preferredDistance,
            float confidence)
        {
            return new BehaviourExecutionProfile(
                Mathf.Clamp(speed, OptimizationValidator.SpeedMultiplierMin, OptimizationValidator.SpeedMultiplierMax),
                Mathf.Clamp(turn, OptimizationValidator.TurnRateMultiplierMin, OptimizationValidator.TurnRateMultiplierMax),
                Mathf.Clamp01(caution),
                preferredDistance,
                _policy.SmoothingFactor,
                Mathf.Clamp01(confidence));
        }

        /// <summary>
        /// Combined execution confidence in [0, 1]: the winning objective's
        /// assessment confidence scaled by the confidence of the behaviour memory
        /// backing that objective and the configured confidence scaling. Search
        /// objectives need no entity memory, so their memory confidence is neutral.
        /// </summary>
        private float CombinedConfidence(DecisionContextSnapshot snapshot)
        {
            var assessmentConfidence = snapshot.Winning.Confidence;
            var memoryConfidence = MemoryConfidence(snapshot);
            return Mathf.Clamp01(assessmentConfidence * memoryConfidence * _policy.ConfidenceScaling);
        }

        private static float MemoryConfidence(DecisionContextSnapshot snapshot)
        {
            if (snapshot.Memory == null)
                return 0f;

            switch (snapshot.Winning.Task.State)
            {
                case MissionTaskState.RescueVictim:
                    return snapshot.Memory.LastVictimSeen.IsValid ? snapshot.Memory.LastVictimSeen.Confidence : 0f;
                case MissionTaskState.AvoidHazard:
                    return snapshot.Memory.LastObstacleSeen.IsValid ? snapshot.Memory.LastObstacleSeen.Confidence : 0f;
                default:
                    return 1f;
            }
        }
    }
}
