namespace ADRL.AI.Decision.Trace
{
    using UnityEngine;

    /// <summary>
    /// The single owner of decision-trace history. It is a bounded, deterministic
    /// store of immutable <see cref="DecisionTraceFrame"/> values that records every
    /// completed decision in append order so the runtime can be inspected, replayed
    /// and regression-tested after the fact. It never decides anything - it only
    /// stores and retrieves frames. The engine appends a frame per step; replay and
    /// validation read the store through the owned accessors below.
    /// </summary>
    /// <remarks>
    /// The backing array is fixed at construction to
    /// <see cref="MaximumCapacity"/> (configurable, 128 by default), so the store
    /// can never grow without bound. When the store is full a new frame evicts the
    /// oldest stored frame, keeping the most recent decisions available. Ordering is
    /// deterministic: frames are stored strictly in append order, index 0 is always
    /// the oldest retained frame and <see cref="Get"/> never throws - an out-of-range
    /// index returns <see cref="DecisionTraceFrame.Empty"/>. Intended for the Unity
    /// main thread only, matching the framework's main-thread assumption.
    /// </remarks>
    public sealed class DecisionTraceStore
    {
        /// <summary>The default number of frames retained before the oldest is evicted.</summary>
        public const int DefaultCapacity = 128;

        private readonly DecisionTraceFrame[] _frames;
        private int _offset;
        private int _count;

        public DecisionTraceStore(int maximumCapacity = DefaultCapacity)
        {
            _frames = new DecisionTraceFrame[Mathf.Max(1, maximumCapacity)];
        }

        /// <summary>The bounded number of trace-frame slots.</summary>
        public int MaximumCapacity => _frames.Length;

        /// <summary>Number of currently stored trace frames.</summary>
        public int Count => _count;

        /// <summary>The most recently appended trace frame, or <see cref="DecisionTraceFrame.Empty"/> when the store is empty.</summary>
        public DecisionTraceFrame Latest => _count == 0 ? DecisionTraceFrame.Empty : Get(_count - 1);

        /// <summary>
        /// Appends a trace frame in append order. When the store is full the oldest
        /// stored frame is evicted first, keeping the store bounded.
        /// </summary>
        public void Append(DecisionTraceFrame frame)
        {
            if (frame == null)
                return;

            if (_count < _frames.Length)
            {
                _frames[(_offset + _count) % _frames.Length] = frame;
                _count++;
            }
            else
            {
                _frames[_offset] = frame;
                _offset = (_offset + 1) % _frames.Length;
            }
        }

        /// <summary>
        /// Returns the trace frame at the given position, where index 0 is the
        /// oldest retained frame and <see cref="Count"/> - 1 is the latest. An
        /// out-of-range index returns <see cref="DecisionTraceFrame.Empty"/>.
        /// </summary>
        public DecisionTraceFrame Get(int index)
        {
            if (index < 0 || index >= _count)
                return DecisionTraceFrame.Empty;

            return _frames[(_offset + index) % _frames.Length];
        }

        /// <summary>
        /// All stored trace frames as an owned copy, in deterministic append order
        /// (oldest first). Never a shared reference to the store's buffer.
        /// </summary>
        public DecisionTraceFrame[] Enumerate()
        {
            var result = new DecisionTraceFrame[_count];
            for (var i = 0; i < _count; i++)
                result[i] = Get(i);

            return result;
        }

        /// <summary>Removes every stored trace frame, restoring the empty store.</summary>
        public void Clear()
        {
            for (var i = 0; i < _frames.Length; i++)
                _frames[i] = null;

            _offset = 0;
            _count = 0;
        }
    }
}
