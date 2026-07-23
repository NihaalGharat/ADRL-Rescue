namespace ADRL.Environment.Procedural
{
    using UnityEngine;

    public abstract class GenerationRule
    {
        private string _name = "Generation Rule";
        private bool _enabled = true;
        private float _weight = 1f;
        private int _priority;
        private int _maxCount = 10;

        public string Name
        {
            get => _name;
            set => _name = value ?? "Generation Rule";
        }

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public float Weight
        {
            get => _weight;
            set => _weight = Mathf.Max(0f, value);
        }

        public int Priority
        {
            get => _priority;
            set => _priority = value;
        }

        public int MaxCount
        {
            get => _maxCount;
            set => _maxCount = Mathf.Max(0, value);
        }

        public abstract int Generate(IGenerationContext context);
    }
}
