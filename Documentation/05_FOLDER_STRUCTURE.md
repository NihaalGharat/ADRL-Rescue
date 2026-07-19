# 05 - Folder Structure

---

## Repository Root

```
ADRL-Rescue/
│
├── 📂 UnityProject/          # Unity game project
├── 📂 Python/                # Training scripts and configs
├── 📂 Documentation/         # Project documentation
├── 📂 Assets/                # Static assets (icons, banners)
├── 📂 Media/                 # Screenshots, videos, GIFs
├── 📂 Research/              # Papers, notes, references
│
├── 📄 README.md              # Project landing page
├── 📄 CHANGELOG.md           # Version history
├── 📄 CONTRIBUTING.md        # Contribution guidelines
├── 📄 CODE_OF_CONDUCT.md     # Community standards
├── 📄 SECURITY.md            # Security policy
├── 📄 LICENSE                # MIT License
├── 📄 CITATION.cff           # Citation metadata
└── 📄 .gitignore             # Git ignore rules
```

---

## UnityProject/

The main Unity project containing all game logic, assets, and configurations.

```
UnityProject/
│
├── 📂 Assets/
│   ├── 📂 Scripts/
│   │   ├── 📂 Core/              # Game manager, utilities
│   │   ├── 📂 AI/                # ML-Agents, decision making
│   │   ├── 📂 Drone/             # Drone behavior, flight control
│   │   ├── 📂 Environment/       # Procedural generation
│   │   ├── 📂 Sensors/           # Ray, thermal, vision sensors
│   │   ├── 📂 Training/          # Reward system, training config
│   │   ├── 📂 Utilities/         # Helper functions, extensions
│   │   └── 📂 UI/                # HUD, debug overlay
│   │
│   ├── 📂 Prefabs/               # Reusable GameObjects
│   ├── 📂 Materials/             # Physics materials, shaders
│   ├── 📂 Textures/              # Texture assets
│   ├── 📂 Models/                # 3D models
│   ├── 📂 Animations/            # Animation controllers
│   ├── 📂 Scenes/                # Unity scenes
│   ├── 📂 Settings/              # Quality, input settings
│   └── 📂 Plugins/               # Third-party plugins
│
├── 📂 ProjectSettings/           # Unity project settings
└── 📂 Packages/                  # Package manifest
```

---

## Python/

Training scripts, configuration files, and model outputs.

```
Python/
│
├── 📂 configs/              # Training YAML configurations
├── 📂 scripts/              # Python training scripts
├── 📂 results/              # Training results
├── 📂 logs/                 # TensorBoard logs
└── 📂 models/               # Exported ONNX models
```

---

## Documentation/

All project documentation with numbered guides.

```
Documentation/
│
├── 01_PROJECT_VISION.md         # Project goals and vision
├── 02_PROJECT_ARCHITECTURE.md   # System architecture
├── 03_SYSTEM_DESIGN.md          # Detailed system design
├── 04_DEVELOPMENT_ROADMAP.md    # Development timeline
├── 05_FOLDER_STRUCTURE.md       # This file
├── 06_AI_SYSTEM.md              # AI/ML system details
├── 07_DRONE_SYSTEM.md           # Drone system details
├── 08_ENVIRONMENT_SYSTEM.md     # Environment system
├── 09_SENSOR_SYSTEM.md          # Sensor specifications
├── 10_REWARD_SYSTEM.md          # Reward function design
├── 11_TRAINING_PIPELINE.md      # Training workflow
├── 12_DATA_FLOW.md              # Data flow diagrams
├── 13_CODING_STANDARDS.md       # Coding conventions
├── 14_GITHUB_WORKFLOW.md        # Git workflow
├── 15_TESTING_GUIDE.md          # Testing procedures
├── 16_FUTURE_SCOPE.md           # Future features
└── PROJECT_GLOSSARY.md          # Terminology reference
```

---

## Assets/

Static assets for documentation and branding.

```
Assets/
│
├── 📂 Icons/                # Project icons
└── 📂 Banners/              # GitHub banners
```

---

## Media/

Screenshots, videos, and GIFs for documentation.

```
Media/
│
├── 📂 Screenshots/          # Application screenshots
├── 📂 Videos/               # Demo videos
└── 📂 GIFs/                 # Animated demonstrations
```

---

## Research/

Academic papers, notes, and references.

```
Research/
│
├── 📂 Papers/               # Reference papers
└── 📂 Notes/                # Research notes
```

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
| [README](../README.md) | Project landing page |

---

*Last updated: July 2026*
