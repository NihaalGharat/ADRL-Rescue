# 10 - Reward System

---

## Overview

The Reward System is the feedback mechanism that teaches the drone what behaviors are desirable. It is the most critical component for training success.

Rewards are computed by `ADRL.AI.Rewards.RewardEvaluator` — one instance per drone — which is decoupled from any concrete ML-Agents agent through the `IRewardSink` interface. Continuous (per-step) rewards run inside `RewardEvaluator.UpdateStep`, while terminal rewards arrive event-driven through the `ADRL.Core.Events.EventBus` and are filtered by drone id.

---

## Reward Design Philosophy

### Principles

1. **Sparse vs Dense Balance** — Provide enough signal without overwhelming
2. **Shaping Rewards** — Guide learning with intermediate rewards
3. **Penalty Design** — Discourage unsafe behaviors
4. **Curriculum Learning** — Increase complexity over time

---

## Reward Configuration

All reward values are authored in the `RewardConfig` ScriptableObject
(`Assets/ADRL/Scripts/Core/Configuration/RewardConfig.cs`) and clamped in `OnValidate`. Defaults are listed below.

| Config Field | Default | Kind | Description |
|--------------|---------|------|-------------|
| VictimFoundReward | +10.0 | Bonus | Terminal reward on victim detection |
| VictimRescuedReward | +20.0 | Bonus | Terminal reward on successful rescue |
| SuccessBonus | +50.0 | Bonus | Terminal reward on mission success |
| CollisionPenalty | -5.0 | Penalty | Terminal penalty on obstacle impact |
| OutOfBoundsPenalty | -10.0 | Penalty | Terminal penalty on leaving environment |
| EnergyDepletedPenalty | -15.0 | Penalty | Terminal penalty on battery exhaustion |
| TimePenalty | -0.01 | Penalty | Per-second time cost |
| NoveltyBonus | +0.05 | Bonus | Reward per newly visited cell |
| NoveltyCellSize | 2.0 | Parameter | Cell size (metres) for novelty tracking |
| ShapingEnabled | true | Flag | Enable potential-based shaping |
| ShapingGamma | 0.99 | Parameter | Discount factor for shaping potential |
| ShapingScale | 0.1 | Parameter | Scale applied to shaping potential |
| RewardScale | 1.0 | Parameter | Scale applied to continuous rewards |
| MinStepReward | -0.1 | Parameter | Lower bound per continuous reward grant |
| MaxStepReward | 0.1 | Parameter | Configured upper bound (not yet enforced) |
| StuckPenalty | -0.5 | Penalty | Per stuck-detection penalty |
| StuckDetectionWindow | 2.0 | Parameter | Seconds per stuck check |
| StuckDistanceThreshold | 0.1 | Parameter | Max displacement (m) to count as stuck |
| OscillationPenalty | -0.5 | Penalty | Per oscillation-detection penalty |
| OscillationDetectionWindow | 2.0 | Parameter | Seconds per oscillation check |
| OscillationThreshold | 3.0 | Parameter | Direction reversals required to trigger |
| DamagePenalty | -2.0 | Penalty | Reserved damage penalty (not yet consumed) |

---

## Reward Structure

### Positive Rewards

| Reward | Value | Trigger | Purpose |
|--------|-------|---------|---------|
| Victim Found | +10.0 | `VictimFoundEvent` (terminal) | Primary objective |
| Victim Rescued | +20.0 | `VictimRescuedEvent` (terminal) | Complete rescue |
| Mission Success | +50.0 | Mission completion (terminal) | Reward mission completion |
| Novelty Bonus | +0.05 | Enter a previously unvisited cell | Encourage exploration |
| Potential Shaping | `F = γ·Φ(s′) − Φ(s)` | Step into a new cell | Smooth learning signal |

### Negative Rewards

| Reward | Value | Trigger | Purpose |
|--------|-------|---------|---------|
| Collision | -5.0 | `CollisionEvent` (terminal) | Safety |
| Out of Bounds | -10.0 | `DroneOutOfBoundsEvent` (terminal) | Boundary respect |
| Energy Depleted | -15.0 | `DroneEnergyDepletedEvent` (terminal) | Resource management |
| Time Penalty | -0.01 / s | Every step (`TimePenalty × dt`) | Efficiency |
| Stuck Penalty | -0.5 | Displacement < 0.1 m over a 2 s window | Prevent stagnation |
| Oscillation Penalty | -0.5 | ≥ 3 reversals over a 2 s window | Prevent oscillating |

---

## Reward Calculation Flow

```mermaid
graph TD
    A[UpdateStep] --> B[Time Penalty -0.01/s]
    A --> C{New Cell?}
    C -->|Yes| D[Novelty +0.05]
    D --> E[Potential Shaping<br/>F = γ*Φ(s') - Φ(s)]
    A --> F{Stuck? 2s window}
    F -->|Yes| G[Stuck -0.5]
    A --> H{Oscillation? 2s window}
    H -->|Yes| I[Oscillation -0.5]
    B --> J[Scale x RewardScale,<br/>clip at MinStepReward]
    D --> J
    E --> J
    G --> J
    I --> J
    J --> K[IRewardSink.AddReward]
    L[EventBus] --> M{Terminal Event?}
    M -->|EnergyDepleted| N[-15.0]
    M -->|OutOfBounds| O[-10.0]
    M -->|VictimFound| P[+10.0]
    M -->|VictimRescued| Q[+20.0]
    M -->|Collision| R[-5.0]
    M -->|Success| S[+50.0]
    N --> T[GrantTerminal verbatim]
    O --> T
    P --> T
    Q --> T
    R --> T
    S --> T
    T --> K
```

---

## Reward Categories

### 1. Task Rewards (Primary)

The main objective rewards that drive the drone toward completing its mission. Delivered as terminal rewards via the `EventBus`.

| Reward | Value | Description |
|--------|-------|-------------|
| Victim Found | +10.0 | First detection of a victim |
| Victim Rescued | +20.0 | Successful rescue operation |
| Mission Success | +50.0 | Successful mission completion |

### 2. Exploration Rewards (Secondary)

Rewards that encourage efficient environment coverage. Computed per step.

| Reward | Value | Description |
|--------|-------|-------------|
| Novelty Bonus | +0.05 | First visit to a 2 m × 2 m cell |
| Potential Shaping | `γ·Φ(s′) − Φ(s)` | Smooths the novelty signal toward completing exploration |

### 3. Safety Rewards (Tertiary)

Penalties that discourage dangerous behaviors.

| Reward | Value | Description |
|--------|-------|-------------|
| Collision | -5.0 | Impact with obstacles |
| Out of Bounds | -10.0 | Leaving the environment |
| Energy Depleted | -15.0 | Exhausting the battery |

### 4. Efficiency Rewards

Rewards that promote optimal behavior patterns.

| Reward | Value | Description |
|--------|-------|-------------|
| Time Penalty | -0.01 / s | Per-step cost |
| Stuck Penalty | -0.5 | No meaningful movement |
| Oscillation Penalty | -0.5 | Rapid direction reversals |

---

## Reward Function Implementation

Reward evaluation lives in `RewardEvaluator` (`Assets/ADRL/Scripts/AI/Rewards/RewardEvaluator.cs`).

```csharp
// Public API (per drone)
public RewardEvaluator(RewardConfig config, IRewardSink sink, EventBus eventBus, int droneId)
public void Reset(Vector3 startPosition)          // snapshots LastEpisodeBreakdown, clears state
public void UpdateStep(float deltaTime, Vector3 position, DroneCommand command)
public void Dispose()                              // unsubscribes from EventBus
public float EpisodeReward { get; }                // cumulative reward this episode
public RewardBreakdown CurrentBreakdown { get; }
public RewardBreakdown LastEpisodeBreakdown { get; }

// Continuous path (inside UpdateStep)
float GrantContinuous(float rawReward)
{
    var amount = rawReward * _config.RewardScale; // RewardScale = 1.0
    if (amount < _config.MinStepReward)           // floor at -0.1
        amount = _config.MinStepReward;
    _sink.AddReward(amount);
    return amount;
}

// Terminal path (EventBus subscription, filtered by drone id)
float GrantTerminal(float amount)                 // verbatim, bypasses scale/clip
{
    if (amount == 0f) return 0f;
    _sink.AddReward(amount);
    return amount;
}
```

Per step, `UpdateStep` applies: time penalty (`TimePenalty × dt`), novelty bonus on first cell visit, potential shaping `F = ShapingScale × (ShapingGamma × Φ(s′) − Φ(s))` where `Φ` is the visited-cell count, and stuck/oscillation detection on their respective 2 s windows. Terminal rewards are granted from `EventBus` subscriptions for `DroneEnergyDepletedEvent`, `DroneOutOfBoundsEvent`, `VictimFoundEvent`, `VictimRescuedEvent`, and `CollisionEvent`, each filtered by drone id.

Rewards reach the agent through `IRewardSink` (`AgentRewardSink`), keeping the evaluator independent of ML-Agents.

---

## Reward Breakdown

`RewardBreakdown` (`Assets/ADRL/Scripts/AI/Rewards/RewardBreakdown.cs`) is a `readonly struct` exposing per-category totals and event counters:

| Category | Total | Counter |
|----------|-------|---------|
| TimePenaltyReward | float | — |
| NoveltyReward | float | NoveltyCellsVisited |
| PotentialReward | float | — |
| StuckPenaltyReward | float | StuckEvents |
| OscillationPenaltyReward | float | OscillationEvents |
| CollisionPenaltyReward | float | CollisionEvents |
| EnergyPenaltyReward | float | EnergyEvents |
| OutOfBoundsPenaltyReward | float | OutOfBoundsEvents |
| VictimFoundReward | float | VictimFoundEvents |
| VictimRescuedReward | float | VictimRescuedEvents |
| SuccessReward | float | SuccessEvents |

**Invariant:** `TotalReward = TimePenalty + Novelty + Potential + Stuck + Oscillation + Collision + Energy + OutOfBounds + VictimFound + VictimRescued + Success` (validated by the smoke test's `sumInvariant` check).

---

## Reward Monitoring

### TensorBoard Metrics

| Metric | Description |
|--------|-------------|
| Episode/Reward | Total reward per episode |
| Reward/VictimFound | Victim detection frequency |
| Reward/Collisions | Collision frequency |
| Reward/Exploration | New area discovery rate |
| Episode/Length | Steps per episode |

### Success Criteria

| Metric | Target | Description |
|--------|--------|-------------|
| Average Reward | > 50 | Consistent positive rewards |
| Victim Detection | > 80% | Find most victims |
| Collision Rate | < 10% | Safe navigation |
| Episode Length | > 1000 | Sustained operation |

---

## Navigation

| Document | Description |
|----------|-------------|
| [06_AI_SYSTEM](06_AI_SYSTEM.md) | AI system overview |
| [11_TRAINING_PIPELINE](11_TRAINING_PIPELINE.md) | Training workflow |
| [12_DATA_FLOW](12_DATA_FLOW.md) | Data flow diagrams |

---

*Last updated: August 2026*
