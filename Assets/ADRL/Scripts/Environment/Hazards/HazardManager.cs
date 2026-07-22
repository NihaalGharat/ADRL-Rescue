namespace ADRL.Environment.Hazards
{
    using System.Collections.Generic;

    public class HazardManager
    {
        private readonly List<Hazard> _hazards = new();

        public IReadOnlyList<Hazard> Hazards => _hazards;

        public void RegisterHazard(Hazard hazard)
        {
            if (hazard == null || _hazards.Contains(hazard))
                return;

            _hazards.Add(hazard);
            hazard.Initialize();
        }

        public bool UnregisterHazard(Hazard hazard)
        {
            if (hazard == null)
                return false;

            hazard.Cleanup();
            return _hazards.Remove(hazard);
        }

        public int ActiveHazardCount
        {
            get
            {
                var count = 0;

                for (var i = 0; i < _hazards.Count; i++)
                {
                    if (_hazards[i].IsActive)
                        count++;
                }

                return count;
            }
        }

        public void ResetAll()
        {
            for (var i = 0; i < _hazards.Count; i++)
            {
                _hazards[i].Reset();
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _hazards.Count; i++)
            {
                _hazards[i].Cleanup();
            }

            _hazards.Clear();
        }
    }
}
