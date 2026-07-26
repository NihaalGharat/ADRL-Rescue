namespace ADRL.Environment.Terrain
{
    using UnityEngine;

    public class HybridHeightmapGenerator : IHeightmapGenerator
    {
        private static readonly IHeightmapGenerator _fbm = new FBMHeightmapGenerator();
        private static readonly IHeightmapGenerator _ridged = new RidgedHeightmapGenerator();
        private static readonly IHeightmapGenerator _domainWarp = new DomainWarpHeightmapGenerator();
        private static readonly IHeightmapGenerator _voronoi = new VoronoiHeightmapGenerator();

        public float[,] Generate(TerrainSettings settings, int seed, int resolution)
        {
            if (settings == null)
                throw new System.ArgumentNullException(nameof(settings));

            var hFBM = _fbm.Generate(settings, seed, resolution);
            var hRidged = _ridged.Generate(settings, seed, resolution);
            var hDomainWarp = _domainWarp.Generate(settings, seed, resolution);
            var hVoronoi = _voronoi.Generate(settings, seed, resolution);

            var wFbm = settings.FbmWeight;
            var wRidged = settings.RidgedWeight;
            var wDomainWarp = settings.DomainWarpWeight;
            var wVoronoi = settings.VoronoiWeight;

            if (wFbm < 0f || wRidged < 0f || wDomainWarp < 0f || wVoronoi < 0f)
                throw new System.ArgumentException("Blend weights must be non-negative", nameof(settings));

            var totalWeight = wFbm + wRidged + wDomainWarp + wVoronoi;
            if (totalWeight <= 0f)
                throw new System.ArgumentException("Total blend weight must be positive", nameof(settings));

            var invTotal = 1f / totalWeight;
            var heights = new float[resolution, resolution];

            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var blended = (hFBM[z, x] * wFbm
                                 + hRidged[z, x] * wRidged
                                 + hDomainWarp[z, x] * wDomainWarp
                                 + hVoronoi[z, x] * wVoronoi)
                                 * invTotal;

                    heights[z, x] = Mathf.Clamp01(blended);
                }
            }

            return heights;
        }
    }
}
