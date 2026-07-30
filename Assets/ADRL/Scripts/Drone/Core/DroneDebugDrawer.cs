namespace ADRL.Drone.Core
{
    using ADRL.Drone.Interfaces;
    using UnityEngine;

    public class DroneDebugDrawer : MonoBehaviour
    {
        private IDroneSubsystem _subsystem;

        public void Initialize(IDroneSubsystem subsystem)
        {
            _subsystem = subsystem;
        }

        private void OnDrawGizmos()
        {
            if (_subsystem == null)
                return;

            var diagnostics = _subsystem.GetDiagnostics();

            DrawSystemState(diagnostics);
            DrawPoolIndicator(diagnostics);
        }

        private static void DrawSystemState(IDroneDiagnostics diagnostics)
        {
            var color = diagnostics.Health == DroneSubsystemHealth.Healthy
                ? Color.green
                : diagnostics.Health == DroneSubsystemHealth.Degraded
                    ? Color.yellow
                    : Color.red;

            Gizmos.color = color;
            var pos = Vector3.zero;
            Gizmos.DrawWireSphere(pos, 0.3f);
        }

        private static void DrawPoolIndicator(IDroneDiagnostics diagnostics)
        {
            if (diagnostics.PoolStatisticsByType == null)
                return;

            int index = 0;
            foreach (var kvp in diagnostics.PoolStatisticsByType)
            {
                var stats = kvp.Value;
                var color = stats.AvailableCount > 0
                    ? Color.cyan
                    : Color.grey;

                Gizmos.color = color;
                var pos = new Vector3(index * 0.5f, -1f, 0f);
                Gizmos.DrawSphere(pos, 0.15f);
                index++;
            }
        }
    }
}
