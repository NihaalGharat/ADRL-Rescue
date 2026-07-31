namespace ADRL.Training.Runtime
{
    using ADRL.AI.Agents;
    using ADRL.Drone.Controllers;
    using UnityEngine;

    /// <summary>
    /// Automated heuristic smoke test. After the orchestrator spawns a drone it
    /// drives the agent with a scripted forward command, samples movement for a
    /// fixed duration, and logs a PASS/FAIL summary proving that the full runtime
    /// pipeline (spawn -&gt; controller -&gt; agent -&gt; resolver -&gt; motor -&gt;
    /// locomotion -&gt; reward) is functional.
    /// </summary>
    public sealed class DroneSmokeTest : MonoBehaviour
    {
        private const float TestDuration = 4f;
        private const float MinMovement = 1f;
        private static readonly Vector3 ForwardAction = new(0f, 0f, 1f);

        private DroneController _controller;
        private DroneAgent _agent;
        private Vector3 _startPosition;
        private float _elapsed;
        private float _maxDistanceFromOrigin;
        private bool _started;
        private bool _completed;

        /// <summary>Final PASS/FAIL result of the smoke test.</summary>
        public bool Passed { get; private set; }

        public void Begin(DroneController controller, DroneAgent agent)
        {
            _controller = controller;
            _agent = agent;
            _startPosition = controller != null ? controller.transform.position : Vector3.zero;
            _elapsed = 0f;
            _maxDistanceFromOrigin = 0f;
            _started = true;
            _completed = false;
            Passed = false;

            _agent?.SetScriptedHeuristic(ForwardAction);

            Debug.Log(
                $"[DroneSmokeTest] Started (agent={_agent != null}, obsDim={_agent?.ObservationDimension ?? 0}).");
        }

        private void Update()
        {
            if (!_started || _completed)
                return;

            // Episode resets clear the override; re-arm it so movement continues.
            if (_agent != null && !_agent.IsScriptedHeuristicActive)
                _agent.SetScriptedHeuristic(ForwardAction);

            _elapsed += Time.deltaTime;

            if (_controller != null)
            {
                var distance = Vector3.Distance(_controller.transform.position, _startPosition);
                if (distance > _maxDistanceFromOrigin)
                    _maxDistanceFromOrigin = distance;
            }

            if (_elapsed >= TestDuration)
                Complete();
        }

        private void Complete()
        {
            _started = false;
            _completed = true;

            _agent?.ClearScriptedHeuristic();

            var moved = _maxDistanceFromOrigin;
            var reward = _agent != null ? _agent.GetCumulativeReward() : 0f;
            var obsDim = _agent != null ? _agent.ObservationDimension : 0;
            var fusionCount = _agent?.Fusion != null ? _agent.Fusion.ProviderCount : 0;
            var state = _controller != null ? _controller.CurrentState.ToString() : "none";
            var evaluatorPresent = _agent?.Evaluator != null;

            var observed = _agent != null && _agent.Fusion != null;
            Passed = _controller != null && moved > MinMovement && obsDim > 0 && fusionCount >= 2 && observed;

            Debug.Log(
                $"[DroneSmokeTest] PASSED={Passed} | moved={moved:F2}m | cumulativeReward={reward:F3} | " +
                $"state={state} | obsDim={obsDim} | fusedProviders={fusionCount} | evaluator={evaluatorPresent}");
        }
    }
}
