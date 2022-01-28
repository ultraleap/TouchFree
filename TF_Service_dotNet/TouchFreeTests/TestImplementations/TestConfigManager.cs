using Ultraleap.TouchFree.Library.Configuration;

namespace TouchFreeTests.TestImplementations
{
    public class TestConfigManager : IConfigManager
    {
        public InteractionConfig InteractionConfig { get => interactionConfig; set { interactionConfig = value; } }
        public PhysicalConfig PhysicalConfig { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        private InteractionConfig interactionConfig = new InteractionConfig();

        public event IConfigManager.InteractionConfigEvent OnInteractionConfigUpdated;
        public event IConfigManager.PhysicalConfigEvent OnPhysicalConfigUpdated;

        public void InteractionConfigWasUpdated()
        {
            throw new System.NotImplementedException();
        }

        public void LoadConfigsFromFiles()
        {
            throw new System.NotImplementedException();
        }

        public void PhysicalConfigWasUpdated()
        {
            throw new System.NotImplementedException();
        }
    }
}
