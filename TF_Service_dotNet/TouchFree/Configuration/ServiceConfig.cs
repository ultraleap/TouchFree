using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    [Serializable]
    public class ServiceConfig
    {
        public string Interface { get; set; }
        public string Port { get; set; }
    }
}