namespace ADRL.Environment.Spawning
{
    using System.Collections.Generic;
    using UnityEngine;

    public class SpawnManager : MonoBehaviour
    {
        private readonly Dictionary<string, List<SpawnPoint>> _spawnPointsByCategory = new();

        public void RegisterSpawnPoint(SpawnPoint spawnPoint)
        {
            var category = spawnPoint.Category;

            if (!_spawnPointsByCategory.TryGetValue(category, out var list))
            {
                list = new List<SpawnPoint>();
                _spawnPointsByCategory[category] = list;
            }

            if (!list.Contains(spawnPoint))
                list.Add(spawnPoint);
        }

        public bool TryGetSpawnPoint(string category, out SpawnPoint result)
        {
            result = null;

            if (!_spawnPointsByCategory.TryGetValue(category, out var list) || list.Count == 0)
                return false;

            result = list[Random.Range(0, list.Count)];
            return true;
        }

        public IReadOnlyList<SpawnPoint> GetSpawnPoints(string category)
        {
            if (_spawnPointsByCategory.TryGetValue(category, out var list))
                return list;

            return System.Array.Empty<SpawnPoint>();
        }

        public void Clear()
        {
            _spawnPointsByCategory.Clear();
        }
    }
}
