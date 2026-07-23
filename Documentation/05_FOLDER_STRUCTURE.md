# 05 - Folder Structure

---

## Repository Root

```
ADRL-Rescue/
│
├── 📂 Assets/                 # Unity project assets
│   └── 📂 ADRL/               # ADRL game content root
│       ├── 📂 Art/            # Visual art assets
│       ├── 📂 Audio/          # Audio clips and sound effects
│       ├── 📂 Documentation/  # In-editor documentation assets
│       ├── 📂 Gizmos/         # Unity Gizmo icons
│       ├── 📂 Materials/      # Physics materials and shaders
│       ├── 📂 Models/         # 3D models and meshes
│       ├── 📂 Prefabs/        # Reusable GameObjects
│       │   ├── 📂 Drone/
│       │   ├── 📂 Effects/
│       │   ├── 📂 Environment/
│       │   ├── 📂 Obstacles/
│       │   ├── 📂 UI/
│       │   └── 📂 Victims/
│       ├── 📂 Resources/      # Runtime-loadable assets
│       ├── 📂 Scenes/         # Unity scenes
│       ├── 📂 ScriptableObjects/  # Configuration ScriptableObjects
│       │   ├── 📂 Configurations/
│       │   ├── 📂 Drone/
│       │   ├── 📂 Environment/
│       │   ├── 📂 Rewards/
│       │   ├── 📂 Sensors/
│       │   └── 📂 Training/
│       ├── 📂 Scripts/        # C# source code
│       │   ├── 📂 AI/         # AI/ML system
│       │   │   ├── 📂 Agents/
│       │   │   ├── 📂 DecisionMaking/
│       │   │   ├── 📂 Policies/
│       │   │   ├── 📂 Rewards/
│       │   │   └── 📂 Training/
│       │   ├── 📂 Core/       # Core framework
│       │   │   ├── 📂 Bootstrap/
│       │   │   ├── 📂 Configuration/
│       │   │   ├── 📂 Events/
│       │   │   ├── 📂 Resources/
│       │   │   ├── 📂 Services/
│       │   │   ├── 📂 Simulation/
│       │   │   └── 📂 Utilities/
│       │   ├── 📂 Drone/      # Drone system
│       │   │   ├── 📂 Behaviours/
│       │   │   ├── 📂 Components/
│       │   │   ├── 📂 Controllers/
│       │   │   ├── 📂 Events/
│       │   │   ├── 📂 Interfaces/
│       │   │   ├── 📂 Navigation/
│       │   │   └── 📂 Physics/
│       │   ├── 📂 Editor/     # Editor tools (Editor-only assembly)
│       │   │   └── 📂 Validation/
│       │   ├── 📂 Environment/  # Environment system
│       │   │   ├── 📂 Core/
│       │   │   ├── 📂 Events/
│       │   │   ├── 📂 Hazards/
│       │   │   ├── 📂 Interfaces/
│       │   │   ├── 📂 Obstacles/
│       │   │   ├── 📂 Procedural/
│       │   │   │   └── 📂 Rules/
│       │   │   ├── 📂 Scenarios/
│       │   │   ├── 📂 Spawning/
│       │   │   ├── 📂 Terrain/
│       │   │   ├── 📂 Validation/
│       │   │   ├── 📂 Victims/
│       │   │   └── 📂 WorldObjects/
│       │   ├── 📂 Sensors/    # Sensor system
│       │   │   ├── 📂 Detection/
│       │   │   ├── 📂 Fusion/
│       │   │   ├── 📂 Mapping/
│       │   │   ├── 📂 Raycasting/
│       │   │   └── 📂 Vision/
│       │   ├── 📂 Training/   # Training pipeline
│       │   └── 📂 UI/         # User interface
│       ├── 📂 Settings/       # Unity asset settings
│       ├── 📂 Shaders/        # Custom shaders
│       ├── 📂 StreamingAssets/  # Streaming data
│       ├── 📂 Tests/          # Test scripts
│       └── 📂 Textures/       # Texture assets
│
├── 📂 Documentation/          # Project documentation
├── 📂 Media/                  # Screenshots, videos, GIFs
├── 📂 Python/                 # Training scripts and configs
├── 📂 Research/               # Papers, notes, references
├── 📂 Packages/               # Unity package manifest
├── 📂 ProjectSettings/        # Unity project settings
│
├── 📄 README.md               # Project landing page
├── 📄 CHANGELOG.md            # Version history
├── 📄 CONTRIBUTING.md         # Contribution guidelines
├── 📄 CODE_OF_CONDUCT.md      # Community standards
├── 📄 SECURITY.md             # Security policy
├── 📄 LICENSE                 # MIT License
├── 📄 CITATION.cff            # Citation metadata
├── 📄 .gitignore              # Git ignore rules
├── 📄 ADRL-Rescue.sln         # Visual Studio solution
└── (assembly .csproj files)   # Auto-generated by Unity
```

---

## Core Project Paths

| Path | Description |
|:-----|:------------|
| `Assets/ADRL/Scripts/` | All C# source code, organized by assembly |
| `Assets/ADRL/Prefabs/` | Reusable prefabs by category (Drone, Environment, UI, etc.) |
| `Assets/ADRL/Scenes/` | Unity scene files |
| `Assets/ADRL/ScriptableObjects/` | Configuration ScriptableObjects |
| `Assets/ADRL/Materials/` | Materials and shader assets |
| `Assets/ADRL/Models/` | 3D models |
| `Assets/ADRL/Resources/` | Runtime-loadable assets (loaded via Resources.Load) |
| `Assets/ADRL/Tests/` | Test scripts |
| `Documentation/` | Project documentation (numbered guides) |
| `Python/` | Training scripts, YAML configs, ONNX models |
| `Media/` | Screenshots, videos, GIFs for documentation |
| `Research/` | Academic papers and references |

---

## Naming Conventions

### Files
- Use `PascalCase` for C# scripts: `DroneAgent.cs`
- Use `snake_case` for Python files: `train_ppo.py`
- Use `PascalCase` for documentation: `01_PROJECT_VISION.md`

### Folders
- Use `PascalCase` for Unity folders: `Scripts/`, `Prefabs/`
- Use `snake_case` for Python folders: `training_scripts/`
- Use `PascalCase` for documentation: `Documentation/`

---

## Navigation

| Document | Description |
|----------|-------------|
| [02_PROJECT_ARCHITECTURE](02_PROJECT_ARCHITECTURE.md) | System architecture |
| [13_CODING_STANDARDS](13_CODING_STANDARDS.md) | Coding conventions |
| [NAMESPACE_GUIDE](NAMESPACE_GUIDE.md) | Namespace conventions |
| [README](../README.md) | Project landing page |

---

*This document is the single authoritative source for the repository folder layout.*
