using UnityEngine;
using Leap.Unity;
using Stopwatch = System.Diagnostics.Stopwatch;
using System;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;
using System.Collections.Generic;

/**
 * The possible Clicking Algorithms are summarised as follows:
 * FORWARD_TO_CLICK
 *  Moving forward continually for a set time will trigger a click event immediately
 *  followed by an unclick event.
 *  For this time, the finger must travel a set distance (implying a minimum finger velocity to
 *  trigger a click).
 *  The 'HOVERING' state implies ready to take input.
 *  When not hovering, no input can be provided.
 */

enum InteractionState
{
    PASSIVE,
    HOVERING,
    PRESSING,
    DRAGGING
}

public class PokeToInvokeInteractionModule : InteractionModule
{
    public override InteractionType InteractionType {get;} = InteractionType.Push;
    public bool InteractionEnabled { get; set; } = true;

    [Header("Tracking Params")]
    public float deadconeEnglargementRatio;
    public float deadconeMaxSizeIncrease;
    public float deadconeShrinkSpeed;

    [Header("PokeToInvoke Params")]
    public double millisecondsCooldownOnEntry;
    public double maxDistance;
    public double millisecondsToClick;
    public float minClickDistance;
    public float cosSquaredMaxAngle;
    public double millisecondsToSlowdown;
    public float slowdownMaxDistance;
    public double clickCooldown;
    public bool immediateUnclick;
    public double millisecondsToUnclick;
    public float minUnclickDistance;
    public float cosSquaredMaxAngleUnclick;

    // The 'cosSquaredMaxAngle' is the cos(maxAngle) squared, where maxAngle is the maximum angle
    // away from the screen that the finger can be moving.
    // Eg:  
    //      90 degrees: 0.0
    //      75 degrees: 0.067
    //      60 degrees: 0.25
    //      45 degrees: 0.5
    //      30 degrees: 0.75
    //      15 degrees: 0.933

    [Header("Drag Params")]
    public float dragStartDistanceThresholdM = 0.04f;
    public float dragStartTimeDelaySecs = 0.1f;
    public float dragDeadzoneShrinkRate = 0.5f;
    public float dragDeadzoneShrinkDistanceThresholdM = 0.001f;
    bool dragDeadzoneShrinkTriggered = false;
    Stopwatch dragStartTimer = new Stopwatch();


    [Header("Debug Params")]
    public float enterDistance;
    public float virtualDistance;

    // Instant-unclick params
    private const int REQUIRED_HOLD_FRAMES = 1;
    private int heldFrames = 0;

    private InteractionState interactionState = InteractionState.PASSIVE;
    // Dragging
    Vector2 downPos;
    private bool isDragging = false;

    // Forward to Click-Unclick
    private bool handLastSeen = false;
    private Stopwatch handAppearedCooldown = new Stopwatch();
    private Queue<Tuple<DateTime, float, Vector2>> previousPositions = new Queue<Tuple<DateTime, float, Vector2>>();
    private Stopwatch clickCooldownTimer = new Stopwatch();
    private bool checkingForClickEnd = false;

    // Internal deadcone params
    float pokeGestureRecognisedDistance = 0f;

    void Update()
    {
        if (SingleHandManager.Instance.CurrentHand == null || !InteractionEnabled)
        {
            return;
        }

        positions = positioningModule.CalculatePositions();

        Vector3 cursorPosition = positions.CursorPosition;
        float distanceFromScreen = cursorPosition.z;

        HandleInputForwardToClick(cursorPosition, distanceFromScreen);
    }

    private void HandleInputForwardToClick(Vector2 cursorPosition, float distanceFromScreen)
    {
        Vector3 clickPosition = positions.ClickPosition;
        SendInputAction(InputType.MOVE, cursorPosition, clickPosition, distanceFromScreen);

        Tuple<DateTime, float, Vector2> currentPosition = new Tuple<DateTime, float, Vector2>(DateTime.Now, distanceFromScreen, cursorPosition);
        previousPositions.Enqueue(currentPosition);
        if (previousPositions.Count <= 1)
        {
            return;
        }

        // Remove old frames from cache
        // Want to have exactly one more frame than the largest time param, so will keep
        // one frame that's out of bounds.
        double cacheSizeMilliseconds = Math.Max(millisecondsToClick, millisecondsToUnclick);
        while (true)
        {
            Queue<Tuple<DateTime, float, Vector2>>.Enumerator enumerator = previousPositions.GetEnumerator();

            enumerator.MoveNext();  // Moving to first element
            enumerator.MoveNext();
            Tuple<DateTime, float, Vector2> secondItem = enumerator.Current;

            DateTime oldestTime = secondItem.Item1;
            TimeSpan deltaTime = currentPosition.Item1.Subtract(oldestTime);
            double deltaMilliseconds = deltaTime.TotalMilliseconds;

            if (deltaMilliseconds > cacheSizeMilliseconds)
            {
                previousPositions.Dequeue();
            }
            else
            {
                break;
            }
        }


        if (distanceFromScreen > maxDistance)
        {
            isDragging = false;
            handLastSeen = false;
            checkingForClickEnd = false;
            handAppearedCooldown.Stop();
            interactionState = InteractionState.PASSIVE;
        }
        else
        {
            if (!handLastSeen)
            {
                // Start a hand appeared cooldown timer
                handAppearedCooldown.Restart();
                interactionState = InteractionState.HOVERING;
            }
            else
            {
                if (handAppearedCooldown.ElapsedMilliseconds >= millisecondsCooldownOnEntry | !handAppearedCooldown.IsRunning)
                {
                    handAppearedCooldown.Stop();
                }
            }
            handLastSeen = true;
        }

        if (clickCooldownTimer.IsRunning)
        {
            if (clickCooldownTimer.ElapsedMilliseconds >= clickCooldown)
            {
                clickCooldownTimer.Stop();
            }
        }

        bool inCooldown = clickCooldownTimer.IsRunning | handAppearedCooldown.IsRunning;

        // If hovering, determine whether to press
        if (interactionState == InteractionState.HOVERING && !inCooldown)
        {
            bool toClick = false;
            if (checkingForClickEnd)
            {
                // Increase deadzone radius
                float defaultDeadzoneSize = positioningModule.Stabiliser.defaultDeadzoneRadius;
                float distanceSincePokeBegan = Mathf.Abs(distanceFromScreen - pokeGestureRecognisedDistance);
                float enlargedDeadZoneSize = defaultDeadzoneSize + deadconeEnglargementRatio * distanceSincePokeBegan;
                float maxSize = deadconeMaxSizeIncrease + defaultDeadzoneSize;
                float newDeadzoneSize = Mathf.Min(enlargedDeadZoneSize, maxSize);
                positioningModule.Stabiliser.SetCurrentDeadzoneRadius(newDeadzoneSize);

                if (!VelocityGestureRecognised(previousPositions, currentPosition, millisecondsToSlowdown, slowdownMaxDistance, false))
                {
                    // Slowdown triggered
                    checkingForClickEnd = false;

                    // Check the angle over the gesture-end to determine whether to trigger a click or not
                    toClick = (CosSqrdAngleFromScreen(currentPosition, millisecondsToClick) > cosSquaredMaxAngle);

                    // This means if the person moves too much parallel to screen at the end of the gesture, it is just abandoned.

                    if (!toClick)
                    {
                        // Reduce the deadzone size back to normal if a click is abandoned
                        positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.TIME_BASED, deadconeShrinkSpeed);
                    }
                }
            }
            else if (VelocityGestureRecognised(previousPositions, currentPosition, millisecondsToClick, minClickDistance, false))
            {
                // If a gesture is recognised, check that the angle is sufficient
                if (CosSqrdAngleFromScreen(currentPosition, millisecondsToClick) > cosSquaredMaxAngle)
                {
                    checkingForClickEnd = true;

                    // Begin expanding deadzone
                    positioningModule.Stabiliser.StopShrinkingDeadzone();
                    pokeGestureRecognisedDistance = distanceFromScreen;
                    positioningModule.Stabiliser.SetCurrentDeadzoneRadius(positioningModule.Stabiliser.defaultDeadzoneRadius);
                }
            }
            if (toClick)
            {
                SendInputAction(InputType.DOWN, cursorPosition, clickPosition, distanceFromScreen);
                interactionState = InteractionState.PRESSING;
                downPos = cursorPosition;
                isDragging = false;
                heldFrames = 0; // for instant-unclick
                dragStartTimer.Restart();
            }
            else if (allowHover)
            {
                SendInputAction(InputType.HOVER, cursorPosition, clickPosition, distanceFromScreen);
            }
        }
        else if (interactionState == InteractionState.PRESSING)
        {
            // If clicking, determine whether to unclick
            bool triggerUnclick = false;    // Whether to trigger a click this frame. If false, send HOLD

            if (immediateUnclick && ignoreDragging)
            {
                heldFrames += 1;
                if (heldFrames > REQUIRED_HOLD_FRAMES)
                {
                    heldFrames = 0;
                    triggerUnclick = true;
                }
            }
            else
            {
                if (VelocityGestureRecognised(previousPositions, currentPosition, millisecondsToUnclick, minUnclickDistance, true))
                {
                    // Specify a different max unclick angle
                    triggerUnclick = (CosSqrdAngleFromScreen(currentPosition, millisecondsToUnclick) > cosSquaredMaxAngleUnclick);
                }
            }

            if (triggerUnclick)
            {
                positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.TIME_BASED, deadconeShrinkSpeed);
                interactionState = InteractionState.HOVERING;
                isDragging = false;
                clickCooldownTimer.Restart();
                SendInputAction(InputType.UP, cursorPosition, clickPosition, distanceFromScreen);
            }
            else
            {
                if (isDragging)
                {
                    if (!dragDeadzoneShrinkTriggered && CheckForStartDragDeadzoneShrink(downPos, currentPosition.Item3))
                    {
                        positioningModule.Stabiliser.StartShrinkingDeadzone(ShrinkType.MOTION_BASED, dragDeadzoneShrinkRate);
                        dragDeadzoneShrinkTriggered = true;
                    }
                    SendInputAction(InputType.DRAG, cursorPosition, clickPosition, distanceFromScreen);
                }
                else
                {
                    if (!ignoreDragging)
                    {
                        isDragging = CheckForStartDrag(downPos, currentPosition.Item3);
                        dragDeadzoneShrinkTriggered = false;
                    }
                    SendInputAction(InputType.HOLD, downPos, clickPosition, distanceFromScreen);
                }
            }
        }
    }


    bool CheckForStartDrag(Vector2 startPos, Vector2 currentPos)
    {
        Vector2 startPosM = GlobalSettings.virtualScreen.PixelsToMeters(startPos);
        Vector2 currentPosM = GlobalSettings.virtualScreen.PixelsToMeters(currentPos);
        float distFromStartPos = (startPosM - currentPosM).magnitude;
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

    bool CheckForStartDragDeadzoneShrink(Vector2 startPos, Vector2 currentPos)
    {
        Vector2 startPosM = GlobalSettings.virtualScreen.PixelsToMeters(startPos);
        Vector2 currentPosM = GlobalSettings.virtualScreen.PixelsToMeters(currentPos);
        float distFromStartPos = (startPosM - currentPosM).magnitude;
        if (distFromStartPos > dragDeadzoneShrinkDistanceThresholdM)
        {
            return true;
        }
        return false;
    }

    /**
     * Check a list of timestamped positions for a velocity-based gesture
     * 
     * TODO:
     *  With the current implementation, if the user travels the required distance in less time
     *  than required, and then a direction-change is discovered, then the gesture will not be
     *  recognised. This has not been seen in tests but may be possible
     * 
     * Args:
     *  positions: Previous positions
     *  currentPosition: The data associated with the current position. This is the data at the back of the queue.
     *  timeThreshold: The time that needs to be elapsed
     *  distanceThreshold: The distance that needs to be travelled
     *  increasing: True if looking for gesture with all numbers increasing, false if decreasing
     */
    private static bool VelocityGestureRecognised(Queue<Tuple<DateTime, float, Vector2>> positions, Tuple<DateTime, float, Vector2> currentPosition, double timeThreshold, float distanceThreshold, bool increasing)
    {
        float unitDirection = 1f;
        if (!increasing)
        {
            unitDirection = -1f;
        }

        float previous = -1000;
        bool allIncreasing = true;

        bool startPositionFound = false;
        Tuple<DateTime, float, Vector2> gestureStartPosition = currentPosition;

        foreach (Tuple<DateTime, float, Vector2> position in positions)
        {
            double deltaTime = currentPosition.Item1.Subtract(position.Item1).TotalMilliseconds;
            if (deltaTime > timeThreshold)
            {
                continue;
            }
            else
            {
                if (!startPositionFound)
                {
                    startPositionFound = true;
                    gestureStartPosition = position;
                }
            }
            float zPosition = position.Item2 * unitDirection;
            if (zPosition < previous)
            {
                allIncreasing = false;
                break;
            }
            previous = zPosition;
        }

        // Need to check that the whole queue exceeds the time required
        double elapsedTime = currentPosition.Item1.Subtract(positions.Peek().Item1).TotalMilliseconds;
        bool timedLongEnough = elapsedTime >= timeThreshold;

        // Need to check the distance from the beginning of the gesture, not the whole queue
        float distanceTravelled = unitDirection * (currentPosition.Item2 - gestureStartPosition.Item2);
        bool travelledFarEnough = (distanceTravelled >= distanceThreshold);

        return (allIncreasing && timedLongEnough && travelledFarEnough);
    }

    /**
     * Get the Cosine-Squared of the angle between the currentPosition and the position a certain time ago
     * 
     * Args:
     *  currentPosition:
     *      The current position and timestamp
     *  timePeriod:
     *      Look for the position a certain time period away
     *
     */
    private float CosSqrdAngleFromScreen(Tuple<DateTime, float, Vector2> currentPosition, double timePeriod)
    {
        Tuple<DateTime, float, Vector2> gestureStartPosition = currentPosition;
        foreach (Tuple<DateTime, float, Vector2> position in previousPositions)
        {
            double deltaTime = currentPosition.Item1.Subtract(position.Item1).TotalMilliseconds;
            if (deltaTime > timePeriod)
            {
                continue;
            }
            else
            {
                gestureStartPosition = position;
                break;
            }
        }

        float paraDist = currentPosition.Item2 - gestureStartPosition.Item2;
        float paraDistSqr = paraDist * paraDist;

        Vector2 perpDistVector = GlobalSettings.virtualScreen.PixelsToMeters(currentPosition.Item3 - gestureStartPosition.Item3);
        float perpDistSqr = perpDistVector.sqrMagnitude;

        float cosSquaredTheta = paraDistSqr / (paraDistSqr + perpDistSqr);
        return cosSquaredTheta;
    }
}