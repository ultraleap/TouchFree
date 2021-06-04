namespace Ultraleap.TouchFree.ServiceShared
{
    public class InteractionConfigFile : ConfigFile<InteractionConfig, InteractionConfigFile>
    {
        protected override string _ConfigFileName => "InteractionConfig.json";
    }
}