namespace ADRL.Core.Configuration
{
    using ADRL.Core.Utilities;
    using UnityEngine;

    [CreateAssetMenu(
        menuName = "ADRL/Configuration/Project Config",
        fileName = "ProjectConfig")]
    public class ProjectConfig : ScriptableObject
    {
        [SerializeField]
        private string _productName = "ADRL-Rescue";

        [SerializeField]
        private string _version = "0.2.0";

        [SerializeField]
        private bool _enableDebugMode;

        [SerializeField]
        private LogLevel _logLevel = LogLevel.Info;

        [SerializeField]
        private string[] _initialScenes;

        public string ProductName => _productName;
        public string Version => _version;
        public bool EnableDebugMode => _enableDebugMode;
        public LogLevel LogLevel => _logLevel;
        public string[] InitialScenes => _initialScenes;
    }
}
