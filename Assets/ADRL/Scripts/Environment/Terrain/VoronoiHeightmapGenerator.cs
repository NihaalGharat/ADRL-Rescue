namespace ADRL.Environment.Terrain
{
    using UnityEngine;

    public class VoronoiHeightmapGenerator : IHeightmapGenerator
    {
        private const float NormalizationFactor = 1.41421356f;
        private const int HashSeedA = 374761393;
        private const int HashSeedB = 668265263;
        private const int HashSeedC = 1274126177;
        private const int HashShiftA = 13;
        private const int HashShiftB = 15;
        private const int HashShiftC = 16;
        private const int HashMultiplier = 1274126177;
        private const int HashBitmask = 0x7FFFFFFF;
        private const int SeedSeparator = 1000;

        public float[,] Generate(TerrainSettings settings, int seed, int resolution)
        {
            if (settings == null)
                throw new System.ArgumentNullException(nameof(settings));

            var heights = new float[resolution, resolution];
            var cellSize = settings.VoronoiCellSize;
            var heightMultiplier = settings.HeightMultiplier;
            var cellSizeInv = 1f / cellSize;

            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var cu = x * cellSizeInv;
                    var cv = z * cellSizeInv;

                    var cx = Mathf.FloorToInt(cu);
                    var cz = Mathf.FloorToInt(cv);

                    var minDistSq = float.MaxValue;

                    for (var nx = cx - 1; nx <= cx + 1; nx++)
                    {
                        for (var nz = cz - 1; nz <= cz + 1; nz++)
                        {
                            var fx = nx + Hash(nx, nz, seed);
                            var fz = nz + Hash(nx, nz, seed + SeedSeparator);

                            var dx = cu - fx;
                            var dz = cv - fz;
                            var d = dx * dx + dz * dz;

                            if (d < minDistSq)
                                minDistSq = d;
                        }
                    }

                    var dist = Mathf.Sqrt(minDistSq);
                    var normalized = Mathf.Clamp01(dist / NormalizationFactor);
                    heights[z, x] = Mathf.Clamp01((1f - normalized) * heightMultiplier);
                }
            }

            return heights;
        }

        private static float Hash(int x, int z, int seed)
        {
            unchecked
            {
                var h = x * HashSeedA + z * HashSeedB + seed * HashSeedC;
                h = (h ^ (h >> HashShiftA)) * HashMultiplier;
                h = h ^ (h >> HashShiftB) ^ (h >> HashShiftC);
                return (h & HashBitmask) / (float)int.MaxValue;
            }
        }
    }
}
