using System.Text.RegularExpressions;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class ConfigManager : IConfigManager
    {
        public event IConfigManager.InteractionConfigEvent OnInteractionConfigUpdated;
        public event IConfigManager.PhysicalConfigEvent OnPhysicalConfigUpdated;
        public event IConfigManager.TrackingConfigEvent OnTrackingConfigSaved;
        public event IConfigManager.TrackingConfigEvent OnTrackingConfigUpdated;
        public event IConfigManager.ServiceConfigEvent OnServiceConfigUpdated;
        public event IConfigManager.TouchFreeConfigEvent OnTouchFreeConfigUpdated;
        private InteractionConfigInternal _interactions;
        private PhysicalConfigInternal _physical;
        private TrackingConfig _tracking;
        private ServiceConfig _service;
        private TouchFreeConfig _tf;

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

        public ServiceConfig ServiceConfig
        {
            get
            {
                if (_service == null && ServiceConfigFile.DoesConfigFileExist())
                {
                    _service = ServiceConfigFile.LoadConfig();
                }

                return _service;
            }
            set
            {
                _service = value;
            }
        }

        public TouchFreeConfig TouchFreeConfig
        {
            get
            {
                if (_tf == null && TouchFreeConfigFile.DoesConfigFileExist())
                {
                    _tf = TouchFreeConfigFile.LoadConfig();
                }

                return _tf;
            }
            set
            {
                _tf = value;
            }
        }

        public void LoadConfigsFromFiles()
        {
            var interactionsUpdated = false;
            InteractionConfig intFromFile = InteractionConfigFile.LoadConfig();
            var loadedInteractions = new InteractionConfigInternal(intFromFile);
            if (_interactions == null || _interactions != loadedInteractions)
            {
                _interactions = loadedInteractions;
                interactionsUpdated = true;
            }

            var physicalUpdated = false;
            PhysicalConfig physFromFile = PhysicalConfigFile.LoadConfig();
            var loadedPhysical = new PhysicalConfigInternal(physFromFile);
            if (_physical == null || _physical != loadedPhysical)
            {
                _physical = loadedPhysical;
                physicalUpdated = true;
            }

            var trackingUpdated = false;
            if (TrackingConfigFile.DoesConfigFileExist())
            {
                var loadedTracking = TrackingConfigFile.LoadConfig();
                if (_tracking == null || _tracking != loadedTracking)
                {
                    _tracking = loadedTracking;
                    trackingUpdated = true;
                }
            }

            var serviceUpdated = false;
            if (ServiceConfigFile.DoesConfigFileExist())
            {
                var loadedService = ServiceConfigFile.LoadConfig();
                if (_service == null || _service != loadedService)
                {
                    _service = loadedService;
                    serviceUpdated = true;
                }
            }

            var tfUpdated = false;
            if (TouchFreeConfigFile.DoesConfigFileExist())
            {
                var loadedTF = TouchFreeConfigFile.LoadConfig();
                if (_tf == null || _tf != loadedTF)
                {
                    _tf = loadedTF;
                    tfUpdated = true;
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

            if (serviceUpdated)
            {
                ServiceConfigWasUpdated();
            }

            if (tfUpdated)
            {
                TouchFreeConfigWasUpdated();
            }


            ErrorLoadingConfigFiles = InteractionConfigFile.ErrorLoadingConfiguration() || PhysicalConfigFile.ErrorLoadingConfiguration();
        }

        public void PhysicalConfigWasUpdated() => OnPhysicalConfigUpdated?.Invoke(_physical);
        public void InteractionConfigWasUpdated() => OnInteractionConfigUpdated?.Invoke(_interactions);
        public void TrackingConfigWasUpdated() => OnTrackingConfigUpdated?.Invoke(_tracking);
        public void ServiceConfigWasUpdated() => OnServiceConfigUpdated?.Invoke(_service);
        public void TouchFreeConfigWasUpdated() => OnTouchFreeConfigUpdated?.Invoke(_tf);


        public bool AreConfigsInGoodState()
        {
            return !ErrorLoadingConfigFiles &&
                _physical.ScreenWidthPX > 0 &&
                _physical.ScreenHeightPX > 0;
        }
    }
}