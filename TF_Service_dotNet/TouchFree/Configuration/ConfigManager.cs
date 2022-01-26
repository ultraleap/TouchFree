using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class ConfigManager : IConfigManager
    {
        public event IConfigManager.InteractionConfigEvent OnInteractionConfigUpdated;
        public event IConfigManager.PhysicalConfigEvent OnPhysicalConfigUpdated;

        public InteractionConfig InteractionConfig
        {
            get
            {
                if (_interactions == null)
                {
                    _interactions = InteractionConfigFile.LoadConfig();
                }

                return _interactions;
            }
            set
            {
                _interactions = value;
            }
        }
        private InteractionConfig _interactions;

        public PhysicalConfig PhysicalConfig
        {
            get
            {
                if (_physical == null)
                {
                    _physical = PhysicalConfigFile.LoadConfig();
                }

                return _physical;
            }
            set
            {
                _physical = value;
            }
        }
        private PhysicalConfig _physical;

        public void LoadConfigsFromFiles()
        {
            _interactions = InteractionConfigFile.LoadConfig();
            _physical = PhysicalConfigFile.LoadConfig();

            InteractionConfigWasUpdated();
            PhysicalConfigWasUpdated();
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