# Changelog

All notable changes to ADRL-Rescue will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [0.1.0] - 2026-07-20

### Added

- Initial project setup
- Unity project structure with .gitkeep placeholders
- Python project structure with .gitkeep placeholders
- Git repository initialization with remote origin

#### Documentation Suite

- **Project Charter** — Master project document with vision, scope, architecture, and rules
- **Project Vision** — Goals, mission, and vision statement
- **Project Architecture** — System architecture and component relationships
- **System Design** — Detailed system design and specifications
- **Development Roadmap** — Development phases and timeline
- **Folder Structure** — Repository organization and file placement
- **AI System** — AI/ML system design and PPO configuration
- **Drone System** — Drone components, flight controller, sensors
- **Environment System** — Procedural generation and disaster types
- **Sensor System** — Sensor specifications and implementations
- **Reward System** — Reward function design and shaping
- **Training Pipeline** — Training workflow and procedures
- **Data Flow** — Data flow diagrams and system communication
- **Coding Standards** — C# conventions and coding guidelines
- **GitHub Workflow** — Git workflow and PR process
- **Testing Guide** — Testing strategies and procedures
- **Future Scope** — Future features and roadmap
- **Software Design Specification** — Implementation blueprint for all C# scripts
- **Developer Handbook** — Practical guide for developers
- **Project Glossary** — Terminology reference
- **Documentation Index** — README for documentation directory

#### Repository Files

- README with project overview, architecture, and roadmap
- CHANGELOG with version history
- CONTRIBUTING with contribution guidelines
- CODE_OF_CONDUCT with community standards
- SECURITY with security policy
- LICENSE (MIT)
- CITATION.cff with citation metadata
- .gitignore for Unity, Python, and IDE files

### This Release Represents

- ✅ Complete software architecture
- ✅ Full technical documentation
- ✅ Repository standards and conventions
- ✅ Development workflow and processes
- ❌ No Unity implementation
- ❌ No AI training
- ❌ No gameplay code

---

## [0.2.0] - 2026-07-20

### Added

#### Phase 2.1 — Unity Foundation

- Unity project foundation initialized
- ProjectVersion.txt configured for Unity 2022.3.62f1 LTS
- Package manifest with approved packages:
  - TextMeshPro 3.0.6
  - Input System 1.7.0
  - AI Navigation 1.1.5
  - ML-Agents 1.1.1
- Tags configured: Drone, Victim, Obstacle, Hazard, Environment, SpawnPoint
- Layers configured: Drone, Victim, Obstacle, Terrain, Sensor, Environment
- Physics settings configured (gravity -9.81, fixed timestep 0.02s)
- Time settings configured (fixed timestep 0.02s, max timestep 0.1s)
- Quality settings configured (Low/Medium/High, VSync off, target 60+ FPS)
- Input Manager with standard axes

#### Phase 2.2 — Project Structure & Assembly Definitions

- Folder hierarchy established under `Assets/ADRL/`
- Assembly Definitions created:
  - ADRL.Core (no dependencies)
  - ADRL.Drone (depends on Core)
  - ADRL.AI (depends on Core, Drone)
  - ADRL.Environment (depends on Core)
  - ADRL.Sensors (depends on Core)
  - ADRL.Training (depends on Core, Drone, AI, Environment, Sensors)
  - ADRL.UI (depends on Core)
  - ADRL.Editor (depends on all, Editor-only)
- Namespace Guide created (`Documentation/NAMESPACE_GUIDE.md`)
- No circular dependencies
- Clean dependency tree established

---

## [0.7.0] - 2026-07-29

### Phase 6.1 — Drone Entity Foundation

- `DroneIdentity` component as single source of drone ID on the prefab; entity validation via `DroneEntityValidator`; `DroneController` updated with `[RequireComponent]` and ID synchronization
- 2 new files, 2 modified — zero asmdef/namespace changes

### Phase 6.2 — Drone Spawn Pipeline & Prefab Runtime

- Deterministic spawn pipeline: `SpawnRequest`/`SpawnParameters`/`SpawnResult` data structs, `DronePrefabRegistry`, `DroneFactory`, `DroneSpawnManager` with queue and validation; `SpawnValidator` utility; spawn lifecycle events
- 8 new files, 2 modified — zero asmdef/namespace changes

### Phase 6.3 — Runtime Object Pooling

- `DronePool` with FIFO borrow and O(1) return; `PoolPolicy`/`PoolStatistics` data structs; `DronePoolManager` implementing `IDroneAllocator` with lazy pool creation, bidirectional tracking, stale cleanup, and prewarm; full integration into `DroneSpawnManager` and `DroneSubsystem`; reverse-creation-order shutdown verified
- 4 new files, 5 modified — zero asmdef/namespace changes

---

## [Unreleased]

- No unreleased changes yet.

---

## [1.0.0] - 2026-08-04

First stable release of the ADRL-Rescue autonomous decision framework: the deterministic **Decision Engine** and its layers (situation assessment, context, knowledge, memory, mission, prioritization, behaviour, optimization, execution, explainability, trace, telemetry, analytics, quality evaluation, advisory). **321/321 EditMode tests passing, zero compiler warnings, runtime batch smoke test passing.** Reinforcement Learning (PPO) training is future work — this release ships the deterministic decision framework only.

### Phase 10.0 — Autonomous Decision Advisory Framework (2026-08-04)

#### Deterministic Advisory Layer (ADRL.AI.Decision.Advisory)

- **`DecisionRecommendationType`** (new) — the 11 recommendation kinds the pipeline can produce: `None`, `MaintainCurrentStrategy`, `IncreaseSearchRadius`, `IncreaseSearchPersistence`, `IncreaseKnowledgeCoverage`, `IncreaseMissionPriority`, `IncreaseObstacleAvoidance`, `ReduceSpeedMultiplier`, `ReviewOptimization`, `ReviewSensorConfidence`, `ReviewKnowledgeCoverage`, `ReviewMissionAllocation`
- **`DecisionRecommendationPriority`** (new) — `None`/`Low`/`Medium`/`High`; **`DecisionRecommendationSeverity`** (new) — `None`/`Low`/`Medium`/`High`/`Critical`
- **`DecisionRecommendation`** (new) — immutable, deterministic recommendation: `Type`, `Priority`, `Severity`, `Reason`, `SuggestedAction`, `Confidence` (0-100), `DecisionStep`, `Timestamp`; strings null-coalesced to empty; `IsValid`; all readonly
- **`DecisionAdvisorySnapshot`** (new) — immutable, deterministic advisory snapshot: `DecisionStep`, `DecisionTimestamp`, `OverallRecommendation`, `Recommendations[]` (ordered highest-priority-first, no duplicate types), `AdvisoryConfidence` (0-100), `RequiresAttention`, `Status` (Nominal/Advisory/Attention/Critical via `StatusFor(priority)`); canonical `Empty` is valid; all readonly
- **`DecisionAdvisoryCalculator`** (new, static) — the single owner of advisory computation: pure, stateless, `Calculate(DecisionEvaluationSnapshot, DecisionAnalyticsSnapshot, DecisionTelemetrySnapshot, DecisionTraceFrame, DecisionExplanation)` recommends what the operator should do about the decisions made; fixed deterministic rules (excellent quality 90+ → `MaintainCurrentStrategy`; critical knowledge coverage < 30 → `IncreaseKnowledgeCoverage` High/Critical; low knowledge coverage < 50 → `ReviewKnowledgeCoverage`; low mission suitability < 50 → `ReviewMissionAllocation`; unbalanced mission distribution > 80 → `IncreaseMissionPriority`; unbalanced behaviour distribution > 80 → `IncreaseSearchPersistence`; low optimization < 50 → `ReviewOptimization`; low confidence quality < 50 → `ReviewSensorConfidence`; active search mission with low knowledge coverage → `IncreaseSearchRadius`; high speed multiplier > 1.1 → `ReduceSpeedMultiplier`; near hazard (< 15 units) without avoidance → `IncreaseObstacleAvoidance`; no rule fires → `MaintainCurrentStrategy`); each signal maps to at most one type (no duplicates by construction), recommendations sorted highest-priority-first (ties by type order), overall = first, confidence = first's confidence; a zero-count analytics input returns the canonical `Empty`; no caching, no LINQ, no allocation beyond the snapshot and its recommendations
- **`DecisionAdvisoryFormatter`** (new, static) — pure, deterministic Decision Advisory Report (header, Overall Recommendation, Advisory Confidence `F1`, High/Medium/Low Priority sections, Summary with Total Recommendations/Requires Attention/Status) with a fixed 22-column dotted label layout, invariant-culture floats, no timestamps; allocates only the final string
- **`DecisionAdvisoryValidator`** (new, static) — pure validation: no duplicate recommendation types, priorities ordered highest-first, every confidence in [0, 100], valid severities, valid recommendation types with overall matching the strongest recommendation (`None` only when empty), snapshot complete with canonical status; plain bools, never throws
- **`DecisionDiagnostics`** — new `LastAdvisory` member + `WithAdvisory`; constructor extended with an optional parameter; `Empty` carries `DecisionAdvisorySnapshot.Empty`; `WithExplanation`/`WithTraceFrame`/`WithTelemetry`/`WithAnalytics`/`WithEvaluation` preserve the advisory
- **`DecisionEngine`** — after evaluation, computes the advisory snapshot via the `DecisionAdvisoryCalculator` (single owner of advisory computation), exposes `LastAdvisory` + `GetAdvisory()` + `ResetAdvisory()`, and recomposes the last snapshot's diagnostics with the advisory so diagnostics, telemetry, analytics, evaluation and advisory stay synchronized; `Reset()` clears advisory; fully backward compatible, zero behavioural change
- `DroneSmokeTest` observes the advisory and logs `advisoryObserved`, `recommendationCount`, `overallRecommendation`, `advisoryConfidence`, `advisoryValid`; observational only — **no PASS criteria change**

#### Tests (ADRL.Tests.Editor.Decision)

- New `DecisionAdvisoryTests` (20): calculator empty for no decisions; calculator uses trace step and timestamp; calculator deterministic output; calculator excellent quality maintains strategy; calculator critical knowledge increases coverage; calculator low knowledge reviews coverage; calculator low mission suitability reviews allocation; calculator unbalanced mission increases priority; calculator unbalanced behaviour increases persistence; calculator low optimization reviews optimization; calculator low confidence reviews sensor; calculator search mission with low knowledge increases radius; calculator high speed multiplier reduces speed; calculator near hazard advisory; calculator orders recommendations priority first; formatter deterministic and contains sections; validator accepts valid; validator rejects malformed snapshots; engine advisory after steps; reset clears advisory

#### Validation

- EditMode suite: **321/321 passing** (301 prior + 20 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, finite reward, **sumInvariant=True**, movement/behaviour/reward/decision/optimization/determinism unchanged, decision/context/optimization/knowledge/explanation/trace/telemetry/analytics/evaluation observed, **advisory observed and validated**
- Mission logic, prioritization, selection, optimization, execution, knowledge, memory, explainability, tracing, telemetry, analytics, evaluation, navigation, rewards, simulation, RL and training unchanged; advisory is read-only and never influences decisions

### Phase 9.5 — Autonomous Decision Quality Evaluation Framework (2026-08-04)

#### Deterministic Evaluation Layer (ADRL.AI.Decision.Evaluation)

- **`DecisionEvaluationSnapshot`** (new) — immutable, deterministic decision-quality snapshot: `DecisionStep`, `DecisionTimestamp`, `DecisionQualityScore` (0-100), `BehaviourSuitabilityScore`, `ConfidenceQualityScore`, `OptimizationBenefitScore`, `MissionSuitabilityScore`, `KnowledgeCoverageScore`, `ConsistencyScore`, `EvaluationGrade` (Excellent/Good/Fair/Poor/Critical), `OverallStatus` (Optimal/Good/Acceptable/Deficient/Critical enum); all readonly, `Empty` canonical and valid
- **`DecisionEvaluationMetrics`** (new, static) — the single owner of evaluation metric math: pure, stateless helpers for behaviour suitability (dominant behaviour share), mission suitability (dominant objective share), confidence quality (average decision confidence × 100), optimization benefit (optimized execution confidence × 100), knowledge coverage (knowledge utilization), consistency (complement of behaviour-distribution evenness), the weighted overall score, the canonical grade table and the status bands; every helper clamps to [0, 100] and degrades NaN/infinity to 0
- **`DecisionEvaluationCalculator`** (new, static) — the single owner of evaluation computation: pure, stateless, `Calculate(DecisionAnalyticsSnapshot, DecisionTelemetrySnapshot, DecisionTraceFrame, DecisionExplanation)` derives the evaluation snapshot (step/timestamp from the trace, falling back to analytics) without caching, without LINQ and without allocation beyond the snapshot; a zero-count analytics input returns the canonical `Empty`; null or truncated inputs degrade gracefully, never exceptions
- **`DecisionEvaluationFormatter`** (new, static) — pure, deterministic Decision Quality report (header, Overall Score `F1`, Grade, Behaviour Suitability, Mission Suitability, Optimization, Consistency, Knowledge, Confidence) with a fixed 22-column dotted label layout, invariant-culture floats, no timestamps; allocates only the final string
- **`DecisionEvaluationValidator`** (new, static) — pure validation: no negatives, no NaN, no infinity, every score in [0, 100], grade matches the overall score, status matches it too, snapshot complete (grade present, step/timestamp non-negative); plain bools, never throws
- **`DecisionDiagnostics`** — new `LastEvaluation` member + `WithEvaluation`; constructor extended with an optional parameter; `Empty` carries `DecisionEvaluationSnapshot.Empty`; `WithExplanation`/`WithTraceFrame`/`WithTelemetry`/`WithAnalytics` preserve the evaluation
- **`DecisionEngine`** — after analytics, computes the evaluation snapshot via the `DecisionEvaluationCalculator` (single owner of evaluation computation), exposes `LastEvaluation` + `GetEvaluation()` + `ResetEvaluation()`, and recomposes the last snapshot's diagnostics with the evaluation so diagnostics, telemetry, analytics and evaluation stay synchronized; `Reset()` clears evaluation; fully backward compatible, zero behavioural change
- `DroneSmokeTest` observes the evaluation and logs `evaluationObserved`, `qualityScore`, `evaluationGrade`, `evaluationValid`; observational only — **no PASS criteria change**

#### Tests (ADRL.Tests.Editor.Decision)

- New `DecisionEvaluationTests` (20): calculator computes quality score; calculator empty for no decisions; calculator uniform distribution reduces consistency; calculator uses trace step and timestamp; calculator deterministic output; formatter deterministic; formatter contains sections; formatter empty output; validator accepts valid; validator rejects negative; validator rejects NaN/Infinity; validator rejects invalid grade/status; metrics suitability and coverage; metrics confidence and optimization; metrics consistency and grade; metrics overall quality weighted; engine evaluation after steps; deterministic evaluation; diagnostics carries evaluation; reset clears evaluation and behaviour unchanged

#### Validation

- EditMode suite: **301/301 passing** (281 prior + 20 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, finite reward, **sumInvariant=True**, movement/behaviour/reward/decision/optimization/determinism unchanged, decision/context/optimization/knowledge/explanation/trace/telemetry/analytics observed, **evaluation observed and validated**
- Mission logic, prioritization, selection, optimization, execution, knowledge, memory, explainability, tracing, telemetry, analytics, navigation, rewards, simulation, RL and training unchanged; evaluation is read-only and never influences decisions

### Phase 9.4 — Autonomous Decision Analytics Framework (2026-08-04)

#### Deterministic Analytics Layer (ADRL.AI.Decision.Analytics)

- **`DecisionAnalyticsSnapshot`** (new) — immutable, deterministic engineering-analytics snapshot computed from decision telemetry: `DecisionCount`, `AverageDecisionConfidence`, `AverageExecutionConfidence`, `AverageOptimizationConfidence`, `KnowledgeUtilizationRate`, `MemoryUtilizationRate`, `BehaviourBalanceScore`, `MissionBalanceScore`, `BehaviourEntropy`, `MissionEntropy`, `DecisionHealthScore`, `DecisionStep`, `DecisionTimestamp` and the `BehaviourAnalytics`/`MissionAnalytics` arrays (one owned entry per enum value in enum order, with count and share); all readonly, `Empty` canonical. Ratio metrics are percentages in [0, 100]; entropy is finite, non-negative Shannon entropy
- **`DecisionAnalyticsCalculator`** (new, static) — the single owner of analytics computation: pure, stateless, `Calculate(DecisionTelemetrySnapshot)` derives the analytics snapshot (confidence averages scaled to percentages; utilization = record count relative to decision count, clamped; balance = normalized Shannon entropy/evenness; entropy = Shannon entropy; health = weighted combination) without caching, without LINQ and without allocation beyond the snapshot; the trace records the optimized execution profile's confidence as its optimization confidence, so execution and optimization averages carry the same signal; null/truncated telemetry degrades to zero-count analytics, never exceptions
- **`DecisionHealthCalculator`** (new, static) — pure health-score helper with the suggested weighting (decision confidence 35%, execution confidence 25%, optimization confidence 15%, knowledge usage 10%, memory usage 10%, behaviour balance 5%), clamping the weighted sum to [0, 100]; deterministic
- **`DecisionAnalyticsFormatter`** (new, static) — pure, deterministic report (Decision Analytics header, Health Score, Decision Confidence, Execution Confidence, Optimization, Knowledge Usage, Memory Usage, Behaviour Balance, Mission Balance, Behaviour Entropy, Mission Entropy, Total Decisions) with a fixed 20-column dotted label layout, invariant-culture floats and deterministic balance labels (Excellent/Good/Fair/Poor/Very Poor); allocates only the final string
- **`DecisionAnalyticsValidator`** (new, static) — pure validation: no negatives, no NaN, no infinity, health score in [0, 100], entropy finite, balance in [0, 100], utilization in [0, 100], confidence in [0, 100], snapshot complete (analytics arrays present, one entry per enum value, count totals equal the decision count); plain bools, never throws
- **`DecisionDiagnostics`** — new `LastAnalytics` member + `WithAnalytics`; constructor extended with an optional parameter; `Empty` carries `DecisionAnalyticsSnapshot.Empty`; `WithExplanation`/`WithTraceFrame`/`WithTelemetry` preserve analytics
- **`DecisionEngine`** — after telemetry, computes the analytics snapshot via the `DecisionAnalyticsCalculator` (single owner of analytics computation), exposes `LastAnalytics` + `GetAnalytics()` + `ResetAnalytics()`, and recomposes the last snapshot's diagnostics with the analytics so diagnostics, telemetry and analytics stay synchronized; `Reset()` clears analytics; fully backward compatible, zero behavioural change
- `DroneSmokeTest` observes the analytics and logs `analyticsObserved`, `decisionCount`, `healthScore`, `knowledgeUsage`, `memoryUsage`, `behaviourEntropy`, `missionEntropy`, `analyticsValid`; observational only — **no PASS criteria change**

#### Tests (ADRL.Tests.Editor.Decision)

- New `DecisionAnalyticsTests` (20): calculator computes averages and health; calculator empty telemetry; calculator utilization rates; calculator balance and entropy; calculator deterministic output; formatter deterministic; formatter contains sections; formatter empty analytics; validator accepts valid; validator rejects negative; validator rejects NaN/Infinity; validator rejects out of range; health weighted computation; health clamped to range; health zero inputs; health perfect inputs; engine analytics after steps; deterministic analytics; diagnostics carries analytics; reset clears analytics and behaviour unchanged

#### Validation

- EditMode suite: **281/281 passing** (261 prior + 20 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, finite reward, **sumInvariant=True**, movement/behaviour/reward/decision/optimization/determinism unchanged, decision/context/optimization/knowledge/explanation/trace/telemetry observed, **analytics observed and validated**
- Mission logic, prioritization, selection, optimization, execution, knowledge, memory, explainability, tracing, telemetry, navigation, rewards, simulation, RL and training unchanged; analytics is read-only and never influences decisions

### Phase 9.3 — Autonomous Decision Telemetry Framework (2026-08-04)

#### Deterministic Telemetry Layer (ADRL.AI.Decision.Telemetry)

- **`DecisionTelemetrySnapshot`** (new) — immutable, deterministic snapshot of the decision pipeline's telemetry: `DecisionCount`, running averages (`AverageDecisionConfidence`, `AverageOptimizationConfidence`, `AverageCandidateCount`, `AverageExecutionSpeedMultiplier`, `AverageTurnRateMultiplier`), `KnowledgeRecordCount`, `MemoryRecordCount`, `CurrentBehaviour`, `CurrentMission`, `CurrentExecutor`, `LatestDecisionStep`, `LatestDecisionTimestamp` and the `BehaviourDistribution`/`MissionDistribution` (one owned entry per enum value, zero-count entries included); all readonly, `Empty` canonical
- **`DecisionTelemetryStatistics`** (new, internal) — running accumulator owned only by the collector: running totals, running averages, behaviour/mission counters (pre-sized arrays indexed by enum value), knowledge/memory counts and the latest-value projection; updates are O(1) and allocation-free; no LINQ; never exposed publicly
- **`DecisionTelemetryCollector`** (new) — the single runtime owner of telemetry: `Observe` (O(1), allocation-free, null-safe), `GetSnapshot` (fresh immutable projection), `Reset`; reads the completed trace frame only and never modifies the engine, snapshot, explanation, trace, knowledge, memory, behaviour or command
- **`DecisionTelemetryFormatter`** (new) — pure, deterministic rendering of the snapshot (Decisions, Average Confidence, Average Optimization Confidence, Behaviour Distribution, Mission Distribution, Knowledge Records, Memory Records, Average Candidates, Execution Speed, Turn Multiplier, current values, latest clocks); floats rendered with the invariant culture, same telemetry → byte-identical output
- **`DecisionTelemetryValidator`** (new) — pure validation: no negatives, no NaN/Infinity, distribution totals equal the decision count, snapshot complete, counts valid, averages in range, current values valid; plain bools, never throws
- **`DecisionDiagnostics`** — new `LastTelemetry` member + `WithTelemetry`; constructor extended with an optional parameter; `Empty` carries `DecisionTelemetrySnapshot.Empty`; `WithExplanation`/`WithTraceFrame` preserve telemetry
- **`DecisionEngine`** — after tracing, observes the trace frame via the `DecisionTelemetryCollector` (single owner of telemetry), exposes `LastTelemetry` + `TelemetryCollector` + `GetTelemetry()` + `ResetTelemetry()`, and recomposes the last snapshot's diagnostics with the telemetry so diagnostics and telemetry stay synchronized; `Reset()` clears telemetry; fully backward compatible, zero behavioural change
- `DroneSmokeTest` observes the telemetry and logs `telemetryObserved`, `decisionCount`, `averageConfidence`, `averageOptimizationConfidence`, `knowledgeRecords`, `memoryRecords`, `candidateAverage`, `behaviourDistribution`, `telemetryValid`; observational only — **no PASS criteria change**

#### Tests (ADRL.Tests.Editor.Decision)

- New `DecisionTelemetryTests` (20): collector observe accumulates count; collector empty before observation; collector reset; collector null frame ignored; collector owned snapshot projection; statistics running averages; statistics behaviour distribution; statistics mission distribution; statistics latest values; formatter deterministic; formatter contains sections; formatter empty snapshot; validator accepts valid; validator rejects negative; validator rejects NaN/Infinity; validator rejects distribution mismatch; engine telemetry after steps; deterministic telemetry; diagnostics carries telemetry; reset clears telemetry and behaviour unchanged

#### Validation

- EditMode suite: **261/261 passing** (241 prior + 20 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, finite reward, **sumInvariant=True**, movement/behaviour/optimization/determinism unchanged, decision/context/optimization/knowledge/explanation/trace observed, **telemetry observed and validated**
- Mission logic, prioritization, selection, optimization, execution, knowledge, memory, explainability, tracing, navigation, rewards, simulation, RL and training unchanged; telemetry is read-only and never influences decisions

### Phase 9.2 — Autonomous Decision Trace & Replay Framework (2026-08-04)

#### Deterministic Trace Layer (ADRL.AI.Decision.Trace)

- **`DecisionTraceFrame`** (new) — immutable, replayable trace of one completed decision step: `DecisionStep`, `DecisionTimestamp`, `Assessment`, `Mission`, `Behaviour`, `OptimizationProfile`, `Command`, `Explanation`, `Diagnostics`, `WinningTask`, `CandidateCount`, `KnowledgeCount`, `MemoryCount`, `ExecutorName`, `OptimizationConfidence` and the narrative `Reasons`; all members readonly, reasons are an owned copy, `Empty` is the canonical pre-step frame. Implemented as a sealed immutable class because the frame embeds the step's `DecisionDiagnostics` while the diagnostics projection exposes the last trace frame - a value type would form an infinitely sized layout (CS0523)
- **`DecisionTraceBuilder`** (new) — single owner of trace composition; pure, stateless, deterministic function of the built `DecisionContextSnapshot` and its `DecisionExplanation`; reads both only, never mutates runtime state; counts and executor names read null-safely
- **`DecisionTraceStore`** (new) — single owner of trace history: bounded, configurable capacity (default 128), `Append`, `Clear`, `Count`, `Latest`, `Get(index)`, `Enumerate` (owned copy, oldest first); deterministic append ordering; oldest-first eviction when full; out-of-range reads return `Empty` (never throw); no LINQ
- **`DecisionReplay`** (new) — deterministic replay utility: `ReplayFrame`, `ReplayLatest`, `ReplayRange` (owned copy); reads immutable trace frames only and never writes runtime, engine, snapshot, explanation, diagnostics, mission or behaviour; null/invalid requests yield empty results, never exceptions
- **`DecisionReplayValidator`** (new) — pure replay-consistency validation: frame complete, behaviour matches trace, mission matches trace, explanation matches trace, diagnostics synchronized (step/behaviour/command/mission/winner), command synchronized, knowledge synchronized, optimization synchronized (confidence identical across frame, profile and explanation); returns plain bools, no exceptions
- **`DecisionContextSnapshot`** — new readonly `Trace` field + `WithTrace`; 9-/10-/11-/12-argument constructors preserved (delegate with the empty trace); `WithExecutionProfile`, `WithKnowledge`, `WithScoredCandidates`, `WithDiagnostics` all preserve every field
- **`DecisionDiagnostics`** — new `LastTraceFrame` member + `WithTraceFrame`; constructor extended with an optional parameter; `Empty` carries `DecisionTraceFrame.Empty`; `WithExplanation` preserves the trace
- **`DecisionEngine`** — after building the explanation, builds the immutable trace frame via the `DecisionTraceBuilder` (single owner of trace composition), embeds it into the snapshot (`WithTrace`) and its diagnostics (`WithTraceFrame`), appends it to the bounded `TraceStore`, and exposes `LastTraceFrame` + `TraceStore`; `Reset()` clears the store and restores the empty frame; fully backward compatible, zero behavioural change
- `DroneSmokeTest` observes the trace store and logs `traceObserved`, `traceCount`, `latestTraceStep`, `latestBehaviour`, `latestMission`, `replayValid` (replayed latest frame validated); observational only — **no PASS criteria change**

#### Tests (ADRL.Tests.Editor.Decision)

- New `DecisionTraceTests` (20): append; retrieval; eviction; replay latest; replay range; deterministic ordering; immutable frame; behaviour synchronization; mission synchronization; optimization synchronization; command synchronization; knowledge synchronization; memory synchronization; explanation synchronization; diagnostics synchronization; replay validator accepts valid; replay validator rejects mismatch; empty trace; reset clears store; identical snapshot → identical trace

#### Validation

- EditMode suite: **241/241 passing** (221 prior + 20 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, finite reward, **sumInvariant=True**, movement/behaviour/optimization/determinism unchanged, decision/context/optimization/knowledge/explanation observed, **trace observed and replay validation succeeds**
- Mission logic, prioritization, selection, optimization, execution, knowledge, memory, explainability, navigation, rewards, simulation, RL and training unchanged; tracing is read-only and never influences decisions

### Phase 9.1 — Autonomous Decision Explainability Framework (2026-08-04)

#### Deterministic Explanation Layer (ADRL.AI.Decision.Explainability)

- **`DecisionReason`** (new) — immutable value object representing one individual reasoning entry (`Section` + `Text` + `IsValid`), e.g. "Target detected (0.91)", "Mission RescueVictim", "Approach selected"; produced only by the builder, consumed read-only by the formatter and validation
- **`DecisionExplanation`** (new) — immutable, deterministic explanation of one completed decision step: `Assessment`, `Knowledge` summary (record count, known victims/hazards/obstacles, nearest victim/hazard distances), `Memory` summary (last behaviour, victim/obstacle records, record count), `Mission`, ordered `CandidateTasks`, full `ScoredCandidates` priority list, `Winning`, `Behaviour`, `Executor`, `OptimizationProfile`, `Command`, `DecisionTimestamp`, `DecisionStep` and the narrative `Reasons`; every array is an owned copy, all members readonly, `Empty` is the canonical pre-step explanation
- **`DecisionExplanationBuilder`** (new) — single owner of explanation composition; pure, stateless, deterministic function of the built `DecisionContextSnapshot`; reads the snapshot only, never scores/selects/mutates; defensively copies every array; numeric text formatted with the invariant culture so output is byte-identical across locales
- **`DecisionExplanationFormatter`** (new) — deterministic, human-readable rendering of the explanation: reasons grouped by section (Assessment → Knowledge → Mission → Candidates → Priority → Winner → Behaviour → Executor → Optimization → Command) with section headers and separators; same explanation → byte-identical output
- **`DecisionExplanationValidator`** (new) — pure validation: complete, no null references, mission valid, winner matches behaviour, behaviour matches executor (`{Behaviour}Executor`), executor matches command (IdleExecutor ⇔ idle command), candidate count consistent, timestamp/step valid; canonical empty explanation validates successfully
- **`DecisionContextSnapshot`** — new readonly `ScoredCandidates` (owned copy) so the snapshot carries the full scored priority list (not just the winner); 9-/10-/11-argument constructors preserved (delegate with an empty list); `WithScoredCandidates`, `WithDiagnostics`, `WithExecutionProfile`, `WithKnowledge` all preserve every field
- **`DecisionDiagnostics`** — new `LastExplanation` member + `WithExplanation`; constructor extended with an optional parameter; `Empty` carries `DecisionExplanation.Empty`; the engine recomposes the snapshot's diagnostics to carry the explanation, so snapshot, diagnostics and explanation stay synchronized
- **`DecisionEngine`** — after capturing the snapshot, builds the immutable explanation via the `DecisionExplanationBuilder` (single owner of explanation composition), stores `LastExplanation` (exposed), and recomposes the snapshot's diagnostics with it; `Reset()` restores the empty explanation; fully backward compatible, zero behavioural change
- `DroneSmokeTest` observes the explanation and logs `explanationObserved`, `explanationWinner`, `explanationBehaviour`, `explanationExecutor`, `explanationCommand`, `formatterValid`, `formatterLines` and the rendered formatter output; observational only — **no PASS criteria change**

#### Tests (ADRL.Tests.Editor.Decision)

- New `DecisionExplanationTests` (20): build explanation; empty explanation valid; immutable explanation; assessment captured; knowledge captured; memory captured; mission captured; candidates captured; priority captured; winner captured; behaviour captured; executor captured; optimization captured; command captured; formatter deterministic; formatter contains sections; validator accepts valid; validator rejects invalid; same snapshot → same explanation; explanation does not modify snapshot

#### Validation

- EditMode suite: **221/221 passing** (201 prior + 20 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, finite reward, **sumInvariant=True**, movement/behaviour/optimization/determinism unchanged, decision/context/optimization/knowledge observed, **explanation observed, formatter output valid**
- Mission logic, prioritization, selection, optimization, execution, knowledge, memory, navigation, rewards, simulation, RL and training unchanged; explainability is read-only and never influences decisions

### Phase 9.0 — Autonomous World Knowledge Framework (2026-08-04)

#### Persistent Knowledge Layer (ADRL.AI.Decision.Knowledge)

- **`KnowledgeType`** (new) — immutable knowledge classification: `Unknown`, `Victim`, `Obstacle`, `Hazard`, `ExploredRegion`, `SafeRegion`
- **`WorldKnowledgeRecord`** (new) — immutable snapshot of one piece of world knowledge: `Type`, `Position`, `Confidence`, `Timestamp`, `Age`, `Source`, `IsValid`; no mutable state, never reclassified in place
- **`KnowledgePolicy`** (new) — single configuration owner: `MaximumRecords`, per-type lifetimes, `MergeDistance`, `MinimumConfidence`, `ConfidenceDecay`, projection geometry (`DetectionDistance`, `SweepSpread`), `HazardProximityThreshold` (aligned with the mission's hazard threshold), memory `CorroborationWindow`/`CorroborationBoost`; configuration only, no logic
- **`WorldKnowledgeStore`** (new) — single owner of persistent knowledge storage: fixed-capacity array, `Store`/`Update`/`Remove`/`Expire`/`Clear`/`QueryNearest`/`QueryAll`/`Contains`/`Count`/`CountOf`; never scores, selects or decides; deterministic tie-break and oldest-timestamp eviction; owned query copies
- **`IKnowledgeQuery`** + **`KnowledgeQuery`** (new) — pure, stateless retrieval facade: `NearestVictim`, `NearestHazard`, `NearestObstacle`, `KnownVictims`, `KnownHazards`, `KnownObstacles`, `KnownRegions`; never writes to the store
- **`KnowledgeUpdater`** (new) — single owner of all knowledge writes: inserts new observations, refreshes/merges re-observations within the merge distance, corroborates records through a fresh short-term memory when the current assessment is occluded, decays confidence and expires stale records; never performs decisions, scoring or selection
- **`DecisionEngine`** — after assessment, runs `KnowledgeUpdater.Update(assessment, memory, stepClock)` before memory/mission; embeds the store into the built snapshot via `WithKnowledge`; exposes `KnowledgeStore`; ctor gains an optional `KnowledgePolicy` parameter; `Reset()` clears knowledge; fully backward compatible
- **`DecisionContextSnapshot`** — new readonly `Knowledge` store field; `WithKnowledge` composes it without mutating the original; 9- and 10-argument constructors preserved (delegate with the empty store); builder unchanged
- **`DecisionRuntimeState`** — diagnostic extension exposing `KnowledgeRecordCount`, `KnownVictims`, `KnownHazards`, `KnownObstacles`, `KnowledgeTimestamp`; 8- and 12-argument constructors preserved; `WithKnowledge` composes a fresh payload
- **`DecisionDiagnostics`** — new `KnowledgeRecordCount`, `NearestVictimDistance`, `NearestHazardDistance`, `KnowledgeTimestamp`; `From` gains a knowledge-store overload; legacy 3-argument `From` preserved (delegates with the empty store); constructor extended with optional parameters
- `DroneSmokeTest` observes the world-knowledge store and logs `knowledgeObserved`, `knowledgeRecords`, `knownVictims`, `knownHazards`, `knownObstacles`, `nearestVictimDistance`, `nearestHazardDistance`, `knowledgeTimestamp`; knowledge observation gated into the smoke PASS criteria

#### Tests (ADRL.Tests.Editor.Decision)

- New `WorldKnowledgeTests` (20): store victim; store obstacle; store hazard; merge duplicate within distance; expire stale records; remove stale record; capacity enforcement; nearest-victim query; nearest-hazard query; empty query; confidence update on refresh; confidence decay removes low confidence; deterministic ordering; snapshot integration; diagnostics synchronization; knowledge updater stores from assessment; knowledge query known lists; engine integration; reset clears knowledge; store invariants (validator)

#### Validation

- EditMode suite: **201/201 passing** (181 prior + 20 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, finite reward, **sumInvariant=True**, decision/context/optimization observed, **world knowledge observed**
- Mission decisions, behaviour selection, prioritization, optimization, execution and determinism unchanged; knowledge is recorded and queried only, never used to alter decisions (read-only for consumers)

### Phase 8.9 — Autonomous Behaviour Optimization Framework (2026-08-03)

#### Deterministic Execution Optimization

- **`BehaviourExecutionProfile`** (new, `ADRL.AI.Decision.Optimization`) — immutable execution profile: `SpeedMultiplier`, `TurnRateMultiplier`, `CautionLevel`, `PreferredDistance`, `Smoothness`, `ExecutionConfidence`; readonly, `Empty` = neutral no-op (multipliers 1), never mutated
- **`OptimizationPolicy`** (new) — single configuration owner of execution optimization: per-behaviour speed targets (multipliers over the executor base), turn-rate bounds, obstacle caution, victim approach distance, confidence scaling, smoothing factor; configuration only, no logic
- **`IBehaviourOptimizer`** + **`BehaviourOptimizer`** (new) — single owner of execution optimization; reads the current `DecisionContextSnapshot` (mission objective, winning priority score/confidence, memory confidence, obstacle proximity) and produces a deterministic profile that adjusts speed, turn rate, caution, smoothing and execution confidence; never changes the mission, the priority, the behaviour or the command
- **`OptimizationValidator`** (new) — pure validation: profile valid, confidence/speed/turn/caution in range, deterministic outputs; multiplier bounds are the single source of truth the optimizer clamps to
- **`DecisionEngine`** — after behaviour selection, builds the current context, runs the optimizer, then hands the profile to the executor; the built snapshot carries `ExecutionProfile`; ctor, `Decide`, `Reset`, `GetDiagnostics`, `LastSnapshot` remain backward compatible (new optional `optimizationPolicy` ctor parameter)
- **`DecisionContextSnapshot`** — new readonly `ExecutionProfile` field; snapshot becomes the complete per-step context; `WithExecutionProfile` composes it without mutating the original; 9-argument constructor preserved (delegates with the empty profile), builder unchanged
- **`DecisionRuntimeState`** — diagnostic extension exposing `ExecutionSpeedMultiplier`, `ExecutionTurnRate`, `ExecutionConfidence`, `OptimizationTimestamp`; 8-argument constructor preserved
- **Executors** — `IBehaviourExecutor.Resolve(assessment, profile)` added; each executor scales its base movement constants by the profile (search: forward × speed, yaw × turn; approach: steering/yaw × turn and caution; avoid: backoff × speed, evasion × turn and caution; idle: profile ignored); the legacy `Resolve(assessment)` overload delegates with the empty profile, so the Phase 8.7 path is byte-identical
- `DroneSmokeTest` observes the optimized execution profile and logs `optimizationObserved`, `execConfidence`, `speedMult`, `turnRateMult`, `optimizationTimestamp`; profile observation gated into the smoke PASS criteria

#### Tests (ADRL.Tests.Editor.Decision)

- New `BehaviourOptimizerTests` (15): deterministic identical inputs; search speed optimization; approach turn-rate optimization; avoid caution; rescue speed + preferred distance; idle returns empty profile; confidence scaling; smoothing from policy; speed clamping; turn-rate clamping; profile validity; validator rejects out-of-range profiles; validator confirms determinism; executor legacy path matches empty profile; engine integration (profile applied end-to-end, snapshot + diagnostics synchronized, determinism)

#### Validation

- EditMode suite: **181/181 passing** (166 prior + 15 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, finite reward, **sumInvariant=True**, decision/mission/winning-task/behaviour/command observed, context snapshot observed, **optimized execution profile observed**
- Mission decisions, behaviour selection, prioritization and determinism unchanged; only execution parameters (speed/turn/caution) are scaled; movement remains deterministic

### Phase 8.8 — Autonomous Decision Context Framework (2026-08-03)

#### Unified Immutable Runtime Context

- **`DecisionContextSnapshot`** (new, `ADRL.AI.Decision.Context`) — immutable runtime snapshot unifying every subsystem output of one decision step into a single consistent context: `Assessment`, `Memory`, `Mission`, `Candidates`, `Winning`, `Behaviour`, `Command`, synchronized `Diagnostics` and `RuntimeState`; all fields readonly, never mutated in place, `Empty` canonical pre-step snapshot
- **`DecisionRuntimeState`** (new) — pure runtime metadata: `DecisionStep`, `EpisodeStep`, `DecisionTimestamp`, `CurrentBehaviour`, `CurrentMission`, `LastCommand`, `CurrentExecutor`, `CurrentWinner`; the two step clocks advance together in the current single-decision-per-step runtime
- **`IDecisionContextBuilder`** (new) + **`DecisionContextBuilder`** (new) — the single owner of context composition: pure, stateless, deterministic, null-safe, defensively copies the candidate array so a built snapshot never aliases the engine's working buffer; no decision logic, no scoring, no transitions
- **`DecisionContextValidator`** (new) — pure validation utilities confirming a snapshot is complete, holds no null references, the behaviour agrees with the winning objective, the mission objective is present, the embedded diagnostics agree field-for-field (so the step can be replayed), the candidate count is consistent, and the command agrees with the diagnostics
- **`DecisionEngine`** — after each step captures the whole decision chain as a `DecisionContextSnapshot` through the builder, exposes it as `LastSnapshot`, and derives `GetDiagnostics()` from the built snapshot (single source of truth); the redundant per-step `_last*` bookkeeping was removed
- **`DecisionDiagnostics`** — new `From(runtimeState, assessment, candidateCount)` projection so the embedded diagnostics are synchronized with the runtime state and candidate array by construction; `Empty` and the constructor contract are unchanged
- `DroneSmokeTest` now observes the built context snapshot and logs `contextObserved` / `contextStep`

#### Tests (ADRL.Tests.Editor.Decision)

- New `DecisionContextTests` (15): snapshot builds correctly; immutable snapshot; builder deterministic; runtime metadata correct; candidate count matches diagnostics; behaviour matches winner; mission matches diagnostics; command matches executor; validator rejects invalid snapshots; empty snapshot valid; repeated builds identical; no hidden state; null protection; reset clears runtime state; backward compatibility

#### Validation

- EditMode suite: **166/166 passing** (151 prior + 15 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, finite reward, **sumInvariant=True**, movement preserved, decision/mission/winning-task/behaviour/command observed, and the context snapshot observed
- No runtime behaviour, scoring, ordering, reward or determinism changes; every Phase 8.7.1 and earlier semantic preserved

### Phase 8.7.1 — Architectural Refinement: Candidate Generation Separation + Extended Decision Diagnostics (2026-08-03)

#### Candidate Generation Separation

- **`TaskCandidate`** (new) — immutable snapshot coupling a candidate `MissionTask` with its `CandidateOrigin` (`CurrentPerception` or `Continuity`) plus the deterministic evidence (`Proximity`, `Confidence`) the scorer consumes; the contract between generator and scorer
- **`CandidateOrigin`** (new) — why a candidate is currently available; determined solely by the generator, never by the scorer
- **`TaskCandidateGenerator`** (new) — the single owner of candidate availability: pure, stateless, deterministic. Derives the currently-valid objective states from perception, behaviour memory and the coordinator's task and emits each as a `TaskCandidate` in a fixed order with no duplicates. Never scores, compares, chooses winners or modifies runtime state; an invalid (empty) assessment yields no candidates
- **`PriorityEvaluator`** responsibility reduced to scoring only — consumes `TaskCandidate[]` and emits `TaskPriority[]`; the availability logic moved to the generator
- **`TaskPrioritizer`** reduced to pure arbitration — new core `Select(TaskPriority[])` returns the highest-priority winner; the legacy assessment/memory/mission overload is retained for backward compatibility and simply runs generation and scoring before delegating
- **`DecisionEngine`** runtime now runs `Assessment → Memory → MissionCoordinator → TaskCandidateGenerator → PriorityEvaluator → TaskPrioritizer → MissionTask → BehaviourSelector → Executor → DroneCommand`; the chain is explicit and `DecisionEngine` remains the only runtime decision authority. No scoring, ordering, policy or behaviour values changed

#### Extended Decision Diagnostics

- **`DecisionDiagnostics`** now also exposes the selected executor (`SelectedExecutor`), the resolved command of the last step (`LastCommand`), the deterministic step-clock timestamp (`DecisionTimestamp`) and the generated candidate count (`CandidateCount`) for complete runtime replay; only immutable snapshots are exposed, never mutable state or internal collections. Constructor extended with optional parameters (backward compatible)
- `DroneSmokeTest` now logs the selected executor, candidate count and decision timestamp alongside the decision path

#### Tests (ADRL.Tests.Editor.Decision)

- New `TaskCandidateGeneratorTests` (11): hazard generates avoid candidate; confirmed victim generates rescue candidate; neutral assessment generates continuity candidate; no duplicate candidates; deterministic ordering; same inputs → same candidate list; generation independent from scoring; generation independent from prioritization; generated candidates always valid; invalid assessment produces no candidates; continuity keeps the coordinator's task identity
- `TaskPrioritizerTests` extended with 4 diagnostics tests: candidate count, selected executor, last command, decision timestamp

#### Validation

- EditMode suite: **151/151 passing** (136 prior + 15 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, winning task observed (AvoidHazard) with a deterministic priority score, selected executor observed (`AvoidExecutor`), candidate count and decision timestamp surfaced, behaviour follows the winning task, movement preserved, reward finite and **sumInvariant=True**
- No runtime behaviour, scoring, priority ordering, reward or determinism changes; all Phase 8.7 semantics preserved

### Phase 8.7 — Autonomous Task Prioritization Framework (2026-08-03)

#### Task Prioritization Layer (ADRL.AI.Decision.Prioritization)

- **New `ADRL.AI.Decision.Prioritization` namespace** (`Assets/ADRL/Scripts/AI/Decision/Prioritization/`, existing `ADRL.AI` assembly): deterministic objective arbitration. When multiple valid objectives exist simultaneously, the prioritizer decides which the drone pursues. Not path planning, navigation, SLAM, reinforcement learning, or swarm coordination
- **`TaskPriority`** — immutable scored candidate (`MissionTask`, `PriorityScore`, `Confidence`, `IsValid`); never mutated in place
- **`PriorityPolicy`** — single-owner configuration surface for all scoring weights (`VictimPriority`, `HazardPriority`, `SearchPriority`, `ResumePriority`, `IdlePriority`, `MemoryBonus`, `ConfidenceBonus`, `DistancePenalty`, `CooldownPenalty`, `PriorityTieTolerance`); no magic numbers elsewhere. Safety-first defaults matching the mission coordinator's stance (hazard outranks victim, victim outranks search)
- **`IPriorityEvaluator`** — pure, stateless scoring contract
- **`PriorityEvaluator`** — the single owner of deterministic objective scoring. Derives currently-valid candidates from current perception (hazard, confirmed victim, unconfirmed target), behaviour memory and the coordinator's task; each candidate is scored `BasePriority + ConfidenceBonus/MemoryBonus - DistancePenalty*(1 - proximity) - CooldownPenalty`, emitted at most once per objective state in a fixed order
- **`TaskPrioritizer`** — the single runtime owner of objective arbitration: scores every valid candidate via the evaluator and returns the highest-priority winner. No transitions, no movement, no memory ownership, no hidden state; deterministic ties resolve to the first candidate in generation order within `PriorityTieTolerance`

#### DecisionEngine, Selection & Diagnostics Integration

- **`DecisionEngine`** now runs `Assessment → Memory → MissionCoordinator → TaskPrioritizer → MissionTask → BehaviourSelector → Executor → DroneCommand`; the prioritizer's winning task feeds behaviour selection, replacing the coordinator's task directly (they agree by design; the prioritizer is the explicit arbitration authority). `DecisionEngine` remains the only runtime decision authority
- `DecisionEngine` constructor gains an optional `PriorityPolicy` parameter (backward compatible); `Reset()` clears the prioritizer's last winner; the mission policy is retained for prioritizer thresholds
- **`DecisionDiagnostics`** now also exposes the prioritizer's winning objective (`LastWinning`) and its deterministic score (`WinningPriorityScore`) alongside step count, last behaviour, last assessment and the current mission task; synchronized every decision step
- `DroneSmokeTest` now logs the winning task (`lastWinningTask`) and score (`winningScore`) observationally alongside the decision path

#### Tests (ADRL.Tests.Editor.Decision)

- New `TaskPrioritizerTests` (15): higher score wins; equal scores tie-break deterministically; victim outranks search; hazard outranks victim; cooldown penalty lowers score; memory bonus increases score; distance penalty reduces score; invalid candidates ignored; no duplicate candidate states; stable ordering; same inputs → same outputs; DecisionEngine uses the prioritizer; diagnostics expose the winning score; policy constants respected (configurable victim-first hierarchy); behaviour follows the winning task

#### Validation

- EditMode suite: **136/136 passing** (121 prior + 15 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, winning task observed (AvoidHazard) with a deterministic priority score, behaviour follows the winning task, movement preserved, reward finite and **sumInvariant=True**

### Phase 8.6 — Autonomous Mission Task Coordination Framework (2026-08-03)

#### Mission Task Coordination Layer (ADRL.AI.Decision.Mission)

- **New `ADRL.AI.Decision.Mission` namespace** (`Assets/ADRL/Scripts/AI/Decision/Mission/`, existing `ADRL.AI` assembly): a deterministic mission task coordinator that determines *what objective* the drone currently pursues, sitting between behaviour memory and behaviour selection. An architectural extension of the Phase 8.5 runtime — not path planning, mapping, SLAM, or reinforcement learning
- **`MissionTaskState`** — immutable objective enum: `Idle`, `SearchArea`, `InvestigateTarget`, `RescueVictim`, `AvoidHazard`, `ResumeSearch`
- **`MissionTask`** — immutable snapshot of the current objective (`State`, `EntryStep`, `PreviousState`, `IsValid`); never mutated in place, every transition produces a fresh value
- **`MissionPolicy`** — single-owner configuration surface for all mission constants (`ResumeTimeout`, `TransitionCooldown`, `HazardProximityThreshold`, `VictimConfirmationProximity`, `HazardPriority`, `VictimPriority`, `ConfidenceThreshold`); no magic numbers elsewhere; durations are decision steps so the layer stays deterministic and testable
- **`MissionTransitionRules`** — pure, stateless transition function: current perception always overrides any objective (imminent hazard and perceived target handled first); SearchArea → InvestigateTarget on target detected; InvestigateTarget → RescueVictim on victim confirmed; InvestigateTarget → SearchArea when the target is lost before confirmation and the victim is no longer in memory; RescueVictim → ResumeSearch once the victim leaves perception and memory; ResumeSearch → SearchArea after `ResumeTimeout`; ANY state → AvoidHazard on hazard (priority-driven); AvoidHazard → previous task once the hazard clears and `TransitionCooldown` elapses; Idle → SearchArea on the first viable assessment; an invalid assessment holds the current task (no fabrication from garbage data)
- **`MissionCoordinator`** — the single owner of the current mission task, its transitions, priority, continuity and state. Never generates movement, controls motors, reads physics, computes rewards, or performs path planning/mapping

#### DecisionEngine & Selection Integration

- **`DecisionEngine`** now sits above the mission coordinator: `Decide()` refreshes memory, advances the mission task from the assessment + memory at the deterministic step clock, selects a behaviour (current perception first, then the mission task), records the chosen behaviour, then resolves the command via the executor factory. `DecisionEngine` remains the sole runtime decision authority
- **`IBehaviourSelector` / `BehaviourSelector`** — added a mission-aware `Select(assessment, mission)` overload (existing single-argument and memory-aware overloads preserved for backward compatibility). Mission maps to behaviour for neutral situations only: SearchArea/ResumeSearch → Search, InvestigateTarget/RescueVictim → Approach, AvoidHazard → Avoid, Idle → Idle; current sensor data always wins
- `DecisionEngine` constructor gains an optional `MissionPolicy` parameter (backward compatible); `Reset()` also resets the mission coordinator; the mission task is exposed via `DecisionEngine.Mission`
- **`DecisionDiagnostics`** now also exposes the current mission task (`LastMissionTask`) alongside step count, last behaviour and last assessment; `GetDiagnostics()` is synchronized every decision step
- `DroneSmokeTest` now logs the observed mission task (`lastMission`) observationally alongside the decision path

#### Tests (ADRL.Tests.Editor.Decision)

- New `MissionCoordinatorTests` (12): initial idle mission; valid assessment starts SearchArea; target detected → InvestigateTarget; confirmed victim → RescueVictim; rescue completed → ResumeSearch; ResumeSearch → SearchArea after timeout; hazard override from any mission; hazard cooldown delays restoration; previous task restored after cooldown; mission determinism for identical history; invalid assessment produces an Idle decision and holds the mission; mission continuity while the victim is in memory

#### Validation

- EditMode suite: **121/121 passing** (109 prior + 12 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, movement preserved, reward finite and **sumInvariant=True**, `decisionSeen=True`, mission task observed (AvoidHazard), behaviour follows the mission task

### Phase 8.5 — Autonomous Behaviour Memory & Coordination Framework (2026-08-03)

#### Behaviour Memory Layer (ADRL.AI.Decision.Memory)

- **New `ADRL.AI.Decision.Memory` namespace** (`Assets/ADRL/Scripts/AI/Decision/Memory/`, existing `ADRL.AI` assembly): a short-term behaviour memory so the drone carries recently perceived context across consecutive frames. An architectural extension of the Phase 8.4 runtime — not path planning, mapping, SLAM, or reinforcement learning
- **`MemoryRecord`** — immutable runtime memory snapshot (`BehaviourState`, `Timestamp`, `Age`, `Confidence`, `IsValid`); no mutable public fields; ageing and confidence decay are expressed by replacing the record with an updated immutable one
- **`MemoryPolicy`** — single-owner configuration surface for all memory timing/decay/capacity constants (`VictimMemoryDuration`, `ObstacleMemoryDuration`, `ConfidenceDecay`, `RefreshThreshold`, `MaxRecords`); no magic numbers elsewhere; durations are decision steps so the layer stays deterministic and testable without wall-clock dependency
- **`BehaviourMemory`** — bounded, deterministic store (`LastVictimSeen`, `LastObstacleSeen`, `LastBehaviour`) with `Store`/`Retrieve`/`Remove`/`Clear`/`Expire`; fixed-capacity backing array so memory can never grow without bound; main-thread only, matching framework assumptions
- **`BehaviourMemoryService`** — the single owner of all runtime memory updates: refreshes existing memory from the current assessment, re-bases stale memories (per `RefreshThreshold`), expires records past their configured duration, decays confidence, clears invalid records. Never generates commands and never selects behaviours

#### DecisionEngine & Selection Integration

- **`DecisionEngine`** now consumes `SituationSnapshot` + `BehaviourMemory`: `Decide()` refreshes memory from the assessment at a deterministic step clock, selects a behaviour through the memory-aware selector, records the chosen behaviour, then resolves the command via the executor factory. `DecisionEngine` remains the sole runtime decision authority; memory never replaces current perception
- **`IBehaviourSelector` / `BehaviourSelector`** — added a memory-aware `Select(assessment, memory)` overload (existing single-argument overload preserved for backward compatibility). Continuity rules: current sensor data always wins; only neutral current situations consult memory — a recently seen victim keeps the drone approaching until the memory expires, and a recent obstacle keeps it avoiding briefly (preventing left/right oscillation) before resuming search
- `DecisionEngine` constructor gains an optional `MemoryPolicy` parameter (backward compatible); `Reset()` also clears memory

#### Tests (ADRL.Tests.Editor.Decision)

- New `BehaviourMemoryTests` (11): store victim memory; retrieve victim memory; store obstacle memory; expiration removes stale memory; refresh extends lifetime; clear removes records; memory never exceeds configured capacity; same history produces identical decisions; expired memory no longer influences behaviour; current perception overrides remembered perception; victim continuity keeps approaching while memory is fresh

#### Validation

- EditMode suite: **109/109 passing** (98 prior + 11 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, movement 1.07m, reward finite and **sumInvariant=True**, `decisionSeen=True`, last behaviour Avoid (memory continuity now reaches a second victim reward deterministically)

### Phase 8.4 — Autonomous Behaviour Execution Framework (2026-08-03)

#### Behaviour Execution Layer (ADRL.AI.Decision.Execution)

- **New `ADRL.AI.Decision.Execution` namespace** (`Assets/ADRL/Scripts/AI/Decision/Execution/`, existing `ADRL.AI` assembly): a dedicated behaviour execution layer so movement generation is owned by behaviour-specific executors rather than `DecisionEngine` itself
- **`IBehaviourExecutor`** — single owner of movement generation for one behaviour; pure, stateless function of the assessed situation (same `SituationSnapshot` → identical `DroneCommand`); never decides what to do and never runs the drone
- **`IdleExecutor`** — always commands `DroneCommand.Idle`; no parameters
- **`SearchExecutor`** — steady forward exploration with a gentle, constant-rate yaw sweep; deterministic, no random numbers, no left/right oscillation, configurable forward speed + yaw rate
- **`ApproachExecutor`** — steers toward the assessed target side with clamped lateral steering and smoothed yaw (reduces jitter); configurable forward speed, steer gain, yaw gain
- **`AvoidExecutor`** — deterministic avoidance side (evades away from the assessed obstacle side), backoff + smooth turn that begins recovering heading; no left/right oscillation; configurable backoff speed, steer gain, yaw gain
- **`BehaviourExecutorFactory`** — the single behaviour-to-executor mapping; returns the matching executor for Idle/Search/Approach/Avoid (unknown falls back to Idle); no switch duplication elsewhere

#### DecisionEngine Refactor

- **`DecisionEngine` no longer contains behaviour-specific movement logic** — its `ResolveCommand` switch was removed; `Decide()` now performs assessment → behaviour selection → executor lookup → `DroneCommand` via `BehaviourExecutorFactory`, then returns the `DecisionResult`
- `DecisionEngine` remains the single runtime decision authority; `DecisionResult`/`DroneCommand`/`DroneAgent`/`DroneController` contracts unchanged; existing `DecisionContext` constructor preserved (new executor constants added via a second, backward-compatible constructor)

#### Tests (ADRL.Tests.Editor.Decision)

- New `DecisionBehaviourExecutionTests` (11): idle command; deterministic search exploration; approach steering toward target (both sides + clamping); avoid command generation (both sides + deterministic side / no oscillation); factory returns the correct executor for each behaviour; executors report matching behaviour; single mapping (no duplicate command generation, stable instances); identical snapshot → identical command across all executors; engine delegates to the behaviour executor for the resolved command

#### Validation

- EditMode suite: **98/98 passing** (87 prior + 11 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, movement 1.14m, reward finite and **sumInvariant=True**, `decisionSeen=True`, last behaviour Avoid

### Phase 8.3 — Decision Engine Runtime Integration (2026-08-02)

#### Runtime Authority Handover (DroneAgent)

- **DecisionEngine is now the sole runtime decision authority** — `DroneAgent.OnActionReceived` derives movement from the fused sensor reading through `DecisionEngine.Decide(fused) → DecisionResult.Command` instead of translating the ML-Agents action buffer. The action buffer is no longer the movement source; autonomous decisions drive the existing actuator pipeline
- **Single command authority preserved** — `DroneAgent.LastDecision` exposes the latest `DecisionResult`; `DroneEngine.Decision` exposes the engine; engine state resets per episode via `OnEpisodeBegin`
- **`DroneActionResolver` retained as the actuator translator** — the class and its `DroneCommand`/index contract are preserved (still used by `ConfigureBrain` and `Heuristic`) and reserved as the translation layer for the future reinforcement-learning policy; not duplicated, not removed
- **No bridges, mode switches, or config flags introduced** — phased handover keeps a single high-level decision producer and a single ownership boundary

#### Smoke Test (DroneSmokeTest)

- Repurposed from the scripted-action forward drive to validate the DecisionEngine path: places a deterministic probe collider ahead of the spawned drone so sensors yield real fused readings, then confirms the drone moved, observations were produced, both providers fused, the DecisionEngine ran and issued a non-idle command, and the reward evaluator is active
- Movement, reward accounting, determinism, and the reward sum invariant are preserved

#### Tests (ADRL.Tests.Editor.Decision)

- New `DecisionRuntimeIntegrationTests` (4): single non-idle command from a live fused reading; no duplicate/divergent command generation across steps; invalid reading → idle; engine determinism across identical runtime inputs

#### Validation

- EditMode suite: **87/87 passing** (83 prior + 4 new), exit 0, zero compiler warnings
- Runtime batch smoke test: PASSED, exit 0, movement 1.10m, reward finite and **sumInvariant=True**, `decisionSeen=True`, last behaviour Avoid

### Phase 8.2 — Autonomous Decision Framework (2026-08-02)

#### Decision Framework (ADRL.AI.Decision)

- **ADRL.AI.Decision** — New namespace (`Assets/ADRL/Scripts/AI/Decision/`, existing `ADRL.AI` assembly): a deterministic decision layer that owns *decision making only*. Movement, physics, rewards, simulation, mission, environment, and episode lifecycle stay with their existing owners
- **SituationSnapshot** — Immutable, allocation-free projection of one sensed situation (target detected/proximity/side, obstacle proximity/side, viability); read-only assessment, never drives sensing or rewards
- **ISituationAssessor / FogOfWarSituationAssessor** — Single owner of situation assessment: interprets a fused `ISensorReading` (band layout = `[proximity, liveness]` pairs) into a `SituationSnapshot`, gated by `DecisionContext.MinimumActiveRatio`
- **BehaviourState / IBehaviourSelector / BehaviourSelector** — Deterministic selection policy: invalid → `Idle`, imminent obstacle → `Avoid`, detected target → `Approach`, else `Search`; safety ranked above approach
- **DecisionContext** — Immutable config-driven tuning (detection/avoid thresholds, active ratio, approach/evade gains); fully deterministic for a fixed input
- **DecisionEngine** — Execution entry point: `Decide(ISensorReading) → DecisionResult(BehaviourState, DroneCommand, SituationSnapshot)`; resolves the command against the existing `DroneActionResolver`/`DroneController` actuator contract (`ADRL.AI.DecisionMaking.DroneCommand`); no movement/execution performed; `GetDiagnostics()` / `Reset()` state accessors
- **DecisionDiagnostics** — Read-only snapshot of running state (step count, last behaviour, last assessment); never influences decisions
- **DecisionResult** — Immutable step output binding behaviour + command + assessment

#### Integration Boundary

- The framework consumes the existing fusion layer (`ISensorReading` from `ADRL.Sensors`) and produces the existing command type (`DroneCommand`); no existing system (DroneAgent, RewardEvaluator, SimulationManager, MissionProgressTracker, SensorFusionProvider, DroneActionResolver, DroneController, RuntimeOrchestrator) was modified or replaced

#### Validation

- 11 new EditMode tests in DecisionFrameworkTests: invalid-reading assessment, target detection/side from band layout, selector precedence (avoid > approach > search > idle), engine resolution/step-tracking, idle command for invalid input, determinism for identical readings, reset semantics — **83/83 passing** (72 prior + 11 new), exit 0
- Runtime smoke test re-run: PASSED, exit 0, reward sum invariant true (no regression; existing runtime behaviour preserved)

### Phase 8.1.3 — Victim Prefab Integration & Runtime Registration (2026-08-02)

#### Victim Prefab (ADRL.Environment / ADRL.Editor)

- **Victim.prefab** — New prefab at `Assets/ADRL/Resources/Prefabs/Victim/Victim.prefab` (resource path `Prefabs/Victim/Victim`): capsule primitive on the Default layer with a **solid (non-trigger) collider** (required because `DroneVictimInteraction` and the thermal/ray sensors scan with `QueryTriggerInteraction.Ignore` over `DefaultRaycastLayers`) and the `Victim` lifecycle component (`Waiting` start state)
- **VictimPrefabWiring** — One-time editor step (`ADRL.Editor.Validation`, menu `ADRL/Phase 8.1.3/Wire Victim Prefab`) that creates the prefab deterministically and exits 0/1; idempotent (skips when the prefab already exists)

#### Runtime Registration (ADRL.Training)

- **RuntimeOrchestrator** — Registers `PrefabCategory.Victim` / `"Default"` → `Prefabs/Victim/Victim` in `ResourceLocator.Prefabs` **before** `EnvironmentBootstrap.Boot` (the `VictimGenerationRule` reads the registry in its constructor during boot), so procedural generation now spawns 1–10 victims per `EnvironmentConfig` (MinVictims/MaxVictims) instead of 0
- Victims instantiated by `VictimGenerationRule` flow through `EnvironmentManager.RegisterVictim` → `VictimRegisteredEvent` → `MissionProgressTracker` registered count, enabling mission completion for the first time at runtime

#### Validation

- 6 new EditMode tests in VictimPrefabIntegrityTests: prefab exists, loadable via Resources path, carries `Victim`, solid non-trigger collider, Default layer, starts `Waiting` — **72/72 passing** (66 prior + 6 new), exit 0
- Runtime smoke test re-run: PASSED, exit 0, environment log now reports `victims=N (N ≥ 1)` (previously `victims=0`)

### Phase 8.1.2 — Mission Success Reward Integration (2026-08-02)

#### Reward Evaluator (ADRL.AI.Rewards)

- **RewardEvaluator** — Now subscribes `MissionCompletedEvent` (in addition to the existing drone-id-filtered terminal events) and grants `RewardConfig.SuccessBonus` (+50) through the existing `GrantTerminal` path; the reward is granted at most once per episode — a duplicate `MissionCompletedEvent` never stacks the bonus (`_successEvents` guard, reset in `Reset()`)
- **RewardBreakdown** — Reuses the existing `SuccessReward` / `SuccessEvents` fields and counters; no new statistics introduced
- **RewardConfig** — `SuccessBonus` (+50) already existed; reused unchanged
- **Diagnostics** — Mission success already flows through the existing `SuccessReward` breakdown field into the smoke test `sumInvariant` checks; no duplicate logging added

#### Validation

- 4 new tests in EventRewardPipelineTests: success bonus granted once, duplicate publish does not stack, reset clears success state, total equals breakdown sum — **66/66 passing** (62 prior + 4 new), exit 0
- Runtime smoke test re-run: PASSED, exit 0, reward sum invariant true (no regression)

### Phase 8.1.1 — Runtime Event Integration: Collision & Victim Pipeline Foundation (2026-08-02)

#### Collision Pipeline (ADRL.Drone)

- **DroneCollisionDetector** — New `MonoBehaviour` (`ADRL.Drone.Components`, `[RequireComponent(DroneController)]`) on the drone prefab: kinematic Rigidbody + trigger CapsuleCollider surface; `OnTriggerEnter` publishes `CollisionEvent(droneId, tag-or-name, clamped impact)` on the `EventBus` and applies `DroneConfig.CollisionDamage` via `DroneHealth.TakeDamage`; self-child colliders ignored; destroyed-health guarded
- **Drone prefab wiring** — One-time `DronePrefabWiring.Run` (`ADRL.Editor.Validation`) makes the existing capsule a trigger, adds the kinematic rigidbody, and attaches `DroneCollisionDetector` + `DroneVictimInteraction` (exit 0 verified, component GUIDs confirmed in prefab)
- **DroneController** — Exposed public `EventBus` and `Config` accessors for prefab wiring (no behavioural change)

#### Victim Pipeline (ADRL.Sensors / ADRL.Environment)

- **IVictimDetectable** — Extended with `MarkDetected()` / `MarkRescued()` so the drone can signal detection/rescue without owning victim state
- **Victim** — Single lifecycle owner: `Initialize(EventBus)` injection; guarded transitions publish `VictimFoundEvent` (`Waiting→Detected`) and `VictimRescuedEvent` (`Detected→Rescued`), each exactly once
- **MissionProgressTracker** — New `MonoBehaviour` (`ADRL.Environment.Core`): counts `VictimRegisteredEvent` / `VictimRescuedEvent`, publishes `MissionCompletedEvent` exactly once when all registered victims are rescued; no episode/reward/simulation control; `EnvironmentResetEvent` keeps the registered total and resets progress
- **EnvironmentManager** — Creates/initializes the tracker under `[MissionProgressTracker]`, injects the bus via `victim.Initialize(_eventBus)` in `RegisterVictim`

#### Mission → Episode Finalization (ADRL.Core)

- **MissionCompletedEvent(RescuedCount, TotalVictimCount)** — New `IEvent` in `GameEvent.cs`
- **SimulationManager** — Sole episode owner: subscribes `MissionCompletedEvent`, finalizes a running unfinalized episode (`EpisodeCompletedEvent` + `Completed` + `SimulationStoppedEvent`), ignores events outside `Running`, unsubscribes in `OnDestroy`
- **DroneAgent** — Subscribes `EpisodeCompletedEvent` → `EndEpisode()`; `_episodeTerminating` guard prevents double ends from energy/out-of-bounds/max-step paths (reset in `OnEpisodeBegin`)
- **RewardEvaluator** — Unchanged; reacts to the existing terminal events. Mission-episode reward is 0 (success-bonus wiring is out of scope and logged as remaining Phase 8.1 work)

#### Drone–Victim Interaction (ADRL.AI)

- **DroneVictimInteraction** — New `MonoBehaviour` (`ADRL.AI.Interaction`): `Initialize(detectionRange, rescueRange)` with defaults from `SensorConfig.ThermalSensorRange` (30) × 0.25 rescue ratio; `FixedUpdate` scan uses `OverlapSphereNonAlloc` + `GetComponentInParent<IVictimDetectable>`; calls `MarkDetected`, then `MarkRescued` within rescue range; lives in `ADRL.AI` so `ADRL.Drone → Core` stays the only documented contract (no asmdef changes, no cycle)

#### Validation

- 5 new EditMode fixtures: CollisionPipelineTests (5), VictimInteractionTests (7), MissionProgressTrackerTests (5), SimulationMissionCompletedTests (3), plus `DroneTestHarness` — **62/62 passing** (42 prior + 20 new), exit 0
- Batch `DronePrefabWiring.Run` exit 0; prefab YAML verified (kinematic Rigidbody, trigger capsule, `DroneCollisionDetector`/`DroneVictimInteraction`/`DroneController` script GUIDs present)
- Runtime smoke test PASSED (moved 1.20 m, cumulative reward 0.148, sum invariant true), exit 0 — no regression from the collider-to-trigger change

### Phase 7.3 — Reward System (2026-08-02)

#### Reward Evaluator (ADRL.AI.Rewards)

- **RewardEvaluator** — Event-driven reward computation, one instance per drone, decoupled from ML-Agents through `IRewardSink`:
  - Continuous path in `UpdateStep(float deltaTime, Vector3 position, DroneCommand command)`: per-second time penalty (`TimePenalty × dt`), novelty bonus per newly visited cell, potential-based shaping `F = ShapingScale × (ShapingGamma × Φ(s′) − Φ(s))`, stuck detection (2 s window, < 0.1 m displacement), and oscillation detection (2 s window, ≥ 3 reversals)
  - Terminal path via `EventBus` subscriptions filtered by drone id: `DroneEnergyDepletedEvent` (−15), `DroneOutOfBoundsEvent` (−10), `VictimFoundEvent` (+10), `VictimRescuedEvent` (+20), `CollisionEvent` (−5)
  - Continuous rewards scaled by `RewardScale` and floored at `MinStepReward`; terminal rewards applied verbatim via `GrantTerminal`
  - Public API: `Reset(Vector3)`, `UpdateStep(...)`, `Dispose()`, `EpisodeReward`/`CumulativeReward`, `CurrentBreakdown`/`LastEpisodeBreakdown`; `Dispose()` detaches all subscriptions
- **RewardBreakdown** — New `readonly struct` with per-category reward totals and event counters (TimePenalty, Novelty, Potential, Stuck, Oscillation, Collision, Energy, OutOfBounds, VictimFound, VictimRescued, Success); invariant `TotalReward` = sum of all categories
- **IRewardSink** / **AgentRewardSink** — Reward delivery abstraction; `AgentRewardSink` forwards increments to an ML-Agents `Agent`

#### Reward Configuration (ADRL.Core.Configuration)

- **RewardConfig** defaults verified and documented: VictimFound +10, VictimRescued +20, Collision −5, Time −0.01/s, Success +50, OutOfBounds −10, EnergyDepleted −15, Novelty +0.05/cell (cell size 2 m), ShapingEnabled (γ 0.99, scale 0.1), RewardScale 1, MinStepReward −0.1, Stuck −0.5 (2 s window, 0.1 m), Oscillation −0.5 (2 s window, 3 reversals); `OnValidate` clamping for all penalties/bonuses/windows/thresholds

#### Validation

- 7 EditMode test fixtures restructured: RewardEvaluatorTests (14), EventRewardPipelineTests (8), RewardDiagnosticsTests (6), RewardConfigConsistencyTests (4), EpisodeLifecycleTests (5), PerformanceValidationTests (3), DeterminismTests (2) — **42/42 passing**
- Smoke test extended to assert `RewardBreakdown` sum invariant; batch run PASSED (moved 1.20 m, cumulative reward 0.148, evaluator active, sum invariant true)

#### Documentation

- Synchronized reward documentation with the verified implementation: `10_REWARD_SYSTEM.md` rewritten to event-driven values; stale reward values removed from `03_SYSTEM_DESIGN.md`, `12_DATA_FLOW.md`, `15_TESTING_GUIDE.md`, `17_SOFTWARE_DESIGN_SPECIFICATION.md`; `18_DEVELOPER_HANDBOOK.md` §8.4 updated to `RewardEvaluator`/`IRewardSink`/`EventBus` pattern; roadmap/CHANGELOG/release tables updated

### Phase 7.2 — RL Foundation: Sensors, AI Agent & Runtime Activation (2026-08-01)

#### ML-Agents Integration

- **ML-Agents 2.0.2** added via `Packages/manifest.json`; **Barracuda 3.0.0** resolved in `packages-lock.json`; `ADRL.AI` and `ADRL.Training` assemblies now reference the `Unity.ML-Agents` package

#### Sensor Layer (ADRL.Sensors)

- **DroneRaySensor** — deterministic raycast proximity + victim-flag readings; reusable buffer, zero per-frame allocation
- **DroneThermalSensor** — victim presence + proximity via `OverlapSphereNonAlloc` with a reused buffer
- **SensorFusionProvider** — deterministic concatenation of providers in registration order; owns the reading buffers
- **Sensor contracts** — `ISensorReading`, `ISensorDataProvider`, `IVictimDetectable`; `Victim` implements `IVictimDetectable`
- Sensors are pure, ML-free data sources — no ML-Agents references, no runtime state mutation

#### AI Layer (ADRL.AI)

- **DroneAgent** — ML-Agents agent with 4 continuous actions and a 28-float vector observation (24 ray + 2 thermal + energy + health); brain configured in `Awake` before the policy is built in `OnEnable`; scripted-heuristic override for deterministic smoke testing
- **DroneActionResolver** — maps 4 continuous actions to a normalized, ML-free `DroneCommand` (strafe/forward/yaw/altitude)
- **RewardEvaluator** — RewardConfig-driven step time penalty, per-meter exploration bonus, and terminal hooks; isolated from controller mechanics
- **Heuristic input fix** — the non-scripted heuristic emits neutral (zero) actions instead of the legacy `UnityEngine.Input` API, which is incompatible with the project's Input System-only configuration; manual play no longer throws

#### Runtime Activation (ADRL.Training)

- **RuntimeOrchestrator** — composition root that boots the environment and drone subsystems, registers generated settings and the drone prefab, spawns a drone deterministically, and kicks off the smoke test
- **DroneSmokeTest** — scripted forward-run pipeline validation; completes the moment PASS conditions are satisfied (movement, observations, sensor fusion, reward evaluator) rather than after a fixed-duration play session, so batch validation finishes in seconds
- **SmokeTestBatchRunner** (editor) — batch entry point `ADRL.Editor.Validation.SmokeTestBatchRunner.Run` with reduced activation/completion timeouts for a <30 s batch target and a two-phase exit for reliable CI exit codes

#### Drone Prefab & Configuration

- **Drone.prefab** under `Assets/ADRL/Resources/Prefabs/Drone/` — Transform plus DroneIdentity, DroneController, DroneLocomotion, CapsuleCollider, BehaviorParameters, DecisionRequester, and DroneAgent
- **Config assets** — Project, Runtime, Simulation, Drone, Environment, Sensor, Reward, Training under `ScriptableObjects/`; `ConfigAssetGenerator` editor utility
- **Scene wiring** — `Main.unity` Bootstrapper now references the 8 config assets

#### Delivery Notes

- 60 files, +2105/−14; asmdef reference additions only (`ADRL.AI`, `ADRL.Environment`, `ADRL.Training`); zero new asmdefs, zero namespace changes
- Automated validation: batch compile with zero errors; batch smoke test PASSED in under 20 seconds (exit code 0) with deterministic movement, 28-dim observations, 2 fused sensor providers, and an active reward evaluator

### Phase 6.4 — Runtime Completion & Diagnostics (2026-07-30)

- **Live diagnostics enhancement** — `IDroneDiagnostics` extended with `PendingSpawnCount`, `TotalPoolObjects`, `TotalBorrowCount`, `TotalReturnCount`, `TotalPoolMissCount`, `PoolStatisticsByType`, `HealthReport`; `DroneDiagnostics` constructor updated with optional parameters (backward compatible)
- **DroneHealthReport** — New `readonly struct` (`ADRL.Drone.Core`): runtime health summary with `IsOperational`, drone counts, pool counters, spawn queue depth, subsystem status; included in every `GetDiagnostics()` call
- **DronePoolManager aggregate stats** — New properties: `TotalActiveCount`, `TotalAvailableCount`, `TotalPoolObjectsCount`, `TotalBorrowCount`, `TotalReturnCount`, `TotalPoolMissCount`, `PoolCount`, `AllPoolTypes`; new `GetAllStatistics()` returns typed dictionary; enables diagnostics without per-pool iteration
- **Pool validation** — `DroneSubsystem.Validate()` now validates pool manager state: enumerates active pools, records pool components in validation report; zero new classes
- **Spawn queue depth** — `DroneSubsystem.GetDiagnostics()` now reads `DroneSpawnManager.PendingCount` for live queue depth reporting
- **DroneDebugDrawer** — New `MonoBehaviour` (`ADRL.Drone.Core`): optional Gizmos visualization for subsystem health (green/yellow/red wireframe sphere) and per-pool indicators (cyan/grey spheres); attached automatically in editor builds via `[Conditional("UNITY_EDITOR")]`; detached during shutdown; zero runtime cost in release builds
- **DroneFleetValidator static buffer fix** — Replaced static `_buffer` list with per-call allocation; eliminates thread-unsafe shared state; `buffer` parameter threaded through all private validation methods
- 2 new files, 5 modified — zero asmdef/namespace/dependency changes

### Phase 5.4 — Runtime Lifecycle & Fleet State Management (2026-07-27)

- **DroneRuntimeState** — New enum (`ADRL.Drone.Core`): `Uninitialized, Registered, Initializing, Idle, Active, Paused, Returning, Shutdown, Destroyed`; completely separate from `DroneState` (controller behaviour); no cross-references
- **DroneRuntimeInfo** — New class (`ADRL.Drone.Core`): per-drone immutable metadata store (`DroneId`, `RuntimeState`, `RegistrationTime`, `ActivationCount`, `EpisodeNumber`, `LastTransitionTime`, `RuntimeFlags`, `ControllerReference`); owned by `DroneManager._runtimeInfos` dictionary; no behaviour logic
- **DroneFleetStatistics** — New `readonly struct` (`ADRL.Drone.Core`): fleet-level computed metrics (`Registered`, `Active`, `Idle`, `Paused`, `Returning`, `Destroyed`, `Initializing`, `TotalRegistered`); computed on-the-fly by `DroneManager.GetFleetStatistics()`; immutable
- **DroneStateTransitionValidator** — New static class (`ADRL.Drone.Core`): validates runtime state transitions (`IsValid()` / `Validate()`); allowed forward lifecycle path (`Registered→Initializing→Idle→Active↔Paused→Returning→Shutdown→Destroyed`) plus emergency destroy from any operational state; rejects all other transitions with `InvalidOperationException`
- **DroneRuntimeEvents** — New event structs (`ADRL.Drone.Events`): `DroneRuntimeStateChangingEvent`, `DroneRuntimeStateChangedEvent`, `FleetStateChangedEvent`, `FleetResettingEvent`, `FleetResetCompletedEvent`; all `readonly struct` implementing `IEvent`; completely separate from `DroneEvents.cs` and `DroneSystemEvents.cs`
- **DroneFleetValidator** — New static class (`ADRL.Drone.Utilities`): validates duplicate IDs, registry consistency, invalid runtime states, context consistency, negative counters, statistics consistency, snapshot consistency; returns `FleetValidationResult`; no scene validation
- **DroneManager extended** — `TransitionRuntimeState(int, DroneRuntimeState)` validates via `DroneStateTransitionValidator`, updates runtime info, syncs context counters, publishes runtime events; `GetRuntimeInfo(int)` returns per-drone metadata; `GetFleetStatistics()` computes from runtime store; `ValidateFleet()` delegates to `DroneFleetValidator`; `ResetFleet()` now publishes `FleetResettingEvent` + `FleetStateChangedEvent` + `FleetResetCompletedEvent`; `Initialize()` publishes `FleetStateChangedEvent`; `Shutdown()` clears `_runtimeInfos`
- **DroneContext counters synced** — `ActiveDroneCount`, `InactiveCount`, `DestroyedCount` now maintained automatically by `UpdateStateCounter()` during runtime transitions; `RegisterDrone()` no longer increments `ActiveDroneCount` directly (delegated to lifecycle); `UnregisterDrone()` decrements the correct counter based on current runtime state
- **FleetSnapshot extended** — Now includes `IdleCount`, `PausedCount`, `ReturningCount`, `InitializingCount`, `ShutdownCount` — full runtime lifecycle breakdown
- 6 new files, zero asmdef changes, zero namespace changes, zero dependency graph changes
- Zero spawning, prefabs, GameObjects, Resources.Load, Environment, AI, sensors, navigation, physics, rewards, ML-Agents, Update(), coroutines, FindObjectOfType, GameObject.Find, singletons, static mutable state

### Phase 5.6 — Drone Infrastructure Layer & Subsystem Foundation (2026-07-27)

- **IDroneSubsystem** — New interface (`ADRL.Drone.Interfaces`): high-level subsystem operations (`Health`, `Boot`, `Shutdown`, `Reset`, `Validate`, `GetDiagnostics`); future systems depend on this interface, never on `DroneManager` directly
- **DroneSubsystem** — New class (`ADRL.Drone.Core`): public subsystem façade implementing `IDroneSubsystem`; wraps `DroneBootstrap` for Boot/Shutdown lifecycle; owns `DroneServiceProvider`, health state, and validation; `Boot(EventBus)` delegates to `DroneBootstrap`, creates service provider, initializes diagnostics and validator; `Shutdown()` delegates cleanly; `Validate()` runs startup validation; `GetDiagnostics()` returns immutable state snapshot; `Services` property exposes full `DroneServiceProvider`; no runtime logic — orchestration only
- **DroneServiceProvider** — New class (`ADRL.Drone.Core`): centralized service access exposing `DroneManager`, `DroneRegistry`, `DroneContext`, `DroneConfiguration`, `DroneDiagnostics`, `DroneStartupValidator`; owned by `DroneSubsystem`; no singleton, no static state
- **DroneSubsystemHealth** — New enum (`ADRL.Drone.Core`): `Healthy`, `Initializing`, `Degraded`, `Faulted`, `Shutdown` — subsystem health lifecycle
- **DroneDiagnostics** — New immutable class (`ADRL.Drone.Core`): `Health`, `CurrentRuntimeState`, `RegisteredDroneCount`, `DroneFleetStatistics`, `CurrentEpisode`, `InitializationDuration`, `SubsystemUptime`, `LastValidationTime`; implements `IDroneDiagnostics`; no Unity objects
- **DroneStartupValidator** — New class (`ADRL.Drone.Core`): validates `DroneManager`, `DroneContext`, `DroneRegistry`, `DroneConfiguration`, `DroneSubsystemHealth`, and dependencies; implements `IDroneValidator`; returns `DroneSubsystemValidationReport`; separate from runtime `DroneFleetValidator`
- **DroneSubsystemValidationReport** — New `readonly struct` (`ADRL.Drone.Core`): `Passed`, `Warnings`, `Errors`, `ExecutionTimeMs`, `ValidatedComponents` — full validation report, not a simple boolean
- **DroneLifecyclePolicy** — New static class (`ADRL.Drone.Core`): centralized lifecycle rules for `DroneSystemState` transitions (`IsAllowed`, `Validate`, `CanBoot`, `CanInitialize`, `CanRun`, `CanReset`, `CanShutdown`); 14 allowed transitions; replaces hardcoded lifecycle decisions
- **Service interfaces** — 4 new lightweight interfaces (`ADRL.Drone.Interfaces`): `IDroneRegistry`, `IDroneContext`, `IDroneDiagnostics`, `IDroneValidator`; existing implementations unchanged (DroneRegistry, DroneContext kept as-is); future systems depend on interfaces for decoupling
- **Architecture frozen** — `IDroneSubsystem` is the single public entry point for the drone subsystem; `DroneManager` is no longer a direct dependency target; all future phases build on this infrastructure without redesign
- 12 new files, 0 modified files, 0 asmdef changes, 0 namespace changes, 0 dependency graph changes
- Zero spawning, prefabs, GameObjects (new), Environment, AI, sensors, navigation, physics, rewards, ML-Agents, Update(), coroutines, singletons, static mutable state, assembly changes

### Phase 5.7 — Runtime Integration & System Wiring (2026-07-27)

- **Complete subsystem wiring** — `DroneSubsystem` now stores `_eventBus` reference; wires `DroneSubsystem` → `DroneServiceProvider` → `DroneManager` → `Context` → `Registry` → `Validators` → `Diagnostics` through method injection; no hidden initialization
- **Startup pipeline** — Deterministic boot sequence: health guard → `SubsystemStartingEvent` → `DroneBootstrap.Boot()` (try/catch) → acquire Manager/Context/Registry/Configuration → create Diagnostics/Validator/ServiceProvider → `DroneStartupValidator.Validate()` (post-boot) → `SubsystemFaultedEvent` + cleanup on failure → health = Healthy → `SubsystemReadyEvent`; failure at any stage stops cleanly
- **Shutdown pipeline** — Deterministic teardown: idempotent guard → `GetDiagnostics()` (capture) → `Validate()` (capture) → `SubsystemShutdownEvent` → `DroneBootstrap.Shutdown(eventBus)` → null all refs → health = Shutdown; idempotent via Shutdown guard
- **Full validation integration** — `Validate()` now combines: dependency validation (6 components), health/lifecycle validation, `DroneStartupValidator` (Manager/Context/Registry/Config/Health), `DroneFleetValidator` (runtime store/registry consistency/counters/snapshot) — all in one unified `DroneSubsystemValidationReport`; publishes `SubsystemValidatedEvent`
- **Full diagnostics integration** — `GetDiagnostics()` now aggregates: `SubsystemHealth`, `CurrentRuntimeState`, `SystemState` (DroneSystemState lifecycle), `IsServiceProviderReady` (service status), `LastValidationPassed` (validation state), `RegisteredDroneCount`, `FleetStatistics`, `CurrentEpisode`, `InitializationDuration`, `SubsystemUptime`, `LastValidationTime`
- **Health monitoring** — Health auto-updates: `Shutdown` → `Initializing` (boot begins) → `Healthy` (boot succeeds); `Faulted` on exception or validation failure; `Shutdown` on teardown; publishes `SubsystemFaultedEvent` with reason on fault
- **Lifecycle enforcement in DroneManager** — `Initialize()` now transitions `Uninitialized` → `Initializing` → `Ready` (both validated through `DroneLifecyclePolicy`); `Shutdown()` validates current → `Uninitialized`; `ResetFleet()` uses `CanReset()` guard + validates both `Resetting` → `Ready` transitions; original `FleetStateChangedEvent` updated from `Uninitialized→Ready` to `Initializing→Ready`
- **Dependency validation** — `ValidateDependencies()` checks all 6 runtime dependencies: `DroneManager`, `DroneContext`, `DroneRegistry`, `DroneConfiguration`, `DroneServiceProvider`, `EventBus`; null detection returned as validation errors
- **5 new infrastructure events** — `SubsystemStartingEvent` (before boot), `SubsystemReadyEvent` (after boot succeeds), `SubsystemValidatedEvent(bool Passed)` (after validation), `SubsystemFaultedEvent(string Reason)` (on fault), `SubsystemShutdownEvent` (before teardown); all in `ADRL.Drone.Events`; no spawning, scene, or AI events
- **DroneDiagnostics extended** — 3 new properties: `SystemState` (DroneSystemState), `IsServiceProviderReady` (bool), `LastValidationPassed` (bool); added to both `DroneDiagnostics` class and `IDroneDiagnostics` interface; constructor updated; all call sites updated
- **Runtime consistency** — `Validate()` verifies no duplicate services (single instances), no null references (6-component check), no invalid health states (Faulted/Shutdown detection), fleet counters (via `DroneFleetValidator`), diagnostics consistency, validation state consistency
- 5 files modified (`DroneSubsystem.cs` rewritten, `DroneManager.cs` lifecycle enforcement, `DroneSystemEvents.cs` +5 events, `IDroneDiagnostics.cs` +3 properties, `DroneDiagnostics.cs` +3 properties), 0 new files, 0 asmdef changes, 0 namespace changes, 0 dependency graph changes
- Zero spawning, prefabs, GameObjects, Environment, AI, sensors, navigation, physics, rewards, ML-Agents, Update(), coroutines, FindObjectOfType, GameObject.Find, singletons, static mutable state, assembly changes

### Phase 5.8 — Runtime Persistence & Recovery (2026-07-27)

- **DroneRuntimeSnapshot** — New immutable readonly struct (`ADRL.Drone.Core`): complete fleet runtime snapshot containing `SnapshotVersion` (versioned for forward compat), `Timestamp`, `EpisodeNumber`, `FleetRuntimeState`, `RegisteredDroneCount`, `TotalRegistered`, `NextAvailableId`, `Entries` (IReadOnlyList of drone entries), `FleetStatistics`; pure data only — no Unity objects, no GameObjects, no MonoBehaviours, no serialization dependencies
- **DroneSnapshotEntry** — New immutable readonly struct (`ADRL.Drone.Core`): per-drone entry containing `DroneId`, `RuntimeState`, `ActivationCount`, `RegistrationTime`, `LastTransitionTime`, `RuntimeFlags`; no `DroneController` reference — pure runtime data only
- **DronePersistenceManager** — New pure C# class (`ADRL.Drone.Core`): owns snapshot history (`List<DroneRuntimeSnapshot>`, max 100 checkpoints); `CaptureSnapshot(DroneManager)` creates + stores snapshot; `RestoreSnapshot(DroneRuntimeSnapshot, DroneManager)` delegates to manager; `ClearSnapshots()` clears history; `TryGetLatestSnapshot(out DroneRuntimeSnapshot)` retrieves most recent; `GetSnapshotHistory()` returns read-only list; no file IO, no serialization, in-memory only
- **DroneRecoveryPolicy** — New static class (`ADRL.Drone.Core`): `ValidateSnapshot(DroneRuntimeSnapshot)` validates version compatibility, entry integrity (no Uninitialized states, no duplicate IDs, no negative counts), counter consistency (RegisteredDroneCount matches entries, TotalRegistered >= Registered, NextAvailableId > 0 and >= Registered); `CanRecover(DroneRuntimeSnapshot)` shorthand; returns `RecoveryValidationResult` (IsRecoverable, Errors, Warnings)
- **DroneRecoveryValidator** — New class (`ADRL.Drone.Core`): validates restored runtime; integrates `DroneFleetValidator` (fleet consistency, counter matching, state alignment) with registry-consistency-warning suppression (expected after restore — no controllers exist until Phase 6.0+); integrates `DroneStartupValidator` (Manager/Context/Registry/Config structure); returns `RecoveryValidationResult`
- **DronePersistenceEvents** — New event structs (`ADRL.Drone.Events`): `RuntimeSnapshotCreatedEvent(int SnapshotIndex, int EntryCount)`, `RuntimeRestoreStartedEvent`, `RuntimeRestoreCompletedEvent(bool RolledBack)`, `RuntimeRestoreFailedEvent(string Reason)`, `RuntimeSnapshotClearedEvent`; all readonly struct implementing IEvent; use existing EventBus
- **DroneManager 4 new methods** — `CreateSnapshot()` reads `_runtimeInfos`, creates `DroneSnapshotEntry` per drone, returns full `DroneRuntimeSnapshot` with version + timestamp + fleet statistics; `RestoreSnapshot(DroneRuntimeSnapshot)` validates state (Ready/Running), clears runtime infos + registry, restores entries as `DroneRuntimeInfo` objects (no controller — null `DroneController` reference), restores counters and statistics, recalculates state counters, returns success; `ResetRuntime()` clears runtime infos and context counters (keeps system Ready); `ClearRuntime()` extends ResetRuntime with ID + EpisodeNumber reset
- **DroneSubsystem 6 new methods** — `CreateSnapshot()` guards health (Healthy/Degraded), delegates to `PersistenceManager.CaptureSnapshot`, publishes `RuntimeSnapshotCreatedEvent`; `RestoreSnapshot(DroneRuntimeSnapshot)` implements full recovery pipeline: snapshot validation (DroneRecoveryPolicy) → pre-snapshot capture (rollback target) → `RuntimeRestoreStartedEvent` → `ResetRuntime` → restore via persistence manager → `DroneRecoveryValidator.Validate()` → success capture + `RuntimeRestoreCompletedEvent(false)` or rollback to pre-snapshot + `RuntimeRestoreFailedEvent` + `RuntimeRestoreCompletedEvent(true)`; `ResetRuntime()` delegates to manager `ResetRuntime()` with health guard; `TryGetLatestSnapshot()` delegates to persistence manager (null-safe); `ClearSnapshots()` clears history + `RuntimeSnapshotClearedEvent`; `ValidateRestore()` runs recovery validator, returns `DroneSubsystemValidationReport`
- **Recovery pipeline** — Deterministic: validate snapshot → validate version → validate fleet integrity → clear current runtime → restore runtime infos → restore counters → restore statistics → validate restored state → publish events; if any validation fails: restore pre-snapshot → publish failure event → leave runtime unchanged; rollback tested via pre-snapshot/restore cycle
- 6 new files (`DroneSnapshotEntry.cs`, `DroneRuntimeSnapshot.cs`, `DronePersistenceManager.cs`, `DroneRecoveryPolicy.cs`, `DroneRecoveryValidator.cs`, `DronePersistenceEvents.cs`), 2 files modified (`DroneManager.cs` +4 methods, `DroneSubsystem.cs` +2 fields +6 methods + initialization + cleanup), 0 asmdef changes, 0 namespace changes, 0 dependency graph changes
- Zero spawning, prefabs, scene interaction, Environment, AI, sensors, navigation, physics, rewards, ML-Agents, Update(), coroutines, file IO, serialization, Resources, ScriptableObjects, asmdef modifications, namespace changes, dependency graph changes, architecture expansion



### Phase 5.5 — Runtime ↔ Controller Integration (2026-07-27)

- **ID ownership transferred** — `DroneController` no longer self-allocates IDs via `_nextId` static counter; removed `InterlockedIncrement` method; delegates to `DroneManager.RegisterDrone(this)` during initialization
- **DroneManager reference injected** — `DroneController.Initialize(EventBus, DroneConfig, IMotor, DroneManager)` now takes a `DroneManager` parameter; controller stores it for runtime state synchronization
- **Runtime state bridge** — Every controller DroneState transition now maps to a DroneRuntimeState transition via `DroneManager.TransitionRuntimeState()`:
  - `Initialize()` → `Registered` → `Initializing` → `Idle`
  - `Activate()` → `Active`
  - `Deactivate()` → `Idle`
  - `Pause()` → `Paused`
  - `Resume()` → `Active`
  - `DestroyDrone()` → `Destroyed`
  - `EmergencyStop()` / `Disable()`: No runtime equivalent (behaviour-only states)
- **Transition validator extended** — Added `(Active, Idle)` to `DroneStateTransitionValidator._allowedTransitions` to support controller `Deactivate()` lifecycle
- **Fleet counters driven by real transitions** — `ActiveDroneCount`, `InactiveCount`, `DestroyedCount` now synchronized through actual controller behaviour via `UpdateStateCounter()` during `TransitionRuntimeState()`
- **Runtime events fully wired** — `DroneRuntimeStateChangingEvent` + `DroneRuntimeStateChangedEvent` fire for every behaviour-driven runtime transition; `DroneStateChangedEvent` (controller scope) and `DroneRuntimeStateChangedEvent` (fleet scope) coexist without duplication
- **No duplicate event publication** — `DroneSpawnedEvent` (ADRL.Core) preserved for backward compatibility; `DroneRegisteredEvent` published by `RegisterDrone` remains the authoritative fleet registration event
- **Ownership preserved** — `DroneManager` owns IDs, runtime lifecycle, statistics, validation, runtime events; `DroneController` owns behaviour, components, motor, health, energy, state machine; no ownership inversion
- 2 files modified (`DroneController.cs`, `DroneStateTransitionValidator.cs`), zero asmdef changes, zero namespace changes, zero new files
- Zero spawning, prefabs, GameObjects, Environment, AI, sensors, navigation, physics, rewards, ML-Agents, Update(), coroutines, singletons, static mutable state, assembly changes

### Phase 5.3 — Fleet Runtime Management (2026-07-27)

- **Automatic ID management** — `DroneManager` now owns `_nextDroneId` (private, starts at 1); `AllocateDroneId()` returns and advances; IDs are deterministic, sequential, never duplicate; reset on fleet reset
- **Registration overload** — `RegisterDrone(DroneController)` auto-allocates ID via `AllocateDroneId()` then delegates to existing `RegisterDrone(int, DroneController)`; full backward compatibility preserved
- **Fleet statistics** — `DroneContext` extended with `TotalRegistered` (cumulative), `InactiveCount`, `DestroyedCount`, `NextAvailableId`; all synced during registration/unregistration/reset
- **Runtime fleet queries** — `GetDrone(int)`, `ContainsDrone(int)`, `GetRegisteredDroneCount()`, `GetActiveDroneCount()`, `GetInactiveDroneCount()` — all read-only wrappers over Registry/Context
- **FleetSnapshot** — New `readonly struct` in `ADRL.Drone.Core` capturing `RegisteredCount`, `TotalRegistered`, `ActiveCount`, `InactiveCount`, `DestroyedCount`, `NextAvailableId`, `SystemState`; no mutable internals exposed
- **Validation hardening** — `RegisterDrone` and `UnregisterDrone` now throw `ArgumentNullException` (null controller) and `InvalidOperationException` (duplicate ID, missing ID, invalid state) instead of returning -1/false; `AllocateDroneId()` also guards against invalid states
- **State tracking** — `ActiveDroneCount` is now maintained during `RegisterDrone` (increment) and `UnregisterDrone` (decrement, floor at 0)
- **`ActiveDroneCount` maintained** — Previously passive counter, now incremented on register, decremented on unregister
- Zero asmdef changes — ADRL.Drone still references only ADRL.Core
- Zero changes to DroneController, DroneMotor, DroneHealth, DroneEnergy, DroneState, DroneStateMachine, DroneEvents, DroneConfiguration, DroneSystemState, DroneSystemEvents, DroneBootstrap, DroneRegistry
- Zero spawning, prefab loading, hierarchy creation, environment integration, AI, navigation, sensors, ML-Agent, gameplay logic, or Update()

### Phase 5.2 — Drone Runtime Lifecycle & Registration (2026-07-27)

- **DroneManager lifecycle** — `Shutdown()` delegates cleanup from Bootstrap; `ResetFleet()` transitions through Resetting→Ready, clears registry and context counters; guards against invalid state transitions
- **Drone registration** — `RegisterDrone(int, DroneController)` validates null/duplicate, stores in registry, updates `RegisteredDroneCount`, publishes `DroneRegisteredEvent`; `UnregisterDrone(int)` validates existence, removes from registry, updates count, publishes `DroneUnregisteredEvent`; follows EnvironmentManager.RegisterVictim pattern
- **DroneRegistry** — Added `GetAll()` returning `IReadOnlyCollection<DroneController>` for enumeration
- **DroneContext** — Added `RegisteredDroneCount` and `ActiveDroneCount` properties; both reset in `Reset()`
- **DroneBootstrap** — `Shutdown()` now invokes `DroneManager.Shutdown()` before destroying GameObject (proper cleanup chain)
- Zero asmdef changes — ADRL.Drone still references only ADRL.Core
- Zero changes to DroneController, DroneMotor, DroneHealth, DroneEnergy, DroneState, DroneStateMachine, DroneEvents, DroneConfiguration, DroneSystemState, DroneSystemEvents
- Zero spawning, prefab loading, hierarchy creation, environment integration, AI, navigation, sensors, ML-Agent, or gameplay logic

### Phase 5.1 — Drone Framework Architecture Foundation (2026-07-27)

- **DroneBootstrap** — Static bootstrap class with `Boot(EventBus)` and `Shutdown(EventBus)`; creates persistent `[DroneSystem]` GameObject; composes DroneManager, DroneContext, DroneRegistry, DroneConfiguration; publishes Initializing/Initialized/Shutdown lifecycle events; idempotent via `_initialized` guard; no runtime logic, no GameBootstrap wiring, no Environment interaction
- **DroneManager** — MonoBehaviour owning only EventBus, DroneContext, DroneRegistry, DroneConfiguration, and DroneSystemState; method injection via `Initialize(EventBus, DroneContext, DroneRegistry, DroneConfiguration)`; publishes DroneInitializedEvent; cleanup in `OnDestroy()`; no spawning, no registration, no fleet creation, no hierarchy creation, no scene searching, no prefab instantiation
- **DroneContext** — Plain C# class holding only runtime references: FleetState, EpisodeNumber, DroneRoot, CurrentConfiguration; `Reset()` clears data; no derived/cached state (RegisteredDroneCount deferred to Phase 5.2)
- **DroneRegistry** — Instance-based (no singleton) pure storage with `Register(int, DroneController)`, `Unregister(int)`, `Contains(int)`, `Get(int)`, `Clear()`, `Count`; no auto-incrementing ID, no GetAll enumeration, no scene scanning
- **DroneConfiguration** — Plain C# class with system-level settings only: MaxFleetSize, DefaultSpawnCount, EnableEnergyManagement; no duplicated DroneConfig fields (SpawnHeight removed); no DroneConfig reference (Manager retrieves it independently)
- **DroneSystemState** — Enum in `ADRL.Drone.Core`: Uninitialized, Initializing, Ready, Running, Resetting, Completed, Failed (mirrors EnvironmentState)
- **DroneSystemEvents** — 5 new events in `ADRL.Drone.Events`: `DroneInitializingEvent`, `DroneInitializedEvent`, `DroneShutdownEvent`, `DroneRegisteredEvent(int)`, `DroneUnregisteredEvent(int)` — all readonly structs implementing IEvent; lifecycle only, no gameplay/movement/battery/ML events
- All code in ADRL.Drone assembly (`ADRL.Drone.Core`, `ADRL.Drone.Events` namespaces)
- Zero changes to ADRL.Core, ADRL.Environment, ADRL.AI, ADRL.Sensors, ADRL.Training, ADRL.UI, ADRL.Editor
- Zero asmdef changes — ADRL.Drone still references only ADRL.Core
- Zero changes to existing Drone code (DroneController, DroneMotor, DroneHealth, DroneEnergy, DroneState, DroneStateMachine, DroneEvents)
- Zero AI, zero physics, zero ML-Agent — fully decoupled architecture scaffolding

### Phase 2.3 — Core Framework & Configuration System (2026-07-22)

- **Bootstrap System** — `GameBootstrap` initialization pipeline + `Bootstrapper` MonoBehaviour entry point
- **Configuration Framework** — 8 ScriptableObject configs (Project, Runtime, Simulation, Drone, Environment, Sensor, Reward, Training)
- **Event Framework** — Typed `EventBus` with 15 event types for simulation lifecycle, drone state, and victim events
- **Service Layer** — `IService` interface + instance-based `ServiceLocator` registry
- **Simulation Framework** — `SimulationManager` state machine with episode lifecycle and pause/resume support
- **Utility Library** — `Logger` (structured, level-based), `MathExtensions` (remap, clamp, vector helpers), `UnityExtensions` (GetOrAddComponent, DestroyChildren, SetLayerRecursively)
- All configs use `[CreateAssetMenu]` for inspector creation
- All code follows namespace conventions documented in NAMESPACE_GUIDE
- Updated NAMESPACE_GUIDE.md Event Pattern section to reference EventBus
- Removed 6 `.gitkeep` files from directories that now contain implementation scripts

### Phase 3.0 — Runtime Asset & Resource Management Framework (2026-07-23)

- **ConfigRegistry** — Type-safe registry for all 8 ScriptableObject configs with runtime lookup (`Get<T>`, `TryGet<T>`)
- **PrefabRegistry** — Metadata registry for future prefabs organized by category (Drone, Victim, Environment, Obstacle, Hazard, Effects, UI)
- **RuntimeAssetCache** — Generic caching layer (`Dictionary<string, object>`) preventing duplicate `Resources.Load` calls

### Documentation

- Declared the ADRL-Rescue documentation suite frozen following repository-wide synchronization.
- Established the Documentation Freeze Policy in the Project Charter.
- Future documentation updates will follow single-authority governance.
- **AssetProvider** — Static utility wrapping `Resources.Load<T>` with cache integration (`Load`, `TryLoad`, `Exists`, `Unload`, `UnloadAll`)
- **AssetValidation** — `ValidationResult`-based validation for configs and prefabs during bootstrap (non-fatal, never crashes)
- **ResourceLocator** — Static accessor providing centralized access to ConfigRegistry, PrefabRegistry, and RuntimeAssetCache
- **Editor Validation Tools** — 3 menu items under `Tools/ADRL/`: Validate Project, Validate Configurations, Validate References
- **Editor Menu Items** — Quick-access documentation links (Namespace Guide, Developer Handbook)
- **Bootstrap Integration** — `GameBootstrap.Initialize` extended with 5 optional config params; `Bootstrapper` gets serialized fields for all 8 configs
- All runtime code in ADRL.Core assembly (`ADRL.Core.Resources` namespace)
- All editor code in ADRL.Editor assembly (`ADRL.Editor.Validation` namespace)
- Updated NAMESPACE_GUIDE.md with new `ADRL.Core.Resources` namespace entry
- No existing public APIs broken — fully backward compatible

### Phase 3.1 — Environment Foundation (2026-07-23)

- **EnvironmentBootstrap** — Static bootstrap class with `Boot(WorldSettings, EventBus)` and `Shutdown(EventBus)`; creates persistent `[EnvironmentSystem]` GameObject; publishes Initializing/Initialized lifecycle events; integrates with existing EnvironmentManager
- **EnvironmentContext** — Plain C# class holding shared runtime state: WorldSettings reference, ActiveSeed, RuntimeState, EpisodeElapsedTime, CurrentEpisode; `Reset()` method for episode transitions
- **WorldSettings** — `[CreateAssetMenu]` ScriptableObject for world-level configuration: WorldSize, PlayArea, SeedMode (Fixed/Random/TimeBased), FixedSeed, GravityOverride, DebugFlags (EnableDebugLogs, ShowGizmos, SkipEnvironmentValidation); zero terrain parameters (deferred to later phases)
- **SeedManager** — Static class with deterministic seed generation; three modes: Fixed (user-provided seed), Random (Environment.TickCount), TimeBased (UTC Unix seconds); exposes `GenerateSeed()`, `GenerateSeed(int)`, `GenerateSeed(WorldSettings)`, `SetSeed(int)`, `Reset()`
- **2 new events** in `ADRL.Environment.Events`: `EnvironmentInitializingEvent`, `EnvironmentShutdownEvent`
- All code in ADRL.Environment assembly (`ADRL.Environment.Core` namespace)
- Zero changes to existing Environment systems, ADRL.Core asmdef, or any other assembly
- Zero AI, zero physics, zero ML-Agent — fully decoupled foundation scaffolding

### Phase 3.1.1 — Foundation Hardening (2026-07-23)

- **Boot idempotency** — Verified existing `_initialized` guard prevents duplicate `[EnvironmentSystem]` GameObjects and double initialization; no changes required
- **Shutdown completeness** — Added `SeedManager.Reset()` to clear stale `_currentSeed` after `Shutdown()`; `EnvironmentContext.Reset()` now also clears `WorldSettings` reference for full runtime state cleanup
- **Debug seed logging** — Added conditional `Debug.Log` in `Boot()` (guarded by `WorldSettings.EnableDebugLogs`) showing episode, seed, and seed mode; zero logging in production
- Zero out-of-scope modifications — no terrain, AI, drone, sensor, training, or reward systems touched

### Phase 3.2 — Terrain Generation Framework (2026-07-23)

- **TerrainSettings** — `[CreateAssetMenu]` ScriptableObject in `ADRL.Environment.Terrain` namespace; terrain-level configuration: TerrainWidth, TerrainLength, HeightmapResolution, HeightScale, NoiseScale, SeedOffset, AutoGenerate; zero generation logic (pure configuration)
- **TerrainGenerator** — Concrete class in `ADRL.Environment.Terrain` namespace; `Initialize(TerrainSettings)`, `Generate(int seed, EventBus)` creates Unity Terrain with Perlin noise heightmap; fully deterministic via SeedManager; publishes terrain lifecycle events; `Reset()` destroys generated terrain
- **TerrainEvents** — 3 new `IEvent` structs in `ADRL.Environment.Events`: `TerrainGenerationStartedEvent`, `TerrainGeneratedEvent`, `TerrainGenerationFailedEvent`
- **EnvironmentContext** — Extended with `GeneratedTerrain` (TerrainData) and `TerrainSize` (Vector2) for runtime terrain reference
- **EnvironmentManager** — Integrated `InitializeTerrainGenerator()` step before procedural generation; terrain GameObject parented to `[EnvironmentSystem]` for lifecycle management; cleanup in `OnDestroy()`
- **EnvironmentBootstrap** — Populates `EnvironmentContext.GeneratedTerrain` and `.TerrainSize` after `EnvironmentManager.Initialize()` returns
- All code in ADRL.Environment assembly (`ADRL.Environment.Terrain`, `ADRL.Environment.Events` namespaces)
- Zero changes to ADRL.Core, ADRL.Drone, ADRL.AI, or any other assembly
- Zero disasters, victims, hazards, obstacles, AI, navigation, or RL rewards
- Removed `.gitkeep` from `Terrain/` directory

### Phase 3.2.1 — Terrain Framework Hardening (2026-07-23)

- **Lifecycle guards** — `Initialize()` is idempotent via `_isInitialized` flag; `Generate()` rejects duplicate calls with explicit `TerrainGenerationFailedEvent`; `Reset()` fully resets generator state including `_isInitialized`
- **Settings validation** — `ValidateSettings()` checks TerrainWidth, TerrainLength, HeightmapResolution, HeightScale, NoiseScale before generation
- **Exception safety** — `Generate()` wrapped in `try-catch`; any exception publishes `TerrainGenerationFailedEvent` and cleans up partial TerrainData
- **Memory cleanup** — `CleanupTerrainData()` destroys TerrainData asset in `Reset()` (prevents orphaned asset leak); TerrainData destroyed via `Object.Destroy` with `Application.isPlaying` check
- **Magic numbers** — Extracted `SeedOffsetMultiplier`, `SeedOffsetSeparator`, `TerrainCenterFactor` to named constants
- **Method decomposition** — Extracted `CreateTerrainData()`, `CreateTerrainGameObject()`, `ValidateSettings()`, `CleanupTerrainData()` from `Generate()` for readability and testability
- No API breaking changes, no new assemblies, no new namespaces, no new dependencies

### Phase 3.3 — Procedural Heightmap Generation (2026-07-23)

- **IHeightmapGenerator** — Interface in `ADRL.Environment.Terrain` with single `Generate(TerrainSettings, int seed, int resolution) → float[,]`; pure height computation, no Unity Terrain, no events, no lifecycle
- **HeightmapGenerator** — fBM (fractal Brownian motion) implementation using multi-octave Perlin noise; configurable Octaves, Persistence, Lacunarity; deterministic (same seed + settings → identical heightmap); falls back to single-octave Perlin when `UseFractalNoise = false`
- **TerrainSettings** — Extended with 5 new `[SerializeField]` fields: Octaves (4), Persistence (0.5), Lacunarity (2), HeightMultiplier (1), UseFractalNoise (true); all with inspector validation
- **TerrainGenerator** — Added `HeightmapGenerator` property (IHeightmapGenerator); delegates heightmap generation to interface, defaults to `HeightmapGenerator` if not set; removed inline Perlin noise code
- **EnvironmentManager** — Wires `HeightmapGenerator = new HeightmapGenerator()` into TerrainGenerator during initialization
- All new code in ADRL.Environment.Terrain namespace (IHeightmapGenerator, HeightmapGenerator)
- Zero new assemblies, zero namespace changes, zero event changes, zero breaking API changes
- Zero terrain textures, materials, biomes, vegetation, water, erosion, disasters, victims, obstacles, navigation, ML-Agent, drone, or sensor changes

### Phase 3.3.1 — Heightmap Framework Hardening (2026-07-23)

- **Remove dead code** — Deleted unused `NormalizedMin`/`NormalizedMax` constants from `HeightmapGenerator`
- **Null safety** — Added `ArgumentNullException` on null `settings` in `HeightmapGenerator.Generate()`
- **Settings validation** — Extended `TerrainGenerator.ValidateSettings()` with runtime checks for Octaves (< 1), Persistence (<= 0), Lacunarity (< 1), HeightMultiplier (<= 0)
- No API breaking changes, no new assemblies, no namespace changes, no event changes

### Phase 3.4 — Extensible Heightmap Framework (2026-07-23)

- **TerrainAlgorithm enum** — New enum in `ADRL.Environment.Terrain`: FBM, Ridged, DomainWarp, Voronoi, Hybrid (only FBM implemented; others reserved for future algorithms)
- **FBMHeightmapGenerator** — Renamed from `HeightmapGenerator`; produces identical fBM output; removed single-octave Perlin fallback (now handled by algorithm selection at config level); fully backward compatible for all default configs
- **HeightmapGeneratorFactory** — Static factory `Create(TerrainAlgorithm)` returning `IHeightmapGenerator`; returns `FBMHeightmapGenerator` for FBM; throws `NotSupportedException` for unimplemented algorithms; no reflection, no singleton, no service locator
- **TerrainSettings** — Replaced `_useFractalNoise` (bool) with `_terrainAlgorithm` (TerrainAlgorithm enum, default FBM); `UseFractalNoise` preserved as computed property (`true` for FBM) for backward compatibility
- **TerrainGenerator** — Default fallback changed from `new HeightmapGenerator()` to `HeightmapGeneratorFactory.Create(_settings.TerrainAlgorithm)`; injection property preserved for custom generator scenarios
- **EnvironmentManager** — Removed `HeightmapGenerator = new HeightmapGenerator()` assignment; factory now resolves algorithm from settings
- Zero new assemblies, zero namespace changes, zero event changes, zero public API breaks
- Zero terrain textures, materials, biomes, vegetation, water, erosion, disasters, victims, obstacles, navigation, ML-Agent, drone, or sensor changes

### Phase 3.5 — Ridged Multi-Fractal Heightmap Algorithm (2026-07-23)

- **RidgedHeightmapGenerator** — Second IHeightmapGenerator implementation using ridged multi-fractal noise: center Perlin noise around zero, apply `1 - abs` ridge transform, square for sharp peaks, accumulate across octaves with standard fBM frequency/amplitude progression
- **Factory activation** — HeightmapGeneratorFactory.Create(TerrainAlgorithm.Ridged) now returns RidgedHeightmapGenerator instead of throwing NotSupportedException
- **Zero new settings** — Reuses existing Octaves, Persistence, Lacunarity, NoiseScale, HeightMultiplier, SeedOffset from TerrainSettings
- **Deterministic** — No random, no Time, no Unity Random, no allocations inside inner loops; identical seed → identical heightmap
- **Strategy architecture validated** — Adding a new algorithm required only 1 new file + 1 factory line change. Zero changes to IHeightmapGenerator, TerrainGenerator, TerrainSettings, EnvironmentManager, or asmdef
- Zero terrain textures, materials, biomes, vegetation, water, erosion, disasters, victims, obstacles, navigation, ML-Agent, drone, or sensor changes

### Phase 3.6 — Domain Warp Terrain Generator (2026-07-23)

- **DomainWarpHeightmapGenerator** — Third IHeightmapGenerator implementation: samples Perlin noise at WarpScale to compute distortion offsets, applies offsets scaled by WarpStrength to base noise coordinates, then runs standard fBM at warped coordinates
- **Factory activation** — HeightmapGeneratorFactory.Create(TerrainAlgorithm.DomainWarp) now returns DomainWarpHeightmapGenerator instead of throwing NotSupportedException
- **New settings** — Added `WarpStrength` (float, default 4) and `WarpScale` (float, default 0.02) to TerrainSettings; both required for domain warp control; no existing settings could substitute
- **Deterministic** — No random, no Time, no Unity Random, no allocations inside inner loops; identical seed → identical heightmap
- **Strategy architecture validated again** — Adding a third algorithm required only 1 new file, 1 factory line, and 2 settings fields. Zero changes to IHeightmapGenerator, TerrainGenerator, EnvironmentManager, or asmdef
- Zero terrain textures, materials, biomes, vegetation, water, erosion, disasters, victims, obstacles, navigation, ML-Agent, drone, or sensor changes

### Phase 3.7 — Voronoi Terrain Generator (2026-07-27)

- **VoronoiHeightmapGenerator** — Fourth IHeightmapGenerator implementation: deterministic Voronoi (Worley) noise using per-cell seed-hashed feature points, 3×3 neighborhood search, Euclidean distance normalized by sqrt(2), height inverted so valleys form at feature points for cellular/cracked-earth terrain
- **Factory activation** — HeightmapGeneratorFactory.Create(TerrainAlgorithm.Voronoi) now returns VoronoiHeightmapGenerator instead of throwing NotSupportedException
- **New setting** — Added `VoronoiCellSize` (float, `[Min(0.1f)]`, default 8) to TerrainSettings; required because Voronoi needs a cell grid scale that cannot be represented by any existing Perlin/fBM setting
- **Deterministic** — Feature points derived from seed-hashed integer cell coordinates (no Unity Random, no System.Random, no Time, no Perlin noise); identical seed → identical heightmap
- **No allocations inside loops** — All hash and distance computations use local variables only
- **Strategy architecture validated again** — Adding a fourth algorithm required only 1 new file, 1 factory line, and 1 settings field. Zero changes to IHeightmapGenerator, TerrainGenerator, EnvironmentManager, or asmdef
- Zero terrain textures, materials, biomes, vegetation, water, erosion, disasters, victims, obstacles, navigation, ML-Agent, drone, or sensor changes

### Phase 3.8 — Hybrid Terrain Generator (2026-07-27)

- **HybridHeightmapGenerator** — Fifth and final IHeightmapGenerator implementation; pure **composition** of existing generators (FBM + Ridged + DomainWarp + Voronoi) via internal `static readonly` instances; no new procedural algorithm
- **Blending strategy** — Each generator produces its full heightmap independently; per-pixel blend: `(fbm×wF + ridged×wR + warp×wW + voronoi×wV) / totalWeight`; weights normalized automatically (no user requirement to sum to 1)
- **Factory activation** — HeightmapGeneratorFactory.Create(TerrainAlgorithm.Hybrid) now returns HybridHeightmapGenerator instead of throwing NotSupportedException
- **New settings** — Added `FbmWeight` (0.40), `RidgedWeight` (0.25), `DomainWarpWeight` (0.20), `VoronoiWeight` (0.15) to TerrainSettings; all `[Min(0f)]`; runtime validation rejects negative weights and zero total
- **Open/Closed Principle validated conclusively** — All 5 algorithms added without modifying IHeightmapGenerator, TerrainGenerator, EnvironmentManager, or any assembly definition. Each new algorithm: 1 file + 1 factory line + optional settings fields
- **Zero "not yet implemented" exceptions remain** — The factory handles all 5 TerrainAlgorithm values with concrete implementations
- Zero terrain textures, materials, biomes, vegetation, water, erosion, disasters, victims, obstacles, navigation, ML-Agent, drone, or sensor changes

### Phase 4.0 — Environment Runtime World Builder (2026-07-27)

- **EnvironmentWorldBuilder** — Plain C# class in `ADRL.Environment.Core` responsible for constructing the runtime world hierarchy; creates 4 named child containers (`Runtime`, `Systems`, `SpawnPoints`, `Debug`) under the `[EnvironmentSystem]` root; hierarchy names centralized as `public const string` constants
- **Exception-safe construction** — `Build(Transform rootTransform)` uses try-catch; any failure triggers `Destroy()` to tear down partially created hierarchy and re-throws; EnvironmentManager wraps the call with event publication
- **Deterministic hierarchy** — Guaranteed same structure every build; all containers created via `new GameObject(name)` + `SetParent(rootTransform, worldPositionStays: false)` for clean local-space transforms
- **Editor-safe cleanup** — `Destroy()` and internal `DestroyChild()` use `Application.isPlaying` guard to select `Object.Destroy` vs `Object.DestroyImmediate`
- **EnvironmentContext extended** — 5 new Transform references: `RootTransform`, `RuntimeRoot`, `SystemsRoot`, `SpawnRoot`, `DebugRoot`; all cleared in `Reset()`
- **3 new events** — `WorldBuildingStartedEvent`, `WorldBuiltEvent`, `WorldBuildingFailedEvent(string Reason)` follow existing `TerrainEvents` pattern (Started/Generated/Failed)
- **Pipeline integration** — `EnvironmentManager.Initialize()` now calls `BuildWorld()` after `InitializeTerrainGenerator()` and before `InitializeProceduralGenerator()`; `EnvironmentBootstrap.Boot()` populates context hierarchy references after `EnvironmentManager.Initialize()` returns
- **Lifecycle cleanup** — `EnvironmentManager.OnDestroy()` calls `CleanupWorldBuilder()` which invokes `WorldBuilder.Destroy()` and nulls the reference
- **Zero out-of-scope** — No victims, obstacles, hazards, navigation, AI, ML-Agents, drone spawning, sensors, biomes, vegetation, texturing, disasters, gameplay, or physics systems touched
- **Zero asmdef changes** — All code in existing `ADRL.Environment.asmdef`; reuses existing `ADRL.Environment.Core` namespace
- **Zero existing file API breaks** — All additions are backward compatible (new fields, new events, new method calls inserted into existing pipeline)

### Phase 4.0 — Modular Drone Framework (2026-07-23)

- **DroneController** — MonoBehaviour orchestrator composing IMotor, DroneHealth, DroneEnergy, DroneStateMachine via method injection
- **DroneState** — Typed enum: Uninitialized, Initializing, Idle, Active, Paused, Emergency, Disabled, Destroyed
- **DroneStateMachine** — Dictionary-based valid transition graph with `TryTransitionTo` and `CanTransitionTo`; publishes `DroneStateChangedEvent` on transitions
- **DroneMotor** — IMotor implementation with `Move`, `Rotate`, `Stop`, `EmergencyStop`; config-driven (MaxSpeed, RotationSpeed)
- **DroneHealth** — Damage/repair system; publishes `DroneDamagedEvent`, `DroneDestroyedEvent`; reads MaxHealth, CollisionDamage from `DroneConfig`
- **DroneEnergy** — Battery system with `Consume`, `Recharge`, threshold detection; publishes `BatteryLowEvent`, `BatteryCriticalEvent`; reads MaxEnergy, EnergyDrainRate from `DroneConfig`
- **IMotor** — Motor interface for decoupled movement (future physics implementations)
- **INavigationSystem**, **IPathProvider**, **INavigationTarget** — Navigation interfaces for future pathfinding/obstacle avoidance
- **5 new events** in `ADRL.Drone.Events`: `DroneActivatedEvent`, `DroneStateChangedEvent`, `BatteryLowEvent`, `BatteryCriticalEvent`, `EmergencyStopEvent`
- **Energy drain** tracked in DroneController.Update() — triggers EmergencyStop when depleted
- **Thread-safe drone ID generation** via `Interlocked.Increment`
- All code in ADRL.Drone assembly (`ADRL.Drone.Controllers`, `ADRL.Drone.Components`, `ADRL.Drone.Interfaces`, `ADRL.Drone.Events` namespaces)
- Zero changes to ADRL.Core, GameBootstrap, Bootstrapper, EventBus, or ConfigRegistry
- Zero AI, zero physics, zero ML-Agent — fully decoupled for future milestones
- Removed 3 `.gitkeep` files from directories that now contain implementation scripts

### Phase 5.0 — Environment Framework (2026-07-23)

- **EnvironmentManager** — MonoBehaviour orchestrator with EnvironmentState lifecycle (Uninitialized → Initializing → Ready → Running → Resetting → Completed/Failed); method injection of EventBus; reads EnvironmentConfig from ResourceLocator
- **EnvironmentState** — Typed enum: Uninitialized, Initializing, Ready, Running, Resetting, Completed, Failed
- **IEnvironmentObject** — Interface contract for environment entities supporting Initialize, Reset, Cleanup, Id
- **ISpawnable** — Interface for spawnable entities with OnSpawned callback
- **SpawnPoint** — MonoBehaviour with category, position, rotation for placement
- **SpawnManager** — Category-based spawn point registry with random selection and query
- **Victim** — MonoBehaviour implementing IEnvironmentObject with VictimState (Unknown/Waiting/Detected/Rescued/Lost), ID, and discovery lifecycle
- **VictimState** — Typed enum: Unknown, Waiting, Detected, Rescued, Lost
- **Hazard** — MonoBehaviour implementing IEnvironmentObject with HazardType, active state, ID
- **HazardType** — Typed enum: Fire, Smoke, Chemical, Structural, Electrical, Debris
- **HazardManager** — Plain C# registry tracking hazards with reset, clear, active count
- **4 new events** in `ADRL.Environment.Events`: `EnvironmentInitializedEvent`, `EnvironmentResetEvent`, `VictimRegisteredEvent`, `HazardRegisteredEvent`
- **Auto-registration** — `EnvironmentManager.Initialize` discovers existing victims, hazards, and spawn points in the scene
- **Zero new configs** — reuses existing `EnvironmentConfig` from ADRL.Core.Configuration
- **Zero new asmdefs** — reuses existing `ADRL.Environment.asmdef` (references ADRL.Core)
- All code in ADRL.Environment assembly (`ADRL.Environment.Core`, `ADRL.Environment.Interfaces`, `ADRL.Environment.Spawning`, `ADRL.Environment.Victims`, `ADRL.Environment.Hazards`, `ADRL.Environment.Events` namespaces)
- Zero changes to ADRL.Core, ADRL.Drone, GameBootstrap, Bootstrapper, EventBus, or ConfigRegistry
- Zero AI, zero physics, zero ML-Agent, zero procedural generation — fully decoupled
- Removed 3 `.gitkeep` files from directories that now contain implementation scripts

### Phase 5.1 — Environment Expansion & World Object Framework (2026-07-23)

- **IWorldObject** — Interface contract for all environment objects: Id, Category, IsActive, Initialize, Reset, Cleanup
- **WorldObjectCategory** — Extensible enum: Unknown, Victim, Hazard, Obstacle, Resource, Landmark
- **WorldObjectBase** — MonoBehaviour base component implementing IWorldObject with serialized ID/category, IsActive state, and virtual lifecycle hooks
- **WorldObjectRegistry** — Instance-based (no singleton) registry with dual lookup (by ID and by category); supports Register, Unregister, GetById, GetByCategory, ResetAll, Clear
- **3 new events** in `ADRL.Environment.Events`: `WorldObjectRegisteredEvent(id, category)`, `WorldObjectRemovedEvent(id, category)`, `WorldObjectResetEvent(id)`
- **WorldObjectValidator** — Static utility for runtime/editor validation: ValidateId, ValidateCategory, CheckDuplicateRegistration, CheckMissingReferences
- **EnvironmentManager integration** — Owns WorldObjectRegistry lifecycle: initializes in Initialize(), resets in ResetEnvironment(), cleans up in OnDestroy()
- Zero changes to existing Environment systems (Victim, Hazard, Spawnpoint, or their managers)
- Zero changes to ADRL.Core, ADRL.Drone, ADRL.AI, ADRL.Sensors, ADRL.Training
- Zero AI, zero physics, zero ML-Agent, zero procedural generation
- 6 new files across 2 new directories (`WorldObjects/`, `Validation/`)

### Phase 5.2 — Obstacle & Environmental Object Framework (2026-07-23)

- **ObstacleType** — Extensible enum: Unknown, Building, CollapsedStructure, Vehicle, Tree, Wall, Debris
- **ObstacleState** — Lifecycle enum: Inactive, Active, Destroyed
- **Obstacle** — MonoBehaviour extending WorldObjectBase with ObstacleType + ObstacleState; auto-sets WorldObjectCategory.Obstacle on init; extensible for physics/navigation
- **ObstacleManager** — Instance-based manager using existing WorldObjectRegistry; typed registration, unregistration, and query methods (by type, by active, by destroyed)
- **4 new events** in `ADRL.Environment.Events`: `ObstacleRegisteredEvent(id)`, `ObstacleRemovedEvent(id)`, `ObstacleActivatedEvent(id)`, `ObstacleDeactivatedEvent(id)`
- **WorldObjectValidator extension** — Added `ValidateObstacleType()` method
- **EnvironmentManager integration** — Owns ObstacleManager lifecycle; auto-discovers obstacles in scene; publishes ObstacleRegisteredEvent on registration
- Zero changes to existing Environment systems (Victim, Hazard, Spawnpoint, WorldObject, or their managers)
- Zero changes to ADRL.Core, ADRL.Drone, ADRL.AI, ADRL.Sensors, ADRL.Training
- Zero AI, zero physics, zero ML-Agent, zero procedural generation
- 5 new files across existing directories (`Obstacles/`, `Events/`)

### Phase 5.3 — Procedural Environment Foundation (2026-07-23)

- **IGenerationContext** — Interface providing GenerationRule with RNG, bounds, registry, spacing, and max placement attempts
- **GenerationSettings** — ScriptableObject for procedural generation configuration; uses EnvironmentConfig for shared settings (terrain size); adds fixed/runtime seed, bounds override, min spacing, and max placement attempts
- **GenerationRule** — Abstract base class for reusable generation instructions; configurable name, enabled, weight, priority, max count; single abstract `Generate(IGenerationContext)` method
- **ProceduralGenerator** — Plain C# class managing generation lifecycle; System.Random with seed management (fixed seed support); priority-sorted rule execution; Initialize/Generate/Reset/Clear lifecycle matching existing manager patterns; no singleton
- **EnvironmentManager integration** — ProceduralGenerator owned by EnvironmentManager; initialized after RegisterExistingObjects() when EnvironmentConfig.EnableProceduralGeneration is true; generator state reset during ResetEnvironment(); cleaned up during OnDestroy()
- **Seed management** — Fixed seed via GenerationSettings._useFixedSeed; runtime seed via Environment.TickCount; deterministic reproduction via System.Random with explicit seed; Reset() restores initial seed; no global Random state pollution
- Zero changes to existing Environment systems (Victim, Hazard, Obstacle, Spawnpoint, WorldObject)
- **ProceduralValidator** — Static utility for GenerationSettings validation: ValidateMinSpacing, ValidateMaxAttempts, ValidateBounds, ValidateEnvironmentConfig, ValidateAll; follows WorldObjectValidator pattern (bool + out string message)
- Zero changes to ADRL.Core, ADRL.Drone, ADRL.AI, ADRL.Sensors, ADRL.Training
- Zero AI, zero physics, zero ML-Agent — fully decoupled foundation
- 5 new files across existing directory (`Procedural/`)
- Removed `.gitkeep` from Procedural directory

### Phase 5.4 — Concrete Procedural Generation Rules (2026-07-23)

- **IGenerationRule** — Interface in `ADRL.Environment.Procedural.Rules` providing Name, Enabled, Priority, MaxCount, and Generate(IGenerationContext); enables non-abstract rule implementations and test mocking
- **GenerationRule implements IGenerationRule** — Existing abstract base class now implements the interface; fully backward compatible, zero behavior change
- **IGenerationContext extended** — Added `PlacedPositions` (`List<Vector3>`) for cross-category minimum spacing enforcement across rules; added `TotalGenerated` setter for per-rule tracking
- **PlacementUtility** — Static utility in `ADRL.Environment.Procedural` for shared placement logic: `GetRandomPositionInBounds`, `HasMinimumSpacing` (checks registry + placed positions), `TryFindValidPosition` (retry loop); eliminates duplicate algorithms across rules
- **VictimGenerationRule** — Concrete rule (Priority 3) generating victims within bounds using EnvironmentConfig MinVictims/MaxVictims; prefab loaded via ResourceLocator.Prefabs + AssetProvider; registers through EnvironmentManager.RegisterVictim
- **HazardGenerationRule** — Concrete rule (Priority 2) generating hazards using EnvironmentConfig.HazardDensity; assigns random HazardType; registers through HazardManager
- **ObstacleGenerationRule** — Concrete rule (Priority 1) generating obstacles using EnvironmentConfig MinObstacles/MaxObstacles; assigns random ObstacleType (skips Unknown); registers through ObstacleManager.RegisterObstacle (which uses WorldObjectRegistry)
- **Deterministic generation order** — Victims → Hazards → Obstacles; all rules use System.Random from IGenerationContext for seed-reproducible output
- **EnvironmentManager integration** — InitializeProceduralGenerator creates and adds all three rules with explicit constructor dependencies (no singletons)
- **ResourceLocator pattern** — All rules load prefabs via `ResourceLocator.Prefabs.TryGetPath` + `AssetProvider.Load<GameObject>`; gracefully return 0 when prefabs not registered
- Zero changes to ADRL.Core, ADRL.Drone, ADRL.AI, ADRL.Sensors, ADRL.Training
- Zero AI, zero physics, zero ML-Agent — fully decoupled generation rules
- 5 new files across 1 new directory (`Procedural/Rules/`)
- 4 existing files modified (GenerationRule, IGenerationContext, ProceduralGenerator, EnvironmentManager)

### Phase 5.5 — Scenario & Mission Profile Framework (2026-07-23)

- **ScenarioType** — Extensible enum: Earthquake, UrbanFire, Flood, IndustrialAccident, ForestFire, Custom
- **ScenarioDifficulty** — Extensible enum: Easy, Medium, Hard, Expert, Custom
- **ScenarioState** — Lifecycle enum: Unloaded, Loaded, Starting, Running, Resetting, Completed, Failed, Unloading
- **IScenario** — Interface contract: ScenarioId, ScenarioName, ScenarioType, Difficulty, Seed, Initialize(), Reset(), Validate()
- **ScenarioProfile** — ScriptableObject with Metadata (name, description, author, version), Configuration references (EnvironmentConfig, GenerationSettings), Generation overrides (Seed, OverrideSeed, EnableProceduralGeneration), Count/Density overrides (VictimCount, HazardDensity, ObstacleCount), Mission settings (Duration, Success/Failure Thresholds), and Reserved fields (Weather, Lighting, TimeOfDay, Wind, DifficultyScaling); computed override methods (GetVictimCount, GetHazardDensity, GetObstacleCount, GetEffectiveSeed) return override value or fallback to EnvironmentConfig/GenerationSettings
- **ScenarioManager** — MonoBehaviour orchestrator with method-injected EnvironmentManager and EventBus dependencies; lifecycle (Initialize → LoadScenario → StartScenario → Running → Complete/Fail → Reset → Unload); auto-fails on mission duration expiry via Update(); no singleton, no direct object generation — delegates entirely to EnvironmentManager; registers ScenarioProfile configs (EnvironmentConfig, GenerationSettings) in ConfigRegistry during LoadScenario for deterministic config override
- **ScenarioValidator** — Static validation utility (WorldObjectValidator/ProceduralValidator pattern) with ValidateProfile, ValidateSeed, ValidateGenerationSettings, ValidateEnvironmentConfig, ValidateMissionSettings, ValidateAll — all returning `bool + out string message`; validates null references, seed negativity, spacing/attempt bounds, terrain size consistency, min/max ordering, mission threshold ranges; avoids exceptions for configuration errors
- **ScenarioEvents** — 5 new event types in ADRL.Environment.Events implementing IEvent: `ScenarioLoadedEvent(scenarioId, scenarioName, scenarioType)`, `ScenarioStartedEvent(scenarioId)`, `ScenarioResetEvent(scenarioId)`, `ScenarioCompletedEvent(scenarioId, completionTime)`, `ScenarioFailedEvent(scenarioId, reason)`
- **Config override via registration** — ScenarioManager registers ScenarioProfile's EnvironmentConfig and GenerationSettings in ConfigRegistry during LoadScenario; EnvironmentManager reads these overridden configs during its own Initialize(), creating a deterministic config pipeline: ScenarioProfile → ConfigRegistry → EnvironmentManager → ProceduralGenerator
- Zero changes to existing Environment systems (EnvironmentManager, ProceduralGenerator, rules, events, validators)
- Zero changes to ADRL.Core, ADRL.Drone, ADRL.AI, ADRL.Sensors, ADRL.Training
- Zero AI, zero physics, zero ML-Agent — fully decoupled scenario framework
- 8 new files across 2 new directories (`Scenarios/`, `Events/`)
- 0 existing files modified

### Phase 5.5.1 — Project Stabilization & Warning Resolution (2026-07-23)

- **Duplicate MenuItem fix** — Removed `[MenuItem]` attributes from `ProjectValidator.cs`, `ConfigurationValidator.cs`, and `ReferenceValidator.cs` that duplicated the centralized registrations in `AdrlMenuItems.cs`; consolidated to single entry points: `Tools/ADRL/Validate Project`, `Tools/ADRL/Validate Configurations`, `Tools/ADRL/Validate References`
- **PrefabRegistry diagnostic improvement** — Changed warning from "PrefabRegistry is empty. No prefabs registered." to "PrefabRegistry is empty (expected before Phase 6). Register prefabs after Phase 5 completes." — downgrades developer confusion while preserving validation
- **ScenarioProfile compilation fix** — Added missing `ScenarioType` and `ScenarioDifficulty` serialized fields with public accessors, plus public accessors for reserved future fields; these were required by `ScenarioManager.LoadScenario` (publishes `ScenarioLoadedEvent` with `ScenarioType`) and the `IScenario` interface contract, making the committed Phase 5.5 code properly compilable
- 4 existing files modified (3 editor validators, 1 core validator)
- 1 pre-existing file patched (ScenarioProfile.cs)
- Zero new files, zero namespace/assembly changes

### Phase 5.6 — Project Stabilization & ML Readiness (2026-07-23)

#### Editor Validation
- **ConfigurationValidator bug fix** — Replaced mutable static dictionary with local state per validation call; eliminated stale-state bug where re-validation reported incorrect results
- **ConfigurationValidator dead code removal** — Removed unused `warnings` list that was never populated
- **ReferenceValidator dead code removal** — Removed unused `errors` variable; added null-guard for `LoadAssetAtPath` return value
- **ReferenceValidator dialog fix** — Corrected dialog message that always displayed "Errors: 0" regardless of actual issues
- **AdrlMenuItems crash fix** — Added null/empty guards to `FindAssets` and `LoadAssetAtPath` in documentation opener methods; extracted shared `PingDocumentation` helper

#### Runtime Diagnostics
- **Bootstrapper null guards** — Added early-exit validation for required configs (ProjectConfig, RuntimeConfig, SimulationConfig) with clear error messages
- **AssetValidation messages improved** — ConfigRegistry empty upgraded from warning to error (blocks runtime); added summary log line with pass/fail counts

#### Drone Stability
- **DroneMotor zero-vector guard** — Added `sqrMagnitude < Epsilon` check before `Quaternion.LookRotation` to prevent runtime crash on zero-direction input
- **DroneEnergy recharge deadlock fix** — Removed early-return from `Recharge()` that prevented depleted batteries from being recharged, making depleted drones unrecoverable
- **DroneEnergy magic numbers** — Extracted `LowThreshold` (0.25f) and `CriticalThreshold` (0.10f) to named constants

#### Procedural Stability
- **ProceduralGenerator sort caching** — Rules are now sorted once and cached; `_rulesDirty` flag triggers re-sort only after AddRule/RemoveRule
- **PlacedPositions clearing** — `context.PlacedPositions` is now cleared between rule executions to prevent cross-rule position contamination

#### Validator Cleanup
- **ProceduralValidator DRY** — Extracted private `IsValid(GenerationSettings, out string)` helper eliminating 4 repeated null-check blocks
- **ScenarioValidator DRY** — Extracted private `IsValid(ScenarioProfile, out string)` helper eliminating 4 repeated null-check blocks

#### Documentation Alignment
- **SDS namespace fix** — Updated `ADRLRescue.Core` and `ADRL Rescue.Core` references to `ADRL.Core` to match actual implementation
- **README project status** — Updated version badge and milestone table to reflect current implementation progress
- **README folder structure** — Fixed `UnityProject/` reference to actual `Assets/ADRL/` structure
- README ML-Agents badge noted as documentation drift (package not yet installed)

- 14 existing files modified across Core, Drone, Environment, Editor assemblies
- Zero new files, zero new asmdefs, zero namespace changes, zero public API changes
- Zero behavioral changes to any runtime system

#### Pre-Phase 7 Engineering Audit (2026-07-23)
- **Comprehensive read-only audit** performed following Constitution v4.1 across all 13 sections
- **Architecture verified** — 8 assembly DAG is clean, no circular dependencies
- **Documentation drift measured** — 7 documents with `UnityProject/`, `ADRLRescue`, or `v0.1.0` references (all in documentation, not code)
- **Code health verified** — zero dead code after Phase 5.6, zero unused variables, zero namespace violations
- **4 placeholder assemblies confirmed empty**: ADRL.AI, ADRL.Sensors, ADRL.Training, ADRL.UI
- **ML Readiness scored**: 3/10 (ML-Agents package not installed, no agent, no sensors)
- **Technical debt catalogued**: 1 critical (missing ML-Agents), 5 high, 6 medium, 4 low items
- **Overall Repository Health Score**: 7.8/10
- Full audit report documented in session record

---

| Version | Phase | Description |
|---------|-------|-------------|
| v0.1.0 | Foundation | Repository architecture and documentation |
| v0.2.0 | Unity Foundation | Core framework, resource management, drone framework |
| v0.3.0 | Environment | Environment framework, terrain generation, disaster types |
| v0.4.0 | Drone Runtime | Drone framework, runtime lifecycle, fleet management |
| v0.5.0 | Runtime Infrastructure | Subsystem wiring, persistence, recovery, validation |
| v0.6.0 | Runtime Completion | Diagnostics, spawn pipeline, object pooling |
| v0.7.0 | Infrastructure & Environment | Drone entity foundation, spawn pipeline, runtime integration |
| v0.8.0 | RL Foundation | Sensors, AI agent, runtime activation (Phases 7.1/7.2) |
| v0.8.1 | Reward System | Reward evaluator, reward breakdown, reward config (Phase 7.3) |
| v0.8.2 | Runtime Event Integration | Collision & victim pipeline, mission-completed episode finalization (Phase 8.1) |
| v0.8.3 | Decision Runtime | DecisionEngine as the runtime decision authority (Phase 8.3) |
| v0.8.4 | Behaviour Execution | Behaviour executor layer, factory-based command generation (Phase 8.4) |
| v0.8.5 | Behaviour Memory | Short-term behaviour memory, continuity-based selection (Phase 8.5) |
| v0.8.6 | Mission Coordination | Mission task coordinator, deterministic transitions (Phase 8.6) |
| v0.8.7 | Task Prioritization | Objective arbitration, deterministic priority scoring (Phase 8.7) |
| v0.8.8 | Architectural Refinement | Candidate generation separation, extended decision diagnostics (Phase 8.7.1) |
| v0.8.9 | Decision Context | Unified immutable per-step context snapshot (Phase 8.8) |
| v0.9.0 | Behaviour Optimization | Deterministic execution optimization layer (Phase 8.9) |
| v0.10.0 | World Knowledge | Persistent world-knowledge layer (Phase 9.0) |
| v0.11.0 | Decision Explainability | Deterministic immutable explanation layer (Phase 9.1) |
| v0.12.0 | Decision Trace & Replay | Deterministic replayable trace layer (Phase 9.2) |
| v0.13.0 | Decision Telemetry | Deterministic read-only telemetry layer (Phase 9.3) |
| v0.14.0 | Decision Analytics | Deterministic read-only analytics layer (Phase 9.4) |
| v0.15.0 | Decision Quality Evaluation | Deterministic read-only quality evaluation layer (Phase 9.5) |
| v1.0.0 | Autonomous Decision Advisory | Deterministic read-only decision advisory layer (Phase 10.0) — first stable release |

---

*This changelog follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format.*
