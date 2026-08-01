namespace ADRL.AI.Rewards
{
    using Unity.MLAgents;

    /// <summary>
    /// Applies reward increments to an ML-Agents <see cref="Agent"/>.
    /// </summary>
    public sealed class AgentRewardSink : IRewardSink
    {
        private readonly Agent _agent;

        public AgentRewardSink(Agent agent)
        {
            _agent = agent ?? throw new System.ArgumentNullException(nameof(agent));
        }

        public void AddReward(float amount)
        {
            _agent.AddReward(amount);
        }
    }
}
