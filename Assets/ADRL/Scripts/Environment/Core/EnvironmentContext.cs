namespace ADRL.Environment.Core
{
    using UnityEngine;

    public class EnvironmentContext
    {
        public WorldSettings WorldSettings { get; set; }

        public int ActiveSeed { get; set; }

        public EnvironmentState RuntimeState { get; set; }

        public float EpisodeElapsedTime { get; set; }

        public int CurrentEpisode { get; set; }

        public TerrainData GeneratedTerrain { get; set; }

        public Vector2 TerrainSize { get; set; }

        public void Reset()
        {
            WorldSettings = null;
            GeneratedTerrain = null;
            TerrainSize = Vector2.zero;
            ActiveSeed = 0;
            RuntimeState = EnvironmentState.Uninitialized;
            EpisodeElapsedTime = 0f;
            CurrentEpisode = 0;
        }
    }
}
