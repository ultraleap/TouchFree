namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules
{
    public class InputActionResult
    {
        public readonly bool actionDetected = false;
        public readonly InputType inputType;
        public readonly Positions positions;
        public readonly float progressToClick;

        public InputActionResult () { }

        public InputActionResult (InputType _inputType, Positions _positions, float _progressToClick)
        {
            inputType = _inputType;
            positions = _positions;
            progressToClick = _progressToClick;
            actionDetected = true;
        }
    }
}
