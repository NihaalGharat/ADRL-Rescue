namespace ADRL.Core.Configuration
{
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Configuration/Training Config",
        fileName = "TrainingConfig")]
    public class TrainingConfig : ScriptableObject
    {
        [SerializeField]
        [Min(1)]
        private int _maxSteps = 500000;

        [SerializeField]
        [Min(1)]
        private int _batchSize = 64;

        [SerializeField]
        [Min(1)]
        private int _bufferSize = 2048;

        [SerializeField]
        [Min(0f)]
        private float _learningRate = 3e-4f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _gamma = 0.99f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _lambda = 0.95f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _epsilon = 0.2f;

        [SerializeField]
        [Min(1)]
        private int _numEpoch = 3;

        [SerializeField]
        [Min(1f)]
        private float _timeHorizon = 128f;

        [SerializeField]
        private bool _useRecurrent = false;

        [SerializeField]
        [Min(1)]
        private int _sequenceLength = 64;

        public int MaxSteps => _maxSteps;
        public int BatchSize => _batchSize;
        public int BufferSize => _bufferSize;
        public float LearningRate => _learningRate;
        public float Gamma => _gamma;
        public float Lambda => _lambda;
        public float Epsilon => _epsilon;
        public int NumEpoch => _numEpoch;
        public float TimeHorizon => _timeHorizon;
        public bool UseRecurrent => _useRecurrent;
        public int SequenceLength => _sequenceLength;
    }
}
