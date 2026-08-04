namespace ADRL.AI.Decision.Advisory
{
    /// <summary>
    /// The kind of advisory recommendation the decision pipeline can produce. Each
    /// value names a specific course of action an operator could take; the
    /// <see cref="DecisionAdvisoryCalculator"/> maps deterministic quality signals
    /// to exactly one recommendation type per signal, so the same observations
    /// always produce the same recommendation types. <see cref="None"/> is the
    /// canonical value of the overall recommendation before any decision has been
    /// observed.
    /// </summary>
    public enum DecisionRecommendationType
    {
        /// <summary>No recommendation - the canonical value of an empty advisory.</summary>
        None,

        /// <summary>Keep the current behaviour and mission strategy unchanged.</summary>
        MaintainCurrentStrategy,

        /// <summary>Expand the search radius while sweeping for objectives.</summary>
        IncreaseSearchRadius,

        /// <summary>Persist in the current behaviour longer instead of switching.</summary>
        IncreaseSearchPersistence,

        /// <summary>Gather more world knowledge during the mission.</summary>
        IncreaseKnowledgeCoverage,

        /// <summary>Focus on the single most important mission objective.</summary>
        IncreaseMissionPriority,

        /// <summary>Strengthen obstacle-avoidance behaviour.</summary>
        IncreaseObstacleAvoidance,

        /// <summary>Slow the execution speed multiplier down.</summary>
        ReduceSpeedMultiplier,

        /// <summary>Review how behaviour execution is optimized.</summary>
        ReviewOptimization,

        /// <summary>Review the confidence of the fused sensor inputs.</summary>
        ReviewSensorConfidence,

        /// <summary>Review how world knowledge is being covered and gathered.</summary>
        ReviewKnowledgeCoverage,

        /// <summary>Review how objectives are allocated across missions.</summary>
        ReviewMissionAllocation,
    }
}
