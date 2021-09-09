namespace Ultraleap.TouchFree.ServiceShared
{
    public class TouchFreeAppConfigFile : ConfigFile<ConfigurationState, TouchFreeAppConfigFile>
    {
        protected override string _ConfigFileName => "TouchFreeConfig.json";
    }
}