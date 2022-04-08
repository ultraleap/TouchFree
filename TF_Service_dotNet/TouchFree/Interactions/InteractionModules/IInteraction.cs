using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Library.Interactions
{
    public interface IInteraction
    {
        InteractionType InteractionType { get; }
        InputActionResult Update();
    }
}
