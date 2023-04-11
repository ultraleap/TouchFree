using System.Collections.Generic;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library.Interactions.InteractionModules;

public abstract class InteractionModule : IInteraction
{
    public virtual InteractionType InteractionType => InteractionType.PUSH;

    private HandChirality _handChirality;
    private HandType _handType; // TODO: This is never set?

    protected bool IgnoreDragging { get; private set; }
    protected bool IgnoreSwiping { get; private set; }

    protected Positions positions;

    protected float DistanceFromScreenMm { get; private set; }
    protected long LatestTimestamp { get; private set; }
    protected bool HadHandLastFrame { get; private set; }

    protected IPositioningModule PositioningModule { get; }
    protected IPositionStabiliser PositionStabiliser { get; }
    protected IHandManager HandManager { get; }
    protected IVirtualScreen VirtualScreen { get; }
    protected IEnumerable<PositionTrackerConfiguration> PositionConfiguration { get; set; }

    private InteractionZoneState _lastInteractionZoneState = InteractionZoneState.HAND_EXITED;
    
    private readonly IConfigManager _configManager;
    private readonly IClientConnectionManager _connectionManager;

    protected InteractionModule(
        IHandManager handManager, 
        IVirtualScreen virtualScreen,
        IConfigManager configManager,
        IClientConnectionManager connectionManager,
        IPositioningModule positioningModule,
        IPositionStabiliser positionStabiliser
    )
    {
        HandManager = handManager;
        VirtualScreen = virtualScreen;
        _configManager = configManager;
        _connectionManager = connectionManager;
        PositioningModule = positioningModule;
        PositionStabiliser = positionStabiliser;

        _configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;
        
        // TODO: Virtual member call in constructor - rearchitect this out
        OnInteractionSettingsUpdated(_configManager.InteractionConfig);
    }

    public InputActionResult Update(float confidence)
    {
        // Obtain the relevant Hand Data from the HandManager, and call the main UpdateData function
        LatestTimestamp = HandManager.Timestamp;

        var hand = GetHand();
        var inputAction = UpdateData(hand, confidence);

        HadHandLastFrame = hand != null;

        return inputAction;
    }

    // This is the main update loop of the interaction module
    protected abstract InputActionResult UpdateData(Leap.Hand hand, float confidence);

    protected virtual void OnInteractionSettingsUpdated(InteractionConfigInternal interactionConfig)
    {
        IgnoreDragging = !interactionConfig.UseScrollingOrDragging;
        IgnoreSwiping = !interactionConfig.UseSwipeInteraction;
        PositionStabiliser.ResetValues();
    }

    protected InputActionResult CreateInputActionResult(InputType inputType, Positions pos, float progressToClick) =>
        new(new InputAction(LatestTimestamp, InteractionType, _handType, _handChirality, inputType, pos, progressToClick),
            progressToClick);

    private Leap.Hand GetHand()
    {
        var hand = _handType switch
        {
            HandType.PRIMARY => HandManager.PrimaryHand,
            HandType.SECONDARY => HandManager.SecondaryHand,
            _ => null
        };

        if (hand != null)
        {
            _handChirality = hand.IsLeft ? HandChirality.LEFT : HandChirality.RIGHT;

            positions = PositioningModule.CalculatePositions(hand, PositionConfiguration);
            positions = ApplyAdditionalPositionModifiers(positions);
            positions = PositioningModule.ApplyStabilisation(positions, PositionStabiliser);
            DistanceFromScreenMm = positions.DistanceFromScreen * 1000f;
            hand = CheckHandInInteractionZone(hand);
        }

        return hand;
    }

    protected virtual Positions ApplyAdditionalPositionModifiers(Positions pos) => pos;
    
    protected virtual bool CheckForStartDrag(Vector2 startPos, Vector2 currentPos) => startPos != currentPos;

    /// <summary>
    /// Check if the hand is within the interaction zone. Return relevant results.
    /// This should be performed after 'positions' has been calculated.
    /// </summary>
    /// <param name="hand"></param>
    /// <returns>Returns null if the hand is outside of the interaction zone</returns>
    private Leap.Hand CheckHandInInteractionZone(Leap.Hand hand)
    {
        if (hand != null && _configManager.InteractionConfig.InteractionZoneEnabled)
        {
            if (DistanceFromScreenMm < _configManager.InteractionConfig.InteractionMinDistanceMm ||
                DistanceFromScreenMm > _configManager.InteractionConfig.InteractionMaxDistanceMm)
            {   

                if (_lastInteractionZoneState != InteractionZoneState.HAND_EXITED) 
                {
                    _connectionManager.HandleInteractionZoneEvent(InteractionZoneState.HAND_EXITED);
                    _lastInteractionZoneState = InteractionZoneState.HAND_EXITED;
                }
                return null;
            }

            if (_lastInteractionZoneState != InteractionZoneState.HAND_ENTERED) 
            {
                _connectionManager.HandleInteractionZoneEvent(InteractionZoneState.HAND_ENTERED);
                _lastInteractionZoneState = InteractionZoneState.HAND_ENTERED;
            }

        }

        return hand;
    }
}