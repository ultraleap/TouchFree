namespace Ultraleap.TouchFree.Library.Interactions
{
    public interface IInteraction
    {
        InteractionType InteractionType { get; }
        InputAction? Update();
    }
}
