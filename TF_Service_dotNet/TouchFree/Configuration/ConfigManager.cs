using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class ConfigManager : IConfigManager
    {
        public event IConfigManager.InteractionConfigEvent OnInteractionConfigUpdated;
        public event IConfigManager.PhysicalConfigEvent OnPhysicalConfigUpdated;
        private InteractionConfig _interactions;
        private PhysicalConfig _physical;

        public bool ErrorLoadingConfigFiles { get; private set; }

        public InteractionConfig InteractionConfig
        {
            get
            {
                if (_interactions == null)
                {
                    InteractionConfigForFile fromFile = InteractionConfigFile.LoadConfig();
                    _interactions = new InteractionConfig(fromFile);
                }

                return _interactions;
            }
            set
            {
                _interactions = value;
            }
        }

        public PhysicalConfig PhysicalConfig
        {
            get
            {
                if (_physical == null)
                {
                    PhysicalConfigForFile fromFile = PhysicalConfigFile.LoadConfig();
                    _physical = new PhysicalConfig(fromFile);
                }

                return _physical;
            }
            set
            {
                _physical = value;
            }
        }

        public void LoadConfigsFromFiles()
        {
            InteractionConfigForFile intFromFile = InteractionConfigFile.LoadConfig();
            _interactions = new InteractionConfig(intFromFile);

            PhysicalConfigForFile physFromFile = PhysicalConfigFile.LoadConfig();
            _physical = new PhysicalConfig(physFromFile);

            InteractionConfigWasUpdated();
            PhysicalConfigWasUpdated();

            ErrorLoadingConfigFiles = InteractionConfigFile.ErrorLoadingConfiguration() || PhysicalConfigFile.ErrorLoadingConfiguration();
        }

        public void PhysicalConfigWasUpdated()
        {
            OnPhysicalConfigUpdated?.Invoke(_physical);
        }

        public void InteractionConfigWasUpdated()
        {
            OnInteractionConfigUpdated?.Invoke(_interactions);
        }
    }
}