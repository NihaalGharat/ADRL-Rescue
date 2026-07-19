# 15 - Testing Guide

---

## Overview

This document describes the testing strategy and procedures for ADRL-Rescue.

---

## Testing Strategy

```mermaid
graph TD
    A[Testing Strategy] --> B[Unit Tests]
    A --> C[Integration Tests]
    A --> D[Manual Testing]
    A --> E[Performance Tests]
    
    B --> B1[Individual Classes]
    B --> B2[Utility Functions]
    
    C --> C1[System Interactions]
    C --> C2[ML-Agents Integration]
    
    D --> D1[Flight Controls]
    D --> D2[Sensor Behavior]
    
    E --> E1[Frame Rate]
    E --> E2[Memory Usage]
```

---

## Test Types

### 1. Unit Tests

Test individual components in isolation.

**Examples:**
| Component | Test | Expected |
|-----------|------|----------|
| RewardSystem | CalculateReward with collision | Returns -5.0 |
| ObservationProcessor | Normalize value in range | Returns [0, 1] |
| DroneMemory | Record new position | Position added |
| SensorManager | Initialize sensors | All sensors active |

**Framework:** NUnit (Unity Test Framework)

```csharp
[Test]
public void RewardSystem_VictimFound_ReturnsPositiveReward()
{
    // Arrange
    var system = new RewardSystem();
    var state = new DroneState { VictimDetected = true };
    
    // Act
    float reward = system.CalculateReward(state);
    
    // Assert
    Assert.AreEqual(10.0f, reward, 0.001f);
}
```

### 2. Integration Tests

Test how components work together.

**Examples:**
| Test | Components | Expected |
|------|-----------|----------|
| Sensor → Observation | RaySensor + ObservationProcessor | Valid observation vector |
| Memory → AI | DroneMemory + DroneAgent | Memory influences decisions |
| Action → Physics | FlightController + Rigidbody | Drone moves correctly |

### 3. Manual Testing

Human verification of behavior.

**Checklist:**
- [ ] Drone hovers stably
- [ ] Drone responds to controls
- [ ] Sensors detect obstacles
- [ ] Thermal sensor detects victims
- [ ] Collision detection works
- [ ] Environments generate correctly
- [ ] Victims spawn properly

### 4. Performance Tests

Measure system performance.

**Metrics:**
| Metric | Target | Measurement |
|--------|--------|-------------|
| Frame Rate | > 30 FPS | Unity Profiler |
| Memory | < 2 GB | Task Manager |
| Training Speed | > 1000 steps/sec | ML-Agents stats |
| Model Inference | < 10ms | Profiler |

---

## Test Environment Setup

### Unity Test Runner

1. Open Window > General > Test Runner
2. Select EditMode or PlayMode
3. Click Run All

### Python Tests

```bash
# Run all tests
python -m pytest tests/

# Run with coverage
python -m pytest tests/ --cov=src

# Run specific test
python -m pytest tests/test_reward.py
```

---

## Test Cases

### Drone System

| ID | Test Case | Expected Result |
|----|-----------|-----------------|
| D01 | Drone stays still with no input | Position unchanged |
| D02 | Apply upward force | Drone rises |
| D03 | Apply forward force | Drone moves forward |
| D04 | Apply rotation | Drone rotates |
| D05 | Stabilization enabled | Drone stays level |

### Sensor System

| ID | Test Case | Expected Result |
|----|-----------|-----------------|
| S01 | Ray hits obstacle | Distance returned |
| S02 | Ray hits nothing | Max distance returned |
| S03 | Thermal near victim | High value returned |
| S04 | Thermal far from victim | Low value returned |
| S05 | Vision sees victim | 1.0 returned |
| S06 | Vision blocked | 0.0 returned |
| S07 | Collision detected | Event triggered |

### Reward System

| ID | Test Case | Expected Result |
|----|-----------|-----------------|
| R01 | Victim found | +10.0 reward |
| R02 | Victim rescued | +25.0 reward |
| R03 | Collision | -5.0 reward |
| R04 | Out of bounds | -10.0 reward |
| R05 | New area explored | +0.5 reward |
| R06 | Time step | -0.01 reward |

### Environment System

| ID | Test Case | Expected Result |
|----|-----------|-----------------|
| E01 | Generate terrain | Terrain created |
| E02 | Place obstacles | No overlaps |
| E03 | Spawn victims | Victims in valid positions |
| E04 | Random seed | Different layouts |
| E05 | Same seed | Same layout |

---

## Test Reporting

### Coverage Report

```bash
# Generate coverage
python -m pytest tests/ --cov=src --cov-report=html

# Open htmlcov/index.html
```

### Test Results

- All tests must pass before merge
- Coverage should be > 80%
- Performance tests should meet targets

---

## Continuous Integration

### GitHub Actions Workflow

```yaml
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - checkout
      - setup-python
      - install-dependencies
      - run-unit-tests
      - run-integration-tests
      - upload-coverage
```

---

## Navigation

| Document | Description |
|----------|-------------|
| [13_CODING_STANDARDS](13_CODING_STANDARDS.md) | Coding conventions |
| [14_GITHUB_WORKFLOW](14_GITHUB_WORKFLOW.md) | Git workflow |

---

*Last updated: July 2026*
