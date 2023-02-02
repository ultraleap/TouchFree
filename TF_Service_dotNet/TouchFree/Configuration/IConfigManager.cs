namespace Ultraleap.TouchFree.Library.Configuration
{
    public interface IConfigManager
    {
        InteractionConfigInternal InteractionConfig { get; set; }
        PhysicalConfigInternal PhysicalConfig { get; set; }
        TrackingConfig TrackingConfig { get; set; }
        TrackingLoggingConfig TrackingLoggingConfig { get; set; }
        InteractionConfig InteractionConfigFromApi { set; }
        PhysicalConfig PhysicalConfigFromApi { set; }
        
        delegate void InteractionConfigEvent(InteractionConfigInternal config = null);
        delegate void PhysicalConfigEvent(PhysicalConfigInternal config = null);
        delegate void TrackingConfigEvent(TrackingConfig config = null);
        delegate void TrackingLoggingConfigEvent(TrackingLoggingConfig config = null);
        
        event InteractionConfigEvent OnInteractionConfigUpdated;
        event PhysicalConfigEvent OnPhysicalConfigUpdated;
        event TrackingConfigEvent OnTrackingConfigSaved;
        event TrackingConfigEvent OnTrackingConfigUpdated;
        event TrackingLoggingConfigEvent OnTrackingLoggingConfigUpdated;
        
        void PhysicalConfigWasUpdated();
        void InteractionConfigWasUpdated();
        void TrackingLoggingConfigWasUpdated();
        void LoadConfigsFromFiles();
        bool ErrorLoadingConfigFiles { get; }
        bool AreConfigsInGoodState();
    }
}
