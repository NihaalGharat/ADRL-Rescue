namespace ADRL.AI.Interaction
{
    using ADRL.Core.Configuration;
    using ADRL.Core.Resources;
    using ADRL.Sensors.Interfaces;
    using UnityEngine;

    /// <summary>
    /// Autonomous interaction behaviour that acts on victims exclusively through
    /// <see cref="IVictimDetectable"/>. Every <see cref="FixedUpdate"/> it scans for
    /// living victims inside the detection range and advances their lifecycle:
    /// <c>MarkDetected</c> on entry, then <c>MarkRescued</c> once the rescue range is
    /// reached. The victim is the single owner of its state and event publishing, so
    /// this component tracks nothing; repeated calls in range are harmless.
    /// </summary>
    public sealed class DroneVictimInteraction : MonoBehaviour
    {
        private const float DefaultDetectionRange = 30f;
        private const float RescueRangeRatio = 0.25f;

        private float _detectionRange;
        private float _rescueRange;
        private Collider[] _overlapBuffer;

        public float DetectionRange => _detectionRange;

        public float RescueRange => _rescueRange;

        /// <summary>Configures the detection and rescue ranges before use.</summary>
        public void Initialize(float detectionRange, float rescueRange)
        {
            _detectionRange = Mathf.Max(0f, detectionRange);
            _rescueRange = Mathf.Max(0f, rescueRange);
        }

        private void Start()
        {
            if (_detectionRange > 0f)
                return;

            var range = DefaultDetectionRange;
            if (ResourceLocator.IsInitialized && ResourceLocator.Configs.TryGet(out SensorConfig sensorConfig))
                range = sensorConfig.ThermalSensorRange;

            Initialize(range, range * RescueRangeRatio);
        }

        private void FixedUpdate()
        {
            Scan();
        }

        /// <summary>
        /// Scans for living victims in range and advances their lifecycle. Runs on
        /// every physics tick and is exposed publicly so the pipeline is testable
        /// without entering play mode.
        /// </summary>
        public void Scan()
        {
            if (_detectionRange <= 0f)
                return;

            if (_overlapBuffer == null)
                _overlapBuffer = new Collider[16];

            var origin = transform;
            var count = Physics.OverlapSphereNonAlloc(
                origin.position, _detectionRange, _overlapBuffer,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            for (var i = 0; i < count; i++)
            {
                var collider = _overlapBuffer[i];
                if (collider == null || collider.transform.IsChildOf(origin))
                    continue;

                var victim = collider.GetComponentInParent<IVictimDetectable>();
                if (victim == null || !victim.IsAlive)
                    continue;

                var distance = Vector3.Distance(origin.position, victim.VictimPosition);

                victim.MarkDetected();
                if (distance <= _rescueRange)
                    victim.MarkRescued();
            }
        }
    }
}
