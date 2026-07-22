namespace ADRL.Core.Events
{
    using System;

    public interface IEvent
    {
    }

    public readonly struct BootstrapCompletedEvent : IEvent
    {
    }

    public readonly struct SimulationStartedEvent : IEvent
    {
    }

    public readonly struct SimulationPausedEvent : IEvent
    {
    }

    public readonly struct SimulationResumedEvent : IEvent
    {
    }

    public readonly struct SimulationStoppedEvent : IEvent
    {
    }

    public readonly struct SimulationResetEvent : IEvent
    {
    }

    public readonly struct EpisodeStartedEvent : IEvent
    {
        public int EpisodeNumber { get; }

        public EpisodeStartedEvent(int episodeNumber)
        {
            EpisodeNumber = episodeNumber;
        }
    }

    public readonly struct EpisodeCompletedEvent : IEvent
    {
        public int EpisodeNumber { get; }
        public float TotalReward { get; }
        public int StepsCompleted { get; }

        public EpisodeCompletedEvent(int episodeNumber, float totalReward, int stepsCompleted)
        {
            EpisodeNumber = episodeNumber;
            TotalReward = totalReward;
            StepsCompleted = stepsCompleted;
        }
    }

    public readonly struct ConfigurationLoadedEvent : IEvent
    {
    }

    public readonly struct SceneLoadRequestedEvent : IEvent
    {
        public string SceneName { get; }

        public SceneLoadRequestedEvent(string sceneName)
        {
            SceneName = sceneName;
        }
    }

    public readonly struct SceneLoadedEvent : IEvent
    {
        public string SceneName { get; }

        public SceneLoadedEvent(string sceneName)
        {
            SceneName = sceneName;
        }
    }

    public readonly struct DroneSpawnedEvent : IEvent
    {
        public int DroneId { get; }

        public DroneSpawnedEvent(int droneId)
        {
            DroneId = droneId;
        }
    }

    public readonly struct DroneDamagedEvent : IEvent
    {
        public int DroneId { get; }
        public float DamageAmount { get; }
        public float CurrentHealth { get; }

        public DroneDamagedEvent(int droneId, float damageAmount, float currentHealth)
        {
            DroneId = droneId;
            DamageAmount = damageAmount;
            CurrentHealth = currentHealth;
        }
    }

    public readonly struct DroneDestroyedEvent : IEvent
    {
        public int DroneId { get; }

        public DroneDestroyedEvent(int droneId)
        {
            DroneId = droneId;
        }
    }

    public readonly struct VictimFoundEvent : IEvent
    {
        public int VictimId { get; }

        public VictimFoundEvent(int victimId)
        {
            VictimId = victimId;
        }
    }

    public readonly struct VictimRescuedEvent : IEvent
    {
        public int VictimId { get; }

        public VictimRescuedEvent(int victimId)
        {
            VictimId = victimId;
        }
    }

    public readonly struct CollisionEvent : IEvent
    {
        public int DroneId { get; }
        public string CollidedTag { get; }
        public float ImpactForce { get; }

        public CollisionEvent(int droneId, string collidedTag, float impactForce)
        {
            DroneId = droneId;
            CollidedTag = collidedTag;
            ImpactForce = impactForce;
        }
    }
}
