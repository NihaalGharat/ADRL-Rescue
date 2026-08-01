namespace ADRL.Core.Events
{
    using System;
    using System.Collections.Generic;
    using ADRL.Core.Services;

    public class EventBus : IService
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void Initialize()
        {
        }

        public void Shutdown()
        {
            Clear();
        }

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);

            if (_handlers.TryGetValue(eventType, out var existing))
            {
                if (ContainsHandler(existing, handler))
                    return;

                _handlers[eventType] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[eventType] = handler;
            }
        }

        /// <summary>
        /// True when the handler is already part of the multicast delegate, so a
        /// subscriber can never register the same handler twice for one event.
        /// </summary>
        private static bool ContainsHandler(Delegate invocationList, Delegate candidate)
        {
            foreach (var invocation in invocationList.GetInvocationList())
            {
                if (invocation.Target == candidate.Target &&
                    invocation.Method == candidate.Method)
                {
                    return true;
                }
            }

            return false;
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var eventType = typeof(T);

            if (_handlers.TryGetValue(eventType, out var existing))
            {
                var combined = Delegate.Remove(existing, handler);

                if (combined == null)
                {
                    _handlers.Remove(eventType);
                }
                else
                {
                    _handlers[eventType] = combined;
                }
            }
        }

        public void Publish<T>(T eventData) where T : IEvent
        {
            var eventType = typeof(T);

            if (_handlers.TryGetValue(eventType, out var handler))
            {
                (handler as Action<T>)?.Invoke(eventData);
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }

        public void ClearEvent<T>() where T : IEvent
        {
            _handlers.Remove(typeof(T));
        }
    }
}
