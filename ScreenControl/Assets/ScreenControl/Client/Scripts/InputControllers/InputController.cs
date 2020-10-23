using UnityEngine;
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
            protected override void Start()
            {
                base.Start();

                ConnectionManager.AddConnectionListener(OnCoreConnection);
            }

            protected void OnCoreConnection()
            {
                ConnectionManager.coreConnection.TransmitInputAction += HandleInputAction;
            }

            protected override void OnDestroy()
            {
                base.OnDestroy();
                ConnectionManager.coreConnection.TransmitInputAction -= HandleInputAction;
            }

            protected virtual void HandleInputAction(ScreenControlTypes.ClientInputAction _inputData)
            {
                switch (_inputData.Type)
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