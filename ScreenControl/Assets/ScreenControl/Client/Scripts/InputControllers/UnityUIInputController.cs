using UnityEngine;
using UnityEngine.EventSystems;

namespace Ultraleap.ScreenControl.Client
{
    namespace InputControllers
    {
        /// <summary>
        /// A class for dealing with Unity UI interactions based on ScreenControl inputs.
        /// </summary>
        public class UnityUIInputController : InputController
        {
            [SerializeField] private StandaloneInputModule inputModule;
            [SerializeField] private EventSystem eventSystem;

            public override Vector2 mousePosition => isHovering ? touchPosition : base.mousePosition;
            public override bool mousePresent => isHovering? true : base.mousePresent;
            public override bool touchSupported => isTouching ? true : base.touchSupported;
            public override int touchCount => isTouching ? 1 : base.touchCount;
            public override Touch GetTouch(int index) => isTouching ? CheckForTouch(index) : base.GetTouch(index);
            public bool isHovering = true;

            private Vector2 touchPosition;
            private TouchPhase touchPhase = TouchPhase.Ended;
            private TouchPhase previousTouchPhase;
            private int baseDragThreshold = 100000;
            private bool isTouching = false;

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


            private Touch CheckForTouch(int index)
            {
                previousTouchPhase = touchPhase;
                return new Touch()
                {
                    fingerId = index,
                    position = touchPosition,
                    radius = 0.1f,
                    phase = touchPhase
                };
            }

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
                        // TODO: this causes immediate clicks, we need to stop sending move events from Core after a 'DOWN' click until an 'UP' click unless we dragged
                        touchPhase = TouchPhase.Ended;
                        break;

                    case ScreenControlTypes.InputType.CANCEL:
                        touchPhase = TouchPhase.Canceled;
                        eventSystem.pixelDragThreshold = baseDragThreshold;
                        isTouching = false;
                        break;

                    case ScreenControlTypes.InputType.UP:
                        touchPhase = TouchPhase.Ended;
                        eventSystem.pixelDragThreshold = baseDragThreshold;
                        isTouching = false;
                        break;
                }
            }
        }
    }
}