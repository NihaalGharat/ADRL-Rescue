namespace ADRL.Core.Resources
{
    using System.Collections.Generic;
    using UnityEngine;

    public enum PrefabCategory
    {
        Drone,
        Victim,
        Environment,
        Obstacle,
        Hazard,
        Effects,
        UI
    }

    public class PrefabRegistry
    {
        private readonly Dictionary<string, string> _prefabs = new Dictionary<string, string>();

        private static string BuildKey(PrefabCategory category, string key)
        {
            return $"{category}/{key}";
        }

        public void Register(PrefabCategory category, string key, string resourcePath)
        {
            var lookupKey = BuildKey(category, key);

            if (_prefabs.ContainsKey(lookupKey))
            {
                Debug.LogWarning($"[PrefabRegistry] Prefab '{lookupKey}' is already registered. Overwriting.");
            }

            _prefabs[lookupKey] = resourcePath;
        }

        public bool TryGetPath(PrefabCategory category, string key, out string resourcePath)
        {
            return _prefabs.TryGetValue(BuildKey(category, key), out resourcePath);
        }

        public bool IsRegistered(PrefabCategory category, string key)
        {
            return _prefabs.ContainsKey(BuildKey(category, key));
        }

        public void Clear()
        {
            _prefabs.Clear();
        }

        public int Count => _prefabs.Count;
    }
}
