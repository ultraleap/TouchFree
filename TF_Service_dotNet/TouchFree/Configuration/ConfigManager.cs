using Newtonsoft.Json;

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
            var interactionsUpdated = false;
            InteractionConfig intFromFile = InteractionConfigFile.LoadConfig();
            var loadedInteractions = new InteractionConfigInternal(intFromFile);
            if (_interactions == null || !InefficientEqualsComparison(_interactions, loadedInteractions)) {
                _interactions = loadedInteractions;
                interactionsUpdated = true;
            }

            var physicalUpdated = false;
            PhysicalConfig physFromFile = PhysicalConfigFile.LoadConfig();
            var loadedPhysical = new PhysicalConfigInternal(physFromFile);
            if (_physical == null || !InefficientEqualsComparison(_physical, loadedPhysical))
            {
                _physical = loadedPhysical;
                physicalUpdated = true;
            }

            var trackingUpdated = false;
            if (TrackingConfigFile.DoesConfigFileExist())
            {
                var loadedTracking = TrackingConfigFile.LoadConfig();
                if (_tracking == null || !InefficientEqualsComparison(_tracking, loadedTracking))
                {
                    _tracking = loadedTracking;
                    trackingUpdated = true;
                }
            }

            if (interactionsUpdated)
            {
                InteractionConfigWasUpdated();
            }

            if (physicalUpdated)
            {
                PhysicalConfigWasUpdated();
            }

            if (trackingUpdated)
            {
                TrackingConfigWasUpdated();
            }


            ErrorLoadingConfigFiles = InteractionConfigFile.ErrorLoadingConfiguration() || PhysicalConfigFile.ErrorLoadingConfiguration();
        }

        private bool InefficientEqualsComparison<T>(T lhs, T rhs)
        {
            return JsonConvert.SerializeObject(lhs) == JsonConvert.SerializeObject(rhs);
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