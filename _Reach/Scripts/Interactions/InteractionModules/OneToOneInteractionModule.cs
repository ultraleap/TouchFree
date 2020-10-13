using UnityEngine;
using Leap.Unity;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine.UIElements;
using System;

public class OneToOneInteractionModule : InteractionModule
{
    public override InteractionType InteractionType {get;} = InteractionType.Push;
    public bool InteractionEnabled { get; set; } = true;

    // The distance from screen at which the progressToClick is 0
    private float screenDistanceAtNoProgress = Mathf.Infinity;

    // The distance from screen at which the progressToClick is 1
    private float screenDistanceAtMaxProgress = 0f;

    [Header("Drag Params")]
    public float dragStartDistanceThresholdM = 0.04f;
    public float dragStartTimeDelaySecs = 0.6f;
    public float dragLerpSpeed = 10f;

    private bool pressing = false;
    private bool performInstantClick = false;
    private bool instantClickHoldFrame = false;

    // Dragging
    private Vector2 posLastFrame;
    private Vector2 downPos;
    private bool isDragging;
    private Stopwatch dragStartTimer = new Stopwatch();

    void Update()
    {
        if (SingleHandManager.Instance.CurrentHand == null)
        {
            SendInputAction(InputType.CANCEL, Vector2.zero, Vector2.zero, 1);
            pressing = false;
            return;
        }

        if (!InteractionEnabled)
        {
            return;
        }

        positions = positioningModule.CalculatePositions();
        float distanceFromScreen = positions.CursorPosition.z;
        positioningModule.Stabiliser.ScaleDeadzoneByDistance(distanceFromScreen);

        HandleInteractions(distanceFromScreen);
    }

    private void HandleInteractions(float distanceFromScreen)
    {
        Vector3 currentCursorPosition = positions.CursorPosition;
        Vector3 clickPosition = positions.ClickPosition;

        float progressToClick = 1f - Mathf.InverseLerp(screenDistanceAtMaxProgress, screenDistanceAtNoProgress, distanceFromScreen);

        SendInputAction(InputType.MOVE, currentCursorPosition, clickPosition, progressToClick);

        // determine if the fingertip is across one of the surface thresholds (hover/press) and send event
        if (distanceFromScreen < 0f)
        {
            // we are touching the screen
            if (!pressing)
            {
                SendInputAction(InputType.DOWN, currentCursorPosition, clickPosition, progressToClick);

                downPos = currentCursorPosition;
                posLastFrame = currentCursorPosition;
                dragStartTimer.Restart();
                pressing = true;
                performInstantClick = true;
                instantClickHoldFrame = true;
            }
            else
            {
                if (isDragging)
                {
                    // Lerp the drag position. This ensures the screen content doesn't JUMP to currentPos from the downPos
                    // after entering the drag state.
                    Vector2 pos = Vector2.Lerp(posLastFrame, currentCursorPosition, 10f * Time.deltaTime);
                    posLastFrame = pos;
                    SendInputAction(InputType.DRAG, pos, clickPosition, progressToClick);
                }
                else
                {
                    // Do an instant touch up to select a button instantly.
                    if (ignoreDragging && performInstantClick)
                    {
                        if (instantClickHoldFrame)
                        {
                            SendInputAction(InputType.HOLD, downPos, clickPosition, progressToClick);
                            instantClickHoldFrame = false;
                        }
                        else
                        {
                            SendInputAction(InputType.UP, downPos, clickPosition, progressToClick);
                            performInstantClick = false;
                        }
                    }
                    else
                    {
                        if (!ignoreDragging)
                        {
                            SendInputAction(InputType.HOLD, downPos, clickPosition, progressToClick);
                        }
                        // Lock in to the touch down position until dragging occurs.
                        if (!ignoreDragging && CheckForStartDrag(downPos, currentCursorPosition))
                        {
                            isDragging = true;
                        }
                    }
                }
            }
        }
        else
        {
            // we are hovering
            if (pressing)
            {
                if (!ignoreDragging)
                {
                    SendInputAction(InputType.UP, currentCursorPosition, clickPosition, progressToClick);
                }

                pressing = false;
            }

            if (allowHover)
            {
                SendInputAction(InputType.HOVER, currentCursorPosition, clickPosition, progressToClick);
            }

            isDragging = false;
        }
        positioningModule.ApplyDragLerp = isDragging;
    }

    bool CheckForStartDrag(Vector2 startPos, Vector2 currentPos)
    {
        Vector3 a = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(startPos, 0f);
        Vector3 b = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(currentPos, 0f);
        float distFromStartPos = (a - b).magnitude;

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

    protected override void OnSettingsUpdated(){
        base.OnSettingsUpdated();
        screenDistanceAtNoProgress = SettingsConfig.Config.CursorMaxRingScaleAtDistanceM;
    }
}