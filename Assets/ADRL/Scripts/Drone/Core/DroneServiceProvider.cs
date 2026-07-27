namespace ADRL.Drone.Core
{
    using ADRL.Drone.Interfaces;

    public class DroneServiceProvider
    {
        public DroneManager Manager { get; }
        public DroneRegistry Registry { get; }
        public DroneContext Context { get; }
        public DroneConfiguration Configuration { get; }
        public DroneDiagnostics Diagnostics { get; }
        public DroneStartupValidator Validator { get; }

        public DroneServiceProvider(
            DroneManager manager,
            DroneRegistry registry,
            DroneContext context,
            DroneConfiguration configuration,
            DroneDiagnostics diagnostics,
            DroneStartupValidator validator)
        {
            Manager = manager;
            Registry = registry;
            Context = context;
            Configuration = configuration;
            Diagnostics = diagnostics;
            Validator = validator;
        }
    }
}
