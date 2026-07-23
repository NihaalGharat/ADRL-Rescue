namespace ADRL.Environment.Terrain
{
    using UnityEngine;

    public class HeightmapGenerator : IHeightmapGenerator
    {
        private const float SeedOffsetMultiplier = 0.1f;
        private const float SeedOffsetSeparator = 1000f;

        public float[,] Generate(TerrainSettings settings, int seed, int resolution)
        {
            if (settings == null)
                throw new System.ArgumentNullException(nameof(settings));

            var heights = new float[resolution, resolution];
            var combinedOffset = seed + settings.SeedOffset;
            var offsetX = combinedOffset * SeedOffsetMultiplier;
            var offsetZ = combinedOffset * SeedOffsetMultiplier + SeedOffsetSeparator;
            var noiseScale = settings.NoiseScale;
            var heightMultiplier = settings.HeightMultiplier;

            if (settings.UseFractalNoise)
            {
                var octaves = settings.Octaves;
                var persistence = settings.Persistence;
                var lacunarity = settings.Lacunarity;

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
                        var noiseValue = 0f;
                        var amp = amplitude;
                        var freq = frequency;

                        for (var o = 0; o < octaves; o++)
                        {
                            var nx = x * noiseScale * freq + offsetX;
                            var nz = z * noiseScale * freq + offsetZ;
                            noiseValue += amp * Mathf.PerlinNoise(nx, nz);
                            freq *= lacunarity;
                            amp *= persistence;
                        }

                        heights[z, x] = Mathf.Clamp01(noiseValue / maxAmplitude * heightMultiplier);
                    }
                }
            }
            else
            {
                for (var z = 0; z < resolution; z++)
                {
                    for (var x = 0; x < resolution; x++)
                    {
                        var nx = x * noiseScale + offsetX;
                        var nz = z * noiseScale + offsetZ;
                        heights[z, x] = Mathf.Clamp01(Mathf.PerlinNoise(nx, nz) * heightMultiplier);
                    }
                }
            }

            return heights;
        }
    }
}
