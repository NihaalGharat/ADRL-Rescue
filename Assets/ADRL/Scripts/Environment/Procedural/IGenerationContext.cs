namespace ADRL.Environment.Procedural
{
    using System.Collections.Generic;
    using ADRL.Environment.WorldObjects;
    using UnityEngine;

    public interface IGenerationContext
    {
        System.Random Random { get; }

        Bounds Bounds { get; }

        WorldObjectRegistry Registry { get; }

        int MaxPlacementAttempts { get; }

        float MinSpacing { get; }

        int TotalGenerated { get; set; }

        List<Vector3> PlacedPositions { get; }
    }
}
