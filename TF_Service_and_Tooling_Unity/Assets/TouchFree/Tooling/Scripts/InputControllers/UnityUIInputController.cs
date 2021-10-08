using UnityEngine;
using UnityEngine.EventSystems;

namespace Ultraleap.TouchFree.Tooling.InputControllers
{
    // Class: UnityUIInputController
    // Provides Unity UI Input based on the incoming data from TouchFree Service via a
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

        public bool sendHoverEvents = true;

        public TouchData primaryTouchData = new TouchData();
        public TouchData secondaryTouchData = new TouchData();

        // Group: Inherited Values
        // The remaining variables all come from Unity's <BaseInput: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.BaseInput.html>
        // and are overridden here so their values can be determined from the TouchFree Service.
        public override Vector2 mousePosition => IsTouchFreeInputWithinScreen() ? primaryTouchData.touchPosition : base.mousePosition;
        public override bool mousePresent => IsTouchFreeInputWithinScreen() ? true : base.mousePresent;
        public override bool touchSupported => true;
        public override int touchCount => GetTouchCount() != 0 ? GetTouchCount() : base.touchCount;

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
            eventSystem.pixelDragThreshold = 0;
        }

        // Function: GetTouch
        // Used to update the current Touch state based on the
        // latest InputActions processed by <HandleInputAction>.
        //
        // Parameters:
        //     index - The Touch index, passed down from <GetTouch>
        public override Touch GetTouch(int index)
        {
            if(touchCount == 0)
            {
                return base.GetTouch(index);
            }
            
            if(touchCount == 1)
            {
                if(primaryTouchData.isTouching)
                {
                    return GetTouchFromData(index, ref primaryTouchData);
                }
                else
                {
                    return GetTouchFromData(index, ref secondaryTouchData);
                }
            }
            else
            {
                if (index == 0)
                {
                    return GetTouchFromData(index, ref primaryTouchData);
                }
                else
                {
                    return GetTouchFromData(index, ref secondaryTouchData);
                }
            }
        }

        Touch GetTouchFromData(int index,  ref TouchData _touchData)
        {
            if (_touchData.touchPhase == TouchPhase.Ended || _touchData.touchPhase == TouchPhase.Canceled)
            {
                _touchData.isTouching = false;
            }

            return new Touch()
            {
                fingerId = index,
                position = _touchData.touchPosition,
                radius = 0.1f,
                phase = _touchData.touchPhase
            };
        }

        int GetTouchCount()
        {
            int count = 0;

            if (primaryTouchData.isTouching)
            {
                count++;
            }

            if(secondaryTouchData.isTouching)
            {
                count++;
            }

            return count;
        }

        // Function: HandleInputAction
        // Called with each <InputAction> as it comes into the <ServiceConnection>. Updates the
        // underlying InputModule and EventSystem based on the incoming actions.
        //
        // Parameters:
        //     _inputData - The latest Action to arrive via the <ServiceConnection>.
        protected override void HandleInputAction(InputAction _inputData)
        {
            switch (_inputData.HandType)
            {
                case HandType.PRIMARY:
                    HandlePerHandInputAction(_inputData, ref primaryTouchData);
                    break;
                case HandType.SECONDARY:
                    HandlePerHandInputAction(_inputData, ref secondaryTouchData);
                    break;
            }
        }

        void HandlePerHandInputAction(InputAction _inputData, ref TouchData _touchData)
        {
            switch (_inputData.InputType)
            {
                case InputType.DOWN:
                    _touchData.touchPhase = TouchPhase.Began;
                    _touchData.isTouching = true;
                    break;

                case InputType.MOVE:
                    if (_touchData.isTouching && _touchData.touchPosition != _inputData.CursorPosition)
                    {
                        _touchData.touchPhase = TouchPhase.Moved;
                    }
                    break;

                case InputType.CANCEL:
                    _touchData.touchPhase = TouchPhase.Canceled;
                    break;

                case InputType.UP:
                    _touchData.touchPhase = TouchPhase.Ended;
                    break;
            }

            _touchData.touchPosition = _inputData.CursorPosition;
        }

        bool IsTouchFreeInputWithinScreen()
        {
            if(!sendHoverEvents || primaryTouchData.touchPhase == TouchPhase.Canceled)
            {
                return false;
            }

            if(primaryTouchData.touchPosition.x < 0 || primaryTouchData.touchPosition.y < 0 || primaryTouchData.touchPosition.x > Screen.width || primaryTouchData.touchPosition.y > Screen.height)
            {
                return false;
            }

            return true;
        }

        protected override void OnDisable()
        {
            primaryTouchData.touchPhase = TouchPhase.Canceled;
            secondaryTouchData.touchPhase = TouchPhase.Canceled;

            base.OnDisable();
        }
    }
    [System.Serializable]
    public class TouchData
    {
        public Vector2 touchPosition;
        public TouchPhase touchPhase = TouchPhase.Ended;
        public bool isTouching = false;
    }
}