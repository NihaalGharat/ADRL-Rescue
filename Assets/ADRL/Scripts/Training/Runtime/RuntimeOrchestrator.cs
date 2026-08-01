namespace ADRL.Training.Runtime
{
    using ADRL.AI.Agents;
    using ADRL.AI.Rewards;
    using ADRL.Core.Bootstrap;
    using ADRL.Core.Events;
    using ADRL.Core.Resources;
    using ADRL.Drone.Core;
    using ADRL.Environment.Core;
    using ADRL.Environment.Procedural;
    using ADRL.Environment.Terrain;
    using UnityEngine;

    /// <summary>
    /// Composition root for runtime activation. Because the phase rules forbid
    /// editing <c>GameBootstrap</c> (Core) and the existing bootstrap singletons,
    /// this orchestrator lives in the training assembly and performs the activation
    /// that was previously dormant: it boots the environment and drone subsystems,
    /// registers the drone prefab, and spawns a drone for the smoke test.
    /// </summary>
    public static class RuntimeOrchestrator
    {
        private const string DronePrefabResourcePath = "Prefabs/Drone/Drone";
        private const string DefaultDroneType = "default";

        private static bool _activated;
        private static DroneSubsystem _subsystem;
        private static GameObject _root;
        private static DroneSmokeTest _smokeTest;

        /// <summary>The activated drone subsystem, or null when not active.</summary>
        public static DroneSubsystem Subsystem => _subsystem;

        public static bool IsActivated => _activated;

        public static bool SmokeTestPassed => _smokeTest != null && _smokeTest.Passed;

        /// <summary>
        /// Exposes the smoke test's current reward breakdown for editor-time
        /// validation (M5 Task 8). Read-only; never mutates runtime state.
        /// </summary>
        public static bool TryGetRewardDiagnostics(out RewardBreakdown breakdown)
        {
            if (_smokeTest != null && _smokeTest.HasEvaluator)
            {
                breakdown = _smokeTest.CurrentBreakdown;
                return true;
            }

            breakdown = default;
            return false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (_activated)
                return;

            var eventBus = GameBootstrap.EventBus;
            if (eventBus == null)
            {
                Debug.LogWarning(
                    "[RuntimeOrchestrator] GameBootstrap has not been initialized; activation skipped.");
                return;
            }

            Activate(eventBus);
        }

        private static void Activate(EventBus eventBus)
        {
            if (_activated)
                return;

            Debug.Log("[RuntimeOrchestrator] Activating runtime systems...");

            RegisterGeneratedSettings();

            EnvironmentBootstrap.Boot(CreateWorldSettings(), eventBus);
            LogEnvironmentState();

            _subsystem = new DroneSubsystem();
            _subsystem.Boot(eventBus);
            LogSubsystemState();

            if (_subsystem.Health != DroneSubsystemHealth.Healthy)
            {
                Debug.LogError(
                    $"[RuntimeOrchestrator] Drone subsystem did not reach Healthy (state: {_subsystem.Health}).");
                return;
            }

            RegisterDronePrefab();
            SpawnDrone();

            _activated = true;
        }

        private static void RegisterGeneratedSettings()
        {
            if (!ResourceLocator.IsInitialized)
                return;

            var configs = ResourceLocator.Configs;

            if (!configs.IsRegistered<TerrainSettings>())
            {
                var settings = ScriptableObject.CreateInstance<TerrainSettings>();
                configs.Register(settings);
                Debug.Log("[RuntimeOrchestrator] Generated and registered TerrainSettings.");
            }

            if (!configs.IsRegistered<GenerationSettings>())
            {
                var settings = ScriptableObject.CreateInstance<GenerationSettings>();
                configs.Register(settings);
                Debug.Log("[RuntimeOrchestrator] Generated and registered GenerationSettings.");
            }
        }

        private static WorldSettings CreateWorldSettings()
        {
            var settings = ScriptableObject.CreateInstance<WorldSettings>();
            ResourceLocator.Configs.Register(settings);
            return settings;
        }

        private static void RegisterDronePrefab()
        {
            if (_subsystem?.Services?.SpawnManager == null)
                return;

            var prefab = AssetProvider.Load<GameObject>(DronePrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError(
                    $"[RuntimeOrchestrator] Drone prefab not found at Resources path '{DronePrefabResourcePath}'.");
                return;
            }

            ResourceLocator.Prefabs.Register(PrefabCategory.Drone, DefaultDroneType, DronePrefabResourcePath);
            _subsystem.Services.SpawnManager.PrefabRegistry.Register(DefaultDroneType, prefab);
            Debug.Log($"[RuntimeOrchestrator] Registered drone prefab '{prefab.name}'.");
        }

        private static void SpawnDrone()
        {
            var spawnManager = _subsystem?.Services?.SpawnManager;
            if (spawnManager == null)
                return;

            var result = spawnManager.Spawn(
                new SpawnRequest(DefaultDroneType, string.Empty, string.Empty, string.Empty));

            if (!result.Success)
            {
                Debug.LogError($"[RuntimeOrchestrator] Drone spawn failed: {result.FailureReason}");
                return;
            }

            var agent = result.Drone != null
                ? result.Drone.GetComponent<ADRL.AI.Agents.DroneAgent>()
                : null;

            Debug.Log(
                $"[RuntimeOrchestrator] Drone spawned (id={result.AssignedDroneId}, state={result.Drone.CurrentState}, agent={agent != null}).");

            if (_root == null)
            {
                _root = new GameObject("[ADRL.RuntimeOrchestrator]");
                Object.DontDestroyOnLoad(_root);
            }

            _smokeTest = _root.GetComponent<DroneSmokeTest>();
            if (_smokeTest == null)
                _smokeTest = _root.AddComponent<DroneSmokeTest>();

            _smokeTest.Begin(result.Drone, agent);
        }

        private static void LogEnvironmentState()
        {
            var manager = EnvironmentBootstrap.EnvironmentManager;
            if (manager == null)
            {
                Debug.LogWarning("[RuntimeOrchestrator] Environment manager is null after boot.");
                return;
            }

            Debug.Log(
                $"[RuntimeOrchestrator] Environment ready (state={manager.State}, terrain={manager.TerrainGenerator != null}, victims={manager.Victims.Count}).");
        }

        private static void LogSubsystemState()
        {
            if (_subsystem == null)
                return;

            Debug.Log(
                $"[RuntimeOrchestrator] Drone subsystem health={_subsystem.Health}.");
        }
    }
}
