using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A class for dealing with Unity UI interactions based on Reach inputs.
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
        if (SingleHandManager.Instance.CurrentHand != null && allowInteractions)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsTouching()
    {
        if (!allowInteractions)
        {
            return false;
        }

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

    protected override void HandleInputAction(InputActionData _inputData)
    {
        base.HandleInputAction(_inputData);
        
        InputType _type = _inputData.Type;
        Vector2 _cursorPosition = _inputData.CursorPosition;
        Vector2 _clickPosition = _inputData.ClickPosition;
        float _distanceFromScreen = _inputData.ProgressToClick;

        touchPosition = _clickPosition;

        if (_type == InputType.MOVE || _type == InputType.HOVER)
            return;

        touchPhase = TouchPhase.Ended;
        eventSystem.pixelDragThreshold = baseDragThreshold;

        switch (_type)
        {
            case InputType.DOWN:
                touchPhase = TouchPhase.Began;
                break;
            case InputType.HOLD:
                touchPhase = TouchPhase.Moved;
                break;
            case InputType.DRAG:
                touchPhase = TouchPhase.Moved;
                eventSystem.pixelDragThreshold = 0;
                break;
            case InputType.CANCEL:
                touchPhase = TouchPhase.Canceled;
                break;
            case InputType.UP:
                touchPhase = TouchPhase.Ended;
                break;
        }
    }
}