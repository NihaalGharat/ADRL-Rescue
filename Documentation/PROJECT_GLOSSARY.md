# Project Glossary

---

## A

**Action Space**
The set of all possible actions an agent can take. In ADRL-Rescue, this includes movement (X, Y, Z) and rotation.

**Agent**
The autonomous entity that learns and acts. In this project, the drone is the agent.

**AI (Artificial Intelligence)**
Computer systems that can perform tasks that normally require human intelligence.

---

## B

**Batch Size**
The number of training samples used in one iteration of model training.

**Buffer Size**
The amount of experience stored for training. Larger buffers provide more diverse samples.

---

## C

**Collision Detection**
System that detects when the drone makes contact with obstacles.

**Continuous Action Space**
Actions that can take any value within a range (e.g., -1.0 to 1.0), as opposed to discrete choices.

**Curriculum Learning**
Training strategy that gradually increases difficulty over time.

---

## D

**Disaster Type**
The category of disaster environment (Earthquake, Flood, Landslide, Building Collapse).

**Drone Agent**
The main ML-Agents component that controls the drone's behavior.

**Drone Memory**
System that stores information about visited locations and detected objects.

---

## E

**Episode**
One complete run of the simulation, from start to finish. Each episode is a new attempt.

**Episode Length**
The number of steps (frames) in an episode.

**Exploration**
The process of visiting new, unvisited areas of the environment.

**Exploration vs Exploitation**
The balance between trying new actions (exploration) and using known good actions (exploitation).

---

## F

**FixedUpdate**
Unity's physics update method, called at a fixed interval (default 50Hz).

**Flight Controller**
Component that translates AI decisions into physics-based movement.

---

## G

**Gamma (γ)**
Discount factor for future rewards. Higher values (closer to 1.0) make the agent value future rewards more.

---

## H

**Heuristic**
A manual control mode for testing, allowing human input instead of AI decisions.

**Hidden Units**
The number of neurons in each hidden layer of the neural network.

---

## I

**Inference**
Using a trained model to make decisions without further training.

---

## L

**Lambda (λ)**
GAE (Generalized Advantage Estimation) parameter that controls bias-variance tradeoff.

**Learning Rate**
How much the model weights change during training. Too high causes instability, too low causes slow learning.

---

## M

**ML-Agents**
Unity's machine learning framework for training intelligent agents.

**Model**
The trained neural network that maps observations to actions.

---

## N

**Neural Network**
Computing system inspired by biological brains. Maps inputs (observations) to outputs (actions).

**Normalized**
Scaling values to a standard range (typically [0, 1] or [-1, 1]) for better training.

---

## O

**Observation**
Information the agent receives about the environment. Includes sensor data, position, and velocity.

**ONNX (Open Neural Network Exchange)**
Standard format for exporting trained models across frameworks.

**Obstacle**
Any object the drone must avoid (trees, buildings, debris).

---

## P

**Parallel Training**
Running multiple environment instances simultaneously to collect experience faster.

**Perlin Noise**
Mathematical function used for procedural terrain generation.

**PPO (Proximal Policy Optimization)**
The reinforcement learning algorithm used for training. Known for stability and efficiency.

**Procedural Generation**
Algorithmically creating content (terrain, obstacles) rather than manually designing it.

---

## R

**Ray Sensor**
Sensor that casts rays in multiple directions to detect obstacles and measure distances.

**Reward**
Feedback signal given to the agent after each action. Positive for good behavior, negative for bad.

**Reward Shaping**
Adding intermediate rewards to guide learning toward the final goal.

**Rigidbody**
Unity component that enables physics simulation (mass, drag, forces).

---

## S

**SAR (Search and Rescue)**
The primary mission of the drone: finding and helping disaster victims.

**Sensor Fusion**
Combining data from multiple sensors into a unified representation.

**Seed**
A value that determines random generation. Same seed = same environment.

**Step**
One iteration of the simulation loop. The agent observes, acts, and receives reward.

---

## T

**TensorBoard**
Visualization toolkit for monitoring training metrics (rewards, losses, etc.).

**Thermal Sensor**
Sensor that detects heat signatures, used for finding victims.

**Time Horizon**
How many steps the agent considers when calculating rewards.

**Training**
The process of improving the agent's behavior through experience.

---

## V

**Vision Sensor**
Sensor that detects objects within a field of view, used for victim confirmation.

**Victim**
A disaster survivor that the drone must locate and rescue.

---

## W

**World Position**
The position of an object in the game world coordinates (X, Y, Z).

---

## Quick Reference

| Term | Definition |
|------|-----------|
| PPO | Proximal Policy Optimization algorithm |
| ONNX | Model export format |
| ML-Agents | Unity's ML framework |
| Episode | One complete simulation run |
| Step | One iteration in an episode |
| Reward | Feedback signal for learning |
| Observation | Input to the neural network |
| Action | Output from the neural network |
| Policy | The neural network itself |
| Buffer | Experience storage for training |

---

## Navigation

| Document | Description |
|----------|-------------|
| [01_PROJECT_VISION](01_PROJECT_VISION.md) | Project goals |
| [06_AI_SYSTEM](06_AI_SYSTEM.md) | AI system details |
| [README](../README.md) | Project overview |

---

*Last updated: July 2026*
