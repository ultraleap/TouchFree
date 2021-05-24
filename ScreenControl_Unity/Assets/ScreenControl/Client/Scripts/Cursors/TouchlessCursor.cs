using UnityEngine;
using Ultraleap.ScreenControl.Client.Connection;

namespace Ultraleap.ScreenControl.Client.Cursors
{
    // Class: TouchlessCursor
    // This class is a base class for creating custom Touchless cursors for use with ScreenControl.
    //
    // Override <HandleInputAction> to react to <ClientInputActions> as they are recieved.
    //
    // For an example of a reactive cursor, see <DotCursor>.
    public class TouchlessCursor : MonoBehaviour
    {
        // Group: Variables

        // Variable: cursorTransform
        // The transform for the image presented by this cursor
        public RectTransform cursorTransform;
        protected Vector2 targetPos;

        public Color primaryColor
        { 
            get { return primaryColor; }
            protected set { primaryColor = value; }
        }
        public Color secondaryColor
        { 
            get { return secondaryColor; } 
            protected set { secondaryColor = value; }
        }
        public Color tertiaryColor
        { 
            get { return tertiaryColor; } 
            protected set { tertiaryColor = value; }
        }

        // Group: MonoBehaviour Overrides

        // Function: Update
        // Runs on Unity's update loop to keep the attached Cursor at the position
        // last stored in <HandleInputAction>
        protected virtual void Update()
        {
            cursorTransform.anchoredPosition = targetPos;
        }

        // Function: OnEnable
        // Initialises & displays the cursor to its default state when the scene is fully loaded.
        // Also registers the Cursor for updates from the <InputActionManager>
        protected virtual void OnEnable()
        {
            InputActionManager.TransmitInputAction += HandleInputAction;
            InitialiseCursor();
        }

        // Function: OnDisable
        // Deregisters the Cursor so it no longer recieves updates from the
        // <InputActionManager>
        protected virtual void OnDisable()
        {
            InputActionManager.TransmitInputAction -= HandleInputAction;
        }

        // Group: Functions

        // Function: HandleInputAction
        // The core of the logic for Cursors, this is invoked with each <ClientInputAction> as
        // they are recieved. Override this function to implement cursor behaviour in response.
        //
        // Parameters:
        //    _inputData - The latest input action recieved from ScreenControl Service.
        protected virtual void HandleInputAction(ClientInputAction _inputData)
        {
            targetPos = _inputData.CursorPosition;
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

        // Function: SetColors
        // Used to change the colors of the cursor at runtime. Override it to update the specific
        // UI elements that the colors relate to.
        public virtual void SetColors(Color _primary, Color _secondary, Color _tertiary)
        {
            primaryColor = _primary;
            secondaryColor = _secondary;
            tertiaryColor = _tertiary;
        }
    }
}