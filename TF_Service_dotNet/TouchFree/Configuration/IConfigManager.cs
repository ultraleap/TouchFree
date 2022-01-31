namespace Ultraleap.TouchFree.Library.Configuration
{
    public interface IConfigManager
    {
        InteractionConfig InteractionConfig { get; set; }
        PhysicalConfig PhysicalConfig { get; set; }
        delegate void InteractionConfigEvent(InteractionConfig config = null);
        delegate void PhysicalConfigEvent(PhysicalConfig config = null);
        event InteractionConfigEvent OnInteractionConfigUpdated;
        event PhysicalConfigEvent OnPhysicalConfigUpdated;
        void PhysicalConfigWasUpdated();
        void InteractionConfigWasUpdated();
        void LoadConfigsFromFiles();
    }
}
