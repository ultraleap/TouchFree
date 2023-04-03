using System;

namespace Ultraleap.TouchFree.Library.Configuration;

public class ServiceConfigFile : ConfigFile<ServiceConfig, ServiceConfigFile>
{
    protected override string _ConfigFileName => "ServiceConfig.json";
}

[Serializable]
public record ServiceConfig
{
    public string ServiceIP = "127.0.0.1";
    public string ServicePort = "9739";
}