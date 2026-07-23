namespace ADRL.Environment.Procedural
{
    using System.Collections.Generic;
    using ADRL.Environment.WorldObjects;
    using UnityEngine;

    public class ProceduralGenerator
    {
        private GenerationSettings _settings;
        private System.Random _random;
        private int _initialSeed;
        private readonly List<GenerationRule> _rules;

        public IReadOnlyList<GenerationRule> Rules => _rules;

        public GenerationSettings Settings => _settings;

        public int TotalGenerated { get; private set; }

        public ProceduralGenerator()
        {
            _rules = new List<GenerationRule>();
        }

        public void Initialize(GenerationSettings settings)
        {
            _settings = settings;
            _initialSeed = settings.GetEffectiveSeed();
            _random = new System.Random(_initialSeed);
            TotalGenerated = 0;
        }

        public void AddRule(GenerationRule rule)
        {
            if (rule != null && !_rules.Contains(rule))
                _rules.Add(rule);
        }

        public bool RemoveRule(GenerationRule rule)
        {
            return _rules.Remove(rule);
        }

        public int Generate(WorldObjectRegistry registry)
        {
            if (_settings == null || _random == null)
                return 0;

            TotalGenerated = 0;

            var bounds = _settings.GetGenerationBounds();

            var context = new GenerationContextImpl
            {
                Random = _random,
                Bounds = bounds,
                Registry = registry,
                MaxPlacementAttempts = _settings.MaxPlacementAttempts,
                MinSpacing = _settings.MinSpacing
            };

            _rules.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            for (var i = 0; i < _rules.Count; i++)
            {
                var rule = _rules[i];

                if (!rule.Enabled)
                    continue;

                context.TotalGenerated = 0;
                var count = rule.Generate(context);
                TotalGenerated += count;
            }

            return TotalGenerated;
        }

        public void Reset()
        {
            if (_settings == null)
                return;

            _random = new System.Random(_initialSeed);
            TotalGenerated = 0;
        }

        public void Clear()
        {
            _rules.Clear();
            _settings = null;
            _random = null;
            TotalGenerated = 0;
        }

        private class GenerationContextImpl : IGenerationContext
        {
            public System.Random Random { get; set; }

            public Bounds Bounds { get; set; }

            public WorldObjectRegistry Registry { get; set; }

            public int MaxPlacementAttempts { get; set; }

            public float MinSpacing { get; set; }

            public int TotalGenerated { get; set; }

            public List<Vector3> PlacedPositions { get; set; } = new();
        }
    }
}
