namespace Ultraleap.TouchFree.ServiceShared
{
    public class PhysicalConfigFile : ConfigFile<PhysicalConfig, PhysicalConfigFile>
    {
        protected override string _ConfigFileName => "PhysicalConfig.json";
    }
}