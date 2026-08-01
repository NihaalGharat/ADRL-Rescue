namespace ADRL.AI.Rewards
{
    /// <summary>
    /// Immutable, allocation-free snapshot of the reward statistics accumulated
    /// by a <see cref="RewardEvaluator"/> during the current (or just-finished)
    /// episode. Diagnostic only - it never drives reward computation.
    /// </summary>
    public readonly struct RewardBreakdown
    {
        public readonly float TotalReward;

        public readonly float TimePenaltyReward;
        public readonly float NoveltyReward;
        public readonly float PotentialReward;
        public readonly float StuckPenaltyReward;
        public readonly float OscillationPenaltyReward;
        public readonly float CollisionPenaltyReward;
        public readonly float EnergyPenaltyReward;
        public readonly float OutOfBoundsPenaltyReward;
        public readonly float VictimFoundReward;
        public readonly float VictimRescuedReward;
        public readonly float SuccessReward;

        public readonly int NoveltyCellsVisited;
        public readonly int StuckEvents;
        public readonly int OscillationEvents;
        public readonly int CollisionEvents;
        public readonly int EnergyEvents;
        public readonly int OutOfBoundsEvents;
        public readonly int VictimFoundEvents;
        public readonly int VictimRescuedEvents;
        public readonly int SuccessEvents;

        public RewardBreakdown(
            float totalReward,
            float timePenaltyReward,
            float noveltyReward,
            float potentialReward,
            float stuckPenaltyReward,
            float oscillationPenaltyReward,
            float collisionPenaltyReward,
            float energyPenaltyReward,
            float outOfBoundsPenaltyReward,
            float victimFoundReward,
            float victimRescuedReward,
            float successReward,
            int noveltyCellsVisited,
            int stuckEvents,
            int oscillationEvents,
            int collisionEvents,
            int energyEvents,
            int outOfBoundsEvents,
            int victimFoundEvents,
            int victimRescuedEvents,
            int successEvents)
        {
            TotalReward = totalReward;
            TimePenaltyReward = timePenaltyReward;
            NoveltyReward = noveltyReward;
            PotentialReward = potentialReward;
            StuckPenaltyReward = stuckPenaltyReward;
            OscillationPenaltyReward = oscillationPenaltyReward;
            CollisionPenaltyReward = collisionPenaltyReward;
            EnergyPenaltyReward = energyPenaltyReward;
            OutOfBoundsPenaltyReward = outOfBoundsPenaltyReward;
            VictimFoundReward = victimFoundReward;
            VictimRescuedReward = victimRescuedReward;
            SuccessReward = successReward;
            NoveltyCellsVisited = noveltyCellsVisited;
            StuckEvents = stuckEvents;
            OscillationEvents = oscillationEvents;
            CollisionEvents = collisionEvents;
            EnergyEvents = energyEvents;
            OutOfBoundsEvents = outOfBoundsEvents;
            VictimFoundEvents = victimFoundEvents;
            VictimRescuedEvents = victimRescuedEvents;
            SuccessEvents = successEvents;
        }
    }
}
