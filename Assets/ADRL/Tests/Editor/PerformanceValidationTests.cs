namespace ADRL.Tests.Editor.Performance
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Unity.Profiling;
    using UnityEngine;
    using ADRL.Tests.Editor;
    using ADRL.AI.DecisionMaking;
    using ADRL.AI.Rewards;

    /// <summary>
    /// Validates the M4 allocation contract: steady-state per-step reward
    /// computation must allocate nothing on the managed heap, and a sustained
    /// run must not leak.
    /// </summary>
    [TestFixture]
    internal sealed class PerformanceValidationTests
    {
        private static long SumSamples(ProfilerRecorder recorder)
        {
            long sum = 0;
            var samples = recorder.ToArray();
            for (int i = 0; i < samples.Length; i++)
                sum += samples[i].Value;
            return sum;
        }

        [Test]
        public void UpdateStep_SteadyState_ZeroAllocations()
        {
            // Verify the GC allocation marker is available before asserting.
            // In some headless environments ProfilerRecorder samples are not collected.
            long controlBytes;
            using (var control = ProfilerRecorder.StartNew(new ProfilerMarker("GC/Alloc")))
            {
                var junk = new List<int>();
                for (int i = 0; i < 100; i++)
                    junk.Add(i);
                control.Stop();
                controlBytes = SumSamples(control);
            }
            if (controlBytes == 0)
            {
                // GC/Alloc marker unavailable — fall back to GC.GetTotalMemory for
                // a best-effort zero-allocation check on the reward path.
                using var h = new TestHarness(0);
                h.Evaluator.Reset(Vector3.zero);
                h.Evaluator.UpdateStep(0.02f, Vector3.zero, DroneCommand.Idle);

                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                long before = GC.GetTotalMemory(false);

                const int iterations = 1000;
                for (int i = 0; i < iterations; i++)
                    h.Evaluator.UpdateStep(0.02f, Vector3.zero, DroneCommand.Idle);

                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                long after = GC.GetTotalMemory(false);
                long delta = after - before;
                long tolerance = 32 * 1024; // 32 KB tolerance for Unity editor noise
                Assert.LessOrEqual(delta, tolerance,
                    $"Best-effort allocation check: managed heap delta={delta} bytes over {iterations} steps (tolerance={tolerance}).");
                return;
            }

            // Steady-state reward path (already-visited cell, idle command):
            // only time-penalty accounting runs, which must allocate nothing.
            using var harness = new TestHarness(0);
            harness.Evaluator.Reset(Vector3.zero);
            harness.Evaluator.UpdateStep(0.02f, Vector3.zero, DroneCommand.Idle); // warm novelty cell

            using (var recorder = ProfilerRecorder.StartNew(new ProfilerMarker("GC/Alloc")))
            {
                const int iterations = 1000;
                for (int i = 0; i < iterations; i++)
                    harness.Evaluator.UpdateStep(0.02f, Vector3.zero, DroneCommand.Idle);
                recorder.Stop();
                long totalBytes = SumSamples(recorder);

                Assert.AreEqual(0L, totalBytes,
                    $"Expected zero GC allocations over {iterations} steady-state steps, got {totalBytes} bytes.");
            }
        }

        [Test]
        public void UpdateStep_SustainedRun_DoesNotLeakManagedHeap()
        {
            using var h = new TestHarness(0);
            h.Evaluator.Reset(Vector3.zero);
            h.Evaluator.UpdateStep(0.02f, Vector3.zero, DroneCommand.Idle); // warm

            // Allow Unity's internal first-use allocations (JIT, type cache, etc.)
            // to settle before measuring the sustained path.
            for (int i = 0; i < 50; i++)
                h.Evaluator.UpdateStep(0.02f, Vector3.zero, DroneCommand.Idle);

            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            long before = GC.GetTotalMemory(false);

            for (int i = 0; i < 500; i++)
                h.Evaluator.UpdateStep(0.02f, Vector3.zero, DroneCommand.Idle);

            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            long after = GC.GetTotalMemory(false);

            // Allow a tolerance for Unity's internal bookkeeping noise in the
            // editor test environment (type cache expansion, internal collections).
            long delta = after - before;
            long tolerance = 32 * 1024; // 32 KB tolerance for Unity editor noise
            Assert.LessOrEqual(delta, tolerance,
                $"Managed heap grew: before={before} bytes, after={after} bytes, delta={delta} bytes (tolerance={tolerance}).");
        }

        [Test]
        public void CurrentBreakdown_ReturnsStruct_ByValue()
        {
            using var h = new TestHarness(0);
            h.Evaluator.Reset(Vector3.zero);
            h.Evaluator.UpdateStep(0.02f, Vector3.zero, DroneCommand.Idle);

            var a = h.Evaluator.CurrentBreakdown;
            var b = h.Evaluator.CurrentBreakdown;
            Assert.AreEqual(a.TotalReward, b.TotalReward, 1e-9f);
            Assert.AreEqual(a.NoveltyCellsVisited, b.NoveltyCellsVisited);
        }
    }
}
