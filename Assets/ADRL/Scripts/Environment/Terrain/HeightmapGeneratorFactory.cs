namespace ADRL.Environment.Terrain
{
    public static class HeightmapGeneratorFactory
    {
        public static IHeightmapGenerator Create(TerrainAlgorithm algorithm)
        {
            return algorithm switch
            {
                TerrainAlgorithm.FBM => new FBMHeightmapGenerator(),
                TerrainAlgorithm.Ridged => new RidgedHeightmapGenerator(),
                TerrainAlgorithm.DomainWarp => new DomainWarpHeightmapGenerator(),
                TerrainAlgorithm.Voronoi => new VoronoiHeightmapGenerator(),
                TerrainAlgorithm.Hybrid => new HybridHeightmapGenerator(),
                _ => throw new System.ArgumentOutOfRangeException(nameof(algorithm),
                    algorithm, $"Unknown terrain algorithm: {algorithm}")
            };
        }
    }
}
