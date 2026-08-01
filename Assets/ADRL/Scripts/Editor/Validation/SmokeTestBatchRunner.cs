namespace ADRL.Editor.Validation
{
    using ADRL.AI.Rewards;
    using ADRL.Training.Runtime;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    /// <summary>
    /// Batch-mode smoke test driver. Opens the Main scene, enters play mode, and
    /// polls until <see cref="RuntimeOrchestrator.SmokeTestPassed"/> becomes true
    /// or a timeout elapses. Exits the editor with code 0 on success and 1 on
    /// failure so it can gate CI/batch validation.
    /// </summary>
    /// <example>
    /// Unity.exe -batchmode -nographics -projectPath &lt;path&gt; \
    ///   -executeMethod ADRL.Editor.Validation.SmokeTestBatchRunner.Run
    /// </example>
    public static class SmokeTestBatchRunner
    {
        private const string ScenePath = "Assets/ADRL/Scenes/Main.unity";
        private const float ActivationTimeout = 15f;
        private const float CompletionTimeout = 20f;

        private static double _startTime;
        private static bool _running;
        private static int _pendingExitCode = -1;

        [MenuItem("ADRL/Smoke Test/Run Batch")]
        public static void Run()
        {
            if (_running)
                return;

            _running = true;
            _startTime = EditorApplication.timeSinceStartup;

            Debug.Log("[ADRL_SMOKE_TEST] Opening scene and entering play mode.");

            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.update += Poll;

            EditorApplication.isPlaying = true;
        }

        private static void Poll()
        {
            var elapsed = EditorApplication.timeSinceStartup - _startTime;

            if (RuntimeOrchestrator.SmokeTestPassed)
            {
                Finish(true, "DroneSmokeTest PASSED after " + elapsed.ToString("F1") + "s.");
                return;
            }

            if (elapsed > ActivationTimeout && !RuntimeOrchestrator.IsActivated)
            {
                Finish(false, "RuntimeOrchestrator did not activate within " +
                              ActivationTimeout + "s.");
                return;
            }

            if (elapsed > CompletionTimeout)
            {
                Finish(false, "Smoke test did not complete within " +
                              CompletionTimeout + "s.");
            }
        }

        private static void Finish(bool passed, string message)
        {
            EditorApplication.update -= Poll;
            _running = false;

            Debug.Log(
                "[ADRL_SMOKE_TEST] " + (passed ? "PASSED" : "FAILED") + ": " + message);
            Debug.Log(
                "[ADRL_SMOKE_TEST] Activated=" + RuntimeOrchestrator.IsActivated +
                " SmokeTestPassed=" + RuntimeOrchestrator.SmokeTestPassed);

            // Reward diagnostics (M5, Task 8). Observational only: it never alters
            // the pass/fail exit code, so existing smoke behaviour is preserved.
            // The breakdown is captured from the smoke drone and surfaces reward
            // regressions (e.g. reward == ~0.148) in CI output.
            if (RuntimeOrchestrator.TryGetRewardDiagnostics(out var breakdown))
                Debug.LogFormat(
                    "[ADRL_SMOKE_TEST] RewardDiagnostics | total={0:F4} | time={1:F4} | novelty={2:F4} | " +
                    "potential={3:F4} | stuck={4:F4} | oscillation={5:F4} | energy={6:F4} | outOfBounds={7:F4} | " +
                    "finite={8} | sumInvariant={9}",
                    breakdown.TotalReward,
                    breakdown.TimePenaltyReward,
                    breakdown.NoveltyReward,
                    breakdown.PotentialReward,
                    breakdown.StuckPenaltyReward,
                    breakdown.OscillationPenaltyReward,
                    breakdown.EnergyPenaltyReward,
                    breakdown.OutOfBoundsPenaltyReward,
                    IsBreakdownFinite(breakdown),
                    BreakdownSumApproximatesTotal(breakdown));

            _pendingExitCode = passed ? 0 : 1;
            EditorApplication.isPlaying = false;
            EditorApplication.update += ExitWhenIdle;
        }

        /// <summary>
        /// Exits play mode first, then quits the editor on the following frame.
        /// Calling EditorApplication.Exit while play mode is still active can hang
        /// a batch process.
        /// </summary>
        private static void ExitWhenIdle()
        {
            if (EditorApplication.isPlaying)
                return;

            EditorApplication.update -= ExitWhenIdle;
            EditorApplication.Exit(_pendingExitCode);
        }

        private static bool IsBreakdownFinite(RewardBreakdown breakdown)
        {
            var sum = breakdown.TimePenaltyReward + breakdown.NoveltyReward + breakdown.PotentialReward +
                      breakdown.StuckPenaltyReward + breakdown.OscillationPenaltyReward +
                      breakdown.CollisionPenaltyReward + breakdown.EnergyPenaltyReward +
                      breakdown.OutOfBoundsPenaltyReward + breakdown.VictimFoundReward +
                      breakdown.VictimRescuedReward + breakdown.SuccessReward;

            return !float.IsNaN(sum) && !float.IsInfinity(sum)
                && !float.IsNaN(breakdown.TotalReward) && !float.IsInfinity(breakdown.TotalReward);
        }

        private static bool BreakdownSumApproximatesTotal(RewardBreakdown breakdown)
        {
            var sum = breakdown.TimePenaltyReward + breakdown.NoveltyReward + breakdown.PotentialReward +
                      breakdown.StuckPenaltyReward + breakdown.OscillationPenaltyReward +
                      breakdown.CollisionPenaltyReward + breakdown.EnergyPenaltyReward +
                      breakdown.OutOfBoundsPenaltyReward + breakdown.VictimFoundReward +
                      breakdown.VictimRescuedReward + breakdown.SuccessReward;

            return Mathf.Abs(sum - breakdown.TotalReward) <= 1e-4f;
        }
    }
}
