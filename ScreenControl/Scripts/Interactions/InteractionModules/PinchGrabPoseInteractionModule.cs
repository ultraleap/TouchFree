using UnityEngine;
using Leap.Unity;
using Stopwatch = System.Diagnostics.Stopwatch;

public class PinchGrabPoseInteractionModule : InteractionModule
{
    public override InteractionType InteractionType {get;} = InteractionType.Grab;
    
    [Header("positioningModule.Stabiliser Params")]
    public float deadzoneEnlargementDistance;
    public float deadzoneShrinkSpeed;

    [Header("Other params")]
    [Tooltip("If hand is moving faster than this speed (in m/s), grabs will not be recognised")]
    public float maxHandVelocity = 0.15f;
    public float verticalCursorOffset = 0.1f;

    public bool InteractionEnabled { get; set; } = true;

    public bool alwaysHover = false;

    public float inputPositionLerpSpeed;

    public bool debugDist;

    public GeneralisedGrabDetector grabDetector;

    [Header("Drag Params")]
    public float dragStartDistanceThresholdM;
    public float dragStartTimeDelaySecs;
    public float dragLerpSpeed;

    private bool pressing = false;
    private bool hovering = false;

    public bool instantUnclick = false;
    private bool requireHold = false;
    private int REQUIRED_HOLD_FRAMES = 1;
    private int heldFrames = 0;
    private bool requireClick = false;

    // Dragging
    private Vector2 posLastFrame;
    private Vector2 cursorDownPos;
    private Vector3 clickDownPos;
    private bool isDragging;
    private Stopwatch dragStartTimer = new Stopwatch();

    void Update()
    {
        if (SingleHandManager.Instance.CurrentHand == null)
        {
            return;
        }

        if (!InteractionEnabled)
        {
            return;
        }

        positions = positioningModule.CalculatePositions();
        Vector3 cursorPosition = positions.CursorPosition;
        float distanceFromScreen = cursorPosition.z;
        Vector3 clickPosition = positions.ClickPosition;


        if (SingleHandManager.Instance.CurrentHand.IsRight)
        {
            SendInputAction(InputType.SETRIGHT, cursorPosition, clickPosition, distanceFromScreen);
        }
        else
        {
            SendInputAction(InputType.SETLEFT, cursorPosition, clickPosition, distanceFromScreen);
        }

        float velocity = SingleHandManager.Instance.CurrentHand.PalmVelocity.Magnitude;
        HandleInteractions(cursorPosition, clickPosition, distanceFromScreen, velocity);
    }

    private void HandleInteractions(Vector2 _cursorPosition, Vector3 _clickPosition, float _distanceFromScreen, float _velocity)
    {
        SendInputAction(InputType.MOVE, _cursorPosition, _clickPosition, _distanceFromScreen);
        // If already pressing, continue regardless of velocity
        if (grabDetector.IsGrabbing() && (pressing || _velocity < maxHandVelocity))
        {
            HandleInvoke(_cursorPosition, _clickPosition, _distanceFromScreen);
        }
        else
        {
            HandlePotentialUnclick(_cursorPosition, _clickPosition, _distanceFromScreen);
            HandleHover(_cursorPosition, _clickPosition, _distanceFromScreen);
        }
    }

    private void HandleInvoke(Vector2 _cursorPosition, Vector3 _clickPosition, float _distanceFromScreen)
    {
        // we are touching the screen
        if (!pressing)
        {
            HandlePress(_cursorPosition, _clickPosition, _distanceFromScreen);
        }
        else
        {
            HandlePressHold(_cursorPosition, _clickPosition, _distanceFromScreen);
        }
    }

    private void HandlePress(Vector2 _screenPosition, Vector3 _clickPosition, float _distanceFromScreen)
    {
        SendInputAction(InputType.DOWN, _screenPosition, _clickPosition, _distanceFromScreen);
        cursorDownPos = _screenPosition;
        clickDownPos = _clickPosition;
        posLastFrame = _screenPosition;
        dragStartTimer.Restart();
        pressing = true;
        if (instantUnclick && ignoreDragging)
        {
            requireHold = true;
            heldFrames = 0;
            requireClick = false;
        }

        // Adjust deadzone
        positioningModule.Stabiliser.StopShrinkingDeadzone();
        float newDeadzoneRadius = deadzoneEnlargementDistance + positioningModule.Stabiliser.defaultDeadzoneRadius;
        positioningModule.Stabiliser.SetCurrentDeadzoneRadius(newDeadzoneRadius);
    }

    private void HandlePressHold(Vector2 _cursorPosition, Vector3 _clickPosition, float _distanceFromScreen)
    {
        if (isDragging)
        {
            SendInputAction(InputType.DRAG, _cursorPosition, _clickPosition, _distanceFromScreen);
        }
        else
        {
            if (instantUnclick && ignoreDragging)
            {
                if (requireHold)
                {
                    SendInputAction(InputType.HOLD, cursorDownPos, clickDownPos, _distanceFromScreen);
                    if (heldFrames >= REQUIRED_HOLD_FRAMES)
                    {
                        requireHold = false;
                        requireClick = true;
                        heldFrames = 0;
                    }
                    else
                    {
                        heldFrames += 1;
                    }
                }
                else if (requireClick)
                {
                    SendInputAction(InputType.UP, cursorDownPos, clickDownPos, _distanceFromScreen);
                    positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, deadzoneShrinkSpeed);
                    requireClick = false;
                }
            }
            else
            {
                SendInputAction(InputType.HOLD, cursorDownPos, clickDownPos, _distanceFromScreen);
                // Lock in to the touch down position until dragging occurs.
                if (CheckForStartDrag(cursorDownPos, _cursorPosition) && !ignoreDragging)
                {
                    isDragging = true;
                    positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, deadzoneShrinkSpeed);
                }
            }
        }
    }

    private void HandlePotentialUnclick(Vector2 _cursorPosition, Vector3 _clickPosition, float _distanceFromScreen)
    {
        // Check if an unclick is needed, and perform if so
        if (pressing)
        {
            if (!(ignoreDragging && instantUnclick))
            {
                if (!requireHold && !requireClick)
                {
                    SendInputAction(InputType.UP, _cursorPosition, _clickPosition, _distanceFromScreen);
                }
                positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, deadzoneShrinkSpeed);
            }

            pressing = false;
        }
    }

    private void HandleHover(Vector2 _cursorPosition, Vector3 _clickPosition, float distanceFromScreen)
    {
        if (allowHover)
        {
            SendInputAction(InputType.HOVER, _cursorPosition, _clickPosition, distanceFromScreen);
        }
        hovering = true;
        isDragging = false;
    }

    bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
    {
        var a = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(_startPos, 0f);
        var b = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(_currentPos, 0f);
        var distFromStartPos = (a - b).magnitude;
        if (distFromStartPos > dragStartDistanceThresholdM)
        {
            return true;
        }

        if (dragStartTimer.ElapsedMilliseconds >= dragStartTimeDelaySecs * 1000f)
        {
            dragStartTimer.Stop();
            return true;
        }

        return false;
    }

    Vector3 worldPos_debug;
    Vector3 planeHit_debug;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(-GlobalSettings.virtualScreen.PhysicalScreenPlane.normal * GlobalSettings.virtualScreen.PhysicalScreenPlane.distance, 0.01f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(planeHit_debug + (-GlobalSettings.virtualScreen.VirtualScreenPlane.normal * GlobalSettings.virtualScreen.VirtualScreenPlane.distance), 0.01f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(worldPos_debug, 0.01f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(planeHit_debug, 0.005f);
        }
    }
#endif
}