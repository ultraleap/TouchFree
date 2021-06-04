using UnityEngine.EventSystems;
using Ultraleap.TouchFree.Tooling;
using Ultraleap.TouchFree.Tooling.Connection;

namespace Ultraleap.TouchFree.Tooling.InputControllers
{
    // Class: InputController
    // A layer over Unity's <BaseInput : https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.BaseInput.html>
    // that connects the BaseInput to TouchFree Tooling <InputActions> as they are
    // provided. Contains setup functions allowing inheritors to define only the input behaviour
    // they wish to see.
    //
    // Override <HandleInputAction> to react to InputActions as they are recieved.
    //
    // For an example InputController, see <UnityUIInputController>.
    public abstract class InputController : BaseInput
    {
        // Group: MonoBehaviour Overrides

        // Function: Start
        // Adds a listener to <InputActionManager> to invoke <HandleInputAction> with <InputActions> as they
        // are received.
        protected override void Start()
        {
            base.Start();
            InputActionManager.TransmitInputAction += HandleInputAction;
        }

        // Function: OnDestroy
        // Deregisters <HandleInputAction> from the <InputActionManager> so this can go out of scope.
        protected override void OnDestroy()
        {
            base.OnDestroy();
            InputActionManager.TransmitInputAction -= HandleInputAction;
        }

        // Functions:

        // Function: HandleInputAction
        // This method is the core of the functionality of this class. It will be invoked with
        // the <InputAction> as they are provided to the Tooling from the TouchFree Service.
        //
        // Override this function to implement any custom input handling functionality you wish to see.
        //
        // Parameters:
        //     _inputData - The latest input action recieved from TouchFree Service.
        protected virtual void HandleInputAction(InputAction _inputData)
        {
            switch (_inputData.InputType)
            {
                case InputType.MOVE:
                    break;

                case InputType.DOWN:
                    break;

                case InputType.UP:
                    break;

                case InputType.CANCEL:
                    break;
            }
        }
    }

}