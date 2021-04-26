using UnityEngine.EventSystems;
using Ultraleap.ScreenControl.Client;
using Ultraleap.ScreenControl.Client.Connection;

namespace Ultraleap.ScreenControl.Client.InputControllers
{
    // Class: InputController
    // A layer over Unity's <BaseInput : https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.BaseInput.html>
    // that connects the BaseInput to ScreenControl's <ClientInputActions> as they are
    // provided. Contains setup functions allowing inheritors to define only the input behaviour
    // they wish to see.
    //
    // Override <HandleInputAction> to react to ClientInputActions as they are recieved.
    //
    // For an example InputController, see <UnityUIInputController>.
    public abstract class InputController : BaseInput
    {
        // Group: MonoBehaviour Overrides

        // Function: Start
        // Adds a listener to <ConnectionManager> to invoke <HandleInputAction> with <ClientInputActions> as they
        // are received.
        protected override void Start()
        {
            base.Start();
            ConnectionManager.TransmitInputAction += HandleInputAction;
        }

        // Function: OnDestroy
        // Deregisters <HandleInputAction> from the <ConnectionManager> so this can go out of scope.
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ConnectionManager.TransmitInputAction -= HandleInputAction;
        }

        // Functions:

        // Function: HandleInputAction
        // This method is the core of the functionality of this class. It will be invoked with
        // the <ClientInputAction> as they are provided to the Client from the ScreenControl Service.
        //
        // Override this function to implement any custom input handling functionality you wish to see.
        //
        // Parameters:
        //     _inputData - The latest input action recieved from ScreenControl Service.
        protected virtual void HandleInputAction(ClientInputAction _inputData)
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