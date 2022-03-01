namespace Ultraleap.TouchFree.Library.Configuration
{
    public interface IConfigManager
    {
        InteractionConfigInternal InteractionConfig { get; set; }
        PhysicalConfigInternal PhysicalConfig { get; set; }
        InteractionConfig InteractionConfigFromApi { set; }
        PhysicalConfig PhysicalConfigFromApi { set; }
        delegate void InteractionConfigEvent(InteractionConfigInternal config = null);
        delegate void PhysicalConfigEvent(PhysicalConfigInternal config = null);
        event InteractionConfigEvent OnInteractionConfigUpdated;
        event PhysicalConfigEvent OnPhysicalConfigUpdated;
        void PhysicalConfigWasUpdated();
        void InteractionConfigWasUpdated();
        void LoadConfigsFromFiles();
    }
}
