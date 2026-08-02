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

    /// <summary>
    /// Raised by a drone agent whenever its ML-Agents episode ends, so the
    /// simulation layer can finalize the episode with the real reward data that
    /// the agent actually accumulated instead of a placeholder value.
    /// </summary>
    public readonly struct AgentEpisodeEndedEvent : IEvent
    {
        public int DroneId { get; }
        public float TotalReward { get; }
        public int StepsCompleted { get; }

        public AgentEpisodeEndedEvent(int droneId, float totalReward, int stepsCompleted)
        {
            DroneId = droneId;
            TotalReward = totalReward;
            StepsCompleted = stepsCompleted;
        }
    }

    /// <summary>
    /// Raised when a drone's energy is fully depleted, so reward systems can
    /// respond to the terminal condition event-driven instead of calling the
    /// evaluator directly. Only the drone with the matching <see cref="DroneId"/>
    /// should apply the corresponding penalty.
    /// </summary>
    public readonly struct DroneEnergyDepletedEvent : IEvent
    {
        public int DroneId { get; }

        public DroneEnergyDepletedEvent(int droneId)
        {
            DroneId = droneId;
        }
    }

    /// <summary>
    /// Raised when a drone leaves the allowed area, so reward systems can
    /// respond to the terminal condition event-driven. Only the drone with the
    /// matching <see cref="DroneId"/> should apply the corresponding penalty.
    /// </summary>
    public readonly struct DroneOutOfBoundsEvent : IEvent
    {
        public int DroneId { get; }

        public DroneOutOfBoundsEvent(int droneId)
        {
            DroneId = droneId;
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

    /// <summary>
    /// Raised once when every registered victim has been rescued. Published by the
    /// environment's mission tracker; the simulation manager is the only owner of
    /// the episode lifecycle that reacts to it.
    /// </summary>
    public readonly struct MissionCompletedEvent : IEvent
    {
        public int RescuedCount { get; }
        public int TotalVictimCount { get; }

        public MissionCompletedEvent(int rescuedCount, int totalVictimCount)
        {
            RescuedCount = rescuedCount;
            TotalVictimCount = totalVictimCount;
        }
    }
}
