# 19 - Decision Framework

**Version:** 1.0  
**Phase:** 10.0 — Autonomous Decision Advisory (v1.0.0 Release)  
**Date:** 04/08/2026

---

## Overview

The **Decision Framework** is the autonomous behaviour core of ADRL-Rescue. It is the canonical architecture reference for the deterministic decision pipeline implemented across Phases 8.1–10.0 in `Assets/ADRL/Scripts/AI/Decision/` (`ADRL.AI.Decision.*`).

The framework owns **one responsibility only**: given a fused sensor reading, produce the next drone command and a complete, immutable, observable record of how that command was chosen. It does **not** own movement, rewards, simulation, mission game state, or environment state — those belong to their own owners (`DroneAgent`/executors, `RewardEvaluator`, `SimulationManager`, `EnvironmentManager`).

The `DecisionEngine` (`ADRL.AI.Decision.DecisionEngine`) is the **single runtime decision authority**. Per the Phase 8.3 contract, the engine's output is the only source of drone behaviour; the ML-Agents action buffer is no longer a movement source.

---

## Design Invariants

- **Deterministic** — identical fused-reading sequences produce identical commands, snapshots, and observability output. No hidden randomness, timing, or ordering.
- **Immutable snapshots** — every layer exposes read-only snapshot structs; nothing downstream can mutate a producer's state.
- **Single ownership** — every responsibility has exactly one owner class (see table below). No duplicate or competing authorities.
- **Observational layers are read-only** — explanation, trace, telemetry, analytics, evaluation and advisory *observe* the decision; they can never influence it.
- **Extension over replacement** — new capability extends existing owners; no parallel systems.

---

## Ownership Map

| Responsibility | Single Owner |
|:---------------|:-------------|
| Situation assessment | `FogOfWarSituationAssessor` (`ISituationAssessor`) |
| Decision context composition | `DecisionContextBuilder` (`IDecisionContextBuilder`) |
| Runtime state payload | `DecisionRuntimeState` |
| World knowledge | `WorldKnowledgeStore` + `KnowledgeUpdater` (`IKnowledgeQuery`) |
| Short-term behaviour memory | `BehaviourMemoryService` + `BehaviourMemory` |
| Mission coordination | `MissionCoordinator` |
| Candidate generation | `TaskCandidateGenerator` |
| Priority scoring | `PriorityEvaluator` (`IPriorityEvaluator`) |
| Objective arbitration | `TaskPrioritizer` |
| Behaviour selection | `BehaviourSelector` (`IBehaviourSelector`) |
| Execution optimization | `BehaviourOptimizer` (`IBehaviourOptimizer`) |
| Behaviour → executor mapping | `BehaviourExecutorFactory` |
| Executor behaviour | `ApproachExecutor`, `AvoidExecutor`, `SearchExecutor`, `IdleExecutor` (`IBehaviourExecutor`) |
| Explanation composition | `DecisionExplanationBuilder` |
| Trace composition / history | `DecisionTraceBuilder` + `DecisionTraceStore` |
| Telemetry composition | `DecisionTelemetryCollector` |
| Analytics computation | `DecisionAnalyticsCalculator` (+ `DecisionHealthCalculator`) |
| Quality evaluation computation | `DecisionEvaluationCalculator` (+ `DecisionEvaluationMetrics`) |
| Advisory computation | `DecisionAdvisoryCalculator` |

---

## Decision Pipeline

`DecisionEngine.Decide(ISensorReading fused)` runs, in fixed order:

```
1  Assess situation                 ISituationAssessor.Assess(fused) → SituationSnapshot
2  Update world knowledge           KnowledgeUpdater.Update(assessment, memory, step)
3  Update behaviour memory          BehaviourMemoryService.Update(assessment, step)
4  Advance mission                  MissionCoordinator.Update(assessment, memory, step) → MissionTask
5  Generate candidates              TaskCandidateGenerator.Generate(...) → TaskCandidate[]
6  Score candidates                 PriorityEvaluator.Evaluate(...) → ScoredCandidate[]
7  Arbitrate winning objective      TaskPrioritizer.Select(...) → TaskCandidate
8  Select behaviour                 BehaviourSelector.Select(assessment, winningTask) → BehaviourState
9  Record behaviour                 BehaviourMemoryService.RecordBehaviour(behaviour)
10 Build context snapshot           DecisionContextBuilder.Build(...) → DecisionContextSnapshot
11 Optimize execution               BehaviourOptimizer.Optimize(behaviour, context) → BehaviourExecutionProfile
12 Resolve command                  BehaviourExecutorFactory.Get(behaviour).Resolve(assessment, profile) → DroneCommand
13 Compose final snapshot           WithExecutionProfile / WithKnowledge / WithScoredCandidates
14 Explain (observational)          DecisionExplanationBuilder.Build(snapshot) → DecisionExplanation
15 Trace (observational)            DecisionTraceBuilder.Build(snapshot, explanation) → DecisionTraceFrame (store append)
16 Telemetry (observational)        DecisionTelemetryCollector.Observe(traceFrame) → DecisionTelemetrySnapshot
17 Analytics (observational)        DecisionAnalyticsCalculator.Calculate(telemetry) → DecisionAnalyticsSnapshot
18 Evaluation (observational)       DecisionEvaluationCalculator.Calculate(analytics, telemetry, trace, explanation)
19 Advisory (observational)         DecisionAdvisoryCalculator.Calculate(evaluation, analytics, telemetry, trace, explanation)
20 Return result                    DecisionResult(behaviour, command, assessment)
```

Steps 1–13 form the **decision chain**; steps 14–19 form the **observability chain**. Each observability step reads only the immutable outputs produced before it, and each recomposes the last snapshot's diagnostics so diagnostics, snapshot, explanation, trace, telemetry, analytics, evaluation and advisory stay synchronized.

---

## Data Flow

### Snapshots (all immutable, all with a canonical `Empty` value)

| Snapshot | Produced by | Carries |
|:---------|:------------|:--------|
| `SituationSnapshot` | assessor | world, hazards, obstacles, victims, thermal/ray readings, energy |
| `DecisionContextSnapshot` | context builder | full decision chain for one step |
| `DecisionRuntimeState` | engine | behaviour, mission, winner, command, executor, execution profile, knowledge counts, timestamps |
| `WorldKnowledgeRecord` (per category) | knowledge updater | corroborated observations (victim/hazard/obstacle) |
| `DecisionTraceFrame` | trace builder | replayable record of the decision chain |
| `DecisionTelemetrySnapshot` | telemetry collector | running totals + latest-value projection |
| `DecisionAnalyticsSnapshot` | analytics calculator | health score, balance, entropy, utilization |
| `DecisionEvaluationSnapshot` | evaluation calculator | overall quality score, grade, six component scores |
| `DecisionAdvisorySnapshot` | advisory calculator | overall recommendation, ordered recommendations, confidence, status |
| `DecisionExplanation` | explanation builder | structured, human-readable reasoning |
| `DecisionDiagnostics` | engine | synchronized composition of all observability snapshots |

### Diagnostics composition

`DecisionDiagnostics.From(runtimeState, assessment, candidateCount, knowledgeStore)` seeds the diagnostics; each observability step then recomposes it via `WithExplanation`, `WithTraceFrame`, `WithTelemetry`, `WithAnalytics`, `WithEvaluation`, `WithAdvisory`. The engine's `GetDiagnostics()` therefore exposes one consistent, fully-synchronized record of the last step.

---

## Layer Details

### Situation Assessment
`FogOfWarSituationAssessor` converts raw fused sensor data into a normalized `SituationSnapshot` (fog-of-war perspective: proximity to hazards/obstacles, victim visibility, energy, safe-region knowledge). It is the only reader of `ISensorReading`.

### World Knowledge (Phase 9.0)
`KnowledgeUpdater` writes corroborated observations into `WorldKnowledgeStore`; every consumer reads through `IKnowledgeQuery`. Knowledge is write-only through the updater, read-only everywhere else.

### Behaviour Memory (Phase 8.5)
`BehaviourMemoryService` keeps short-term behaviour history; the selector uses continuity so the drone favours staying in its current behaviour unless the mission demands a change. Memory is the only place per-step behaviour context is retained.

### Mission Coordination (Phase 8.6)
`MissionCoordinator` owns the current `MissionTask` and its deterministic `MissionTransitionRules` (search/rescue/survey/avoid transitions, timeouts driven by the step clock).

### Prioritization (Phase 8.7)
`TaskCandidateGenerator` produces the currently-available objectives; `PriorityEvaluator` scores them; `TaskPrioritizer` arbitrates to exactly one winning objective using the `PriorityPolicy` thresholds. (Candidate generation was separated from scoring/arbitration in Phase 8.7.1.)

### Behaviour Selection (Phase 8.4/8.5)
`BehaviourSelector` maps the winning objective to a `BehaviourState`; `BehaviourExecutorFactory` maps that behaviour to the matching `IBehaviourExecutor`. Executors (`ApproachExecutor`, `AvoidExecutor`, `SearchExecutor`, `IdleExecutor`) own behaviour-specific movement command generation.

### Execution Optimization (Phase 8.9)
`BehaviourOptimizer` reads the current mission, winner, behaviour, assessment and memory and produces a `BehaviourExecutionProfile` (speed multiplier, turn-rate multiplier, execution confidence). It never changes mission, priority, behaviour or command — only how the executor moves.

### Observability Layers

| Layer | Owner | Output | Purpose |
|:------|:------|:-------|:--------|
| Explainability (9.1) | `DecisionExplanationBuilder` | `DecisionExplanation` | structured reasons for the decision |
| Trace & Replay (9.2) | `DecisionTraceBuilder` + `DecisionTraceStore` | `DecisionTraceFrame` (bounded store, `DecisionReplay`) | replayable history |
| Telemetry (9.3) | `DecisionTelemetryCollector` | `DecisionTelemetrySnapshot` | O(1), allocation-free running statistics |
| Analytics (9.4) | `DecisionAnalyticsCalculator` + `DecisionHealthCalculator` | `DecisionAnalyticsSnapshot` | health/balance/entropy/utilization metrics |
| Evaluation (9.5) | `DecisionEvaluationCalculator` + `DecisionEvaluationMetrics` | `DecisionEvaluationSnapshot` | overall quality score + grade/status |
| Advisory (10.0) | `DecisionAdvisoryCalculator` | `DecisionAdvisorySnapshot` | operator recommendations |

**Advisory** (`ADRL.AI.Decision.Advisory`): the calculator emits `DecisionRecommendation` items (12 `DecisionRecommendationType` values, `DecisionRecommendationPriority`, `DecisionRecommendationSeverity`), sorted highest-priority-first with no duplicate types; the snapshot carries the overall recommendation and confidence (0–100). Deterministic fixed rules only; a zero-count analytics input returns the canonical `Empty`.

---

## Determinism & Immutability

- The step clock (`_stepCount`) drives memory timestamps and mission timeouts, so the whole pipeline is deterministic across identical inputs.
- All snapshots are `readonly` value types with `Empty` canonical values that are valid by construction.
- Calculators are `static` and pure: no caching, no LINQ, no allocation beyond the returned snapshot (telemetry is strictly allocation-free).
- Formatters use invariant-culture formatting; validators return plain bools and never throw.
- No hidden state, no hidden timing, no hidden ordering.

---

## Reset Flow

`DecisionEngine.Reset()` resets the engine to its canonical pre-step state in a fixed order:

```
ResetTelemetry()  → DecisionTelemetrySnapshot.Empty
ResetAnalytics()  → DecisionAnalyticsSnapshot.Empty
ResetEvaluation() → DecisionEvaluationSnapshot.Empty
ResetAdvisory()   → DecisionAdvisorySnapshot.Empty
MemoryService.Reset()
Mission.Reset()
KnowledgeUpdater.Reset()
step counter, runtime state, last snapshot, explanation, trace frame cleared
```

Individual layers can be reset independently (`ResetTelemetry`, `ResetAnalytics`, `ResetEvaluation`, `ResetAdvisory`); the observability snapshots return to their canonical `Empty` values and diagnostics are recomposed accordingly.

---

## Runtime Integration

- `DecisionEngine.Decide(...)` is called from `DroneAgent.OnActionReceived` (`Assets/ADRL/Scripts/AI/Agents/DroneAgent.cs`). The returned `DroneCommand` drives the existing actuator pipeline; the engine is the sole decision authority.
- `RuntimeOrchestrator` boots the environment and drone subsystems, registers the drone prefab, and spawns the smoke-test drone.
- `DroneSmokeTest` runs a deterministic batch scenario and logs observations for every layer: decision, context, knowledge, optimization, explanation, trace, telemetry, analytics, evaluation and advisory. Observability is logged but is **not** a PASS criterion — PASS criteria are unchanged across Phases 8.1–10.0.

---

## Testing & Validation

- 321 EditMode tests pass (0 compiler errors, 0 compiler warnings), including the `ADRL.Tests.Editor.Decision` suite covering the calculator, formatter, validator, engine integration, determinism and reset behaviour of every layer.
- The runtime batch smoke test passes: finite reward, `sumInvariant=True`, movement/behaviour/reward/decision/optimization/determinism unchanged, all observability layers observed and validated.
- No PlayMode/scene-level test currently exists; tests are in-memory EditMode tests. See [15_TESTING_GUIDE.md](15_TESTING_GUIDE.md).

---

## Future Work

- Reinforcement Learning (PPO) training of decision policies on top of this framework (the deterministic pipeline remains the training/evaluation substrate). See [11_TRAINING_PIPELINE.md](11_TRAINING_PIPELINE.md) and [16_FUTURE_SCOPE.md](16_FUTURE_SCOPE.md).
- PlayMode/scene-level validation of the real runtime path.

---

## References

- Namespace guide: [NAMESPACE_GUIDE.md](NAMESPACE_GUIDE.md)
- Software design specification: [17_SOFTWARE_DESIGN_SPECIFICATION.md](17_SOFTWARE_DESIGN_SPECIFICATION.md)
- Data flow: [12_DATA_FLOW.md](12_DATA_FLOW.md)
- Development roadmap: [04_DEVELOPMENT_ROADMAP.md](04_DEVELOPMENT_ROADMAP.md)

---

**End of Decision Framework document**
