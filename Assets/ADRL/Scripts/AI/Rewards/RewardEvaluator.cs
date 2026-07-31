namespace ADRL.AI.Rewards
{
    using ADRL.AI.DecisionMaking;
    using ADRL.Core.Configuration;
    using Unity.MLAgents;
    using UnityEngine;

    /// <summary>
    /// Computes and applies reward increments to an ML-Agents <see cref="Agent"/>
    /// according to the configured <see cref="RewardConfig"/>. It also tracks the
    /// distance an agent travels so exploration bonuses can be granted only for
    /// genuine movement rather than for holding still.
    /// </summary>
    public sealed class RewardEvaluator
    {
        /// <summary>Accumulated travel distance that earns one exploration bonus.</summary>
        private const float ExplorationStepDistance = 1f;

        private readonly RewardConfig _config;
        private readonly Agent _agent;
        private Vector3 _lastPosition;
        private float _explorationAccumulator;

        /// <summary>Current cumulative reward as reported by the agent.</summary>
        public float CumulativeReward => _agent.GetCumulativeReward();

        /// <summary>Total reward accumulated during the current episode.</summary>
        public float EpisodeReward { get; private set; }

        public RewardEvaluator(RewardConfig config, Agent agent)
        {
            _config = config ?? throw new System.ArgumentNullException(nameof(config));
            _agent = agent ?? throw new System.ArgumentNullException(nameof(agent));
        }

        public void Reset(Vector3 startPosition)
        {
            _lastPosition = startPosition;
            _explorationAccumulator = 0f;
            EpisodeReward = 0f;
        }

        /// <summary>
        /// Applies the per-step time penalty and the exploration bonus based on
        /// actual movement since the previous step.
        /// </summary>
        public void UpdateStep(float deltaTime, Vector3 position, DroneCommand command)
        {
            AddReward(_config.TimePenalty * Mathf.Max(0f, deltaTime));

            if (command.IsIdle)
            {
                _lastPosition = position;
                return;
            }

            var travelled = Vector3.Distance(position, _lastPosition);
            _explorationAccumulator += travelled;
            _lastPosition = position;

            if (_explorationAccumulator >= ExplorationStepDistance)
            {
                AddReward(_config.ExplorationBonus);
                _explorationAccumulator = 0f;
            }
        }

        public void NotifyVictimFound() => AddReward(_config.VictimFoundReward);

        public void NotifyVictimRescued() => AddReward(_config.VictimRescuedReward);

        public void NotifyCollision() => AddReward(_config.CollisionPenalty);

        public void NotifyOutOfBounds() => AddReward(_config.OutOfBoundsPenalty);

        public void NotifyEnergyDepleted() => AddReward(_config.EnergyDepletedPenalty);

        public void NotifySuccess() => AddReward(_config.SuccessBonus);

        private void AddReward(float amount)
        {
            if (amount == 0f)
                return;

            _agent.AddReward(amount);
            EpisodeReward += amount;
        }
    }
}
