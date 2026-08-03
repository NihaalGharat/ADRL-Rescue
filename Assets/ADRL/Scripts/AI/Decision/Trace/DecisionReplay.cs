namespace ADRL.AI.Decision.Trace
{
    /// <summary>
    /// Deterministic replay utility over the decision-trace history. It reads
    /// immutable <see cref="DecisionTraceFrame"/> values from a
    /// <see cref="DecisionTraceStore"/> and reconstructs them as immutable replay
    /// results (the frame itself, or an owned copy of a range of frames). Replay
    /// never writes runtime state: it never modifies the runtime, the engine, a
    /// snapshot, an explanation, diagnostics, the mission or the behaviour - it
    /// simply returns the already-recorded trace data for inspection, regression
    /// testing and debugging.
    /// </summary>
    /// <remarks>
    /// Every method is a pure, null-safe read: a null or out-of-range request
    /// yields the canonical empty frame or an empty array, never an exception, so
    /// replay can never disrupt a running episode. The array returned by
    /// <see cref="ReplayRange"/> is always a fresh owned copy in deterministic
    /// append order (oldest first).
    /// </remarks>
    public static class DecisionReplay
    {
        /// <summary>
        /// Replays the frame at the given position of the store (index 0 = oldest
        /// retained frame). Out-of-range returns <see cref="DecisionTraceFrame.Empty"/>.
        /// </summary>
        public static DecisionTraceFrame ReplayFrame(DecisionTraceStore store, int index)
        {
            return store != null ? store.Get(index) : DecisionTraceFrame.Empty;
        }

        /// <summary>
        /// Replays the most recently appended frame, or
        /// <see cref="DecisionTraceFrame.Empty"/> when the store is empty.
        /// </summary>
        public static DecisionTraceFrame ReplayLatest(DecisionTraceStore store)
        {
            return store != null ? store.Latest : DecisionTraceFrame.Empty;
        }

        /// <summary>
        /// Replays the frames from <paramref name="fromIndex"/> to
        /// <paramref name="toIndex"/> inclusive as an owned copy in deterministic
        /// append order. A null store, an invalid range or an empty result returns
        /// an empty array - never an exception.
        /// </summary>
        public static DecisionTraceFrame[] ReplayRange(DecisionTraceStore store, int fromIndex, int toIndex)
        {
            if (store == null || fromIndex < 0 || toIndex < fromIndex || fromIndex >= store.Count)
                return new DecisionTraceFrame[0];

            var count = toIndex - fromIndex + 1;
            if (toIndex >= store.Count)
                count = store.Count - fromIndex;
            if (count <= 0)
                return new DecisionTraceFrame[0];

            var result = new DecisionTraceFrame[count];
            for (var i = 0; i < count; i++)
                result[i] = store.Get(fromIndex + i);

            return result;
        }
    }
}
