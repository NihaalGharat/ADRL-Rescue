namespace ADRL.Core.Resources
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class ConfigRegistry
    {
        private readonly Dictionary<Type, ScriptableObject> _configs = new Dictionary<Type, ScriptableObject>();

        public void Register<T>(T config) where T : ScriptableObject
        {
            var type = typeof(T);

            if (_configs.ContainsKey(type))
            {
                Debug.LogWarning($"[ConfigRegistry] Config of type {type.Name} is already registered. Overwriting.");
            }

            _configs[type] = config;
        }

        public T Get<T>() where T : ScriptableObject
        {
            var type = typeof(T);

            if (_configs.TryGetValue(type, out var config))
            {
                return (T)config;
            }

            throw new InvalidOperationException(
                $"Config of type {type.Name} is not registered. Ensure it is assigned in Bootstrapper.");
        }

        public bool TryGet<T>(out T config) where T : ScriptableObject
        {
            var type = typeof(T);

            if (_configs.TryGetValue(type, out var existing))
            {
                config = (T)existing;
                return true;
            }

            config = null;
            return false;
        }

        public bool IsRegistered<T>() where T : ScriptableObject
        {
            return _configs.ContainsKey(typeof(T));
        }

        public void Clear()
        {
            _configs.Clear();
        }

        public int Count => _configs.Count;
    }
}
