namespace ADRL.Tests.Editor.Decision
{
    using ADRL.AI.Decision;
    using ADRL.AI.Decision.Context;
    using ADRL.AI.Decision.Knowledge;
    using ADRL.AI.Decision.Memory;
    using ADRL.Sensors;
    using ADRL.Sensors.Interfaces;
    using NUnit.Framework;
    using UnityEngine;

    /// <summary>
    /// Verifies the Phase 9.0 Autonomous World Knowledge Framework: the
    /// <see cref="WorldKnowledgeStore"/> is the single owner of persistent knowledge
    /// storage, the <see cref="KnowledgeUpdater"/> is the single owner of knowledge
    /// writes (insert/refresh/merge/expire/confidence), the
    /// <see cref="IKnowledgeQuery"/>/<see cref="KnowledgeQuery"/> read only, and the
    /// <see cref="DecisionEngine"/> embeds the store into the per-step
    /// <see cref="DecisionContextSnapshot"/> with synchronized diagnostics and
    /// runtime metadata. Knowledge is read-only for consumers and write-only through
    /// the updater; nothing here performs decisions, scoring or selection.
    /// </summary>
    [TestFixture]
    internal sealed class WorldKnowledgeTests
    {
        private static WorldKnowledgeRecord Record(
            KnowledgeType type,
            Vector3 position,
            float confidence = 1f,
            int timestamp = 0,
            int age = 0,
            string source = "Test")
        {
            return new WorldKnowledgeRecord(type, position, confidence, timestamp, age, source, true);
        }

        private static SituationSnapshot TargetSituation(float proximity, float side = 0f)
        {
            return new SituationSnapshot(true, proximity, side, 0f, 0f, true);
        }

        private static SituationSnapshot ObstacleSituation(float proximity)
        {
            return new SituationSnapshot(false, 0f, 0f, proximity, 0f, true);
        }

        private static ISensorReading ObstacleReading(float proximity) => new SensorReading(new[]
        {
            0f, 0f, 0f, 0f, 0f, 0f, proximity, 0f, 0f, 0f, 0f, 0f,
        });

        private static DecisionEngine CreateEngine()
        {
            return new DecisionEngine(
                DecisionContext.Default,
                new FogOfWarSituationAssessor(DecisionContext.Default),
                new BehaviourSelector(DecisionContext.Default));
        }

        [Test]
        public void Store_InsertsVictim()
        {
            var store = new WorldKnowledgeStore(KnowledgePolicy.Default);
            store.Store(Record(KnowledgeType.Victim, new Vector3(0f, 0f, 10f)));

            Assert.AreEqual(1, store.Count);
            Assert.IsTrue(store.Contains(KnowledgeType.Victim));
            var victims = store.QueryAll(KnowledgeType.Victim);
            Assert.AreEqual(1, victims.Length);
            Assert.AreEqual(KnowledgeType.Victim, victims[0].Type);
            Assert.AreEqual(10f, victims[0].Position.z, 1e-6f);
            Assert.IsTrue(victims[0].IsValid);
        }

        [Test]
        public void Store_InsertsObstacle()
        {
            var store = new WorldKnowledgeStore(KnowledgePolicy.Default);
            store.Store(Record(KnowledgeType.Obstacle, new Vector3(0f, 0f, 20f)));

            Assert.AreEqual(1, store.Count);
            Assert.IsTrue(store.Contains(KnowledgeType.Obstacle));
            Assert.AreEqual(1, store.QueryAll(KnowledgeType.Obstacle).Length);
            Assert.AreEqual(0, store.QueryAll(KnowledgeType.Victim).Length);
        }

        [Test]
        public void Store_InsertsHazard()
        {
            var store = new WorldKnowledgeStore(KnowledgePolicy.Default);
            store.Store(Record(KnowledgeType.Hazard, new Vector3(0f, 0f, 30f)));

            Assert.AreEqual(1, store.Count);
            Assert.IsTrue(store.Contains(KnowledgeType.Hazard));
            Assert.AreEqual(1, store.QueryAll(KnowledgeType.Hazard).Length);
            Assert.AreEqual(KnowledgeType.Hazard, store.QueryNearest(KnowledgeType.Hazard, Vector3.zero).Type);
        }

        [Test]
        public void MergeDuplicate_MergesWithinDistance()
        {
            var updater = new KnowledgeUpdater(KnowledgePolicy.Default);
            updater.Update(TargetSituation(0.90f), BehaviourMemory.Empty, 0f);
            updater.Update(TargetSituation(0.92f), BehaviourMemory.Empty, 1f);

            Assert.AreEqual(1, updater.Store.Count);
            var victim = updater.Store.QueryNearest(KnowledgeType.Victim, Vector3.zero);
            Assert.IsTrue(victim.IsValid);
            Assert.AreEqual(46f, victim.Position.z, 1e-6f);
            Assert.AreEqual(1, victim.Timestamp);
        }

        [Test]
        public void Expire_RemovesStaleRecords()
        {
            var store = new WorldKnowledgeStore(KnowledgePolicy.Default);
            store.Store(Record(KnowledgeType.Victim, new Vector3(0f, 0f, 10f)));
            store.Store(Record(KnowledgeType.Obstacle, new Vector3(0f, 0f, 20f)));

            store.Expire(10f);

            Assert.IsTrue(store.Contains(KnowledgeType.Victim));
            Assert.IsFalse(store.Contains(KnowledgeType.Obstacle));
        }

        [Test]
        public void Remove_RemovesRecord()
        {
            var store = new WorldKnowledgeStore(KnowledgePolicy.Default);
            store.Store(Record(KnowledgeType.Hazard, new Vector3(0f, 0f, 5f)));

            Assert.IsTrue(store.Remove(KnowledgeType.Hazard, new Vector3(0f, 0f, 5f)));
            Assert.AreEqual(0, store.Count);
            Assert.IsFalse(store.Contains(KnowledgeType.Hazard));
        }

        [Test]
        public void Capacity_EvictsOldest()
        {
            var policy = new KnowledgePolicy(
                maximumRecords: 3,
                victimLifetime: 12, obstacleLifetime: 8, hazardLifetime: 8, exploredLifetime: 20,
                mergeDistance: 2f, minimumConfidence: 0.1f, confidenceDecay: 0.1f,
                detectionDistance: 50f, sweepSpread: 10f, hazardProximityThreshold: 0.65f,
                corroborationWindow: 3f, corroborationBoost: 0.25f);
            var store = new WorldKnowledgeStore(policy);
            store.Store(Record(KnowledgeType.Victim, new Vector3(0f, 0f, 10f), timestamp: 0));
            store.Store(Record(KnowledgeType.Obstacle, new Vector3(0f, 0f, 20f), timestamp: 1));
            store.Store(Record(KnowledgeType.Hazard, new Vector3(0f, 0f, 30f), timestamp: 2));
            Assert.AreEqual(3, store.Count);

            store.Store(Record(KnowledgeType.Victim, new Vector3(0f, 0f, 40f), timestamp: 3));

            Assert.AreEqual(3, store.Count);
            var all = store.QueryAll();
            Assert.IsFalse(System.Array.Exists(all, r => r.Position.z == 10f));
            Assert.IsTrue(System.Array.Exists(all, r => r.Position.z == 40f));
            Assert.IsTrue(System.Array.Exists(all, r => r.Position.z == 20f));
            Assert.IsTrue(System.Array.Exists(all, r => r.Position.z == 30f));
        }

        [Test]
        public void QueryNearest_ReturnsNearestVictim()
        {
            var store = new WorldKnowledgeStore(KnowledgePolicy.Default);
            store.Store(Record(KnowledgeType.Victim, new Vector3(0f, 0f, 10f)));
            store.Store(Record(KnowledgeType.Victim, new Vector3(0f, 0f, 30f)));

            var nearest = store.QueryNearest(KnowledgeType.Victim, Vector3.zero);

            Assert.IsTrue(nearest.IsValid);
            Assert.AreEqual(10f, nearest.Position.z, 1e-6f);
        }

        [Test]
        public void QueryNearest_ReturnsNearestHazard()
        {
            var store = new WorldKnowledgeStore(KnowledgePolicy.Default);
            store.Store(Record(KnowledgeType.Hazard, new Vector3(0f, 0f, 50f)));
            store.Store(Record(KnowledgeType.Victim, new Vector3(0f, 0f, 1f)));
            store.Store(Record(KnowledgeType.Hazard, new Vector3(0f, 0f, 20f)));

            var nearest = store.QueryNearest(KnowledgeType.Hazard, Vector3.zero);

            Assert.IsTrue(nearest.IsValid);
            Assert.AreEqual(20f, nearest.Position.z, 1e-6f);
        }

        [Test]
        public void Query_EmptyReturnsInvalid()
        {
            var store = new WorldKnowledgeStore(KnowledgePolicy.Default);

            Assert.AreEqual(0, store.Count);
            Assert.IsFalse(store.Contains(KnowledgeType.Victim));
            Assert.IsFalse(store.QueryNearest(KnowledgeType.Victim, Vector3.zero).IsValid);
            Assert.AreEqual(0, store.QueryAll().Length);
            Assert.AreEqual(0, store.QueryAll(KnowledgeType.Hazard).Length);
            Assert.AreEqual(WorldKnowledgeRecord.Invalid, store.QueryNearest(KnowledgeType.Obstacle, Vector3.zero));
        }

        [Test]
        public void Confidence_UpdateOnRefresh()
        {
            var updater = new KnowledgeUpdater(KnowledgePolicy.Default);
            updater.Update(TargetSituation(0.90f), BehaviourMemory.Empty, 0f);
            updater.Update(TargetSituation(0.90f), BehaviourMemory.Empty, 5f);

            var victim = updater.Store.QueryNearest(KnowledgeType.Victim, Vector3.zero);

            Assert.AreEqual(1f, victim.Confidence, 1e-6f);
            Assert.AreEqual(5, victim.Timestamp);
            Assert.AreEqual(0, victim.Age);
        }

        [Test]
        public void Confidence_DecayRemovesLowConfidence()
        {
            var store = new WorldKnowledgeStore(KnowledgePolicy.Default);
            store.Store(Record(KnowledgeType.Victim, new Vector3(0f, 0f, 10f), confidence: 0.12f));
            store.Store(Record(KnowledgeType.Obstacle, new Vector3(0f, 0f, 20f), confidence: 1f));

            store.Expire(1f);

            Assert.AreEqual(1, store.Count);
            Assert.IsFalse(store.Contains(KnowledgeType.Victim));
            Assert.IsTrue(store.Contains(KnowledgeType.Obstacle));
            var obstacle = store.QueryNearest(KnowledgeType.Obstacle, Vector3.zero);
            Assert.AreEqual(0.9f, obstacle.Confidence, 1e-6f);
        }

        [Test]
        public void Ordering_Deterministic()
        {
            var updater = new KnowledgeUpdater(KnowledgePolicy.Default);
            updater.Update(TargetSituation(0.90f), BehaviourMemory.Empty, 0f);
            updater.Update(ObstacleSituation(0.80f), BehaviourMemory.Empty, 1f);

            var a = updater.Store.QueryAll();
            var b = updater.Store.QueryAll();

            Assert.AreEqual(a.Length, b.Length);
            for (var i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i].Type, b[i].Type);
                Assert.AreEqual(a[i].Position.z, b[i].Position.z, 1e-6f);
                Assert.AreEqual(a[i].Timestamp, b[i].Timestamp);
                Assert.AreEqual(a[i].Confidence, b[i].Confidence, 1e-6f);
            }
        }

        [Test]
        public void Snapshot_IntegratesKnowledge()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var snapshot = engine.LastSnapshot;

            Assert.IsNotNull(snapshot.Knowledge);
            Assert.IsTrue(snapshot.Knowledge.Count > 0);
            Assert.AreSame(snapshot.Knowledge, engine.KnowledgeStore);
        }

        [Test]
        public void Diagnostics_SynchronizedWithKnowledge()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var diagnostics = engine.GetDiagnostics();
            var snapshot = engine.LastSnapshot;

            Assert.AreEqual(snapshot.Knowledge.Count, diagnostics.KnowledgeRecordCount);
            Assert.AreEqual(diagnostics.KnowledgeRecordCount, snapshot.RuntimeState.KnowledgeRecordCount);
            Assert.AreEqual(engine.KnowledgeStore.Count, diagnostics.KnowledgeRecordCount);
            Assert.AreEqual(snapshot.RuntimeState.DecisionTimestamp, diagnostics.KnowledgeTimestamp, 1e-6f);
            Assert.Greater(diagnostics.NearestHazardDistance, 0f);
        }

        [Test]
        public void KnowledgeUpdater_StoresFromAssessment()
        {
            var updater = new KnowledgeUpdater(KnowledgePolicy.Default);
            updater.Update(TargetSituation(0.90f), BehaviourMemory.Empty, 0f);

            Assert.AreEqual(1, updater.Store.Count);
            Assert.IsTrue(updater.Store.Contains(KnowledgeType.Victim));
            var victim = updater.Store.QueryNearest(KnowledgeType.Victim, Vector3.zero);
            Assert.AreEqual(45f, victim.Position.z, 1e-6f);
            Assert.AreEqual("Assessment", victim.Source);
        }

        [Test]
        public void KnowledgeQuery_ReturnsKnownLists()
        {
            var updater = new KnowledgeUpdater(KnowledgePolicy.Default);
            updater.Update(TargetSituation(0.90f), BehaviourMemory.Empty, 0f);
            updater.Update(ObstacleSituation(0.80f), BehaviourMemory.Empty, 1f);

            var query = new KnowledgeQuery(updater.Store);

            Assert.AreEqual(1, query.KnownVictims().Length);
            Assert.AreEqual(0, query.KnownObstacles().Length);
            Assert.AreEqual(1, query.KnownHazards().Length);
            Assert.AreEqual(0, query.KnownRegions().Length);
            Assert.IsTrue(query.NearestVictim(Vector3.zero).IsValid);
            Assert.IsTrue(query.NearestHazard(Vector3.zero).IsValid);
            Assert.IsFalse(query.NearestObstacle(Vector3.zero).IsValid);

            updater.Store.Store(Record(KnowledgeType.ExploredRegion, Vector3.zero));
            Assert.AreEqual(1, query.KnownRegions().Length);
        }

        [Test]
        public void Engine_IntegrationKnowledge()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));

            var snapshot = engine.LastSnapshot;
            var state = snapshot.RuntimeState;

            Assert.IsTrue(snapshot.Knowledge.Contains(KnowledgeType.Hazard));
            Assert.AreEqual(snapshot.Knowledge.Count, state.KnowledgeRecordCount);
            Assert.GreaterOrEqual(state.KnowledgeRecordCount, 1);
            Assert.AreEqual(state.KnownHazards, snapshot.Knowledge.CountOf(KnowledgeType.Hazard));
            Assert.AreEqual(state.KnowledgeTimestamp, state.DecisionTimestamp, 1e-6f);
        }

        [Test]
        public void Reset_ClearsKnowledge()
        {
            var engine = CreateEngine();
            engine.Decide(ObstacleReading(0.9f));
            Assert.IsTrue(engine.LastSnapshot.Knowledge.Count > 0);

            engine.Reset();

            Assert.AreEqual(0, engine.KnowledgeStore.Count);
            Assert.AreEqual(0, engine.LastSnapshot.Knowledge.Count);
            Assert.AreEqual(DecisionContextSnapshot.Empty, engine.LastSnapshot);
        }

        [Test]
        public void Validator_StoreInvariantsHold()
        {
            var updater = new KnowledgeUpdater(KnowledgePolicy.Default);
            updater.Update(TargetSituation(0.90f), BehaviourMemory.Empty, 0f);
            updater.Update(ObstacleSituation(0.80f), BehaviourMemory.Empty, 1f);

            var store = updater.Store;
            var all = store.QueryAll();

            Assert.AreEqual(store.Count, all.Length);
            Assert.LessOrEqual(store.Count, store.Capacity);
            Assert.IsTrue(System.Array.TrueForAll(all, r => r.IsValid));
            Assert.IsTrue(System.Array.TrueForAll(all, r => r.Confidence >= 0f && r.Confidence <= 1f));
            Assert.IsTrue(System.Array.TrueForAll(all, r => r.Age >= 0));
            Assert.AreEqual(
                store.Count,
                store.CountOf(KnowledgeType.Victim)
                + store.CountOf(KnowledgeType.Obstacle)
                + store.CountOf(KnowledgeType.Hazard)
                + store.CountOf(KnowledgeType.ExploredRegion)
                + store.CountOf(KnowledgeType.SafeRegion));

            var again = store.QueryAll();
            for (var i = 0; i < all.Length; i++)
            {
                Assert.AreEqual(all[i].Type, again[i].Type);
                Assert.AreEqual(all[i].Position.z, again[i].Position.z, 1e-6f);
                Assert.AreEqual(all[i].Timestamp, again[i].Timestamp);
            }
        }
    }
}
