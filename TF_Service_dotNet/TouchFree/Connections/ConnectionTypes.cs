using System;

namespace Ultraleap.TouchFree.Library.Connections;

public enum Compatibility
{
    COMPATIBLE,
    SERVICE_OUTDATED,
    CLIENT_OUTDATED,
    SERVICE_OUTDATED_WARNING,
    CLIENT_OUTDATED_WARNING
}

public readonly record struct CompatibilityInformation(Compatibility Compatibility, Version ClientVersion, Version ServiceVersion);

internal readonly record struct CommunicationWrapper<T>(in string action, in T content);