# 13 - Coding Standards

---

## Overview

This document defines the coding standards and conventions for the ADRL-Rescue project. Following these standards ensures consistency, readability, and maintainability.

---

## General Principles

| Principle | Description |
|-----------|-------------|
| **Clarity** | Code should be easy to read and understand |
| **Simplicity** | Prefer simple solutions over clever ones |
| **Consistency** | Follow established patterns throughout |
| **Modularity** | Each class does one thing well |
| **Documentation** | Code should be self-documenting |

---

## C# Naming Conventions

### Files

| Type | Convention | Example |
|------|-----------|---------|
| Classes | PascalCase | `DroneAgent.cs` |
| Interfaces | I + PascalCase | `ISensor.cs` |
| Enums | PascalCase | `DisasterType.cs` |
| Scripts | PascalCase | `FlightController.cs` |

### Variables

| Type | Convention | Example |
|------|-----------|---------|
| Public fields | PascalCase | `public float speed;` |
| Private fields | _camelCase | `private float _speed;` |
| Local variables | camelCase | `float currentSpeed;` |
| Parameters | camelCase | `float targetSpeed` |
| Constants | UPPER_SNAKE | `MAX_SPEED` |

### Methods

| Type | Convention | Example |
|------|-----------|---------|
| Public methods | PascalCase | `public void Move()` |
| Private methods | PascalCase | `private void ApplyForce()` |
| Properties | PascalCase | `public float Speed { get; }` |

---

## C# Code Structure

### Class Organization

```csharp
public class DroneAgent : Agent
{
    // 1. Constants
    private const float MAX_SPEED = 10.0f;
    
    // 2. Serialized Fields
    [SerializeField] private float _moveForce = 5.0f;
    
    // 3. Private Fields
    private Rigidbody _rigidbody;
    private float _currentSpeed;
    
    // 4. Properties
    public float CurrentSpeed => _currentSpeed;
    
    // 5. Unity Lifecycle Methods
    private void Awake() { }
    private void Start() { }
    private void FixedUpdate() { }
    
    // 6. Public Methods
    public void ApplyMovement(float x, float y, float z) { }
    
    // 7. Private Methods
    private void UpdateSpeed() { }
    private void ApplyStabilization() { }
}
```

### File Headers

```csharp
// File: DroneAgent.cs
// Author: Nihaal Gharat
// Description: Controls drone behavior and ML-Agents integration
// Last Updated: 2026-07-20
```

---

## Comments

### When to Comment

| Situation | Action |
|-----------|--------|
| Complex algorithm | Add explanation |
| Non-obvious logic | Add clarification |
| Public API | Add XML documentation |
| Magic numbers | Replace with named constants |
| Obvious code | Don't comment |

### Comment Style

```csharp
/// <summary>
/// Collects observations from all sensors and combines them into a single vector.
/// </summary>
/// <returns>Observation vector for the neural network</returns>
public float[] CollectObservations()
{
    // Collect ray sensor distances
    float[] rayDistances = _raySensor.CollectDistances();
    
    // Normalize to [0, 1] range
    float[] normalized = NormalizeArray(rayDistances, 0f, 10f);
    
    return normalized;
}
```

---

## Python Naming Conventions

### Files

| Type | Convention | Example |
|------|-----------|---------|
| Scripts | snake_case | `train_ppo.py` |
| Configs | snake_case | `drone_config.yaml` |
| Modules | snake_case | `sensor_utils.py` |

### Variables

| Type | Convention | Example |
|------|-----------|---------|
| Functions | snake_case | `def calculate_reward()` |
| Variables | snake_case | `current_step = 0` |
| Classes | PascalCase | `class DroneTrainer` |
| Constants | UPPER_SNAKE | `MAX_EPISODES = 1000` |

---

## File Organization

### Script Structure

```
UnityProject/Assets/Scripts/
├── Core/           # Game manager, utilities
├── AI/             # ML-Agents, decision making
├── Drone/          # Drone behavior, flight
├── Environment/    # Procedural generation
├── Sensors/        # All sensor types
├── Training/       # Reward system
├── Utilities/      # Helper functions
└── UI/             # User interface
```

### File Naming

| Folder | Naming Convention | Example |
|--------|-------------------|---------|
| Core | PascalCase | `GameManager.cs` |
| AI | PascalCase | `DroneAgent.cs` |
| Drone | PascalCase | `FlightController.cs` |
| Environment | PascalCase | `ProceduralGenerator.cs` |
| Sensors | PascalCase | `RaySensor.cs` |
| Training | PascalCase | `RewardSystem.cs` |

---

## Error Handling

### C# Patterns

```csharp
// Use try-catch for operations that might fail
try
{
    LoadModel(modelPath);
}
catch (FileNotFoundException ex)
{
    Debug.LogError($"Model not found: {ex.Message}");
    return false;
}

// Use null checks
if (_rigidbody == null)
{
    Debug.LogError("Rigidbody not assigned!");
    return;
}
```

### Unity Specific

```csharp
// Use Debug.Log for development
Debug.Log("Training started");

// Use Debug.LogWarning for warnings
Debug.LogWarning("Sensor not configured");

// Use Debug.LogError for errors
Debug.LogError("Failed to load model");
```

---

## Unit Testing

### Test Structure

```csharp
[TestFixture]
public class RewardSystemTests
{
    [Test]
    public void CalculateReward_VictimFound_ReturnsPositive()
    {
        // Arrange
        var rewardSystem = new RewardSystem();
        var state = new DroneState { VictimDetected = true };
        
        // Act
        float reward = rewardSystem.CalculateReward(state);
        
        // Assert
        Assert.AreEqual(10.0f, reward);
    }
}
```

---

## Documentation Standards

### README Updates

- Update when adding features
- Include usage examples
- Keep installation instructions current

### Code Comments

- Document public APIs
- Explain complex algorithms
- Don't state the obvious

---

## Navigation

| Document | Description |
|----------|-------------|
| [05_FOLDER_STRUCTURE](05_FOLDER_STRUCTURE.md) | Repository organization |
| [14_GITHUB_WORKFLOW](14_GITHUB_WORKFLOW.md) | Git workflow |

---

*Last updated: July 2026*
