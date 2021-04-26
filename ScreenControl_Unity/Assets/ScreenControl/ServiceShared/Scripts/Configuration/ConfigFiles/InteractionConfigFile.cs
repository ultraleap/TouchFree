namespace Ultraleap.ScreenControl.Core
{
    public class InteractionConfigFile : ConfigFile<InteractionConfig, InteractionConfigFile>
    {
        protected override string _ConfigFileName => "InteractionConfig.json";
    }
}