namespace Ultraleap.TouchFree.Service.Configuration
{
    public class PhysicalConfigFile : ConfigFile<PhysicalConfig, PhysicalConfigFile>
    {
        protected override string _ConfigFileName => "PhysicalConfig.json";
    }
}