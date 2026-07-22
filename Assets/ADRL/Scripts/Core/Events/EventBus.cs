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
                _handlers[eventType] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[eventType] = handler;
            }
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
