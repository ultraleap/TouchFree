using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Interactions;
using Ultraleap.TouchFree.Library.Interactions.InteractionModules;

namespace Ultraleap.TouchFree.Library;

public class InteractionManager
{
    private readonly IEnumerable<IInteraction> _interactions;
    private readonly InteractionTuning _interactionTuning;
    private readonly IHandManager _handManager;
    private readonly ITrackingConnectionManager _trackingConnectionManager;
    private readonly IClientConnectionManager _connectionManager;

    public Dictionary<IInteraction, float> ActiveInteractions { get; private set; }
    private IInteraction _interactionCurrentlyDown;
    private IInteraction _locationInteraction;

    private InputAction _lastLocationInputAction;
    private InputAction _nonLocationRelativeInputAction;

    private Vector2? _lastDownPosition;

    public InteractionManager(
        IUpdateBehaviour updateBehaviour,
        IClientConnectionManager connectionManager,
        IEnumerable<IInteraction> interactions,
        IOptions<InteractionTuning> interactionTuning,
        IConfigManager configManager,
        IHandManager handManager,
        ITrackingConnectionManager trackingConnectionManager)
    {
        _connectionManager = connectionManager;
        _interactions = interactions;
        _interactionTuning = interactionTuning?.Value;
        _handManager = handManager;
        _trackingConnectionManager = trackingConnectionManager;

        configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;

        OnInteractionSettingsUpdated(configManager.InteractionConfig);

        updateBehaviour.OnUpdate += Update;
        updateBehaviour.OnSlowUpdate += UpdateHands;
    }

    // TODO: Should not be public only to facilitate for tests
    public void OnInteractionSettingsUpdated(InteractionConfigInternal config)
    {
        List<InteractionType> interactionsToUse = new List<InteractionType>();

        if (config.InteractionType == InteractionType.PUSH)
        {
            interactionsToUse.Add(InteractionType.PUSH);

            if (_interactionTuning?.EnableAirClickWithAirPush == true)
            {
                interactionsToUse.Add(InteractionType.AIRCLICK);
            }
        }
        else
        {
            interactionsToUse.Add(config.InteractionType);
        }

        if (config.InteractionType != InteractionType.GRAB && config?.UseSwipeInteraction == true)
        {
            interactionsToUse.Add(InteractionType.VELOCITYSWIPE);
        }

        ActiveInteractions = _interactions.Where(x => interactionsToUse.Contains(x.InteractionType)).ToDictionary(x => x, x => 1f);
        _locationInteraction = _interactions.SingleOrDefault(x => x.InteractionType == config.InteractionType);

        // Reset the down position between interactions
        _lastDownPosition = null;
        _interactionCurrentlyDown = null;
    }

    private void UpdateHands()
    {
        if (_trackingConnectionManager.ShouldSendHandData)
        {
            _connectionManager.SendHandData(_handManager.RawHands, _handManager.LastImageData);
        }
    }

    private void Update()
    {
        if (ActiveInteractions != null)
        {
            InputAction? inputAction = null;
            float currentMaxProgress = 0;
            InputAction? lastLocationActionToUpdate = null;

            if (_interactionCurrentlyDown != null)
            {
                var interactionInputAction = _interactionCurrentlyDown.Update(1);
                inputAction = interactionInputAction.InputAction;
                currentMaxProgress = inputAction?.ProgressToClick ?? 0;

                if (_interactionCurrentlyDown == _locationInteraction)
                {
                    lastLocationActionToUpdate = inputAction;
                }

                if (!inputAction.HasValue || inputAction.Value.InputType is InputType.UP or InputType.CANCEL)
                {
                    if (_interactionCurrentlyDown != _locationInteraction)
                    {
                        lastLocationActionToUpdate = _locationInteraction.Update(1).InputAction;
                    }
                    _interactionCurrentlyDown = null;
                }
            }
            else
            {
                foreach (var interaction in ActiveInteractions)
                {
                    var interactionInputAction = interaction.Key.Update(interaction.Value);

                    if (interaction.Key == _locationInteraction)
                    {
                        lastLocationActionToUpdate = interactionInputAction.InputAction;
                        if (_interactionCurrentlyDown == null)
                        {
                            inputAction = interactionInputAction.InputAction;
                        }
                    }

                    if (_interactionCurrentlyDown == null && interactionInputAction.InputAction is { InputType: InputType.DOWN })
                    {
                        inputAction = interactionInputAction.InputAction;
                        _interactionCurrentlyDown = interaction.Key;
                        _nonLocationRelativeInputAction = interactionInputAction.InputAction.Value;

                        if (_interactionTuning?.EnableInteractionConfidence == true)
                        {
                            ActiveInteractions[interaction.Key] = (float)Math.Min(1, interaction.Value + 0.05);
                            foreach (var key in ActiveInteractions.Keys)
                            {
                                if (key != interaction.Key)
                                {
                                    ActiveInteractions[key] = (float)Math.Max(0.25, ActiveInteractions[key] - 0.05);
                                }
                            }
                        }
                    }

                    if (interactionInputAction.InputAction.HasValue && currentMaxProgress < interactionInputAction.InputAction.Value.ProgressToClick)
                    {
                        currentMaxProgress = interactionInputAction.InputAction.Value.ProgressToClick;
                    }
                }
            }

            if (lastLocationActionToUpdate.HasValue)
            {
                _lastLocationInputAction = lastLocationActionToUpdate.Value;
            }

            if (inputAction.HasValue)
            {
                if (_interactionCurrentlyDown != null)
                {
                    // We don't need to do anything if the location interaction is the one causing the DOWN
                    if (_interactionCurrentlyDown != _locationInteraction)
                    {
                        _lastDownPosition = _lastLocationInputAction.CursorPosition + inputAction.Value.CursorPosition - _nonLocationRelativeInputAction.CursorPosition;
                        var updatedPosition = new Positions(_lastDownPosition.Value, inputAction.Value.DistanceFromScreen);
                        inputAction = new InputAction(inputAction.Value.Timestamp, inputAction.Value.InteractionType, inputAction.Value.HandType, inputAction.Value.Chirality, inputAction.Value.InputType,
                            updatedPosition, Math.Max(inputAction.Value.ProgressToClick, currentMaxProgress));
                    }
                }
                else if (_interactionCurrentlyDown == null)
                {
                    if (_lastDownPosition.HasValue)
                    {
                        var differenceInLocations = _lastDownPosition.Value - _lastLocationInputAction.CursorPosition;
                        var differenceLength = differenceInLocations.Length();
                        var decrease = Utilities.MapRangeToRange(differenceLength, 10, Math.Max(300, differenceLength), 10, 50);
                        var decreaseRatio = decrease / differenceLength;
                        // Soften moving back to the location cursor position (this should be changed to use time so that it is consistent when we have lower frame rate)
                        _lastDownPosition = differenceLength > 10 ? (differenceInLocations * decreaseRatio) + _lastLocationInputAction.CursorPosition : null;
                    }

                    var updatedPosition = new Positions(_lastDownPosition ?? _lastLocationInputAction.CursorPosition, _lastLocationInputAction.DistanceFromScreen);
                    inputAction = new InputAction(inputAction.Value.Timestamp, inputAction.Value.InteractionType, inputAction.Value.HandType, inputAction.Value.Chirality, inputAction.Value.InputType,
                        updatedPosition, Math.Max(inputAction.Value.ProgressToClick, currentMaxProgress));
                }
                _connectionManager.SendInputAction(inputAction.Value);
            }
        }
    }
}