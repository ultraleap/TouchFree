using System;

namespace Ultraleap.TouchFree.Library.Configuration.QuickSetup
{
    [Serializable]
    public struct QuickSetupRequest
    {
        public QuickSetupPosition Position { get; set; }
        public string requestID { get; set; }
    }
}
