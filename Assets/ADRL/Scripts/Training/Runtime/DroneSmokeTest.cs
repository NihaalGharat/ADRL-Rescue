namespace ADRL.Training.Runtime
{
    using ADRL.AI.Agents;
    using ADRL.AI.Decision.Explainability;
    using ADRL.AI.Decision.Knowledge;
    using ADRL.AI.Rewards;
    using ADRL.Drone.Controllers;
    using UnityEngine;

    /// <summary>
    /// Automated heuristic smoke test. Since Phase 8.3 made the DecisionEngine the
    /// sole runtime decision authority, the smoke test now validates the autonomous
    /// decision path instead of a scripted action vector. It places a deterministic
    /// obstacle ahead of the spawned drone so sensors yield reliable fused readings,
    /// then confirms the decision framework assessed/selected/commanded and the full
    /// runtime pipeline (spawn -> controller -> agent -> DecisionEngine -> DroneCommand ->
    /// motor -> locomotion -> reward) is functional, so batch validation finishes
    /// within seconds.
    /// </summary>
    public sealed class DroneSmokeTest : MonoBehaviour
    {
        private const float MinMovement = 1f;
        private const float MaxDuration = 3f;
        private const float ProbeDistance = 2.5f;

        private DroneController _controller;
        private DroneAgent _agent;
        private Vector3 _startPosition;
        private float _elapsed;
        private float _maxDistanceFromOrigin;
        private bool _started;
        private bool _completed;
        private bool _sawAutonomousDecision;
        private bool _sawContextSnapshot;
        private bool _sawExecutionProfile;
        private bool _sawKnowledge;
        private bool _sawExplanation;
        private GameObject _probe;

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
            _sawContextSnapshot = false;
            _sawExecutionProfile = false;
            _sawKnowledge = false;
            _sawExplanation = false;

            PlaceDeterministicProbe();

            Debug.Log(
                $"[DroneSmokeTest] Started (agent={_agent != null}, obsDim={_agent?.ObservationDimension ?? 0}, engine={_agent?.Decision != null}).");
        }

        private void PlaceDeterministicProbe()
        {
            if (_agent == null || _controller == null)
                return;

            var forward = _controller.transform.forward;
            var probePos = _controller.transform.position + forward * ProbeDistance;
            var active = _agent.Decision != null && _agent.Fusion != null;

            if (!active)
                return;

            // A deterministic collider ahead of the drone guarantees the ray sensor
            // reports a proximity and liveness band, so the decision framework has
            // real fused input instead of an all-zero (idle) reading.
            _probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _probe.name = "[DroneSmokeProbe]";
            _probe.transform.position = probePos;
            _probe.transform.localScale = new Vector3(1f, 2f, 1f);

            var collider = _probe.GetComponent<Collider>();
            if (collider != null)
                _probe.tag = "Obstacle";
        }

        private void CleanupProbe()
        {
            if (_probe == null)
                return;

            Object.Destroy(_probe);
            _probe = null;
        }

        private void Update()
        {
            if (!_started || _completed)
                return;

            _elapsed += Time.deltaTime;

            if (_controller != null)
            {
                var distance = Vector3.Distance(_controller.transform.position, _startPosition);
                if (distance > _maxDistanceFromOrigin)
                    _maxDistanceFromOrigin = distance;
            }

            // Confirms the DecisionEngine ran a step and commanded non-idle output
            // at least once during the episode, proving the autonomous decision
            // authority feeds the actuator pipeline.
            if (_agent?.Decision != null && _agent.LastDecision.HasValue && !_agent.LastCommand.IsIdle)
                _sawAutonomousDecision = true;

            // Confirms the Phase 8.8 context snapshot was actually built by a
            // decision step (its runtime metadata advanced past the empty state).
            if (_agent?.Decision != null && _agent.Decision.LastSnapshot.RuntimeState.DecisionStep > 0)
                _sawContextSnapshot = true;

            // Confirms the Phase 8.9 execution optimizer ran on a real decision
            // step: the built snapshot carries an optimized execution profile
            // backed by a positive execution confidence.
            if (_agent?.Decision != null
                && _agent.Decision.LastSnapshot.ExecutionProfile.ExecutionConfidence > 0f)
                _sawExecutionProfile = true;

            // Confirms the Phase 9.0 world knowledge layer recorded something the
            // drone actually observed: the built snapshot carries a non-empty
            // knowledge store (the drone knows about the deterministic probe).
            if (_agent?.Decision != null
                && _agent.Decision.LastSnapshot.Knowledge.Count > 0)
                _sawKnowledge = true;

            // Confirms the Phase 9.1 explainability layer explained a real decision
            // step: the engine exposes a valid explanation whose step clock
            // advanced past the empty state. Observational only - it never gates the
            // smoke PASS criteria.
            if (_agent?.Decision != null
                && _agent.Decision.LastExplanation.IsValid
                && _agent.Decision.LastExplanation.DecisionStep > 0)
                _sawExplanation = true;

            if (_elapsed >= MaxDuration || HasPassed())
                Complete();
        }

        /// <summary>
        /// True once every stage of the autonomous runtime pipeline is proven: the
        /// controller exists, the drone actually moved, observations were generated,
        /// both sensor providers are fused, the DecisionEngine ran and produced a
        /// non-idle command, and the reward evaluator is active.
        /// </summary>
        private bool HasPassed()
        {
            var obsDim = _agent != null ? _agent.ObservationDimension : 0;
            var fusionCount = _agent?.Fusion != null ? _agent.Fusion.ProviderCount : 0;
            var observed = _agent != null && _agent.Fusion != null;
            var evaluatorPresent = _agent?.Evaluator != null;
            var enginePresent = _agent?.Decision != null;

            return _controller != null
                && _maxDistanceFromOrigin > MinMovement
                && obsDim > 0
                && fusionCount >= 2
                && observed
                && evaluatorPresent
                && enginePresent
                && _sawAutonomousDecision
                && _sawContextSnapshot
                && _sawExecutionProfile
                && _sawKnowledge;
        }

        private void Complete()
        {
            _started = false;
            _completed = true;

            CleanupProbe();

            var moved = _maxDistanceFromOrigin;
            var reward = _agent != null ? _agent.GetCumulativeReward() : 0f;
            var obsDim = _agent != null ? _agent.ObservationDimension : 0;
            var fusionCount = _agent?.Fusion != null ? _agent.Fusion.ProviderCount : 0;
            var state = _controller != null ? _controller.CurrentState.ToString() : "none";
            var evaluatorPresent = _agent?.Evaluator != null;
            var lastBehaviour = _agent?.LastDecision?.Behaviour.ToString() ?? "none";
            var lastMission = _agent?.Decision?.Mission?.CurrentTask.State.ToString() ?? "none";
            var lastWinningTask = _agent?.Decision?.GetDiagnostics().LastWinning.Task.State.ToString() ?? "none";
            var winningScore = _agent?.Decision?.GetDiagnostics().WinningPriorityScore ?? 0f;
            var selectedExecutor = _agent?.Decision?.GetDiagnostics().SelectedExecutor ?? "none";
            var candidateCount = _agent?.Decision?.GetDiagnostics().CandidateCount ?? 0;
            var decisionTimestamp = _agent?.Decision?.GetDiagnostics().DecisionTimestamp ?? 0f;
            var contextStep = _agent?.Decision?.LastSnapshot.RuntimeState.DecisionStep ?? 0;
            var execProfile = _agent?.Decision?.LastSnapshot.ExecutionProfile;
            var execConfidence = execProfile?.ExecutionConfidence ?? 0f;
            var execSpeed = execProfile?.SpeedMultiplier ?? 0f;
            var execTurn = execProfile?.TurnRateMultiplier ?? 0f;
            var optimizationTimestamp = _agent?.Decision?.LastSnapshot.RuntimeState.OptimizationTimestamp ?? 0f;
            var knowledgeRecords = _agent?.Decision?.LastSnapshot.Knowledge.Count ?? 0;
            var knownVictims = _agent?.Decision?.KnowledgeStore.CountOf(KnowledgeType.Victim) ?? 0;
            var knownHazards = _agent?.Decision?.KnowledgeStore.CountOf(KnowledgeType.Hazard) ?? 0;
            var knownObstacles = _agent?.Decision?.KnowledgeStore.CountOf(KnowledgeType.Obstacle) ?? 0;
            var nearestVictimDistance = _agent?.Decision?.GetDiagnostics().NearestVictimDistance ?? 0f;
            var nearestHazardDistance = _agent?.Decision?.GetDiagnostics().NearestHazardDistance ?? 0f;
            var knowledgeTimestamp = _agent?.Decision?.GetDiagnostics().KnowledgeTimestamp ?? 0f;

            Passed = HasPassed();

            Debug.Log(
                $"[DroneSmokeTest] PASSED={Passed} | moved={moved:F2}m | cumulativeReward={reward:F3} | " +
                $"state={state} | obsDim={obsDim} | fusedProviders={fusionCount} | evaluator={evaluatorPresent} | " +
                $"decisionSeen={_sawAutonomousDecision} | lastBehaviour={lastBehaviour} | lastMission={lastMission} | " +
                $"lastWinningTask={lastWinningTask} | winningScore={winningScore:F3} | " +
                $"selectedExecutor={selectedExecutor} | candidateCount={candidateCount} | decisionTimestamp={decisionTimestamp:F0} | " +
                $"contextObserved={_sawContextSnapshot} | contextStep={contextStep} | " +
                $"optimizationObserved={_sawExecutionProfile} | execConfidence={execConfidence:F3} | " +
                $"speedMult={execSpeed:F3} | turnRateMult={execTurn:F3} | optimizationTimestamp={optimizationTimestamp:F0} | " +
                $"knowledgeObserved={_sawKnowledge} | knowledgeRecords={knowledgeRecords} | " +
                $"knownVictims={knownVictims} | knownHazards={knownHazards} | knownObstacles={knownObstacles} | " +
                $"nearestVictimDistance={nearestVictimDistance:F2} | nearestHazardDistance={nearestHazardDistance:F2} | " +
                $"knowledgeTimestamp={knowledgeTimestamp:F0}");

            // Phase 9.1 explanation observation (observational only - it never
            // alters the PASS/FAIL exit code). Surfaces whether the engine built a
            // valid explanation and whether the deterministic formatter rendered it.
            var explanation = _agent?.Decision?.LastExplanation;
            var explanationWinner = explanation.HasValue ? explanation.Value.Winning.Task.State.ToString() : "none";
            var explanationBehaviour = explanation.HasValue ? explanation.Value.Behaviour.ToString() : "none";
            var explanationExecutor = explanation.HasValue ? explanation.Value.Executor : "none";
            var explanationCommand = explanation.HasValue
                ? (explanation.Value.Command.IsIdle ? "Idle" : "Active")
                : "none";
            var formatterOutput = explanation.HasValue
                ? DecisionExplanationFormatter.Format(explanation.Value)
                : string.Empty;
            var formatterValid = _sawExplanation
                && formatterOutput.Length > 0
                && formatterOutput.Contains("Winner")
                && formatterOutput.Contains(explanationBehaviour)
                && formatterOutput.Contains(explanationExecutor);
            var formatterLineCount = formatterOutput.Length == 0
                ? 0
                : formatterOutput.Split('\n').Length;

            Debug.Log(
                $"[DroneSmokeTest] explanationObserved={_sawExplanation} | explanationWinner={explanationWinner} | " +
                $"explanationBehaviour={explanationBehaviour} | explanationExecutor={explanationExecutor} | " +
                $"explanationCommand={explanationCommand} | formatterValid={formatterValid} | formatterLines={formatterLineCount}");
            Debug.Log(
                "[DroneSmokeTest] ExplanationFormatter | "
                + (formatterOutput.Length == 0
                    ? "(empty)"
                    : formatterOutput.Replace("\r", string.Empty).Replace("\n", " | ")));

            // Reward diagnostics (M5, Task 8). Observational only: it never alters
            // the pass/fail exit code, so existing smoke behaviour is preserved.
            // Surfaces reward regressions in CI output.
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
