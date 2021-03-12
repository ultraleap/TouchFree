using UnityEngine;
using UnityEngine.EventSystems;

namespace Ultraleap.ScreenControl.Client.InputControllers
{
    // Class: UnityUIInputController
    // Provides Unity UI Input based on the incoming data from ScreenControl Service via a
    // <ServiceConnection>
    public class UnityUIInputController : InputController
    {
        // Group: Variables

        // Variable: inputModule
        // The <StandaloneInputModule: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/script-StandaloneInputModule.html>
        // that this Input Controller will override.
        // Will be found from the scene on <Start>
        [SerializeField]
        private StandaloneInputModule inputModule;

        // Variable: eventSystem
        // This is the Unity <EventSystem: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/EventSystem.html>
        // in the scene. We use this to dynamically resize the drag threshold to prevent
        // accidental drags instead of clicks.
        [SerializeField]
        private EventSystem eventSystem;

        // Group: Cached Input Information
        // These variables are determined whenever <HandleInputAction> is called and are used
        // to inform the inherited values in the section below when queried.
        private Vector2 touchPosition;
        private TouchPhase touchPhase = TouchPhase.Ended;
        private int baseDragThreshold = 100000;
        public bool sendHoverEvents = true;
        private bool isTouching = false;
        private bool isCancelled = true;

        // Group: Inherited Values
        // The remaining variables all come from Unity's <BaseInput: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.BaseInput.html>
        // and are overridden here so their values can be determined from the ScreenControl Service.
        public override Vector2 mousePosition => (sendHoverEvents && !isCancelled) ? touchPosition : base.mousePosition;
        public override bool mousePresent => (sendHoverEvents && !isCancelled) ? true : base.mousePresent;
        public override bool touchSupported => isTouching ? true : base.touchSupported;
        public override int touchCount => isTouching ? 1 : base.touchCount;
        public override Touch GetTouch(int index) => isTouching ? CheckForTouch(index) : base.GetTouch(index);

        // Group: Methods

        // Function: Start
        // Locates the EventSystem and StandaloneInputModule that need to be overridden
        protected override void Start()
        {
            base.Start();

            if (eventSystem == null)
            {
                eventSystem = FindObjectOfType<EventSystem>();
                inputModule = FindObjectOfType<StandaloneInputModule>();
            }

            inputModule.inputOverride = this;
        }

        // Function: CheckForTouch
        // Used in the override for <GetTouch> to update the current Touch state based on the
        // latest InputActions processed by <HandleInputAction>.
        //
        // Parameters:
        //     index - The Touch index, passed down from <GetTouch>
        private Touch CheckForTouch(int index)
        {
            if (touchPhase == TouchPhase.Ended || touchPhase == TouchPhase.Canceled)
            {
                isTouching = false;
            }

            return new Touch()
            {
                fingerId = index,
                position = touchPosition,
                radius = 0.1f,
                phase = touchPhase
            };
        }

        // Function: HandleInputAction
        // Called with each <ClientInputAction> as it comes into the <ServiceConnection>. Updates the
        // underlying InputModule and EventSystem based on the incoming actions.
        //
        // Parameters:
        //     _inputData - The latest Action to arrive via the <ServiceConnection>.
        protected override void HandleInputAction(ClientInputAction _inputData)
        {
            base.HandleInputAction(_inputData);

            InputType type = _inputData.InputType;
            Vector2 cursorPosition = _inputData.CursorPosition;

            touchPosition = cursorPosition;
            isCancelled = false;

            switch (type)
            {
                case InputType.DOWN:
                    touchPhase = TouchPhase.Began;
                    eventSystem.pixelDragThreshold = 0;
                    isTouching = true;
                    break;

                case InputType.MOVE:
                    touchPhase = TouchPhase.Moved;
                    break;

                case InputType.CANCEL:
                    touchPhase = TouchPhase.Canceled;
                    eventSystem.pixelDragThreshold = baseDragThreshold;
                    isCancelled = true;
                    break;

                case InputType.UP:
                    touchPhase = TouchPhase.Ended;
                    eventSystem.pixelDragThreshold = baseDragThreshold;
                    break;
            }
        }
    }
}