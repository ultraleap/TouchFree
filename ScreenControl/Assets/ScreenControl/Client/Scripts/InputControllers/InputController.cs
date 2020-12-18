using UnityEngine.EventSystems;

namespace Ultraleap.ScreenControl.Client
{
    namespace InputControllers
    {
        /// <summary>
        /// A base Input Controller to derive from. This base class handles moving the standard cursor.
        /// </summary>
        public abstract class InputController : BaseInput
        {
            int customInputActionListeners = 0;

            protected override void Start()
            {
                base.Start();
                ConnectionManager.AddConnectionListener(OnCoreConnection);
            }

            protected void OnCoreConnection()
            {
                ConnectionManager.coreConnection.TransmitInputAction += HandleInputAction;
            }

            /// <summary>
            /// Used to change the way the input controller receives InputAction events to custom/modified events
            /// </summary>
            /// <param name="_customInputActionEvent"></param>
            public void AddCustomInputActionListener(ref CoreConnection.ClientInputActionEvent _customInputActionEvent)
            {
                if (customInputActionListeners == 0)
                {
                    ConnectionManager.coreConnection.TransmitInputAction -= HandleInputAction;
                }

                customInputActionListeners++;
                _customInputActionEvent += HandleInputAction;
            }

            public void RemoveCustomInputActionListener(ref CoreConnection.ClientInputActionEvent _customInputActionEvent)
            {
                customInputActionListeners--;
                _customInputActionEvent -= HandleInputAction;

                if(customInputActionListeners == 0)
                {
                    OnCoreConnection();
                }
            }

            protected override void OnDestroy()
            {
                base.OnDestroy();

                if (ConnectionManager.coreConnection != null)
                {
                    ConnectionManager.coreConnection.TransmitInputAction -= HandleInputAction;
                }
            }

            protected virtual void HandleInputAction(ScreenControlTypes.ClientInputAction _inputData)
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