namespace ADRL.Environment.Scenarios
{
    public interface IScenario
    {
        int ScenarioId { get; }

        string ScenarioName { get; }

        ScenarioType ScenarioType { get; }

        ScenarioDifficulty Difficulty { get; }

        int Seed { get; }

        void Initialize();

        void Reset();

        bool Validate(out string message);
    }
}
