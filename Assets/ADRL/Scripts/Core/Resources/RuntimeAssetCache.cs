namespace ADRL.Core.Resources
{
    using System.Collections.Generic;

    public class RuntimeAssetCache
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public void Set<T>(string key, T asset) where T : class
        {
            _cache[key] = asset;
        }

        public bool TryGet<T>(string key, out T asset) where T : class
        {
            if (_cache.TryGetValue(key, out var existing))
            {
                asset = (T)existing;
                return true;
            }

            asset = null;
            return false;
        }

        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public int Count => _cache.Count;

        public Dictionary<string, object>.KeyCollection Keys => _cache.Keys;
    }
}
