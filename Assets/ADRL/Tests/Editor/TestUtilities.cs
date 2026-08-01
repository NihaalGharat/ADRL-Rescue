namespace ADRL.Tests.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ADRL.AI.DecisionMaking;
    using ADRL.AI.Rewards;
    using ADRL.Core.Configuration;
    using ADRL.Core.Events;
    using UnityEngine;

    /// <summary>
    /// Test-only helpers for the ADRL.Tests.Editor assembly. Internal so they
    /// remain invisible to runtime assemblies and preserve the dependency graph.
    /// </summary>
    internal static class TestUtilities
    {
        public static RewardConfig CreateRewardConfig() => UnityEngine.ScriptableObject.CreateInstance<RewardConfig>();

        public static SimulationConfig CreateSimulationConfig(bool autoStart = false)
        {
            var config = UnityEngine.ScriptableObject.CreateInstance<SimulationConfig>();
            SetPrivateField(config, "_autoStartSimulation", autoStart);
            return config;
        }

        public static void SetPrivateField(object instance, string name, object value)
        {
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
                throw new InvalidOperationException($"Field '{name}' not found on {instance.GetType()}.");
            field.SetValue(instance, value);
        }

        public static T GetPrivateField<T>(object instance, string name)
        {
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
                throw new InvalidOperationException($"Field '{name}' not found on {instance.GetType()}.");
            return (T)field.GetValue(instance);
        }

        public static void DestroySafe(UnityEngine.Object obj)
        {
            if (obj != null)
                UnityEngine.Object.DestroyImmediate(obj);
        }
    }

    /// <summary>
    /// A reward sink double that records every increment for assertions.
    /// </summary>
    internal sealed class FakeRewardSink : IRewardSink
    {
        public readonly List<float> Amounts = new List<float>();

        public int Count => Amounts.Count;

        public float Sum
        {
            get
            {
                float sum = 0f;
                foreach (var amount in Amounts)
                    sum += amount;
                return sum;
            }
        }

        public void AddReward(float amount) => Amounts.Add(amount);
    }

    /// <summary>
    /// Wires an <see cref="RewardEvaluator"/> to a fake sink and an isolated
    /// <see cref="EventBus"/> with default config for deterministic testing.
    /// </summary>
    internal sealed class TestHarness : IDisposable
    {
        public readonly RewardConfig Config;
        public readonly FakeRewardSink Sink;
        public readonly EventBus EventBus;
        public readonly RewardEvaluator Evaluator;
        private bool _disposed;

        public TestHarness(int droneId = 0, RewardConfig config = null)
        {
            Config = config ?? TestUtilities.CreateRewardConfig();
            Sink = new FakeRewardSink();
            EventBus = new EventBus();
            EventBus.Initialize();
            Evaluator = new RewardEvaluator(Config, Sink, EventBus, droneId);
            Evaluator.Reset(Vector3.zero);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Evaluator?.Dispose();
            TestUtilities.DestroySafe(Config);
        }
    }

    internal static class RewardTestCommands
    {
        public static readonly DroneCommand Idle = DroneCommand.Idle;
        public static readonly DroneCommand Forward = new DroneCommand(new Vector3(0f, 0f, 1f), 0f, false);
    }
}
