using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

public enum SnappingMode { NONE, CLICK, CLICK_AND_CURSOR }

public struct Positions
{
    /**
     * Cursor position is used to guide the position of the cursor representation.
     * It is calculated in Screen Space
     */
    public Vector3 CursorPosition;
    /**
     * Click position is used to specify where the click event happens.
     * It is calculated in Screen Space.
     */
    public Vector3 ClickPosition;
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
    public ColliderSnapper colliderSnapper;

    public PositionStabiliser Stabiliser;
    public bool VerticalOffset = false;
    private float verticalCursorOffset = 0f;

    [NonSerialized]
    public bool ApplyDragLerp;

    private const float DRAG_SMOOTHING_FACTOR = 10f;
    private Positions positions;


    protected void OnEnable()
    {
        Stabiliser.ResetValues();
        SettingsConfig.OnConfigUpdated += OnConfigUpdated;
        OnConfigUpdated();

    }
    protected void OnDisable()
    {
        SettingsConfig.OnConfigUpdated -= OnConfigUpdated;
    }

    private void OnConfigUpdated()
    {
        verticalCursorOffset = verticalCursorOffset = SettingsConfig.Config.CursorVerticalOffset;
    }

    public Positions CalculatePositions()
    {
        if (SingleHandManager.Instance.CurrentHand == null)
        {
            return positions;
        }

        Vector3 oneToOnePosition = CalculateOneToOnePosition();

        switch (snappingMode)
        {
            case SnappingMode.NONE:
                positions.CursorPosition = oneToOnePosition;
                positions.ClickPosition = positions.CursorPosition;
                break;
            case SnappingMode.CLICK:
                positions.CursorPosition = oneToOnePosition;
                positions.ClickPosition = colliderSnapper.CalculateSnappedPosition(oneToOnePosition);
                break;
            case SnappingMode.CLICK_AND_CURSOR:
                positions.CursorPosition = colliderSnapper.CalculateSnappedPosition(oneToOnePosition);
                positions.ClickPosition = positions.CursorPosition;
                break;
        }
        return positions;
    }

    private Vector3 CalculateOneToOnePosition()
    {
        float velocity = SingleHandManager.Instance.CurrentHand.PalmVelocity.Magnitude;
        Vector3 worldPos = GetTrackedPosition();
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

        Vector3 oneToOnePosition = GlobalSettings.virtualScreen.MetersToPixels(screenPosM);
        if (VerticalOffset)
        {
            oneToOnePosition = ApplyVerticalOffset(oneToOnePosition);
        }

        oneToOnePosition.z = distanceFromScreen;
        return oneToOnePosition;
    }

    private Vector3 GetTrackedPosition()
    {
        switch (trackedPosition)
        {
            case TRACKED_POSITION.WRIST:
                return SingleHandManager.Instance.CurrentHand.WristPosition.ToVector3();
            case TRACKED_POSITION.FINGER:
            default:
                return SingleHandManager.Instance.GetTrackedPointingJoint();
        }
    }

    private Vector2 ApplyVerticalOffset(Vector2 screenPos)
    {
        var screenPosM = GlobalSettings.virtualScreen.PixelsToMeters(screenPos);
        screenPosM += Vector2.up * verticalCursorOffset;
        return GlobalSettings.virtualScreen.MetersToPixels(screenPosM);
    }
}
