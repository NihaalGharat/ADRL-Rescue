namespace ADRL.Environment.Events
{
    using ADRL.Core.Events;
    using ADRL.Environment.WorldObjects;

    public readonly struct WorldObjectRegisteredEvent : IEvent
    {
        public int Id { get; }

        public WorldObjectCategory Category { get; }

        public WorldObjectRegisteredEvent(int id, WorldObjectCategory category)
        {
            Id = id;
            Category = category;
        }
    }

    public readonly struct WorldObjectRemovedEvent : IEvent
    {
        public int Id { get; }

        public WorldObjectCategory Category { get; }

        public WorldObjectRemovedEvent(int id, WorldObjectCategory category)
        {
            Id = id;
            Category = category;
        }
    }

    public readonly struct WorldObjectResetEvent : IEvent
    {
        public int Id { get; }

        public WorldObjectResetEvent(int id)
        {
            Id = id;
        }
    }
}
