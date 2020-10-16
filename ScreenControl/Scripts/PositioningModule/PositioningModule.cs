using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

public enum SnappingMode { NONE, CLICK, CLICK_AND_CURSOR, CLICK_AND_CURSOR_SOFT }

public struct Positions
{
    /**
     * Cursor position is used to guide the position of the cursor representation.
     * It is calculated in Screen Space
     */
    public Vector2 CursorPosition;
    /**
     * Click position is used to specify where the click event happens.
     * It is calculated in Screen Space.
     */
    public Vector2 ClickPosition;

    /**
     * Distance from screen is the physical distance of the hand from the screen.
     * It is calculated in meters.
     */
    public float DistanceFromScreen;

    public Positions(Vector2 _cursorPosition, Vector2 _clickPosition, float _distanceFromScreen)
    {
        CursorPosition = _cursorPosition;
        ClickPosition = _clickPosition;
        DistanceFromScreen = _distanceFromScreen;
    }
}

public class PositioningModule : MonoBehaviour
{
    public enum TRACKED_POSITION
    {
        FINGER,
        WRIST
    }

    public TRACKED_POSITION trackedPosition = TRACKED_POSITION.FINGER;
    public SnappingMode snappingMode = SnappingMode.NONE;

    [Tooltip("If assigned, the cursor snapper and stabiliser will be accessed from the utils object.")]
    public GameObject positioningUtils;
    public CursorSnapper cursorSnapper;

    public PositionStabiliser Stabiliser;
    public bool VerticalOffset = false;

    [Tooltip ("How firmly should the cursor snap in SOFT mode. Lower values equate to firmer snapping")]
    public float cursorSnapSoftness = 0.005f;
    private float verticalCursorOffset = 0f;

    [NonSerialized]
    public bool ApplyDragLerp;

    private const float DRAG_SMOOTHING_FACTOR = 10f;
    private Positions positions;


    protected void OnEnable()
    {
        SettingsConfig.OnConfigUpdated += OnConfigUpdated;
        OnConfigUpdated();
        
        if (positioningUtils != null) 
        {
            cursorSnapper = positioningUtils.GetComponent<CursorSnapper>();
            Stabiliser = positioningUtils.GetComponent<PositionStabiliser>();
        }
        
        Stabiliser.ResetValues();
    }
    protected void OnDisable()
    {
        SettingsConfig.OnConfigUpdated -= OnConfigUpdated;
    }

    private void OnConfigUpdated()
    {
        verticalCursorOffset = verticalCursorOffset = SettingsConfig.Config.CursorVerticalOffset;
    }

    public Positions CalculatePositions(Leap.Hand hand)
    {
        if (hand == null)
        {
            return positions;
        }
        if (cursorSnapper == null) 
        {
            cursorSnapper = positioningUtils.GetComponent<CursorSnapper>();
        }
        
        Tuple<Vector2, float> oneToOneData = CalculateOneToOnePositionData(hand);
        Vector2 oneToOnePosition = oneToOneData.Item1;
        float distanceFromScreen = oneToOneData.Item2;

        positions.DistanceFromScreen = distanceFromScreen;

        switch (snappingMode)
        {
            case SnappingMode.NONE:
                positions.CursorPosition = oneToOnePosition;
                positions.ClickPosition = positions.CursorPosition;
                break;
            case SnappingMode.CLICK:
                positions.CursorPosition = oneToOnePosition;
                positions.ClickPosition = cursorSnapper.CalculateSnappedPosition(oneToOnePosition);
                break;
            case SnappingMode.CLICK_AND_CURSOR:
                positions.CursorPosition = cursorSnapper.CalculateSnappedPosition(oneToOnePosition);
                positions.ClickPosition = positions.CursorPosition;
                break;
            case SnappingMode.CLICK_AND_CURSOR_SOFT:
                var snappedPosition = cursorSnapper.CalculateSnappedPosition(oneToOnePosition);
                var distance = Vector2.Distance(snappedPosition, oneToOnePosition);

                positions.CursorPosition = Vector2.Lerp(snappedPosition, oneToOnePosition, distance * cursorSnapSoftness);
                positions.ClickPosition = snappedPosition;
                break;
        }
        return positions;
    }

    private Tuple<Vector2, float> CalculateOneToOnePositionData(Leap.Hand hand)
    {
        // Return the hand position as a tuple:
        // Vector2 position in screen-space (measured in pixels)
        // float distanceFromScreen (measured in meters)

        float velocity = hand.PalmVelocity.Magnitude;
        Vector3 worldPos = GetTrackedPosition(hand);
        float smoothingTime = Time.deltaTime;
        if (ApplyDragLerp)
        {
            // Apply a different smoothing time if dragging
            smoothingTime *= DRAG_SMOOTHING_FACTOR;
        }
        worldPos = Stabiliser.ApplySmoothing(worldPos, velocity, smoothingTime);

        Vector3 screenPos = GlobalSettings.virtualScreen.WorldPositionToVirtualScreen(worldPos, out _);
        Vector2 screenPosM = GlobalSettings.virtualScreen.PixelsToMeters(screenPos);
        float distanceFromScreen = screenPos.z;

        screenPosM = Stabiliser.ApplyDeadzone(screenPosM);

        Vector2 oneToOnePosition = GlobalSettings.virtualScreen.MetersToPixels(screenPosM);
        if (VerticalOffset)
        {
            oneToOnePosition = ApplyVerticalOffset(oneToOnePosition);
        }

        return new Tuple<Vector2, float>(oneToOnePosition, distanceFromScreen);
    }

    private Vector3 GetTrackedPosition(Leap.Hand hand)
    {
        switch (trackedPosition)
        {
            case TRACKED_POSITION.WRIST:
                return hand.WristPosition.ToVector3();
            case TRACKED_POSITION.FINGER:
            default:
                return GetTrackedPointingJoint(hand);
        }
    }

    public Vector3 GetTrackedPointingJoint(Leap.Hand hand)
    {
        const float trackedJointDistanceOffset = 0.0533f;

        var bones = hand.GetIndex().bones;

        Vector3 trackedJointVector = (bones[0].NextJoint.ToVector3() + bones[1].NextJoint.ToVector3()) / 2;
        trackedJointVector.z += trackedJointDistanceOffset;
        return trackedJointVector;
    }
    private Vector2 ApplyVerticalOffset(Vector2 screenPos)
    {
        var screenPosM = GlobalSettings.virtualScreen.PixelsToMeters(screenPos);
        screenPosM += Vector2.up * verticalCursorOffset;
        return GlobalSettings.virtualScreen.MetersToPixels(screenPosM);
    }
}
