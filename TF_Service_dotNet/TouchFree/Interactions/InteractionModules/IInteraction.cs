namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules;

public interface IInteraction
{
    InteractionType InteractionType { get; }
    InputActionResult Update(float confidence);
}