namespace ADRL.Environment.Core
{
    using UnityEngine;

    public class EnvironmentWorldBuilder
    {
        public const string RuntimeRootName = "Runtime";
        public const string SystemsRootName = "Systems";
        public const string SpawnRootName = "SpawnPoints";
        public const string DebugRootName = "Debug";

        private Transform _rootTransform;
        private Transform _runtimeRoot;
        private Transform _systemsRoot;
        private Transform _spawnRoot;
        private Transform _debugRoot;
        private bool _worldBuilt;

        public Transform RootTransform => _rootTransform;
        public Transform RuntimeRoot => _runtimeRoot;
        public Transform SystemsRoot => _systemsRoot;
        public Transform SpawnRoot => _spawnRoot;
        public Transform DebugRoot => _debugRoot;
        public bool WorldBuilt => _worldBuilt;

        public void Build(Transform rootTransform)
        {
            if (rootTransform == null)
                throw new System.ArgumentNullException(nameof(rootTransform));

            if (_worldBuilt)
                throw new System.InvalidOperationException("World has already been built. Call Destroy() before rebuilding.");

            _rootTransform = rootTransform;

            try
            {
                _runtimeRoot = CreateChild(RuntimeRootName);
                _systemsRoot = CreateChild(SystemsRootName);
                _spawnRoot = CreateChild(SpawnRootName);
                _debugRoot = CreateChild(DebugRootName);
                _worldBuilt = true;
            }
            catch
            {
                Destroy();
                throw;
            }
        }

        public void Destroy()
        {
            DestroyChild(ref _runtimeRoot);
            DestroyChild(ref _systemsRoot);
            DestroyChild(ref _spawnRoot);
            DestroyChild(ref _debugRoot);
            _rootTransform = null;
            _worldBuilt = false;
        }

        private Transform CreateChild(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_rootTransform, worldPositionStays: false);
            return go.transform;
        }

        private static void DestroyChild(ref Transform child)
        {
            if (child == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(child.gameObject);
            else
                Object.DestroyImmediate(child.gameObject);

            child = null;
        }
    }
}
