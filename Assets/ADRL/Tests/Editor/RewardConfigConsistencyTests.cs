namespace ADRL.Tests.Editor.Configuration
{
    using System.Reflection;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using ADRL.Tests.Editor;
    using ADRL.Core.Configuration;

    /// <summary>
    /// Verifies the RewardConfig asset is consistent with the in-code defaults
    /// and that OnValidate restores invalid serialized values to their legal
    /// ranges.
    /// </summary>
    [TestFixture]
    internal sealed class RewardConfigConsistencyTests
    {
        private const string AssetPath = "Assets/ADRL/ScriptableObjects/Rewards/RewardConfig.asset";

        private RewardConfig _defaults;
        private RewardConfig _asset;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _defaults = TestUtilities.CreateRewardConfig();
            _asset = AssetDatabase.LoadAssetAtPath<RewardConfig>(AssetPath);
            Assert.IsNotNull(_asset, "RewardConfig asset not found at " + AssetPath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestUtilities.DestroySafe(_defaults);
        }

        [Test]
        public void AssetMatchesDefaultInstance()
        {
            Assert.AreEqual(_defaults.VictimFoundReward, _asset.VictimFoundReward);
            Assert.AreEqual(_defaults.VictimRescuedReward, _asset.VictimRescuedReward);
            Assert.AreEqual(_defaults.CollisionPenalty, _asset.CollisionPenalty);
            Assert.AreEqual(_defaults.TimePenalty, _asset.TimePenalty);
            Assert.AreEqual(_defaults.SuccessBonus, _asset.SuccessBonus);
            Assert.AreEqual(_defaults.OutOfBoundsPenalty, _asset.OutOfBoundsPenalty);
            Assert.AreEqual(_defaults.EnergyDepletedPenalty, _asset.EnergyDepletedPenalty);
            Assert.AreEqual(_defaults.NoveltyBonus, _asset.NoveltyBonus);
            Assert.AreEqual(_defaults.NoveltyCellSize, _asset.NoveltyCellSize);
            Assert.AreEqual(_defaults.ShapingEnabled, _asset.ShapingEnabled);
            Assert.AreEqual(_defaults.ShapingGamma, _asset.ShapingGamma);
            Assert.AreEqual(_defaults.ShapingScale, _asset.ShapingScale);
            Assert.AreEqual(_defaults.RewardScale, _asset.RewardScale);
            Assert.AreEqual(_defaults.MinStepReward, _asset.MinStepReward);
            Assert.AreEqual(_defaults.MaxStepReward, _asset.MaxStepReward);
            Assert.AreEqual(_defaults.StuckPenalty, _asset.StuckPenalty);
            Assert.AreEqual(_defaults.StuckDetectionWindow, _asset.StuckDetectionWindow);
            Assert.AreEqual(_defaults.StuckDistanceThreshold, _asset.StuckDistanceThreshold);
            Assert.AreEqual(_defaults.OscillationPenalty, _asset.OscillationPenalty);
            Assert.AreEqual(_defaults.OscillationDetectionWindow, _asset.OscillationDetectionWindow);
            Assert.AreEqual(_defaults.OscillationThreshold, _asset.OscillationThreshold);
            Assert.AreEqual(_defaults.DamagePenalty, _asset.DamagePenalty);
        }

        [Test]
        public void AssetFieldValues_MatchExpectedLiterals()
        {
            Assert.AreEqual(10f, _asset.VictimFoundReward, 0f);
            Assert.AreEqual(20f, _asset.VictimRescuedReward, 0f);
            Assert.AreEqual(-5f, _asset.CollisionPenalty, 0f);
            Assert.AreEqual(-0.01f, _asset.TimePenalty, 1e-9f);
            Assert.AreEqual(50f, _asset.SuccessBonus, 0f);
            Assert.AreEqual(-10f, _asset.OutOfBoundsPenalty, 0f);
            Assert.AreEqual(-15f, _asset.EnergyDepletedPenalty, 0f);
            Assert.AreEqual(0.05f, _asset.NoveltyBonus, 1e-9f);
            Assert.AreEqual(2f, _asset.NoveltyCellSize, 0f);
            Assert.IsTrue(_asset.ShapingEnabled);
            Assert.AreEqual(0.99f, _asset.ShapingGamma, 1e-9f);
            Assert.AreEqual(0.1f, _asset.ShapingScale, 1e-9f);
            Assert.AreEqual(1f, _asset.RewardScale, 0f);
            Assert.AreEqual(-0.1f, _asset.MinStepReward, 1e-9f);
            Assert.AreEqual(0.1f, _asset.MaxStepReward, 1e-9f);
            Assert.AreEqual(-0.5f, _asset.StuckPenalty, 1e-9f);
            Assert.AreEqual(2f, _asset.StuckDetectionWindow, 0f);
            Assert.AreEqual(0.1f, _asset.StuckDistanceThreshold, 1e-9f);
            Assert.AreEqual(-0.5f, _asset.OscillationPenalty, 1e-9f);
            Assert.AreEqual(2f, _asset.OscillationDetectionWindow, 0f);
            Assert.AreEqual(3f, _asset.OscillationThreshold, 0f);
            Assert.AreEqual(-2f, _asset.DamagePenalty, 0f);
        }

        [Test]
        public void OnValidate_ClampsInvalidFields()
        {
            var config = TestUtilities.CreateRewardConfig();

            TestUtilities.SetPrivateField(config, "_collisionPenalty", 5f);
            TestUtilities.SetPrivateField(config, "_timePenalty", 5f);
            TestUtilities.SetPrivateField(config, "_outOfBoundsPenalty", 5f);
            TestUtilities.SetPrivateField(config, "_energyDepletedPenalty", 5f);
            TestUtilities.SetPrivateField(config, "_stuckPenalty", 5f);
            TestUtilities.SetPrivateField(config, "_oscillationPenalty", 5f);
            TestUtilities.SetPrivateField(config, "_damagePenalty", 5f);
            TestUtilities.SetPrivateField(config, "_victimFoundReward", -3f);
            TestUtilities.SetPrivateField(config, "_victimRescuedReward", -3f);
            TestUtilities.SetPrivateField(config, "_noveltyBonus", -3f);
            TestUtilities.SetPrivateField(config, "_successBonus", -3f);
            TestUtilities.SetPrivateField(config, "_noveltyCellSize", 0f);
            TestUtilities.SetPrivateField(config, "_shapingGamma", 1.5f);
            TestUtilities.SetPrivateField(config, "_rewardScale", -1f);
            TestUtilities.SetPrivateField(config, "_minStepReward", 0.5f);
            TestUtilities.SetPrivateField(config, "_maxStepReward", 0.1f);
            TestUtilities.SetPrivateField(config, "_stuckDetectionWindow", 0.5f);
            TestUtilities.SetPrivateField(config, "_stuckDistanceThreshold", -0.2f);
            TestUtilities.SetPrivateField(config, "_oscillationThreshold", -5f);

            LogAssert.Expect(LogType.Error, "[RewardConfig] CollisionPenalty must be <= 0 (was 5); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] TimePenalty must be <= 0 (was 5); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] OutOfBoundsPenalty must be <= 0 (was 5); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] EnergyDepletedPenalty must be <= 0 (was 5); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] StuckPenalty must be <= 0 (was 5); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] OscillationPenalty must be <= 0 (was 5); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] DamagePenalty must be <= 0 (was 5); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] VictimFoundReward must be >= 0 (was -3); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] VictimRescuedReward must be >= 0 (was -3); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] NoveltyBonus must be >= 0 (was -3); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] SuccessBonus must be >= 0 (was -3); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] NoveltyCellSize must be positive (was 0); clamped to 0.01.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] ShapingGamma must be in [0, 1) (was 1.5); clamped to 0.99.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] RewardScale must be positive (was -1); clamped to 0.01.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] MinStepReward (0.5) must not exceed MaxStepReward (0.1); MinStepReward clamped to MaxStepReward.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] StuckDetectionWindow must be >= 1 (was 0.5); clamped to 1.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] StuckDistanceThreshold must be >= 0 (was -0.2); clamped to 0.");
            LogAssert.Expect(LogType.Error, "[RewardConfig] OscillationThreshold must be >= 0 (was -5); clamped to 0.");

            InvokeOnValidate(config);

            Assert.AreEqual(0f, config.CollisionPenalty, 0f);
            Assert.AreEqual(0f, config.TimePenalty, 0f);
            Assert.AreEqual(0f, config.OutOfBoundsPenalty, 0f);
            Assert.AreEqual(0f, config.EnergyDepletedPenalty, 0f);
            Assert.AreEqual(0f, config.StuckPenalty, 0f);
            Assert.AreEqual(0f, config.OscillationPenalty, 0f);
            Assert.AreEqual(0f, config.DamagePenalty, 0f);
            Assert.AreEqual(0f, config.VictimFoundReward, 0f);
            Assert.AreEqual(0f, config.VictimRescuedReward, 0f);
            Assert.AreEqual(0f, config.NoveltyBonus, 0f);
            Assert.AreEqual(0f, config.SuccessBonus, 0f);
            Assert.AreEqual(0.01f, config.NoveltyCellSize, 1e-9f);
            Assert.AreEqual(0.99f, config.ShapingGamma, 1e-9f);
            Assert.AreEqual(0.01f, config.RewardScale, 1e-9f);
            Assert.AreEqual(0.1f, config.MinStepReward, 1e-9f); // clamped to MaxStepReward
            Assert.AreEqual(0.1f, config.MaxStepReward, 1e-9f);
            Assert.AreEqual(1f, config.StuckDetectionWindow, 1e-9f);
            Assert.AreEqual(0f, config.StuckDistanceThreshold, 1e-9f);
            Assert.AreEqual(0f, config.OscillationThreshold, 1e-9f);

            Object.DestroyImmediate(config);
        }

        [Test]
        public void OnValidate_PreservesValidFields()
        {
            var config = TestUtilities.CreateRewardConfig();
            InvokeOnValidate(config);
            Assert.AreEqual(_defaults.VictimFoundReward, config.VictimFoundReward);
            Assert.AreEqual(_defaults.RewardScale, config.RewardScale);
            Assert.AreEqual(_defaults.ShapingGamma, config.ShapingGamma);
            Object.DestroyImmediate(config);
        }

        private static void InvokeOnValidate(RewardConfig config)
        {
            var method = typeof(RewardConfig).GetMethod(
                "OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "OnValidate not found; expected UNITY_EDITOR compilation.");
            method.Invoke(config, null);
        }
    }
}
