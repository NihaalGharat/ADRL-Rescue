# ADRL-Rescue Namespace Guide

**Version:** 1.7  
**Phase:** 7.3 — Reward System & Validation  
**Date:** 02/08/2026

---

## Overview

This document defines the namespace conventions for ADRL-Rescue. Namespaces mirror the folder structure and Assembly Definition organization.

---

## Namespace Hierarchy

```
ADRL.Core
├── ADRL.Core.Bootstrap
├── ADRL.Core.Configuration
├── ADRL.Core.Events
├── ADRL.Core.Resources
├── ADRL.Core.Services
├── ADRL.Core.Simulation
└── ADRL.Core.Utilities

ADRL.Drone
├── ADRL.Drone.Components
├── ADRL.Drone.Controllers
├── ADRL.Drone.Core
├── ADRL.Drone.Events
├── ADRL.Drone.Interfaces
└── ADRL.Drone.Utilities

ADRL.AI
├── ADRL.AI.Agents
├── ADRL.AI.DecisionMaking
└── ADRL.AI.Rewards

ADRL.Environment
├── ADRL.Environment.Core
├── ADRL.Environment.Events
├── ADRL.Environment.Hazards
├── ADRL.Environment.Interfaces
├── ADRL.Environment.Obstacles
├── ADRL.Environment.Procedural
│   └── ADRL.Environment.Procedural.Rules
├── ADRL.Environment.Scenarios
├── ADRL.Environment.Spawning
├── ADRL.Environment.Terrain
├── ADRL.Environment.Validation
├── ADRL.Environment.Victims
└── ADRL.Environment.WorldObjects

ADRL.Sensors
├── ADRL.Sensors.Detection
├── ADRL.Sensors.Fusion
├── ADRL.Sensors.Interfaces
└── ADRL.Sensors.Raycasting

ADRL.Training
└── ADRL.Training.Runtime

ADRL.UI

ADRL.Editor
├── ADRL.Editor.Bootstrap
└── ADRL.Editor.Validation

ADRL.Tests.Editor
├── ADRL.Tests.Editor.Configuration
├── ADRL.Tests.Editor.Performance
├── ADRL.Tests.Editor.Rewards
└── ADRL.Tests.Editor.Simulation
```

---

## Namespace Rules

### Rule 1: Match Folder Structure

Every C# file's namespace must match its folder path relative to `Assets/ADRL/Scripts/`.

**Example:**
- File: `Assets/ADRL/Scripts/Drone/Controllers/DroneController.cs`
- Namespace: `ADRL.Drone.Controllers`

### Rule 2: Use Assembly Definition Root Namespace

Each Assembly Definition specifies a `rootNamespace`. All scripts within that assembly must use namespaces that start with the root namespace.

| Assembly | Root Namespace |
|:---------|:---------------|
| ADRL.Core | `ADRL.Core` |
| ADRL.Drone | `ADRL.Drone` |
| ADRL.AI | `ADRL.AI` |
| ADRL.Environment | `ADRL.Environment` |
| ADRL.Sensors | `ADRL.Sensors` |
| ADRL.Training | `ADRL.Training` |
| ADRL.UI | `ADRL.UI` |
| ADRL.Editor | `ADRL.Editor` |
| ADRL.Tests.Editor | `ADRL.Tests.Editor` |

### Rule 3: No Cross-Assembly Namespace Usage

Namespaces from one assembly should not be used directly in another assembly. Use interfaces or dependency injection instead.

**Correct:**
```csharp
// In ADRL.Drone
namespace ADRL.Drone.Interfaces
{
    public interface IDroneSubsystem { }
}

// In ADRL.AI
namespace ADRL.AI.Agents
{
    using ADRL.Drone.Interfaces;
    
    public class DroneAgent : MonoBehaviour
    {
        private IDroneSubsystem _subsystem;
    }
}
```

**Incorrect:**
```csharp
// In ADRL.AI
namespace ADRL.AI.Agents
{
    using ADRL.Drone.Controllers; // Direct concrete dependency
    
    public class DroneAgent : MonoBehaviour
    {
        private DroneController _controller;
    }
}
```

### Rule 4: One Namespace Per File

Each C# file should contain exactly one namespace.

**Correct:**
```csharp
namespace ADRL.Core.Events
{
    public class GameEvent
    {
    }
}
```

**Incorrect:**
```csharp
namespace ADRL.Core.Events
{
    public class GameEvent { }
}

namespace ADRL.Drone.Controllers
{
    public class DroneController { }
}
```

### Rule 5: Namespace Declaration Style

Use block-scoped namespace declarations (not file-scoped) for consistency.

**Correct:**
```csharp
namespace ADRL.Drone.Core
{
    public class DroneSubsystem
    {
    }
}
```

**Acceptable (C# 10+):**
```csharp
namespace ADRL.Drone.Core;

public class DroneSubsystem
{
}
```

---

## Assembly Dependencies

```
ADRL.Core (no dependencies)
    ↑
    ├── ADRL.Drone (depends on Core)
    ├── ADRL.Environment (depends on Core, Sensors)
    ├── ADRL.Sensors (depends on Core)
    └── ADRL.UI (depends on Core; empty - reserved)
    ↑
    ├── ADRL.AI (depends on Core, Drone, Sensors, Unity.ML-Agents)
    ├── ADRL.Training (depends on Core, Drone, AI, Environment, Sensors, Unity.ML-Agents)
    ├── ADRL.Editor (depends on all runtime assemblies, Editor-only)
    └── ADRL.Tests.Editor (depends on ADRL.AI, ADRL.Core, Unity.TestFramework)
```

---

## Using Namespaces

### Importing Namespaces

Use the `using` directive at the top of the file:

```csharp
using ADRL.Core.Events;
using ADRL.Drone.Interfaces;
using ADRL.Sensors.Detection;
```

### Fully Qualified Names

For clarity, use fully qualified names when there's ambiguity:

```csharp
public class DroneController : ADRL.Drone.Interfaces.IDroneSubsystem
{
}
```

---

## Namespace Examples

### Core System
```csharp
namespace ADRL.Core.Configuration
{
    public class RewardConfig { }
}

namespace ADRL.Core.Services
{
    public class ServiceLocator { }
}

namespace ADRL.Core.Events
{
    public class GameEvent { }
}
```

### Drone System
```csharp
namespace ADRL.Drone.Controllers
{
    public class DroneController { }
}

namespace ADRL.Drone.Core
{
    public class DroneSubsystem { }
}

namespace ADRL.Drone.Utilities
{
    public static class DroneFleetValidator { }
}
```

### AI System
```csharp
namespace ADRL.AI.Agents
{
    public class DroneAgent { }
}

namespace ADRL.AI.DecisionMaking
{
    public class DroneActionResolver { }
}

namespace ADRL.AI.Rewards
{
    public class RewardEvaluator { }
}
```

### Environment System
```csharp
namespace ADRL.Environment.Core
{
    public class EnvironmentManager { }
}

namespace ADRL.Environment.Interfaces
{
    public interface IEnvironmentObject { }
}

namespace ADRL.Environment.Validation
{
    public static class WorldObjectValidator { }
}

namespace ADRL.Environment.Victims
{
    public class Victim { }
}

namespace ADRL.Environment.Hazards
{
    public class Hazard { }
}

namespace ADRL.Environment.Spawning
{
    public class SpawnManager { }
}

namespace ADRL.Environment.WorldObjects
{
    public class WorldObjectBase { }
}

namespace ADRL.Environment.Obstacles
{
    public class Obstacle { }
    public class ObstacleManager { }
    public enum ObstacleType { }
    public enum ObstacleState { }
}

namespace ADRL.Environment.Procedural
{
    public class ProceduralGenerator { }
    public class GenerationSettings { }
    public abstract class GenerationRule { }
    public interface IGenerationContext { }
    public static class PlacementUtility { }
}

namespace ADRL.Environment.Procedural.Rules
{
    public interface IGenerationRule { }
    public class VictimGenerationRule { }
    public class HazardGenerationRule { }
    public class ObstacleGenerationRule { }
}

namespace ADRL.Environment.Scenarios
{
    public enum ScenarioType { }
    public enum ScenarioDifficulty { }
    public enum ScenarioState { }
    public interface IScenario { }
    public class ScenarioProfile { }
    public class ScenarioManager { }
    public static class ScenarioValidator { }
}
```

### Sensor System
```csharp
namespace ADRL.Sensors.Raycasting
{
    public class DroneRaySensor { }
}

namespace ADRL.Sensors.Detection
{
    public class DroneThermalSensor { }
}

namespace ADRL.Sensors.Fusion
{
    public class SensorFusionProvider { }
}
```

---

## Common Patterns

### Interface Pattern

Define interfaces in the assembly that owns the abstraction:

```csharp
// In ADRL.Drone.Interfaces
namespace ADRL.Drone.Interfaces
{
    public interface IDroneSubsystem
    {
        bool Health { get; }
        void Boot(EventBus eventBus);
        void Shutdown(EventBus eventBus);
    }
}

// In ADRL.AI.Agents
namespace ADRL.AI.Agents
{
    using ADRL.Drone.Interfaces;
    
    public class DroneAgent : MonoBehaviour
    {
        private IDroneSubsystem _subsystem;
    }
}
```

### Event Pattern

Use EventBus from ADRL.Core.Events:

```csharp
namespace ADRL.Drone.Controllers
{
    using ADRL.Core.Events;

    public class DroneController : MonoBehaviour
    {
        private EventBus _eventBus;

        private void Start()
        {
            _eventBus = GameBootstrap.EventBus;
            _eventBus.Subscribe<SimulationStartedEvent>(OnSimulationStarted);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<SimulationStartedEvent>(OnSimulationStarted);
        }

        private void OnSimulationStarted(SimulationStartedEvent eventData)
        {
            // Handle simulation start
        }
    }
}
```

---

## Validation Checklist

When writing code, verify:

- [ ] Namespace matches folder structure
- [ ] Namespace starts with assembly root namespace
- [ ] No circular namespace dependencies
- [ ] One namespace per file
- [ ] Using directives at top of file
- [ ] Interfaces used for cross-assembly communication

---

## References

- Assembly Definitions: `Assets/ADRL/Scripts/*/ADRL.*.asmdef`
- Project Charter: `Documentation/00_PROJECT_CHARTER.md`
- Software Design Specification: `Documentation/17_SOFTWARE_DESIGN_SPECIFICATION.md`

---

**End of Namespace Guide**
