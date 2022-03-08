namespace Ultraleap.TouchFree.Library.Configuration
{
    public interface IConfigManager
    {
        InteractionConfig InteractionConfig { get; set; }
        PhysicalConfig PhysicalConfig { get; set; }
        event InteractionConfigEvent OnInteractionConfigUpdated;
        event PhysicalConfigEvent OnPhysicalConfigUpdated;
        void PhysicalConfigWasUpdated();
        void InteractionConfigWasUpdated();
        void LoadConfigsFromFiles();
    }

    public delegate void InteractionConfigEvent(InteractionConfig config = null);
    public delegate void PhysicalConfigEvent(PhysicalConfig config = null);
}
