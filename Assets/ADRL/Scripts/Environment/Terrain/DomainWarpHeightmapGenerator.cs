namespace ADRL.Environment.Terrain
{
    using UnityEngine;

    public class DomainWarpHeightmapGenerator : IHeightmapGenerator
    {
        private const float SeedOffsetMultiplier = 0.1f;
        private const float SeedOffsetSeparator = 1000f;
        private const float WarpOffsetSeparator = 500f;
        private const float NoiseCenter = 0.5f;
        private const float NoiseScaleFactor = 2f;

        public float[,] Generate(TerrainSettings settings, int seed, int resolution)
        {
            if (settings == null)
                throw new System.ArgumentNullException(nameof(settings));

            var heights = new float[resolution, resolution];
            var combinedOffset = seed + settings.SeedOffset;
            var offsetX = combinedOffset * SeedOffsetMultiplier;
            var offsetZ = combinedOffset * SeedOffsetMultiplier + SeedOffsetSeparator;
            var warpOffsetX = combinedOffset * SeedOffsetMultiplier + WarpOffsetSeparator;
            var warpOffsetZ = combinedOffset * SeedOffsetMultiplier + SeedOffsetSeparator + WarpOffsetSeparator;
            var noiseScale = settings.NoiseScale;
            var heightMultiplier = settings.HeightMultiplier;
            var octaves = settings.Octaves;
            var persistence = settings.Persistence;
            var lacunarity = settings.Lacunarity;
            var warpStrength = settings.WarpStrength;
            var warpScale = settings.WarpScale;

            var amplitude = 1f;
            var frequency = 1f;
            var maxAmplitude = 0f;

            for (var o = 0; o < octaves; o++)
            {
                maxAmplitude += amplitude;
                amplitude *= persistence;
            }

            amplitude = 1f;

            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var warpNX = x * warpScale + warpOffsetX;
                    var warpNZ = z * warpScale + warpOffsetZ;

                    var warpX = (Mathf.PerlinNoise(warpNX, warpNZ) - NoiseCenter) * NoiseScaleFactor;
                    var warpZ = (Mathf.PerlinNoise(warpNX + WarpOffsetSeparator, warpNZ + WarpOffsetSeparator) - NoiseCenter) * NoiseScaleFactor;

                    var sampleX = x * noiseScale + warpX * warpStrength;
                    var sampleZ = z * noiseScale + warpZ * warpStrength;

                    var noiseValue = 0f;
                    var amp = amplitude;
                    var freq = frequency;

                    for (var o = 0; o < octaves; o++)
                    {
                        var nx = sampleX * freq + offsetX;
                        var nz = sampleZ * freq + offsetZ;
                        noiseValue += amp * Mathf.PerlinNoise(nx, nz);
                        freq *= lacunarity;
                        amp *= persistence;
                    }

                    heights[z, x] = Mathf.Clamp01(noiseValue / maxAmplitude * heightMultiplier);
                }
            }

            return heights;
        }
    }
}
