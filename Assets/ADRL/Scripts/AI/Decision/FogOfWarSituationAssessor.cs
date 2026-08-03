namespace ADRL.AI.Decision
{
    using ADRL.Sensors.Interfaces;

    /// <summary>
    /// The single owner of <em>situation assessment</em>. It interprets a fused
    /// <see cref="ISensorReading"/> produced by the sensor fusion layer into a
    /// stable, allocation-free <see cref="SituationSnapshot"/>.
    /// </summary>
    /// <remarks>
    /// The fused vector is interpreted as a sequence of equally-spaced ray bands
    /// laid out in the order registered with the fusion provider. Every two
    /// consecutive samples describe one band: a normalised proximity sample
    /// followed by a target liveness sample. The side of the nearest target and
    /// obstacle is derived from the band's position in the sweep. This mirrors the
    /// sensor layout without coupling the decision framework to any concrete
    /// provider.
    /// </remarks>
    public sealed class FogOfWarSituationAssessor : ISituationAssessor
    {
        private readonly DecisionContext _context;

        public FogOfWarSituationAssessor(DecisionContext context)
        {
            _context = context;
        }

        public SituationSnapshot Assess(ISensorReading fused)
        {
            if (fused == null || !fused.IsValid || fused.Values == null || fused.Values.Length < 2)
                return SituationSnapshot.Invalid;

            var values = fused.Values;
            var bandCount = values.Length / 2;
            var activeBands = 0;

            var targetDetected = false;
            var targetProximity = 0f;
            var targetSide = 0f;
            var obstacleProximity = 0f;
            var obstacleSide = 0f;

            for (var i = 0; i < bandCount; i++)
            {
                var proximity = values[i * 2];
                var isTarget = values[i * 2 + 1] > 0.5f;

                if (proximity > 0f)
                    activeBands++;

                var side = bandCount <= 1 ? 0f : (i / (float)(bandCount - 1)) * 2f - 1f;

                if (isTarget && proximity > targetProximity)
                {
                    targetDetected = true;
                    if (proximity >= _context.DetectionProximity)
                    {
                        targetProximity = proximity;
                        targetSide = side;
                    }
                }
                else if (proximity > obstacleProximity)
                {
                    obstacleProximity = proximity;
                    obstacleSide = side;
                }
            }

            if (!targetDetected)
                return new SituationSnapshot(false, 0f, 0f, obstacleProximity, obstacleSide, activeBands > 0);

            var activeRatio = bandCount == 0 ? 0f : activeBands / (float)bandCount;
            var usable = activeRatio >= _context.MinimumActiveRatio;
            return new SituationSnapshot(
                targetDetected && usable,
                targetProximity,
                targetSide,
                obstacleProximity,
                obstacleSide,
                usable);
        }
    }
}