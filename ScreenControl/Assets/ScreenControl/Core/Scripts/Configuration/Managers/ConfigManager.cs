namespace Ultraleap.ScreenControl.Core
{
    public static class ConfigManager
    {
        public static GlobalSettings GlobalSettings
        {
            get
            {
                if (_globals == null)
                {
                    _globals = new GlobalSettings();
                }

                return _globals;
            }
            set
            {
                _globals = value;
            }
        }
        private static GlobalSettings _globals = null;

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
        private static InteractionConfig _interactions = null;

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
        private static PhysicalConfig _physical;

        public static void InitialiseConfigs()
        {
            _globals = new GlobalSettings();
            _interactions = InteractionConfigFile.LoadConfig();
            _physical = PhysicalConfigFile.LoadConfig();
        }
    }
}