# 01 - Project Vision

> **"The drone should not be programmed to rescue people. It should learn how to rescue people."**

---

## Overview

**ADRL-Rescue** (Autonomous Disaster Response Drone using Reinforcement Learning) is an AI research project that develops an autonomous drone capable of performing Search & Rescue (SAR) operations inside procedurally generated disaster environments.

The drone learns its behavior entirely through **Reinforcement Learning (PPO)** — no hardcoded paths, no scripted responses. The AI discovers optimal strategies through trial and error in simulated environments.

---

## Problem Statement

During natural disasters, rescue teams face critical challenges:

| Challenge | Impact |
|-----------|--------|
| Inaccessible terrain | Humans cannot reach dangerous areas |
| Time pressure | Every minute reduces survival probability |
| Risk to rescuers | Secondary collapses endanger teams |
| Exhaustion | Human rescuers tire; drones do not |
| Limited visibility | Smoke, debris, and darkness reduce effectiveness |

Autonomous drones can address these challenges by:
- Reaching areas too dangerous for humans
- Searching continuously without fatigue
- Operating in zero-visibility conditions (thermal sensors)
- Assisting first responders with real-time intelligence

---

## Project Goals

### Primary Goals

1. **Autonomous Navigation** — The drone navigates unknown environments without pre-programmed paths
2. **Victim Detection** — Onboard sensors identify and locate disaster victims
3. **Obstacle Avoidance** — The drone safely maneuvers around debris, trees, and structures
4. **Generalization** — A single AI model works across multiple disaster scenarios
5. **Procedural Adaptation** — Environments are randomly generated to prevent overfitting

### Secondary Goals

1. **Modular Architecture** — Each system is independent and replaceable
2. **Research Quality** — Suitable for academic publication
3. **Open Source** — Community-driven development
4. **Scalability** — Architecture supports future expansion

---

## Key Differentiators

This is **NOT** a pathfinding project. It is a **Reinforcement Learning** project.

| Traditional Approach | ADRL-Rescue Approach |
|---------------------|---------------------|
| A* or Dijkstra pathfinding | PPO-based learning |
| Pre-mapped environments | Procedurally generated worlds |
| Hardcoded obstacle avoidance | Learned collision avoidance |
| Single scenario | Multiple disaster types |
| Static behavior | Adaptive behavior |

---

## Success Criteria

The project is considered successful when the drone can:

- [ ] Navigate procedurally generated environments autonomously
- [ ] Detect victims using thermal and vision sensors
- [ ] Avoid obstacles without human intervention
- [ ] Generalize across earthquake, flood, landslide, and collapse scenarios
- [ ] Complete rescue missions in under a target time threshold
- [ ] Demonstrate improved performance over training episodes

---

## Target Audience

| Audience | Use Case |
|----------|----------|
| Researchers | RL application in robotics |
| Students | Learning ML-Agents and Unity |
| Developers | Contributing to open-source AI |
| First Responders | Future real-world deployment |

---

## Navigation

| Document | Description |
|----------|-------------|
| [02_PROJECT_ARCHITECTURE](02_PROJECT_ARCHITECTURE.md) | System architecture overview |
| [03_SYSTEM_DESIGN](03_SYSTEM_DESIGN.md) | Detailed system design |
| [04_DEVELOPMENT_ROADMAP](04_DEVELOPMENT_ROADMAP.md) | Development timeline |
| [README](../README.md) | Project landing page |

---

*Last updated: July 2026*
