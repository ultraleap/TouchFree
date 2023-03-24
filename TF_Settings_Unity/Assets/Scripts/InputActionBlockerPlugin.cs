using Ultraleap.TouchFree.Tooling;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class InputActionBlockerPlugin : InputActionPlugin
    {
        bool blocking = false;

        public void SetBlocking(bool _setTo)
        {
            blocking = _setTo;
        }

        protected override InputAction? ModifyInputAction(InputAction _inputAction)
        {
            if (blocking)
            {
                _inputAction.InputType = InputType.CANCEL;
            }

            return _inputAction;
        }
    }
}