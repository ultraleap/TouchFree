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

    protected override void UpdateData(Leap.Hand hand)
    {
        if (hand == null)
        {
            SendInputAction(InputType.CANCEL, new Positions(), 0);
            pressing = false;
            return;
        }

        if (!InteractionEnabled)
        {
            return;
        }

        positions = positioningModule.CalculatePositions(hand);
        positioningModule.Stabiliser.ScaleDeadzoneByDistance(positions.DistanceFromScreen);
        HandleInteractions();
    }

    private void HandleInteractions()
    {
        Vector2 currentCursorPosition = positions.CursorPosition;
        Vector2 clickPosition = positions.ClickPosition;
        float distanceFromScreen = positions.DistanceFromScreen;

        float progressToClick = 1f - Mathf.InverseLerp(screenDistanceAtMaxProgress, screenDistanceAtNoProgress, distanceFromScreen);

        SendInputAction(InputType.MOVE, positions, progressToClick);

        // determine if the fingertip is across one of the surface thresholds (hover/press) and send event
        if (distanceFromScreen < 0f)
        {
            // we are touching the screen
            if (!pressing)
            {
                SendInputAction(InputType.DOWN, positions, progressToClick);

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
                    Positions dragPositions = new Positions(pos, clickPosition, distanceFromScreen);
                    SendInputAction(InputType.DRAG, dragPositions, progressToClick);
                }
                else
                {
                    Positions downPositions = new Positions(downPos, clickPosition, distanceFromScreen);
                    // Do an instant touch up to select a button instantly.
                    if (ignoreDragging && performInstantClick)
                    {
                        if (instantClickHoldFrame)
                        {
                            SendInputAction(InputType.HOLD, downPositions, progressToClick);
                            instantClickHoldFrame = false;
                        }
                        else
                        {
                            SendInputAction(InputType.UP, downPositions, progressToClick);
                            performInstantClick = false;
                        }
                    }
                    else
                    {
                        if (!ignoreDragging)
                        {
                            SendInputAction(InputType.HOLD, downPositions, progressToClick);
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
                    SendInputAction(InputType.UP, positions, progressToClick);
                }

                pressing = false;
            }

            if (allowHover)
            {
                SendInputAction(InputType.HOVER, positions, progressToClick);
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