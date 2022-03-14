using UnityEngine;
using Ultraleap.TouchFree.Tooling.Connection;

namespace Ultraleap.TouchFree.Tooling.Cursors
{
    // Class: TouchlessCursor
    // This class is a base class for creating custom Touchless cursors for use with
    // TouchFree Tooling.
    //
    // Override <HandleInputAction> to react to <InputActions> as they are recieved.
    //
    // For an example of a reactive cursor, see <DotCursor>.
    public class TouchlessCursor : MonoBehaviour
    {
        // Group: Variables

        // Variable: cursorTransform
        // The transform for the image presented by this cursor
        public RectTransform cursorTransform;
        protected Vector2 targetPos;

        // Variable: cursorSize
        // The standard size of the cursor
        public float cursorSize = 0.25f;

        // Variable: cursorRingThickness
        // The thickness of the cursor ring (if it has one)
        public float cursorRingThickness = 1.5f;

        // Variable: minRingThickness
        // The minimum thickness the ring can be.
        public float minRingThickness = 1.5f;

        // Variable: maxRingThickness
        // The maximum thickness the ring can be.
        public float maxRingThickness = 10;

        public Color primaryColor
        {
            get { return _primaryColor; }
        }
        [SerializeField] protected Color _primaryColor = new Color(1, 1, 1, 1);

        public Color secondaryColor
        {
            get { return _secondaryColor; }
        }
        [SerializeField] protected Color _secondaryColor = new Color(1, 1, 1, 1);

        public Color tertiaryColor
        {
            get { return _tertiaryColor; }
        }
        [SerializeField] protected Color _tertiaryColor = new Color(0, 0, 0, 1);

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
        // The core of the logic for Cursors, this is invoked with each <InputAction> as
        // they are recieved. Override this function to implement cursor behaviour in response.
        //
        // Parameters:
        //    _inputData - The latest input action recieved from TouchFree Service.
        protected virtual void HandleInputAction(InputAction _inputData)
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
            _primaryColor = _primary;
            _secondaryColor = _secondary;
            _tertiaryColor = _tertiary;
        }

        // Function: SetRingThickness
        // Used to set the <cursorRingThickness> value. Can be overridden to remap or clamp this value.
        // _thickness should range from 0-1 0 being the thinnest and 1 being the thickest.
        public virtual void SetRingThickness(float _thickness)
        {
            cursorRingThickness = Utilities.MapRangeToRange(_thickness, 0, 1, minRingThickness, maxRingThickness);
        }
    }
}