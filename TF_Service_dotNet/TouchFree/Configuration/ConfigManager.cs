using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public static class ConfigManager
    {
        public delegate void InteractionConfigEvent(InteractionConfig config = null);
        public delegate void PhysicalConfigEvent(PhysicalConfig config = null);
        public static event InteractionConfigEvent OnInteractionConfigUpdated;
        public static event PhysicalConfigEvent OnPhysicalConfigUpdated;
        private static InteractionConfig _interactions;
        private static PhysicalConfig _physical;

        public static InteractionConfig InteractionConfig
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

        public static PhysicalConfig PhysicalConfig
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

        public static void LoadConfigsFromFiles()
        {
            _interactions = InteractionConfigFile.LoadConfig();
            _physical = PhysicalConfigFile.LoadConfig();

            InteractionConfigWasUpdated();
            PhysicalConfigWasUpdated();
        }

        public static void PhysicalConfigWasUpdated()
        {
            OnPhysicalConfigUpdated?.Invoke(_physical);
        }

        public static void InteractionConfigWasUpdated()
        {
            OnInteractionConfigUpdated?.Invoke(_interactions);
        }
    }
}