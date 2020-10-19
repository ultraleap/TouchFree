using UnityEngine;

using System;


public enum InteractionType
{
    Undefined,
    Push,
    Grab,
    Hover,
}
public enum TrackedHand
{
    PRIMARY,    // The current tracked hand
    LEFT,
    RIGHT
}

public class InteractionModule : MonoBehaviour
{
    public virtual InteractionType InteractionType {get;} = InteractionType.Undefined;

    public TrackedHand trackedHand;

    public bool ignoreDragging;
    public PositioningModule positioningModule;
    public bool allowHover;

    public delegate void InputAction(TrackedHand trackedHand, InputActionData _inputData);
    public static event InputAction HandleInputAction;

    protected Positions positions;

    private HandChirality handChirality = HandChirality.UNKNOWN;

    protected long latestTimestamp;


    void Update()
    {
        // Obtain the relevant Hand Data from the HandManager, and call the main UpdateData function

        latestTimestamp = HandManager.Instance.Timestamp;

        switch (trackedHand)
        {
            case TrackedHand.PRIMARY:
                Leap.Hand hand = HandManager.Instance.PrimaryHand;
                if (hand != null)
                {
                    handChirality = hand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;
                }
                // If the hand == null, keep the stored chirality.
                UpdateData(hand);
                break;
            case TrackedHand.LEFT:
                handChirality = HandChirality.LEFT;
                UpdateData(HandManager.Instance.LeftHand);
                break;
            case TrackedHand.RIGHT:
                handChirality = HandChirality.RIGHT;
                UpdateData(HandManager.Instance.RightHand);
                break;
        }

    }

    // This is the main update loop of the interaction module
    protected virtual void UpdateData(Leap.Hand hand) { }

    protected void SendInputAction(InputType _type, Positions _positions, float _progressToClick)
    {
        InputActionData actionData = new InputActionData(latestTimestamp, InteractionType, handChirality, _type, _positions, _progressToClick);
        HandleInputAction?.Invoke(trackedHand, actionData);
    }

    protected virtual void OnEnable()
    {
        SettingsConfig.OnConfigUpdated += OnSettingsUpdated;
        OnSettingsUpdated();
        PhysicalConfigurable.CreateVirtualScreen(PhysicalConfigurable.Config);
        positioningModule.Stabiliser.ResetValues();
    }

    protected virtual void OnDisable()
    {
        SettingsConfig.OnConfigUpdated -= OnSettingsUpdated;
    }

    protected virtual void OnSettingsUpdated()
    {
        ignoreDragging = !SettingsConfig.Config.UseScrollingOrDragging;
        allowHover = SettingsConfig.Config.SendHoverEvents;
//        positioningModule.Stabiliser.defaultDeadzoneRadius = SettingsConfig.Config.DeadzoneRadius;
    }
}