namespace ADRL.Core.Services
{
    using System;
    using System.Collections.Generic;

    public class ServiceLocator
    {
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        public void Register<T>(T service) where T : IService
        {
            var type = typeof(T);

            if (_services.ContainsKey(type))
            {
                throw new InvalidOperationException(
                    $"Service of type {type.Name} is already registered.");
            }

            _services[type] = service;
        }

        public void Register<T>(T service, bool overwrite) where T : IService
        {
            var type = typeof(T);

            if (overwrite && _services.TryGetValue(type, out var existing))
            {
                existing.Shutdown();
            }

            _services[type] = service;
        }

        public T Get<T>() where T : IService
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException(
                $"Service of type {type.Name} is not registered.");
        }

        public bool TryGet<T>(out T service) where T : IService
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var existing))
            {
                service = (T)existing;
                return true;
            }

            service = default;
            return false;
        }

        public bool IsRegistered<T>() where T : IService
        {
            return _services.ContainsKey(typeof(T));
        }

        public void Unregister<T>() where T : IService
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
            {
                service.Shutdown();
                _services.Remove(type);
            }
        }

        public void InitializeAll()
        {
            foreach (var service in _services.Values)
            {
                service.Initialize();
            }
        }

        public void ShutdownAll()
        {
            foreach (var service in _services.Values)
            {
                service.Shutdown();
            }

            _services.Clear();
        }

        public int ServiceCount => _services.Count;
    }
}
