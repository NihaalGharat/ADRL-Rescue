namespace ADRL.Environment.Procedural
{
    using ADRL.Environment.WorldObjects;
    using UnityEngine;

    public interface IGenerationContext
    {
        System.Random Random { get; }

        Bounds Bounds { get; }

        WorldObjectRegistry Registry { get; }

        int MaxPlacementAttempts { get; }

        float MinSpacing { get; }

        int TotalGenerated { get; }
    }
}
