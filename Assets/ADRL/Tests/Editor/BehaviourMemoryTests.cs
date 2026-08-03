namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Memory;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;

    /// <summary>
    /// Verifies the Phase 8.5 Behaviour Memory layer: the bounded, immutable
    /// record store, the config-owned <see cref="MemoryPolicy"/>, and the
    /// <see cref="BehaviourMemoryService"/>'s single ownership of runtime updates
    /// (store, refresh, expire, confidence decay, clear). Also verifies that
    /// memory influences behaviour selection without ever overriding current
    /// perception, and that identical histories yield identical decisions.
    /// </summary>
    [TestFixture]
    internal sealed class BehaviourMemoryTests
    {
        private static readonly MemoryPolicy ShortPolicy = new(
            victimMemoryDuration: 3f,
            obstacleMemoryDuration: 2f,
            confidenceDecay: 0.25f,
            refreshThreshold: 5f,
            maxRecords: 4);

        private static readonly DecisionContext DefaultContext = DecisionContext.Default;

        private static ISensorReading VictimReading() => new SensorReading(new[]
        {
            0.7f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        });

        private static ISensorReading ObstacleReading() => new SensorReading(new[]
        {
            0f, 0f, 0f, 0f, 0f, 0f, 0.9f, 0f, 0f, 0f, 0f, 0f,
        });

        private static ISensorReading NeutralReading() => new SensorReading(new[]
        {
            0.1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
        });

        private static DecisionEngine CreateEngine(MemoryPolicy policy)
        {
            return new DecisionEngine(
                DefaultContext,
                new FogOfWarSituationAssessor(DefaultContext),
                new BehaviourSelector(DefaultContext),
                policy);
        }

        [Test]
        public void StoreVictimMemory_StoresVictimRecord()
        {
            var service = new BehaviourMemoryService(ShortPolicy);

            service.Update(VictimAssessment(), 0f);

            var record = service.LastVictimSeen;
            Assert.IsTrue(record.IsValid);
            Assert.AreEqual(BehaviourState.Approach, record.Behaviour);
        }

        [Test]
        public void RetrieveVictimMemory_ReturnsStoredRecord()
        {
            var memory = new BehaviourMemory(4);

            memory.Store(BehaviourState.Approach, 2f);

            var record = memory.Retrieve(BehaviourState.Approach);
            Assert.IsTrue(record.IsValid);
            Assert.AreEqual(2f, record.Timestamp);
            Assert.AreEqual(BehaviourState.Approach, record.Behaviour);
        }

        [Test]
        public void StoreObstacleMemory_StoresObstacleRecord()
        {
            var service = new BehaviourMemoryService(ShortPolicy);

            service.Update(ObstacleAssessment(), 0f);

            var record = service.LastObstacleSeen;
            Assert.IsTrue(record.IsValid);
            Assert.AreEqual(BehaviourState.Avoid, record.Behaviour);
        }

        [Test]
        public void Expiration_RemovesStaleMemory()
        {
            var service = new BehaviourMemoryService(ShortPolicy);
            service.Update(VictimAssessment(), 0f);
            Assert.IsTrue(service.LastVictimSeen.IsValid);

            service.Update(NeutralAssessment(), ShortPolicy.VictimMemoryDuration + 1f);

            Assert.IsFalse(service.LastVictimSeen.IsValid, "Memory must expire past its configured duration.");
        }

        [Test]
        public void Refresh_ExtendsLifetime()
        {
            var service = new BehaviourMemoryService(ShortPolicy);

            service.Update(VictimAssessment(), 0f);
            var first = service.LastVictimSeen.Timestamp;

            service.Update(VictimAssessment(), 1f);

            var second = service.LastVictimSeen;
            Assert.IsTrue(second.IsValid);
            Assert.AreNotEqual(first, second.Timestamp, "Re-observation must refresh the record timestamp.");
        }

        [Test]
        public void Clear_RemovesAllRecords()
        {
            var service = new BehaviourMemoryService(ShortPolicy);
            service.Update(VictimAssessment(), 0f);
            service.Update(ObstacleAssessment(), 0f);
            Assert.AreEqual(2, service.Memory.Count);

            service.Reset();

            Assert.AreEqual(0, service.Memory.Count);
            Assert.IsFalse(service.LastVictimSeen.IsValid);
            Assert.IsFalse(service.LastObstacleSeen.IsValid);
        }

        [Test]
        public void Memory_NeverExceedsConfiguredCapacity()
        {
            var memory = new BehaviourMemory(2);

            memory.Store(BehaviourState.Approach, 0f);
            memory.Store(BehaviourState.Avoid, 1f);
            memory.Store(BehaviourState.Approach, 2f);
            memory.Store(BehaviourState.Avoid, 3f);

            Assert.LessOrEqual(memory.Count, memory.Capacity);
            Assert.AreEqual(2, memory.Count);
        }

        [Test]
        public void SameHistory_ProducesIdenticalDecisions()
        {
            var a = CreateEngine(ShortPolicy);
            var b = CreateEngine(ShortPolicy);

            var history = new[] { VictimReading(), NeutralReading(), ObstacleReading(), NeutralReading() };

            foreach (var reading in history)
            {
                var ra = a.Decide(reading);
                var rb = b.Decide(reading);

                Assert.AreEqual(ra.Behaviour, rb.Behaviour);
                Assert.AreEqual(ra.Command.MoveDirection, rb.Command.MoveDirection);
                Assert.AreEqual(ra.Command.Yaw, rb.Command.Yaw);
                Assert.AreEqual(ra.Assessment.TargetProximity, rb.Assessment.TargetProximity);
            }
        }

        [Test]
        public void ExpiredMemory_NoLongerInfluencesBehaviour()
        {
            var engine = CreateEngine(ShortPolicy);

            var approached = engine.Decide(VictimReading());
            Assert.AreEqual(BehaviourState.Approach, approached.Behaviour);

            // Victim memory (duration 3, inclusive) keeps Approach alive while fresh:
            // steps 1, 2 and 3 all still age within the configured duration.
            Assert.AreEqual(BehaviourState.Approach, engine.Decide(NeutralReading()).Behaviour);
            Assert.AreEqual(BehaviourState.Approach, engine.Decide(NeutralReading()).Behaviour);
            Assert.AreEqual(BehaviourState.Approach, engine.Decide(NeutralReading()).Behaviour);

            // Step 4 ages past the duration: the expired memory no longer influences
            // behaviour, so neutral perception resumes Search.
            Assert.AreEqual(BehaviourState.Search, engine.Decide(NeutralReading()).Behaviour);
        }

        [Test]
        public void CurrentPerception_OverridesRememberedPerception()
        {
            var engine = CreateEngine(ShortPolicy);

            var approached = engine.Decide(VictimReading());
            Assert.AreEqual(BehaviourState.Approach, approached.Behaviour);

            // An imminent obstacle in the current reading must win over the
            // remembered victim: current perception always has priority.
            var avoided = engine.Decide(ObstacleReading());

            Assert.AreEqual(BehaviourState.Avoid, avoided.Behaviour);
        }

        [Test]
        public void VictimContinuity_KeepsApproaching_WhileMemoryFresh()
        {
            var engine = CreateEngine(ShortPolicy);

            var first = engine.Decide(VictimReading());
            Assert.AreEqual(BehaviourState.Approach, first.Behaviour);

            // Victim disappears but memory is still fresh: continue Approach.
            var continued = engine.Decide(NeutralReading());
            Assert.AreEqual(BehaviourState.Approach, continued.Behaviour);
        }

        private static SituationSnapshot VictimAssessment() =>
            new(true, 0.7f, 0f, 0f, 0f, true);

        private static SituationSnapshot ObstacleAssessment() =>
            new(false, 0f, 0f, 0.9f, 0.8f, true);

        private static SituationSnapshot NeutralAssessment() =>
            new(false, 0f, 0f, 0.1f, 0f, true);
    }
}
