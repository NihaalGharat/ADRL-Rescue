namespace ADRL.Environment.Core
{
    using UnityEngine;

    public class EnvironmentContext
    {
        public WorldSettings WorldSettings { get; set; }

        public int ActiveSeed { get; set; }

        public EnvironmentState RuntimeState { get; set; }

        public TerrainData GeneratedTerrain { get; set; }

        public Vector2 TerrainSize { get; set; }

        public Transform RootTransform { get; set; }

        public Transform RuntimeRoot { get; set; }

        public Transform SystemsRoot { get; set; }

        public Transform SpawnRoot { get; set; }

        public Transform DebugRoot { get; set; }

        public void Reset()
        {
            WorldSettings = null;
            GeneratedTerrain = null;
            TerrainSize = Vector2.zero;
            ActiveSeed = 0;
            RuntimeState = EnvironmentState.Uninitialized;
            RootTransform = null;
            RuntimeRoot = null;
            SystemsRoot = null;
            SpawnRoot = null;
            DebugRoot = null;
        }
    }
}
