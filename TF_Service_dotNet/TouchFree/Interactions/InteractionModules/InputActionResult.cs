namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules
{
    public class InputActionResult
    {
        public readonly bool actionDetected = false;
        public readonly InputAction? inputAction;
        public readonly float confidence;

        public InputActionResult () { }

        public InputActionResult (InputAction _inputAction, float _confidence)
        {
            inputAction = _inputAction;
            confidence = _confidence;
            actionDetected = true;
        }
    }
}
