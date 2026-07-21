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

### Planned (Phase 1)

- Basic drone physics
- Flight controller
- Core managers (GameManager, EpisodeManager, ConfigurationManager)

### Planned (Phase 2)

- Ray sensor implementation
- Thermal sensor implementation
- Vision sensor implementation
- Collision detection system
- ML-Agents integration

### Planned (Phase 3)

- Procedural terrain generation
- Building placement system
- Obstacle spawning
- Victim system
- Disaster environments (Earthquake, Flood, Landslide, Collapse)

### Planned (Phase 4)

- Reward system implementation
- PPO training pipeline
- TensorBoard integration
- ONNX model export

### Planned (Phase 5)

- HUD implementation
- Debug overlay
- Performance optimization
- Final documentation

---

## Version Roadmap

| Version | Phase | Description |
|---------|-------|-------------|
| v0.1.0 | Foundation | Repository architecture and documentation |
| v0.2.0 | Unity Foundation | Core managers, drone physics, basic flight |
| v0.3.0 | Sensors & AI | Sensor implementations, ML-Agents integration |
| v0.4.0 | Environment | Procedural generation, disaster types |
| v0.5.0 | Training | Reward system, PPO training pipeline |
| v0.6.0 | Polish | UI, performance, final documentation |
| v1.0.0 | Release | Full stable release |

---

*This changelog follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format.*
