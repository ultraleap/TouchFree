namespace Ultraleap.TouchFree.Library.Configuration
{
    public class ConfigManager : IConfigManager
    {
        public event IConfigManager.InteractionConfigEvent OnInteractionConfigUpdated;
        public event IConfigManager.PhysicalConfigEvent OnPhysicalConfigUpdated;
        public event IConfigManager.TrackingConfigEvent OnTrackingConfigSaved;
        public event IConfigManager.TrackingConfigEvent OnTrackingConfigUpdated;
        private InteractionConfigInternal _interactions;
        private PhysicalConfigInternal _physical;
        private TrackingConfig _tracking;

        public ConfigManager()
        {
            TrackingConfigFile.OnConfigFileSaved += () => OnTrackingConfigSaved?.Invoke();
        }

        public bool ErrorLoadingConfigFiles { get; private set; }

        public InteractionConfigInternal InteractionConfig
        {
            get
            {
                if (_interactions == null)
                {
                    InteractionConfig fromFile = InteractionConfigFile.LoadConfig();
                    _interactions = new InteractionConfigInternal(fromFile);
                }

                return _interactions;
            }
            set
            {
                _interactions = value;
            }
        }

        public InteractionConfig InteractionConfigFromApi
        {
            set
            {
                _interactions = new InteractionConfigInternal(value);
            }
        }

        public PhysicalConfigInternal PhysicalConfig
        {
            get
            {
                if (_physical == null)
                {
                    PhysicalConfig fromFile = PhysicalConfigFile.LoadConfig();
                    _physical = new PhysicalConfigInternal(fromFile);
                }

                return _physical;
            }
            set
            {
                _physical = value;
            }
        }

        public PhysicalConfig PhysicalConfigFromApi
        {
            set
            {
                _physical = new PhysicalConfigInternal(value);
            }
        }

        public TrackingConfig TrackingConfig
        {
            get
            {
                if (_tracking == null && TrackingConfigFile.DoesConfigFileExist())
                {
                    _tracking = TrackingConfigFile.LoadConfig();
                }

                return _tracking;
            }
            set
            {
                _tracking = value;
            }
        }

        public void LoadConfigsFromFiles()
        {
            InteractionConfig intFromFile = InteractionConfigFile.LoadConfig();
            _interactions = new InteractionConfigInternal(intFromFile);

            PhysicalConfig physFromFile = PhysicalConfigFile.LoadConfig();
            _physical = new PhysicalConfigInternal(physFromFile);

            if (TrackingConfigFile.DoesConfigFileExist())
            {
                _tracking = TrackingConfigFile.LoadConfig();
            }

            InteractionConfigWasUpdated();
            PhysicalConfigWasUpdated();
            TrackingConfigWasUpdated();

            ErrorLoadingConfigFiles = InteractionConfigFile.ErrorLoadingConfiguration() || PhysicalConfigFile.ErrorLoadingConfiguration();
        }

        public void PhysicalConfigWasUpdated() => OnPhysicalConfigUpdated?.Invoke(_physical);
        public void InteractionConfigWasUpdated() => OnInteractionConfigUpdated?.Invoke(_interactions);
        public void TrackingConfigWasUpdated() => OnTrackingConfigUpdated?.Invoke(_tracking);


        public bool AreConfigsInGoodState()
        {
            return !ErrorLoadingConfigFiles &&
                _physical.ScreenWidthPX > 0 &&
                _physical.ScreenHeightPX > 0;
        }
    }
}