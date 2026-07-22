namespace ADRL.Environment.Interfaces
{
    public interface IEnvironmentObject
    {
        int Id { get; }

        void Initialize();

        void Reset();

        void Cleanup();
    }
}
