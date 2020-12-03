using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public class PhysicalConfigFile : ConfigFile<PhysicalConfig, PhysicalConfigFile>
    {
        public override string ConfigFileName => "PhysicalConfig.json";
    }
}