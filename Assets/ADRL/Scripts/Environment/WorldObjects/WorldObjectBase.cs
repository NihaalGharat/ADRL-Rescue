namespace ADRL.Environment.WorldObjects
{
    using UnityEngine;

    public class WorldObjectBase : MonoBehaviour, IWorldObject
    {
        [SerializeField]
        private int _id;

        [SerializeField]
        private WorldObjectCategory _category;

        public int Id => _id;

        public WorldObjectCategory Category => _category;

        public bool IsActive { get; private set; }

        public void SetId(int id)
        {
            _id = id;
        }

        public void SetCategory(WorldObjectCategory category)
        {
            _category = category;
        }

        public void Initialize()
        {
            IsActive = true;
            OnInitialize();
        }

        public void Reset()
        {
            IsActive = true;
            OnReset();
        }

        public void Cleanup()
        {
            IsActive = false;
            OnCleanup();
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnReset()
        {
        }

        protected virtual void OnCleanup()
        {
        }
    }
}
