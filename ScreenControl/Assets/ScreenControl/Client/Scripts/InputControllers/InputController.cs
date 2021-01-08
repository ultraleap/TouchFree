using UnityEngine.EventSystems;
using Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Client
{
    namespace InputControllers
    {
        /**
            Class: InputController
            A layer over Unity's <BaseInput: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.BaseInput.html>
            that connects the BaseInput to Screen Control's <ClientInputActions> as they are provided.
            Provides setup functions allowing inheritors to define only the behaviour.

            Override <HandleInputAction> to react to ClientInputAction as they are recieved.

            For an example InputController, see <UnityUIInputController>, which translates
            ClientInputActions into Unity UI events, allowing ScreenControl to be used with most
            Unity UIs.
         */
        public abstract class InputController : BaseInput
        {
            // Group: MonoBehaviour Overrides

            /**
                Function: Start

                Provides <OnCoreConnection> to be triggered once a <CoreConnection> is established.

                (in almost all cases, this will be a <WebSocketCoreConnection>)
             */
            protected override void Start()
            {
                base.Start();
                ConnectionManager.AddConnectionListener(OnCoreConnection);
            }

            /**
                Function: OnDestroy

                Deregisters <HandleInputAction> from the active <CoreConnection> so this can go out
                of scope.
             */
            protected override void OnDestroy()
            {
                base.OnDestroy();

                if (ConnectionManager.coreConnection != null)
                {
                    ConnectionManager.coreConnection.TransmitInputAction -= HandleInputAction;
                }
            }

            // Functions:

            /**
                Function: OnCoreConnection

                Registers <HandleInputAction> as a listener to recieve <ClientInputActions> from a
                <CoreConnection>.

                (in almost all cases, this will be a <WebSocketCoreConnection>)
             */
            protected void OnCoreConnection()
            {
                ConnectionManager.coreConnection.TransmitInputAction += HandleInputAction;
            }

            /**
                Function: HandleInputAction

                This method is the core of the functionality of this class. It will be invoked with
                the <ClientInputAction> as they are provided to the Client from the ScreenControl Service.

                Override this function to implement any custom input handling functionality you wish to see.

                Parameters:

                    _inputData - The latest input action recieved from Screen Control Service.
             */
            protected virtual void HandleInputAction(ClientInputAction _inputData)
            {
                switch (_inputData.InputType)
                {
                    case ScreenControlTypes.InputType.MOVE:
                        break;

                    case ScreenControlTypes.InputType.DOWN:
                        break;

                    case ScreenControlTypes.InputType.UP:
                        break;

                    case ScreenControlTypes.InputType.CANCEL:
                        break;
                }
            }
        }
    }
}