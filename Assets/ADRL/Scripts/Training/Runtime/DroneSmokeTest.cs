namespace ADRL.Training.Runtime
{
    using ADRL.AI.Agents;
    using ADRL.AI.Rewards;
    using ADRL.Drone.Controllers;
    using UnityEngine;

    /// <summary>
    /// Automated heuristic smoke test. After the orchestrator spawns a drone it
    /// drives the agent with a scripted forward command and completes as soon as
    /// the full runtime pipeline (spawn -&gt; controller -&gt; agent -&gt; resolver -&gt;
    /// motor -&gt; locomotion -&gt; reward) is proven functional, so batch validation
    /// finishes within seconds rather than a fixed play session.
    /// </summary>
    public sealed class DroneSmokeTest : MonoBehaviour
    {
        private const float MinMovement = 1f;
        private const float MaxDuration = 3f;
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

        /// <summary>
        /// True when the smoke-test drone has a reward evaluator attached.
        /// Read-only diagnostic accessor used by editor-time validation.
        /// </summary>
        public bool HasEvaluator => _agent != null && _agent.Evaluator != null;

        /// <summary>
        /// Live reward breakdown for the running episode, or a default
        /// (zero) snapshot when no evaluator is present. Read-only; never
        /// mutates runtime state.
        /// </summary>
        public RewardBreakdown CurrentBreakdown => _agent?.Evaluator?.CurrentBreakdown ?? default;

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

            if (_elapsed >= MaxDuration || HasPassed())
                Complete();
        }

        /// <summary>
        /// True once every stage of the runtime pipeline is proven: the controller
        /// exists, the drone actually moved, observations were generated, both
        /// sensor providers are fused, and the reward evaluator is active.
        /// </summary>
        private bool HasPassed()
        {
            var obsDim = _agent != null ? _agent.ObservationDimension : 0;
            var fusionCount = _agent?.Fusion != null ? _agent.Fusion.ProviderCount : 0;
            var observed = _agent != null && _agent.Fusion != null;
            var evaluatorPresent = _agent?.Evaluator != null;

            return _controller != null
                && _maxDistanceFromOrigin > MinMovement
                && obsDim > 0
                && fusionCount >= 2
                && observed
                && evaluatorPresent;
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

            Passed = HasPassed();

            Debug.Log(
                $"[DroneSmokeTest] PASSED={Passed} | moved={moved:F2}m | cumulativeReward={reward:F3} | " +
                $"state={state} | obsDim={obsDim} | fusedProviders={fusionCount} | evaluator={evaluatorPresent}");

            // Reward diagnostics (M5, Task 8). Observational only: it never alters
            // the pass/fail exit code, so existing smoke behaviour is preserved.
            // Surfaces reward regressions (e.g. reward == ~0.148) in CI output.
            if (_agent?.Evaluator != null)
            {
                var bd = _agent.Evaluator.CurrentBreakdown;
                var sum = bd.TimePenaltyReward + bd.NoveltyReward + bd.PotentialReward +
                          bd.StuckPenaltyReward + bd.OscillationPenaltyReward +
                          bd.EnergyPenaltyReward +
                          bd.OutOfBoundsPenaltyReward + bd.VictimFoundReward +
                          bd.VictimRescuedReward + bd.SuccessReward;

                Debug.LogFormat(
                    "[ADRL_SMOKE_TEST] RewardDiagnostics | total={0:F4} | time={1:F4} | novelty={2:F4} | " +
                    "potential={3:F4} | stuck={4:F4} | oscillation={5:F4} | energy={6:F4} | outOfBounds={7:F4} | " +
                    "finite={8} | sumInvariant={9}",
                    bd.TotalReward,
                    bd.TimePenaltyReward,
                    bd.NoveltyReward,
                    bd.PotentialReward,
                    bd.StuckPenaltyReward,
                    bd.OscillationPenaltyReward,
                    bd.EnergyPenaltyReward,
                    bd.OutOfBoundsPenaltyReward,
                    !float.IsNaN(sum) && !float.IsInfinity(sum)
                        && !float.IsNaN(bd.TotalReward) && !float.IsInfinity(bd.TotalReward),
                    Mathf.Abs(sum - bd.TotalReward) <= 1e-4f);
            }

#if UNITY_EDITOR
            if (Application.isBatchMode)
                RequestEditorExit(Passed ? 0 : 1);
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Terminates a batch-mode editor with the given exit code. Batch mode
        /// does not pump <c>EditorApplication.update</c> while in play mode, so an
        /// editor-side poll loop can never finish; the game loop must request the
        /// exit itself. Never invoked during interactive play.
        /// </summary>
        private static void RequestEditorExit(int exitCode)
        {
            var editorApplicationType =
                System.Type.GetType("UnityEditor.EditorApplication, UnityEditor");
            if (editorApplicationType == null)
            {
                Debug.LogError(
                    "[DroneSmokeTest] Could not resolve UnityEditor.EditorApplication; batch exit skipped.");
                return;
            }

            var exit = editorApplicationType.GetMethod(
                "Exit",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            exit?.Invoke(null, new object[] { exitCode });
        }
#endif
    }
}
