using UnityEngine;
using UnityEngine.EventSystems;

namespace Ultraleap.ScreenControl.Client
{
    namespace InputControllers
    {
        // Class: UnityUIInputController
        // Provides Unity UI Input based on the incoming data from ScreenControl Service via a
        // <WebSocketCoreConnection>
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
            // This is the Unity<EventSystem: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/EventSystem.html>
            // in the scene. We use this to dynamically resize the drag threshold to prevent
            // accidental drags instead of clicks.
            [SerializeField]
            private EventSystem eventSystem;

            // Group: Cached Input Information
            // These variables are determined whenever<HandleInputAction> is called and are used
            // to inform the inherited values in the section below when queried.
            private Vector2 touchPosition;
            private TouchPhase touchPhase = TouchPhase.Ended;
            private TouchPhase previousTouchPhase;
            private int baseDragThreshold = 100000;
            public bool isHovering = false;
            private bool isTouching = false;

            // Group: Inherited Values
            // The remaining variables all come from Unity's <BaseInput: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.BaseInput.html>
            // and are overridden here so their values can be determined from the Screen Control Service.
            public override Vector2 mousePosition => isHovering ? touchPosition : base.mousePosition;
            public override bool mousePresent => isHovering ? true : base.mousePresent;
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
            // latest InputActions processed by<HandleInputAction>.
            // Parameters:
            //     index - The Touch index, passed down from<GetTouch>
            private Touch CheckForTouch(int index)
            {
                previousTouchPhase = touchPhase;

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
            // Called with each<ClientInputAction> as it comes into the <CoreConnection>.
            // Updates the underlying InputModule and EventSystem based on the incoming actions.
            // Parameters:
            //     _inputData - The latest Action to arrive via the<CoreConnection>.
            protected override void HandleInputAction(ScreenControlTypes.ClientInputAction _inputData)
            {
                base.HandleInputAction(_inputData);

                ScreenControlTypes.InputType type = _inputData.InputType;
                Vector2 cursorPosition = _inputData.CursorPosition;
                float distanceFromScreen = _inputData.ProgressToClick;

                touchPosition = cursorPosition;

                switch (type)
                {
                    case ScreenControlTypes.InputType.DOWN:
                        touchPhase = TouchPhase.Began;
                        eventSystem.pixelDragThreshold = 0;
                        isTouching = true;
                        break;

                    case ScreenControlTypes.InputType.MOVE:
                        touchPhase = TouchPhase.Moved;
                        break;

                    case ScreenControlTypes.InputType.CANCEL:
                        touchPhase = TouchPhase.Canceled;
                        eventSystem.pixelDragThreshold = baseDragThreshold;
                        break;

                    case ScreenControlTypes.InputType.UP:
                        touchPhase = TouchPhase.Ended;
                        eventSystem.pixelDragThreshold = baseDragThreshold;
                        break;
                }
            }
        }
    }
}