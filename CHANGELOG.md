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

## [Unreleased]

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

---

## Version Roadmap

| Version | Phase | Description |
|---------|-------|-------------|
| v0.1.0 | Foundation | Repository architecture and documentation |
| v0.2.0 | Unity Foundation | Core framework, resource management, drone framework |
| v0.3.0 | Environment | Environment framework, disaster types |
| v0.4.0 | Sensors & AI | Sensor implementations, ML-Agents integration |
| v0.5.0 | Training | Reward system, PPO training pipeline |
| v0.6.0 | Polish | UI, performance, final documentation |
| v1.0.0 | Release | Full stable release |

---

*This changelog follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format.*
