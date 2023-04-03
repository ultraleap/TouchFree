namespace Ultraleap.TouchFree.Library.Configuration.QuickSetup;

public readonly record struct QuickSetupResponse(bool ConfigurationUpdated, bool PositionRecorded, Error QuickSetupError);