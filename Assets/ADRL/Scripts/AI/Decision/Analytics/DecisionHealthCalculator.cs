namespace ADRL.AI.Decision.Analytics
{
    using UnityEngine;

    /// <summary>
    /// Pure, stateless helper that computes the overall decision health score from
    /// the analytics component scores (each a percentage in [0, 100]). It is the
    /// single owner of health-score computation: the same components always yield
    /// the same score, the result is clamped to [0, 100], and it never influences
    /// decisions.
    /// </summary>
    /// <remarks>
    /// Suggested weighting (all components in [0, 100], weights sum to 1):
    /// decision confidence 35%, execution confidence 25%, optimization confidence
    /// 15%, knowledge usage 10%, memory usage 10%, behaviour balance 5%.
    /// </remarks>
    public static class DecisionHealthCalculator
    {
        /// <summary>Weight of the decision-confidence component.</summary>
        public const float DecisionConfidenceWeight = 0.35f;

        /// <summary>Weight of the execution-confidence component.</summary>
        public const float ExecutionConfidenceWeight = 0.25f;

        /// <summary>Weight of the optimization-confidence component.</summary>
        public const float OptimizationConfidenceWeight = 0.15f;

        /// <summary>Weight of the knowledge-usage component.</summary>
        public const float KnowledgeUsageWeight = 0.10f;

        /// <summary>Weight of the memory-usage component.</summary>
        public const float MemoryUsageWeight = 0.10f;

        /// <summary>Weight of the behaviour-balance component.</summary>
        public const float BehaviourBalanceWeight = 0.05f;

        /// <summary>
        /// Computes the deterministic health score in [0, 100] as the weighted sum
        /// of the six components, clamped to [0, 100]. All inputs are percentages
        /// in [0, 100].
        /// </summary>
        public static float Calculate(
            float decisionConfidence,
            float executionConfidence,
            float optimizationConfidence,
            float knowledgeUtilization,
            float memoryUtilization,
            float behaviourBalance)
        {
            var score = DecisionConfidenceWeight * decisionConfidence
                + ExecutionConfidenceWeight * executionConfidence
                + OptimizationConfidenceWeight * optimizationConfidence
                + KnowledgeUsageWeight * knowledgeUtilization
                + MemoryUsageWeight * memoryUtilization
                + BehaviourBalanceWeight * behaviourBalance;

            return Mathf.Clamp(score, 0f, 100f);
        }
    }
}
