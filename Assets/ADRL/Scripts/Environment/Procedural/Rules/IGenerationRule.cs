namespace ADRL.Environment.Procedural.Rules
{
    using ADRL.Environment.Procedural;

    public interface IGenerationRule
    {
        string Name { get; }

        bool Enabled { get; set; }

        int Priority { get; set; }

        int MaxCount { get; set; }

        int Generate(IGenerationContext context);
    }
}
