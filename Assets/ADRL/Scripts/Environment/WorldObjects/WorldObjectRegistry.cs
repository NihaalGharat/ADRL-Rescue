namespace ADRL.Environment.WorldObjects
{
    using System.Collections.Generic;

    public class WorldObjectRegistry
    {
        private Dictionary<int, IWorldObject> _byId;

        private Dictionary<WorldObjectCategory, List<IWorldObject>> _byCategory;

        public int Count => _byId?.Count ?? 0;

        public void Initialize()
        {
            _byId = new Dictionary<int, IWorldObject>();
            _byCategory = new Dictionary<WorldObjectCategory, List<IWorldObject>>();
        }

        public bool Register(IWorldObject obj)
        {
            if (obj == null || _byId.ContainsKey(obj.Id))
                return false;

            _byId[obj.Id] = obj;

            if (!_byCategory.TryGetValue(obj.Category, out var list))
            {
                list = new List<IWorldObject>();
                _byCategory[obj.Category] = list;
            }

            list.Add(obj);
            obj.Initialize();
            return true;
        }

        public bool Unregister(int id)
        {
            if (!_byId.TryGetValue(id, out var obj))
                return false;

            obj.Cleanup();

            if (_byCategory.TryGetValue(obj.Category, out var list))
                list.Remove(obj);

            _byId.Remove(id);
            return true;
        }

        public IWorldObject GetById(int id)
        {
            _byId.TryGetValue(id, out var obj);
            return obj;
        }

        public IReadOnlyList<IWorldObject> GetByCategory(WorldObjectCategory category)
        {
            if (_byCategory.TryGetValue(category, out var list))
                return list;

            return System.Array.Empty<IWorldObject>();
        }

        public void ResetAll()
        {
            foreach (var obj in _byId.Values)
                obj.Reset();
        }

        public void Clear()
        {
            foreach (var obj in _byId.Values)
                obj.Cleanup();

            _byId.Clear();
            _byCategory.Clear();
        }
    }
}
