namespace ADRL.Drone.Core
{
    using System.Collections.Generic;
    using UnityEngine;

    public class DronePrefabRegistry
    {
        private readonly Dictionary<string, GameObject> _prefabs = new();

        public int Count => _prefabs.Count;

        public void Register(string droneType, GameObject prefab)
        {
            if (string.IsNullOrEmpty(droneType))
            {
                Debug.LogError(
                    "[DronePrefabRegistry] Cannot register prefab with null or empty drone type.");
                return;
            }

            if (prefab == null)
            {
                Debug.LogError("[DronePrefabRegistry] Cannot register null prefab.");
                return;
            }

            _prefabs[droneType] = prefab;
        }

        public bool TryGetPrefab(string droneType, out GameObject prefab)
        {
            return _prefabs.TryGetValue(droneType, out prefab);
        }

        public bool Contains(string droneType)
        {
            return !string.IsNullOrEmpty(droneType) && _prefabs.ContainsKey(droneType);
        }

        public void Clear()
        {
            _prefabs.Clear();
        }
    }
}
