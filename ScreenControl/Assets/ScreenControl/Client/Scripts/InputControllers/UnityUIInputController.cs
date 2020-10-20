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

            public override Vector2 mousePosition => IsHovering() ? touchPosition : base.mousePosition;
            public override bool mousePresent => IsHovering() ? true : base.mousePresent;
            public override bool touchSupported => IsTouching() ? true : base.touchSupported;
            public override int touchCount => IsTouching() ? 1 : base.touchCount;
            public override Touch GetTouch(int index) => IsTouching() ? CheckForTouch(index) : base.GetTouch(index);

            private Vector2 touchPosition;
            private TouchPhase touchPhase = TouchPhase.Ended;
            private TouchPhase previousTouchPhase;
            private int baseDragThreshold = 100000;

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

            private bool IsHovering()
            {
                //TODO: see if this is needed? or if we can store it from an inputaction?
                //if (HandManager.Instance.PrimaryHand != null)
                //{
                    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }

            private bool IsTouching()
            { 
                if ((touchPhase == TouchPhase.Ended && previousTouchPhase == TouchPhase.Ended)
                    || (touchPhase == TouchPhase.Canceled && previousTouchPhase == TouchPhase.Canceled))
                {
                    return false;
                }

                return true;
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

                ScreenControlTypes.InputType type = _inputData.Type;
                Vector2 cursorPosition = _inputData.CursorPosition;
                float distanceFromScreen = _inputData.ProgressToClick;

                touchPosition = cursorPosition;

                if (type == ScreenControlTypes.InputType.MOVE)
                {
                    return;
                }

                touchPhase = TouchPhase.Ended;
                eventSystem.pixelDragThreshold = baseDragThreshold;

                switch (type)
                {
                    case ScreenControlTypes.InputType.DOWN:
                        touchPhase = TouchPhase.Began;
                        break;
                        //TODO: make this functionality work without passing around hold and drag events
                    //case InputType.HOLD:
                    //    touchPhase = TouchPhase.Moved;
                    //    break;
                    //case InputType.DRAG:
                    //    touchPhase = TouchPhase.Moved;
                    //    eventSystem.pixelDragThreshold = 0;
                    //    break;
                    case ScreenControlTypes.InputType.CANCEL:
                        touchPhase = TouchPhase.Canceled;
                        break;
                    case ScreenControlTypes.InputType.UP:
                        touchPhase = TouchPhase.Ended;
                        break;
                }
            }
        }
    }
}