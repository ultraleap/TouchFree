namespace Ultraleap.TouchFree.Library.Configuration;

public interface IConfigManager
{
    InteractionConfigInternal InteractionConfig { get; set; }
    PhysicalConfigInternal PhysicalConfig { get; set; }
    TrackingConfig TrackingConfig { get; set; }
    ServiceConfig ServiceConfig { get; set; }
    TouchFreeConfig TouchFreeConfig { get; set; }

    InteractionConfig InteractionConfigFromApi { set; }
    PhysicalConfig PhysicalConfigFromApi { set; }

    delegate void InteractionConfigEvent(InteractionConfigInternal config = null);
    delegate void PhysicalConfigEvent(PhysicalConfigInternal config = null);
    delegate void TrackingConfigEvent(TrackingConfig config = null);
    delegate void ServiceConfigEvent(ServiceConfig config = null);
    delegate void TouchFreeConfigEvent(TouchFreeConfig config = null);

    event InteractionConfigEvent OnInteractionConfigUpdated;
    event PhysicalConfigEvent OnPhysicalConfigUpdated;
    event TrackingConfigEvent OnTrackingConfigSaved;
    event TrackingConfigEvent OnTrackingConfigUpdated;
    event ServiceConfigEvent OnServiceConfigUpdated;
    event TouchFreeConfigEvent OnTouchFreeConfigUpdated;

    void PhysicalConfigWasUpdated();
    void InteractionConfigWasUpdated();

    void LoadConfigsFromFiles();
    bool ErrorLoadingConfigFiles { get; }
    bool AreConfigsInGoodState();
}