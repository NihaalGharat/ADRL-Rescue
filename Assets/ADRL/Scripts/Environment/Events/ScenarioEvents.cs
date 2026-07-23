namespace ADRL.Environment.Events
{
    using ADRL.Core.Events;
    using ADRL.Environment.Scenarios;

    public readonly struct ScenarioLoadedEvent : IEvent
    {
        public int ScenarioId { get; }

        public string ScenarioName { get; }

        public ScenarioType ScenarioType { get; }

        public ScenarioLoadedEvent(int scenarioId, string scenarioName, ScenarioType scenarioType)
        {
            ScenarioId = scenarioId;
            ScenarioName = scenarioName;
            ScenarioType = scenarioType;
        }
    }

    public readonly struct ScenarioStartedEvent : IEvent
    {
        public int ScenarioId { get; }

        public ScenarioStartedEvent(int scenarioId)
        {
            ScenarioId = scenarioId;
        }
    }

    public readonly struct ScenarioResetEvent : IEvent
    {
        public int ScenarioId { get; }

        public ScenarioResetEvent(int scenarioId)
        {
            ScenarioId = scenarioId;
        }
    }

    public readonly struct ScenarioCompletedEvent : IEvent
    {
        public int ScenarioId { get; }

        public float CompletionTime { get; }

        public ScenarioCompletedEvent(int scenarioId, float completionTime)
        {
            ScenarioId = scenarioId;
            CompletionTime = completionTime;
        }
    }

    public readonly struct ScenarioFailedEvent : IEvent
    {
        public int ScenarioId { get; }

        public string Reason { get; }

        public ScenarioFailedEvent(int scenarioId, string reason)
        {
            ScenarioId = scenarioId;
            Reason = reason;
        }
    }
}
