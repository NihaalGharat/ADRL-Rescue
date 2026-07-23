namespace ADRL.Environment.Events
{
    using ADRL.Core.Events;

    public readonly struct ObstacleRegisteredEvent : IEvent
    {
        public int ObstacleId { get; }

        public ObstacleRegisteredEvent(int obstacleId)
        {
            ObstacleId = obstacleId;
        }
    }

    public readonly struct ObstacleRemovedEvent : IEvent
    {
        public int ObstacleId { get; }

        public ObstacleRemovedEvent(int obstacleId)
        {
            ObstacleId = obstacleId;
        }
    }

    public readonly struct ObstacleActivatedEvent : IEvent
    {
        public int ObstacleId { get; }

        public ObstacleActivatedEvent(int obstacleId)
        {
            ObstacleId = obstacleId;
        }
    }

    public readonly struct ObstacleDeactivatedEvent : IEvent
    {
        public int ObstacleId { get; }

        public ObstacleDeactivatedEvent(int obstacleId)
        {
            ObstacleId = obstacleId;
        }
    }
}
