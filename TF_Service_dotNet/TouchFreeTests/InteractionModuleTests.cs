using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

namespace TouchFreeTests
{
    public class InteractionModuleTests
    {

        private class TestInteractionModule : InteractionModule
        {
            public TestInteractionModule(
                HandManager _handManager,
                IVirtualScreenManager _virtualScreenManager,
                IConfigManager _configManager) : base(_handManager, _virtualScreenManager, _configManager, TrackedPosition.NEAREST)
            {
            }
        }
    }
}
