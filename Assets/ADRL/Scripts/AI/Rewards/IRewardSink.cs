namespace ADRL.AI.Rewards
{
    /// <summary>
    /// Receives reward increments for a single agent. Decouples reward
    /// computation from the concrete agent so the same evaluator can feed any
    /// reward sink without depending on Unity ML-Agents directly.
    /// </summary>
    public interface IRewardSink
    {
        void AddReward(float amount);
    }
}
