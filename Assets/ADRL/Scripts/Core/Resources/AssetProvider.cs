namespace ADRL.Core.Resources
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class AssetProvider
    {
        private static RuntimeAssetCache _cache;

        internal static void Initialize(RuntimeAssetCache cache)
        {
            _cache = cache;
        }

        internal static void Shutdown()
        {
            _cache?.Clear();
            _cache = null;
        }

        public static T Load<T>(string path) where T : Object
        {
            if (_cache == null)
            {
                Debug.LogError("[AssetProvider] Not initialized. Call ResourceLocator.Initialize first.");
                return null;
            }

            if (_cache.TryGet(path, out T cached))
            {
                return cached;
            }

            var asset = Resources.Load<T>(path);

            if (asset != null)
            {
                _cache.Set(path, asset);
            }

            return asset;
        }

        public static bool TryLoad<T>(string path, out T asset) where T : Object
        {
            asset = Load<T>(path);
            return asset != null;
        }

        public static bool Exists(string path)
        {
            if (_cache.Contains(path))
            {
                return true;
            }

            var obj = Resources.Load(path);
            return obj != null;
        }

        public static void Unload(string path)
        {
            if (_cache.TryGet<Object>(path, out var asset))
            {
                Resources.UnloadAsset(asset);
                _cache.Remove(path);
            }
        }

        public static void UnloadAll()
        {
            var keys = new List<string>(_cache.Keys);

            foreach (var key in keys)
            {
                if (_cache.TryGet<Object>(key, out var asset))
                {
                    Resources.UnloadAsset(asset);
                }
            }

            _cache.Clear();
        }
    }
}
