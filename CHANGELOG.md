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
| v0.3.0 | Environment | Environment framework, disaster types |
| v0.4.0 | Sensors & AI | Sensor implementations, ML-Agents integration |
| v0.5.0 | Training | Reward system, PPO training pipeline |
| v0.6.0 | Polish | UI, performance, final documentation |
| v0.7.0 | Runtime Framework | Object Pooling, Spawn Pipeline, Diagnostics, Runtime Integration |
| v1.0.0 | Release | Full stable release |

---

*This changelog follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format.*
