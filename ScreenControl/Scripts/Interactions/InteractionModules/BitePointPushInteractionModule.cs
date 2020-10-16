using UnityEngine;
using Leap.Unity;
using Stopwatch = System.Diagnostics.Stopwatch;

using System.Collections;
using System.Collections.Generic;
using System;

public class BitePointPushInteractionModule : InteractionModule
{
    public bool InteractionEnabled { get; set; } = true;

    [Header("BitePoint Params")]
    public float maxDistanceToPush;
    public float cacheTime; // cache the previous positions over the specified time period

    public float horizontalVelocityThreshold = 0.05f;
    public float zForwardVelocityThreshold = 0.01f;
    public float zBackwardVelocityThreshold = 0.05f;

    private float touchPlanePosition = 0f;  // interally adjusted by the bite point

    // Cache of previous positions
    private Queue<Tuple<float, Vector3>> previousPositions = new Queue<Tuple<float, Vector3>>();

    [Header("Drag Params")]
    public float dragStartDistanceThresholdM = 0.04f;
    public float dragStartTimeDelaySecs = 0.6f;
    public float dragLerpSpeed = 10f;

    private bool pressing = false;
    private bool performInstantClick = false;
    private bool instantClickHoldFrame = false;
    private bool hadHandLastFrame = false;
    private bool handCoolDown = false;

    // Tracking input smoothing
    private Vector3 prevWorldPos;
    private Vector3 prevScreenPos;

    // Dragging
    private Vector2 posLastFrame;
    private Vector2 downPos;
    private bool isDragging;
    private Stopwatch dragStartTimer = new Stopwatch();
    private Coroutine coolDownHand;

    protected override void UpdateData(Leap.Hand hand)
    {
        if (!InteractionEnabled)
        {
            return;
        }

        if (hand == null)
        {
            hadHandLastFrame = false;
            SendInputAction(InputType.CANCEL, new Positions(), 0);
            pressing = false;
            handCoolDown = true;
            if (coolDownHand != null) StopCoroutine(coolDownHand);
            DisengageBitePoint();
            return;
        }

        if (!hadHandLastFrame)
        {
            prevWorldPos = positioningModule.GetTrackedPointingJoint(hand);
            prevScreenPos = GlobalSettings.virtualScreen.WorldPositionToVirtualScreen(prevWorldPos, out _);
            hadHandLastFrame = true;
            coolDownHand = StartCoroutine(CoolDownHand());
        }

        positions = positioningModule.CalculatePositions(hand);

        float distanceFromScreen = positions.DistanceFromScreen;

        // TODO: Use InteractionModule.Timestamp instead
        float timestamp = hand.TimeVisible * 1000f;

        var screenPosM = GlobalSettings.virtualScreen.PixelsToMeters(positions.CursorPosition);
        if (!handCoolDown) UpdateCachedPositions(timestamp, screenPosM, distanceFromScreen);

        HandleInteractions();
    }

    private void HandleInteractions()
    {

        float distanceFromScreen = positions.DistanceFromScreen;
        Vector2 currentPos = positions.CursorPosition;
        Vector2 clickPos = positions.ClickPosition;

        // Adjust the cluth based on the distance from screen. This is the crux of the BitePoint algorithm.
        AdjustClutch(distanceFromScreen);

        // Calculate the progressToPush dependent on the location relative to TouchPlane
        float distanceFromPlane = distanceFromScreen - touchPlanePosition;
        float progressToPush = 1f - Mathf.Clamp01(distanceFromPlane / maxDistanceToPush);

        // Send relevant actions based on current progress and previous state.

        SendInputAction(InputType.MOVE, positions, progressToPush);

        if (distanceFromScreen < touchPlanePosition)
        {
            // we are touching the screen
            if (!pressing)
            {
                SendInputAction(InputType.DOWN, positions, progressToPush);
                downPos = currentPos;
                posLastFrame = currentPos;
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
                    var pos = Vector2.Lerp(posLastFrame, currentPos, 10f * Time.deltaTime);
                    posLastFrame = pos;

                    Positions lerpedPositions = new Positions(pos, pos, distanceFromScreen);
                    SendInputAction(InputType.DRAG, lerpedPositions, progressToPush);
                }
                else
                {
                    // Do an instant touch up to select a button instantly.
                    if (ignoreDragging && performInstantClick)
                    {
                        Positions clickDownPositions = new Positions(downPos, downPos, distanceFromScreen);
                        if (instantClickHoldFrame)
                        {
                            SendInputAction(InputType.HOLD, clickDownPositions, progressToPush);
                            instantClickHoldFrame = false;
                        }
                        else
                        {
                            SendInputAction(InputType.UP, clickDownPositions, progressToPush);
                            performInstantClick = false;
                        }
                    }
                    else if (!ignoreDragging)
                    {
                        Positions draggingPositions = new Positions(downPos, clickPos, distanceFromScreen);
                        SendInputAction(InputType.HOLD, draggingPositions, progressToPush);
                        if (CheckForStartDrag(downPos, currentPos))
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
                    SendInputAction(InputType.UP, positions, progressToPush);
                }

                pressing = false;
            }

            if (allowHover)
            {
                SendInputAction(InputType.HOVER, positions, progressToPush);
            }
            isDragging = false;
        }
    }

    private bool CheckForStartDrag(Vector2 _startPos, Vector2 _currentPos)
    {
        var a = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(_startPos, 0f);
        var b = GlobalSettings.virtualScreen.VirtualScreenPositionToWorld(_currentPos, 0f);
        var distFromStartPos = (a - b).magnitude;
        if (distFromStartPos > dragStartDistanceThresholdM)
        {
            //Debug.Log("Drag started: distance");
            return true;
        }

        if (dragStartTimer.ElapsedMilliseconds >= dragStartTimeDelaySecs * 1000f)
        {
            //Debug.Log("Drag started: time");
            dragStartTimer.Stop();
            return true;
        }

        return false;
    }

    private void UpdateCachedPositions(float _timestamp, Vector2 _screenPosM, float _distanceFromScreen)
    {
        // Store a cache of the previous position over the last "cacheTime" milliseconds.
        // Can be used to calculate time-dependent properties.

        Vector3 currentPositionForCache = Vector3.zero;
        currentPositionForCache.x = _screenPosM.x;
        currentPositionForCache.y = _screenPosM.y;
        currentPositionForCache.z = _distanceFromScreen;

        Tuple<float, Vector3> newCacheEntry = new Tuple<float, Vector3>(_timestamp, currentPositionForCache);
        previousPositions.Enqueue(newCacheEntry);
        if (previousPositions.Count <= 1)
        {
            return;
        }

        // Remove old frames from cache
        // Want to have exactly one more frame than the largest time param, so will keep
        // one frame that's out of bounds.
        while (true)
        {
            Queue<Tuple<float, Vector3>>.Enumerator enumerator = previousPositions.GetEnumerator();

            enumerator.MoveNext();  // Moving to first element
            enumerator.MoveNext();
            Tuple<float, Vector3> secondItem = enumerator.Current;

            float oldestTime = secondItem.Item1;
            float deltaTime = newCacheEntry.Item1 - oldestTime;

            if (deltaTime > cacheTime)
            {
                previousPositions.Dequeue();
            }
            else if (deltaTime < 0)
            {
                // In this case we have a new hand (with a different zero timestamp),
                // and need to empty the cache of old hand data
                previousPositions.Clear();
                previousPositions.Enqueue(newCacheEntry);
                break;
            }
            else
            {
                break;
            }
        }
    }

    private Vector3 GetAverageVelocity()
    {
        // Calculate the average velocity over the cache

        if (previousPositions.Count < 2)
        {
            return Vector3.positiveInfinity;
        }

        var enumerator = previousPositions.GetEnumerator();
        enumerator.MoveNext();  // Moving to first element
        Tuple<float, Vector3> oldestEntry = enumerator.Current;

        Tuple<float, Vector3> newestEntry = oldestEntry;
        while(enumerator.MoveNext())
        {
            newestEntry = enumerator.Current;
        }

        Vector3 totalDistance = newestEntry.Item2 - oldestEntry.Item2;      // metres
        float totalTime = (newestEntry.Item1 - oldestEntry.Item1) / 1000f;  // seconds

        // Simplistic calculation just using the two end-points of the cache.
        Vector3 avgVelocity = totalDistance / totalTime;
        return avgVelocity;
    }

    private void AdjustClutch(float _distanceFromScreen)
    {
        // Determine whether to Engage or Disengage the clutch, based on
        // current and previous positions.

        Vector3 currentVelocity = GetAverageVelocity();

        float zVelocity = -1f * currentVelocity.z;  // Positive = toward screen
        float horizontalVelocity = Mathf.Sqrt(currentVelocity.x * currentVelocity.x + currentVelocity.y + currentVelocity.y);

        bool outsideOfTouchZone = (_distanceFromScreen - touchPlanePosition) > maxDistanceToPush;

        bool possibleToEngage = (touchPlanePosition == 0f) && outsideOfTouchZone;
        if (possibleToEngage)
        {
            bool velocityConditionMet = (zVelocity > zForwardVelocityThreshold) && (horizontalVelocity < horizontalVelocityThreshold);
            if (velocityConditionMet)
            {
                //Debug.Log("Touch Plane Position: " + touchPlanePosition.ToString());
                EngageBitePoint(_distanceFromScreen);
                //Debug.Log("Engaged at: " + distanceFromScreen.ToString());
            }
        }

        bool possibleToDisengage = (touchPlanePosition > 0f) && outsideOfTouchZone;
        if (possibleToDisengage)
        {
            bool velocityConditionMet = (zVelocity < -1f * zBackwardVelocityThreshold);   // Moving backwards significantly
            if (velocityConditionMet)
            {
                DisengageBitePoint();
                //Debug.Log("Disengaged: " + distanceFromScreen.ToString());
            }
        }

    }

    private void EngageBitePoint(float _distanceFromScreen)
    {
        // Set the touch plane position based on the current screen position
        // Can bring closer, can never push further away than the default
        touchPlanePosition = Mathf.Max(0f, _distanceFromScreen - maxDistanceToPush);
    }

    private void DisengageBitePoint()
    {
        // Reset the touch plane position to zero
        touchPlanePosition = 0f;
    }

    public IEnumerator CoolDownHand()
    {
        yield return new WaitForSeconds(1);

        handCoolDown = false;
    }
}