namespace ADRL.Environment.WorldObjects
{
    public interface IWorldObject
    {
        int Id { get; }

        WorldObjectCategory Category { get; }

        bool IsActive { get; }

        void Initialize();

        void Reset();

        void Cleanup();
    }
}
