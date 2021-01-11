using UnityEngine;
using Ultraleap.ScreenControl.Client.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Client.Cursors
{
    // Class: TouchlessCursor
    // This class is a base class for creating custom Touchless cursors for use with ScreenControl.
    //
    // Override <HandleInputAction> to react to ClientInputAction as they are recieved.
    //
    // For an example of a reactive cursor, see <DotCursor>, which positions a cursor at the
    // provided position and presents a scaling ring around the dot to present to a user how
    // close to "clicking" they are.
    public class TouchlessCursor : MonoBehaviour
    {
        // Group: Variables

        // Variable: cursorTransform
        // The transform for the image presented by this cursor
        public RectTransform cursorTransform;
        protected Vector2 _targetPos;

        // Group: MonoBehaviour Overrides

        // Function: Update
        // Runs on Unity's update loop to keep the attached Cursor at the position
        // of the position last stored in <HandleInputAction>
        protected virtual void Update()
        {
            cursorTransform.anchoredPosition = _targetPos;
        }

        // Function: OnEnable
        // Initialises & displays the cursor to its default state when the scene is fully loaded.
        // Also registers the Cursor for updates from the <WebSocketCoreConnection>
        protected virtual void OnEnable()
        {
            ConnectionManager.AddConnectionListener(OnCoreConnection);
            InitialiseCursor();
            ShowCursor();
        }

        // Function: OnDisable
        // Deregisters the Cursor so it no longer recieves updates from the
        // <WebSocketCoreConnection>
        protected virtual void OnDisable()
        {
            if (ConnectionManager.serviceConnection != null)
            {
                ConnectionManager.serviceConnection.TransmitInputAction -= HandleInputAction;
            }
        }

        // Group: Functions

        // Function: OnCoreConnection
        // Passed to a <WebSocketCoreConnection> to be invoked once a connection is set up. Adds
        // <HandleInputAction> as a listener to <ClientInputActions> as they are recieved.
        protected virtual void OnCoreConnection()
        {
            ConnectionManager.serviceConnection.TransmitInputAction += HandleInputAction;
        }

        // Function: HandleInputAction
        // The core of the logic for Cursors, this is invoked with each <ClientInputAction> as
        // they are recieved. Override this function to implement cursor behaviour in response.
        //
        // Parameters:
        //    _inputData - The latest input action recieved from Screen Control Service.
        protected virtual void HandleInputAction(ClientInputAction _inputData)
        {
            _targetPos = _inputData.CursorPosition;
        }

        // Function: InitialiseCursor
        // Invoked when the parent GameObject is enabled.
        // Override this function with any intiialisation steps your cursor needs.
        protected virtual void InitialiseCursor()
        {
        }

        // Function: ShowCursor
        // This ensures that all Cursors will have the ability to be shown or hidden. Be sure to
        // override this function to cover the showing of anything an inheriting cursor uses.
        public virtual void ShowCursor()
        {
            cursorTransform.gameObject.SetActive(true);
        }

        // Function: HideCursor
        // This ensures that all Cursors will have the ability to be shown or hidden. Be sure to
        // override this function to cover the hiding of anything an inheriting cursor uses.
        public virtual void HideCursor()
        {
            cursorTransform.gameObject.SetActive(false);
        }
    }
}