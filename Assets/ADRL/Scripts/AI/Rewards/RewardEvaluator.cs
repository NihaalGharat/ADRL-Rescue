namespace ADRL.AI.Rewards
{
    using System;
    using System.Collections.Generic;
    using ADRL.AI.DecisionMaking;
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using UnityEngine;

    /// <summary>
    /// Computes and applies reward increments for a single drone through an
    /// <see cref="IRewardSink"/>, decoupled from any concrete ML-Agents agent.
    /// The M3 reward mathematics (time penalty, novelty-per-cell, potential
    /// shaping F = gamma*Phi(s') - Phi(s), stuck and oscillation detection) runs
    /// in <see cref="UpdateStep"/>, while terminal rewards arrive event-driven
    /// through the <see cref="EventBus"/>, filtered by drone id. Diagnostic
    /// breakdowns are exposed via <see cref="CurrentBreakdown"/> and
    /// <see cref="LastEpisodeBreakdown"/>.
    /// </summary>
    public sealed class RewardEvaluator : IDisposable
    {
        private readonly RewardConfig _config;
        private readonly IRewardSink _sink;
        private readonly EventBus _eventBus;
        private readonly int _droneId;
        private bool _disposed;

        private readonly HashSet<Vector2Int> _visitedCells = new HashSet<Vector2Int>();
        private int _noveltyCellsVisited;

        private Vector3 _lastStuckPosition;
        private float _stuckTimer;
        private Vector3 _previousPosition;
        private float _lastMoveDirection;
        private float _oscillationTimer;
        private int _oscillationReversals;

        private float _timePenaltyReward;
        private float _noveltyReward;
        private float _potentialReward;
        private float _stuckPenaltyReward;
        private float _oscillationPenaltyReward;
        private float _collisionPenaltyReward;
        private float _energyPenaltyReward;
        private float _outOfBoundsPenaltyReward;
        private float _victimFoundReward;
        private float _victimRescuedReward;
        private float _successReward;

        private int _stuckEvents;
        private int _oscillationEvents;
        private int _collisionEvents;
        private int _energyEvents;
        private int _outOfBoundsEvents;
        private int _victimFoundEvents;
        private int _victimRescuedEvents;
        private int _successEvents;

        /// <summary>Total reward granted during the current episode.</summary>
        public float EpisodeReward { get; private set; }

        /// <summary>Alias of <see cref="EpisodeReward"/> for the current episode total.</summary>
        public float CumulativeReward => EpisodeReward;

        /// <summary>Live snapshot of the current episode's reward categories.</summary>
        public RewardBreakdown CurrentBreakdown => BuildBreakdown();

        /// <summary>Snapshot of the previous episode captured at <see cref="Reset"/>.</summary>
        public RewardBreakdown LastEpisodeBreakdown { get; private set; }

        public RewardEvaluator(RewardConfig config, IRewardSink sink, EventBus eventBus, int droneId)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _droneId = droneId;

            _eventBus.Subscribe<DroneEnergyDepletedEvent>(OnEnergyDepleted);
            _eventBus.Subscribe<DroneOutOfBoundsEvent>(OnOutOfBounds);
            _eventBus.Subscribe<VictimFoundEvent>(OnVictimFound);
            _eventBus.Subscribe<VictimRescuedEvent>(OnVictimRescued);
            _eventBus.Subscribe<CollisionEvent>(OnCollision);
        }

        /// <summary>
        /// Detaches every terminal-reward subscription so the evaluator never
        /// reacts to events after teardown. Safe to call more than once.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            _eventBus.Unsubscribe<DroneEnergyDepletedEvent>(OnEnergyDepleted);
            _eventBus.Unsubscribe<DroneOutOfBoundsEvent>(OnOutOfBounds);
            _eventBus.Unsubscribe<VictimFoundEvent>(OnVictimFound);
            _eventBus.Unsubscribe<VictimRescuedEvent>(OnVictimRescued);
            _eventBus.Unsubscribe<CollisionEvent>(OnCollision);
        }

        /// <summary>
        /// Begins a new episode: snapshots the finished episode into
        /// <see cref="LastEpisodeBreakdown"/> and clears all running state.
        /// </summary>
        public void Reset(Vector3 startPosition)
        {
            LastEpisodeBreakdown = BuildBreakdown();

            EpisodeReward = 0f;
            _visitedCells.Clear();
            _noveltyCellsVisited = 0;

            _timePenaltyReward = 0f;
            _noveltyReward = 0f;
            _potentialReward = 0f;
            _stuckPenaltyReward = 0f;
            _oscillationPenaltyReward = 0f;
            _collisionPenaltyReward = 0f;
            _energyPenaltyReward = 0f;
            _outOfBoundsPenaltyReward = 0f;
            _victimFoundReward = 0f;
            _victimRescuedReward = 0f;
            _successReward = 0f;

            _stuckEvents = 0;
            _oscillationEvents = 0;
            _collisionEvents = 0;
            _energyEvents = 0;
            _outOfBoundsEvents = 0;
            _victimFoundEvents = 0;
            _victimRescuedEvents = 0;
            _successEvents = 0;

            _lastStuckPosition = startPosition;
            _stuckTimer = 0f;
            _previousPosition = startPosition;
            _lastMoveDirection = 0f;
            _oscillationTimer = 0f;
            _oscillationReversals = 0;
        }

        /// <summary>
        /// Applies one step of the M3 reward mathematics. Continuous rewards are
        /// scaled by <see cref="RewardConfig.RewardScale"/> and clipped to
        /// <see cref="RewardConfig.MinStepReward"/>, so a single large penalty
        /// cannot overwhelm an episode.
        /// </summary>
        public void UpdateStep(float deltaTime, Vector3 position, DroneCommand command)
        {
            var dt = Mathf.Max(0f, deltaTime);

            _timePenaltyReward += GrantContinuous(_config.TimePenalty * dt);

            _stuckTimer += dt;
            if (_stuckTimer >= _config.StuckDetectionWindow)
            {
                _stuckTimer = 0f;
                if (Vector3.Distance(position, _lastStuckPosition) < _config.StuckDistanceThreshold)
                {
                    _stuckEvents++;
                    _stuckPenaltyReward += GrantContinuous(_config.StuckPenalty);
                }

                _lastStuckPosition = position;
            }

            if (_visitedCells.Add(CellOf(position)))
            {
                var previousCount = _noveltyCellsVisited;
                _noveltyCellsVisited++;
                _noveltyReward += GrantContinuous(_config.NoveltyBonus);

                if (_config.ShapingEnabled)
                {
                    var potential = _config.ShapingScale *
                                    (_config.ShapingGamma * _noveltyCellsVisited - previousCount);
                    _potentialReward += GrantContinuous(potential);
                }
            }

            if (!command.IsIdle)
            {
                _oscillationTimer += dt;
                var dx = position.x - _previousPosition.x;
                if (dx != 0f)
                {
                    var direction = Mathf.Sign(dx);
                    if (_lastMoveDirection != 0f && direction != _lastMoveDirection)
                        _oscillationReversals++;
                    _lastMoveDirection = direction;
                }

                if (_oscillationTimer >= _config.OscillationDetectionWindow)
                {
                    _oscillationTimer = 0f;
                    if (_oscillationReversals >= _config.OscillationThreshold)
                    {
                        _oscillationEvents++;
                        _oscillationPenaltyReward += GrantContinuous(_config.OscillationPenalty);
                    }

                    _oscillationReversals = 0;
                }
            }

            _previousPosition = position;
        }

        private void OnEnergyDepleted(DroneEnergyDepletedEvent e)
        {
            if (e.DroneId != _droneId)
                return;
            _energyEvents++;
            _energyPenaltyReward += GrantTerminal(_config.EnergyDepletedPenalty);
        }

        private void OnOutOfBounds(DroneOutOfBoundsEvent e)
        {
            if (e.DroneId != _droneId)
                return;
            _outOfBoundsEvents++;
            _outOfBoundsPenaltyReward += GrantTerminal(_config.OutOfBoundsPenalty);
        }

        private void OnVictimFound(VictimFoundEvent e)
        {
            _victimFoundEvents++;
            _victimFoundReward += GrantTerminal(_config.VictimFoundReward);
        }

        private void OnVictimRescued(VictimRescuedEvent e)
        {
            _victimRescuedEvents++;
            _victimRescuedReward += GrantTerminal(_config.VictimRescuedReward);
        }

        private void OnCollision(CollisionEvent e)
        {
            if (e.DroneId != _droneId)
                return;
            _collisionEvents++;
            _collisionPenaltyReward += GrantTerminal(_config.CollisionPenalty);
        }

        /// <summary>
        /// Continuous reward path: scaled by RewardScale, then clipped to the
        /// per-step lower bound before reaching the sink.
        /// </summary>
        private float GrantContinuous(float rawReward)
        {
            var amount = rawReward * _config.RewardScale;
            if (amount < _config.MinStepReward)
                amount = _config.MinStepReward;

            _sink.AddReward(amount);
            EpisodeReward += amount;
            return amount;
        }

        /// <summary>
        /// Terminal reward path: applied verbatim, bypassing scaling and
        /// per-step clipping.
        /// </summary>
        private float GrantTerminal(float amount)
        {
            if (amount == 0f)
                return 0f;

            _sink.AddReward(amount);
            EpisodeReward += amount;
            return amount;
        }

        private Vector2Int CellOf(Vector3 position)
        {
            var size = _config.NoveltyCellSize > 0f ? _config.NoveltyCellSize : 0.01f;
            return new Vector2Int(
                Mathf.FloorToInt(position.x / size),
                Mathf.FloorToInt(position.z / size));
        }

        private RewardBreakdown BuildBreakdown()
        {
            return new RewardBreakdown(
                EpisodeReward,
                _timePenaltyReward,
                _noveltyReward,
                _potentialReward,
                _stuckPenaltyReward,
                _oscillationPenaltyReward,
                _collisionPenaltyReward,
                _energyPenaltyReward,
                _outOfBoundsPenaltyReward,
                _victimFoundReward,
                _victimRescuedReward,
                _successReward,
                _noveltyCellsVisited,
                _stuckEvents,
                _oscillationEvents,
                _collisionEvents,
                _energyEvents,
                _outOfBoundsEvents,
                _victimFoundEvents,
                _victimRescuedEvents,
                _successEvents);
        }
    }
}
