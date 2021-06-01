using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public class PhysicalConfigFile : ConfigFile<PhysicalConfig, PhysicalConfigFile>
    {
        protected override string _ConfigFileName => "PhysicalConfig.json";
    }
}