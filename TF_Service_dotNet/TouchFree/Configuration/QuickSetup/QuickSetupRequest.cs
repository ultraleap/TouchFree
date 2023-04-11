using System;

namespace Ultraleap.TouchFree.Library.Configuration.QuickSetup;

[Serializable]
public readonly record struct QuickSetupRequest(QuickSetupPosition Position, string requestID);