# ADRL-Rescue
## Adaptive Autonomous Disaster Response Drone using Reinforcement Learning

 Version 1.0

 This document explains the architecture, vision, and development plan of the project in simple language so that anyone (students, contributors, or future me) can easily understand how the project works.

---

# 📌 Project Vision

The objective of this project is to build an AI-powered autonomous rescue drone capable of assisting first responders during natural disasters.

Instead of following pre-programmed paths, the drone learns by itself using Reinforcement Learning (PPO) inside a simulated Unity environment.

The AI should be capable of

- Exploring unknown environments
- Avoiding obstacles
- Searching for victims
- Making intelligent navigation decisions
- Working in multiple disaster scenarios without changing the AI

---

# Why This Project

During disasters, rescue teams often waste valuable time searching dangerous locations.

Autonomous drones can

- Reach inaccessible areas
- Search faster than humans
- Reduce rescue risks
- Assist first responders
- Work continuously without fatigue

This project simulates such an intelligent rescue system.

---

# Project Goal

Create a realistic AI simulation where an autonomous drone learns to perform Search & Rescue missions inside procedurally generated disaster environments.

The AI should generalize instead of memorizing a single map.

---

# Core Technologies

 Technology  Purpose 
----------------------
 Unity 2022 LTS  Simulation Engine 
 C#  Drone Behaviour 
 Unity ML-Agents  Reinforcement Learning 
 PPO Algorithm  AI Training 
 Python  Model Training 
 ONNX  Trained AI Model 
 Git & GitHub  Version Control 

---

# Overall Architecture

```
                    ADRL Rescue

                    Game Manager
                         │
     ┌───────────────────┼───────────────────┐
     │                   │                   │
 Environment        Drone System       Training System
     │                   │                   │
     │                   │                   │
Procedural Map      Sensors         PPO Training
Victims             Memory          TensorBoard
Obstacles           Navigation      ONNX Export
Terrain             Flight Control
```

---

# Development Philosophy

The project follows a modular architecture.

Every system should have only one responsibility.

Instead of writing one huge script, each component performs one job.

Example

❌ Drone.cs (3000 lines)

Instead

✔ FlightController.cs

✔ DroneAgent.cs

✔ RewardSystem.cs

✔ ObservationSystem.cs

✔ SensorManager.cs

This makes debugging and future upgrades much easier.

---

# Main Systems

## 1. Game Manager

Responsible for

- Starting the simulation
- Ending episodes
- Resetting the environment
- Tracking game state

Think of it as the brain of the simulation.

---

## 2. Environment System

Responsible for generating the disaster environment.

It creates

- Terrain
- Buildings
- Trees
- Rocks
- Water
- Fire
- Debris
- Victims

Every episode should look slightly different.

---

## 3. Drone System

This is the autonomous robot.

The drone consists of multiple independent modules.

```
Drone

↓

Sensors

↓

Observation Processor

↓

Memory

↓

Decision Making (PPO)

↓

Flight Controller

↓

Physics
```

The drone never cheats.

It only knows what its sensors detect.

---

# Sensors

The drone uses multiple sensors.

## Ray Sensor

Detects nearby obstacles.

Examples

- Tree

- Building

- Rock

---

## Thermal Sensor

Detects body heat.

Used for victim detection.

---

## Vision Sensor

Determines whether victims are inside the drone's field of view.

---

## Collision Sensor

Detects crashes.

Used for negative rewards.

---

# Drone Memory

Instead of forgetting everything every frame, the drone remembers

- Visited locations
- Obstacles
- Victim locations
- Explored areas

This prevents repeatedly searching the same place.

---

# AI System

The AI uses PPO (Proximal Policy Optimization).

The neural network receives observations from the drone.

Example observations

- Drone Position
- Velocity
- Rotation
- Distance to Target
- Ray Sensor Data
- Thermal Sensor
- Vision Sensor

The AI predicts actions such as

- Move Forward
- Move Backward
- Move Left
- Move Right
- Ascend
- Descend
- Rotate

---

# Reward System

The AI learns through rewards.

Positive Rewards

✔ Finding victims

✔ Avoiding obstacles

✔ Efficient movement

✔ Completing rescue

Negative Rewards

✖ Collision

✖ Getting stuck

✖ Flying outside the map

✖ Wasting time

✖ Repeating explored paths

---

# Disaster Types

The same AI should work inside different disaster environments.

Current planned disasters

- Earthquake
- Flood
- Landslide
- Building Collapse

Each environment will be generated procedurally.

---

# Procedural Generation

Every episode should create a slightly different world.

Randomized elements

- Terrain
- Trees
- Buildings
- Rocks
- Fire
- Water
- Victim Position
- Drone Spawn Position

This prevents overfitting.

---

# Drone Behaviour

The drone should learn to

Search

↓

Explore

↓

Avoid Obstacles

↓

Locate Victims

↓

Navigate Efficiently

↓

Complete Rescue

No paths are hardcoded.

Everything is learned.

---

# Training Pipeline

```
Unity

↓

Environment

↓

Drone

↓

Observations

↓

PPO Trainer

↓

Neural Network

↓

Rewards

↓

Model Update

↓

Repeat
```

Eventually, the trained model is exported as an ONNX file.

---

# Future Features

These are intentionally postponed until the core project is complete.

- Multi-Agent Swarm Intelligence
- Drone Communication
- Battery Simulation
- Weather Simulation
- Wind Physics
- GPS Errors
- Camera Vision (YOLO)
- Google Maps Terrain
- ROS Integration

---

# Coding Standards

Every script should follow

- One responsibility only
- Clear naming
- Well-commented methods
- Modular design
- Reusable components

---

# Git Workflow

Every completed feature should be committed separately.

Example

```
Initial Project Setup

↓

Drone Physics

↓

ML-Agents Integration

↓

Reward System

↓

Procedural Terrain

↓

Victim System

↓

Training Complete
```

Never combine unrelated features into one commit.

---

# Project Principles

This project prioritizes

✔ Simplicity

✔ Scalability

✔ Maintainability

✔ Realistic AI Behaviour

✔ Modular Architecture

✔ Academic Quality

✔ Professional GitHub Structure

---

# Long-Term Vision

This project is designed to become more than a college project.

Future versions may include

- Swarm Intelligence
- Real-world GIS Integration
- Real Drone Deployment
- ROS Compatibility
- Advanced Computer Vision
- Multi-Agent Reinforcement Learning
- Research Publication

The architecture should always allow future expansion without requiring a complete rewrite.

---

# Final Philosophy

 The drone should not be programmed to rescue people.

 It should learn how to rescue people.