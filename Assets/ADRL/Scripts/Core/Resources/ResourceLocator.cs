namespace ADRL.Core.Resources
{
    public static class ResourceLocator
    {
        public static ConfigRegistry Configs { get; private set; }
        public static PrefabRegistry Prefabs { get; private set; }
        public static RuntimeAssetCache Cache { get; private set; }

        public static bool IsInitialized { get; private set; }

        internal static void Initialize()
        {
            if (IsInitialized)
                return;

            Configs = new ConfigRegistry();
            Prefabs = new PrefabRegistry();
            Cache = new RuntimeAssetCache();

            AssetProvider.Initialize(Cache);

            IsInitialized = true;
        }

        internal static void Shutdown()
        {
            if (!IsInitialized)
                return;

            AssetProvider.Shutdown();
            Configs.Clear();
            Prefabs.Clear();
            Cache.Clear();

            Configs = null;
            Prefabs = null;
            Cache = null;

            IsInitialized = false;
        }
    }
}
