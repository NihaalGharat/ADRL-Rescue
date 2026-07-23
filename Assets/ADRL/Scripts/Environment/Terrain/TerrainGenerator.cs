namespace ADRL.Environment.Terrain
{
    using ADRL.Core.Events;
    using ADRL.Environment.Events;
    using UnityEngine;

    public class TerrainGenerator
    {
        private const float TerrainCenterFactor = 0.5f;

        private TerrainSettings _settings;
        private Terrain _terrain;
        private TerrainData _terrainData;
        private int _activeSeed;
        private bool _isInitialized;

        public Terrain Terrain => _terrain;
        public TerrainData TerrainData => _terrainData;
        public int ActiveSeed => _activeSeed;
        public TerrainSettings Settings => _settings;

        public bool IsGenerated { get; private set; }

        public IHeightmapGenerator HeightmapGenerator { get; set; }

        public void Initialize(TerrainSettings settings)
        {
            if (_isInitialized)
                return;

            _settings = settings;
            IsGenerated = false;
            _isInitialized = true;
        }

        public void Generate(int seed, EventBus eventBus)
        {
            if (!_isInitialized || _settings == null)
            {
                eventBus?.Publish(new TerrainGenerationFailedEvent("TerrainGenerator has not been initialized"));
                return;
            }

            if (IsGenerated)
            {
                eventBus?.Publish(new TerrainGenerationFailedEvent("Terrain has already been generated. Call Reset() before generating again."));
                return;
            }

            var validationError = ValidateSettings();

            if (validationError != null)
            {
                eventBus?.Publish(new TerrainGenerationFailedEvent(validationError));
                return;
            }

            _activeSeed = seed;
            eventBus?.Publish(new TerrainGenerationStartedEvent());

            try
            {
                _terrainData = CreateTerrainData();
                GenerateHeightmap();
                _terrain = CreateTerrainGameObject();

                IsGenerated = true;
                eventBus?.Publish(new TerrainGeneratedEvent());
            }
            catch (System.Exception ex)
            {
                CleanupTerrainData();
                eventBus?.Publish(new TerrainGenerationFailedEvent($"Terrain generation failed: {ex.Message}"));
            }
        }

        public void Reset()
        {
            if (_terrain != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(_terrain.gameObject);
                else
                    Object.DestroyImmediate(_terrain.gameObject);

                _terrain = null;
            }

            CleanupTerrainData();

            _settings = null;
            _activeSeed = 0;
            IsGenerated = false;
            _isInitialized = false;
        }

        private string ValidateSettings()
        {
            if (_settings.TerrainWidth <= 0f)
                return "TerrainWidth must be greater than zero";

            if (_settings.TerrainLength <= 0f)
                return "TerrainLength must be greater than zero";

            if (_settings.HeightmapResolution < 3)
                return "HeightmapResolution must be at least 3";

            if (_settings.HeightScale < 0f)
                return "HeightScale cannot be negative";

            if (_settings.NoiseScale <= 0f)
                return "NoiseScale must be greater than zero";

            return null;
        }

        private TerrainData CreateTerrainData()
        {
            var data = new TerrainData
            {
                heightmapResolution = _settings.HeightmapResolution,
                size = new Vector3(
                    _settings.TerrainWidth,
                    _settings.HeightScale,
                    _settings.TerrainLength)
            };

            return data;
        }

        private Terrain CreateTerrainGameObject()
        {
            var terrain = Terrain.CreateTerrainGameObject(_terrainData).GetComponent<Terrain>();
            terrain.transform.position = new Vector3(
                -_settings.TerrainWidth * TerrainCenterFactor,
                0f,
                -_settings.TerrainLength * TerrainCenterFactor);

            return terrain;
        }

        private void GenerateHeightmap()
        {
            var resolution = _terrainData.heightmapResolution;
            var generator = HeightmapGenerator ?? new HeightmapGenerator();
            var heights = generator.Generate(_settings, _activeSeed, resolution);
            _terrainData.SetHeights(0, 0, heights);
        }

        private void CleanupTerrainData()
        {
            if (_terrainData != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(_terrainData);
                else
                    Object.DestroyImmediate(_terrainData);

                _terrainData = null;
            }
        }
    }
}
