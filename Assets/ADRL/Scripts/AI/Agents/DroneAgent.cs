namespace ADRL.AI.Agents
{
    using ADRL.AI.DecisionMaking;
    using ADRL.AI.Rewards;
    using ADRL.Core.Configuration;
    using ADRL.Core.Resources;
    using ADRL.Drone.Controllers;
    using ADRL.Sensors.Detection;
    using ADRL.Sensors.Fusion;
    using ADRL.Sensors.Interfaces;
    using ADRL.Sensors.Raycasting;
    using Unity.MLAgents;
    using Unity.MLAgents.Actuators;
    using Unity.MLAgents.Policies;
    using Unity.MLAgents.Sensors;
    using UnityEngine;

    /// <summary>
    /// ML-Agents <see cref="Agent"/> that controls a <see cref="DroneController"/>.
    /// The drone's action space (4 continuous actions) and vector observation size
    /// are configured in <c>Awake</c> because the policy and observation sensors are
    /// created during <c>OnEnable</c>, before the user <c>Initialize</c> runs.
    /// </summary>
    /// <remarks>
    /// Vector observation layout:
    /// <list type="number">
    /// <item><description>Fused sensor values (ray proximity + victim flags, thermal
    /// presence + proximity).</description></item>
    /// <item><description>Energy percentage in [0, 1].</description></item>
    /// <item><description>Health percentage in [0, 1].</description></item>
    /// </list>
    /// </remarks>
    [RequireComponent(typeof(DroneController))]
    [RequireComponent(typeof(BehaviorParameters))]
    public sealed class DroneAgent : Agent
    {
        private const string DefaultBehaviorName = "ADRL_Drone";
        private const int DefaultRayCount = 12;

        [SerializeField]
        [Tooltip("When null the SensorConfig registered during bootstrap is used.")]
        private SensorConfig _sensorConfig;

        [SerializeField]
        [Tooltip("When null the RewardConfig registered during bootstrap is used.")]
        private RewardConfig _rewardConfig;

        [SerializeField]
        [Tooltip("When null the DroneConfig registered during bootstrap is used.")]
        private DroneConfig _droneConfig;

        [SerializeField]
        [Tooltip("Horizontal distance from the origin at which the agent is considered out of bounds.")]
        private float _outOfBoundsRadius = 120f;

        private DroneController _controller;
        private SensorFusionProvider _fusion;
        private DroneActionResolver _resolver;
        private RewardEvaluator _evaluator;
        private BehaviorParameters _behaviorParameters;
        private Vector3 _scriptedHeuristic;
        private bool _useScriptedHeuristic;

        public SensorFusionProvider Fusion => _fusion;
        public RewardEvaluator Evaluator => _evaluator;
        public DroneCommand LastCommand { get; private set; }

        /// <summary>Total size of the vector observation space.</summary>
        public int ObservationDimension
        {
            get
            {
                var fusionDim = _fusion != null ? _fusion.DimensionCount : 0;
                return fusionDim + 2;
            }
        }

        /// <summary>
        /// Fixed observation dimension computed from configuration only, so it can
        /// be used in <c>Awake</c> before the fusion provider is built.
        /// </summary>
        public int PredictedObservationDimension
        {
            get
            {
                var rayCount = GetSensorConfig()?.RayCount ?? DefaultRayCount;
                return Mathf.Max(1, rayCount) * 2 + 2 + 2;
            }
        }

        private void Awake()
        {
            _controller = GetComponent<DroneController>();
            _behaviorParameters = GetComponent<BehaviorParameters>();
            ConfigureBrain();

            var requester = GetComponent<DecisionRequester>();
            if (requester != null)
                requester.DecisionPeriod = 1;
        }

        /// <summary>
        /// Sets the behavior parameters that must be in place before the policy is
        /// generated. Because the ML-Agents policy and observation sensors are built
        /// during <c>OnEnable</c>, this must run in <c>Awake</c>.
        /// </summary>
        private void ConfigureBrain()
        {
            if (_behaviorParameters == null)
                return;

            _behaviorParameters.BehaviorName = DefaultBehaviorName;
            _behaviorParameters.BehaviorType = BehaviorType.HeuristicOnly;
            _behaviorParameters.BrainParameters.ActionSpec =
                ActionSpec.MakeContinuous(DroneActionResolver.ActionCount);
            _behaviorParameters.BrainParameters.VectorObservationSize =
                PredictedObservationDimension;
        }

        public override void Initialize()
        {
            _sensorConfig = GetSensorConfig();
            _rewardConfig = GetRewardConfig();
            _droneConfig = GetDroneConfig();

            if (_sensorConfig == null)
            {
                Debug.LogError("[DroneAgent] SensorConfig is missing; sensor readings will be empty.", this);
                return;
            }

            _fusion = new SensorFusionProvider();
            _fusion.Add(new DroneRaySensor(_sensorConfig));
            _fusion.Add(new DroneThermalSensor(_sensorConfig));

            _resolver = new DroneActionResolver();
            _evaluator = _rewardConfig != null
                ? new RewardEvaluator(_rewardConfig, this)
                : null;

            Debug.Assert(
                _fusion.DimensionCount + 2 == PredictedObservationDimension,
                "[DroneAgent] Fusion dimension does not match the configured observation size.",
                this);
        }

        public override void OnEpisodeBegin()
        {
            _useScriptedHeuristic = false;

            _fusion?.Reset();

            if (_controller == null)
            {
                _evaluator?.Reset(Vector3.zero);
                return;
            }

            // Do not call ResetDrone here: it returns the drone to the Registered
            // runtime state, from which Activate is not a valid transition (the
            // validator only allows Registered->Initializing->Idle->Active). The
            // controller keeps the state machine cycling Idle/Active/Emergency so
            // the next OnActionReceived can simply Activate again.
            if (_controller.CurrentState == DroneState.Emergency)
                _controller.Deactivate();

            _controller.Energy?.Reset();
            _controller.Health?.Reset();
            _evaluator?.Reset(_controller.transform.position);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            if (_controller == null || _fusion == null)
                return;

            var origin = _controller.transform;
            ISensorReading fused = _fusion.Fuse(origin);

            for (var i = 0; i < fused.DimensionCount; i++)
                sensor.AddObservation(fused.Values[i]);

            sensor.AddObservation(_controller.Energy != null
                ? _controller.Energy.EnergyPercentage
                : 0f);
            sensor.AddObservation(_controller.Health != null
                ? _controller.Health.HealthPercentage
                : 0f);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (_controller == null)
                return;

            if (_controller.CurrentState != DroneState.Active && _controller.Motor != null)
                _controller.Activate();

            var command = _resolver != null ? _resolver.Resolve(actions) : DroneCommand.Idle;
            LastCommand = command;

            ApplyCommand(command);

            _evaluator?.UpdateStep(
                Time.fixedDeltaTime,
                _controller.transform.position,
                command);

            if (_controller.Energy != null && _controller.Energy.IsDepleted)
            {
                _evaluator?.NotifyEnergyDepleted();
                EndEpisode();
                return;
            }

            if (IsOutOfBounds(_controller.transform.position))
            {
                _evaluator?.NotifyOutOfBounds();
                EndEpisode();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (actionsOut.ContinuousActions.Length < DroneActionResolver.ActionCount)
                return;

            var continuous = actionsOut.ContinuousActions;

            if (_useScriptedHeuristic)
            {
                SetContinuous(continuous, DroneActionResolver.IndexStrafe, _scriptedHeuristic.x);
                SetContinuous(continuous, DroneActionResolver.IndexForward, _scriptedHeuristic.z);
                SetContinuous(continuous, DroneActionResolver.IndexYaw, _scriptedHeuristic.y);
                SetContinuous(continuous, DroneActionResolver.IndexAltitude, 0f);
                return;
            }

            // Manual controls are not implemented yet. The project is configured
            // with the Input System package only, so the legacy Input API cannot
            // be used here; emit neutral actions until a manual-control path exists.
            SetContinuous(continuous, DroneActionResolver.IndexStrafe, 0f);
            SetContinuous(continuous, DroneActionResolver.IndexForward, 0f);
            SetContinuous(continuous, DroneActionResolver.IndexYaw, 0f);
            SetContinuous(continuous, DroneActionResolver.IndexAltitude, 0f);
        }

        /// <summary>
        /// Writes one action through the segment's backing array, because the
        /// segment indexer setter operates on a copy of the readonly struct.
        /// </summary>
        private static void SetContinuous(
            Unity.MLAgents.Actuators.ActionSegment<float> segment, int index, float value)
        {
            segment.Array[segment.Offset + index] = value;
        }

        /// <summary>
        /// Overrides the default (neutral) heuristic with a fixed action vector
        /// until the episode ends or <see cref="ClearScriptedHeuristic"/> is
        /// called. Used by the automated smoke test to demonstrate deterministic
        /// movement.
        /// </summary>
        public void SetScriptedHeuristic(Vector3 action)
        {
            _scriptedHeuristic = action;
            _useScriptedHeuristic = true;
        }

        /// <summary>True while the scripted heuristic override is active.</summary>
        public bool IsScriptedHeuristicActive => _useScriptedHeuristic;

        public void ClearScriptedHeuristic()
        {
            _useScriptedHeuristic = false;
        }

        public bool IsOutOfBounds(Vector3 position)
        {
            return new Vector2(position.x, position.z).magnitude > _outOfBoundsRadius;
        }

        private void ApplyCommand(DroneCommand command)
        {
            var motor = _controller.Motor;
            if (motor == null)
                return;

            if (command.IsIdle)
            {
                motor.Stop();
                return;
            }

            var rotationSpeed = _droneConfig != null ? _droneConfig.RotationSpeed : 90f;
            var maxSpeed = _droneConfig != null ? _droneConfig.MaxSpeed : 1f;

            motor.Move(command.MoveDirection, maxSpeed * command.MoveDirection.magnitude);

            if (Mathf.Abs(command.Yaw) > 0.001f)
            {
                var yawStep = command.Yaw * rotationSpeed * Time.fixedDeltaTime;
                var target = Quaternion.Euler(0f, yawStep, 0f) * _controller.transform.rotation;
                motor.Rotate(target, rotationSpeed);
            }
        }

        private SensorConfig GetSensorConfig()
        {
            if (_sensorConfig != null)
                return _sensorConfig;

            return ResourceLocator.IsInitialized
                ? ResourceLocator.Configs.Get<SensorConfig>()
                : null;
        }

        private RewardConfig GetRewardConfig()
        {
            if (_rewardConfig != null)
                return _rewardConfig;

            return ResourceLocator.IsInitialized
                ? ResourceLocator.Configs.Get<RewardConfig>()
                : null;
        }

        private DroneConfig GetDroneConfig()
        {
            if (_droneConfig != null)
                return _droneConfig;

            return ResourceLocator.IsInitialized
                ? ResourceLocator.Configs.Get<DroneConfig>()
                : null;
        }
    }
}
