namespace ADRL.Environment.Terrain
{
    public interface IHeightmapGenerator
    {
        float[,] Generate(TerrainSettings settings, int seed, int resolution);
    }
}
