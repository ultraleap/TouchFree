using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.Options;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

namespace Ultraleap.TouchFree.Library
{
    public class InteractionManager
    {
        private readonly IEnumerable<IInteraction> interactions;
        private readonly InteractionTuning interactionTuning;
        private readonly UpdateBehaviour updateBehaviour;
        private readonly IClientConnectionManager connectionManager;

        private Dictionary<IInteraction, float> activeInteractions;
        private IInteraction interactionCurrentlyDown;
        private IInteraction locationInteraction;

        private InputAction lastLocationInputAction;
        private InputAction nonLocationRelativeInputAction;

        private Vector2? lastDownPosition;

        public InteractionManager(
            UpdateBehaviour _updateBehaviour,
            IClientConnectionManager _connectionManager,
            IEnumerable<IInteraction> _interactions,
            IOptions<InteractionTuning> _interactionTuning,
            IConfigManager _configManager)
        {
            updateBehaviour = _updateBehaviour;
            connectionManager = _connectionManager;
            interactions = _interactions;
            interactionTuning = _interactionTuning?.Value;

            _configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;

            OnInteractionSettingsUpdated(_configManager.InteractionConfig);
        }

        protected void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            var initialisationNotStarted = activeInteractions == null;

            List<InteractionType> interactionsToUse = new List<InteractionType>();

            if (_config.InteractionType == InteractionType.PUSH)
            {
                interactionsToUse.Add(InteractionType.PUSH);

                if (interactionTuning?.EnableAirClickWithAirPush == true)
                {
                    interactionsToUse.Add(InteractionType.AIRCLICK);
                }

                if (interactionTuning?.EnableVelocitySwipeWithAirPush == true)
                {
                    interactionsToUse.Add(InteractionType.VELOCITYSWIPE);
                }
            }
            else
            {
                interactionsToUse.Add(_config.InteractionType);
            }

            activeInteractions = interactions.Where(x => interactionsToUse.Contains(x.InteractionType)).ToDictionary(x => x, x => 1f);
            locationInteraction = interactions.SingleOrDefault(x => x.InteractionType == _config.InteractionType);

            if (initialisationNotStarted)
            {
                updateBehaviour.OnUpdate += Update;
            }
        }

        protected void Update()
        {
            if (activeInteractions != null)
            {
                InputAction? inputAction = null;
                float currentMaxProgress = 0;
                InputAction? lastLocationActionToUpdate = null;

                if (interactionCurrentlyDown != null)
                {
                    var interactionInputAction = interactionCurrentlyDown.Update(1);
                    inputAction = interactionInputAction.inputAction;
                    currentMaxProgress = inputAction?.ProgressToClick ?? 0;

                    if (!inputAction.HasValue || inputAction.Value.InputType == InputType.UP || inputAction.Value.InputType == InputType.CANCEL)
                    {
                        if (interactionCurrentlyDown != locationInteraction)
                        {
                            lastLocationActionToUpdate = locationInteraction.Update(1).inputAction;
                        }
                        interactionCurrentlyDown = null;
                    }
                }
                else
                {
                    foreach (var interaction in activeInteractions)
                    {
                        var interactionInputAction = interaction.Key.Update(interaction.Value);

                        if (interaction.Key == locationInteraction)
                        {
                            lastLocationActionToUpdate = interactionInputAction.inputAction;
                            if (interactionCurrentlyDown == null)
                            { 
                                inputAction = interactionInputAction.inputAction;
                            }
                        }

                        if (interactionCurrentlyDown == null && interactionInputAction != null && interactionInputAction.actionDetected && interactionInputAction.inputAction.InputType == InputType.DOWN)
                        {
                            inputAction = interactionInputAction.inputAction;
                            interactionCurrentlyDown = interaction.Key;
                            nonLocationRelativeInputAction = interactionInputAction.inputAction;

                            if (interactionTuning?.EnableInteractionConfidence == true)
                            {
                                activeInteractions[interaction.Key] = (float)Math.Min(1, interaction.Value + 0.05);
                                foreach (var key in activeInteractions.Keys)
                                {
                                    if (key != interaction.Key)
                                    {
                                        activeInteractions[key] = (float)Math.Max(0.25, activeInteractions[key] - 0.05);
                                    }
                                }
                            }
                        }

                        if (currentMaxProgress < interactionInputAction.inputAction.ProgressToClick)
                        {
                            currentMaxProgress = interactionInputAction.inputAction.ProgressToClick;
                        }
                    }
                }

                if (lastLocationActionToUpdate.HasValue)
                {
                    lastLocationInputAction = lastLocationActionToUpdate.Value;
                }

                if (inputAction.HasValue)
                {
                    if (interactionCurrentlyDown != null)
                    {
                        lastDownPosition = lastLocationInputAction.CursorPosition + inputAction.Value.CursorPosition - nonLocationRelativeInputAction.CursorPosition;
                        var updatedPosition = new Positions(lastDownPosition.Value, inputAction.Value.DistanceFromScreen);
                        inputAction = new InputAction(inputAction.Value.Timestamp, inputAction.Value.InteractionType, inputAction.Value.HandType, inputAction.Value.Chirality, inputAction.Value.InputType,
                            updatedPosition, Math.Max(inputAction.Value.ProgressToClick, currentMaxProgress));
                    }
                    else
                    {
                        if (lastDownPosition.HasValue)
                        {
                            var differenceInLocations = lastDownPosition.Value - lastLocationInputAction.CursorPosition;
                            var differenceLength = differenceInLocations.Length();
                            var decrease = Utilities.MapRangeToRange(differenceLength, 10, Math.Max(300, differenceLength), 10, 50);
                            var decreaseRatio = decrease / differenceLength;
                            // Soften moving back to the location cursor position (this should be changed to use time so that it is consistent when we have lower frame rate)
                            lastDownPosition = differenceLength > 10 ? (differenceInLocations * decreaseRatio) + lastLocationInputAction.CursorPosition : null;
                        }

                        var updatedPosition = new Positions(lastDownPosition ?? lastLocationInputAction.CursorPosition, lastLocationInputAction.DistanceFromScreen);
                        inputAction = new InputAction(inputAction.Value.Timestamp, inputAction.Value.InteractionType, inputAction.Value.HandType, inputAction.Value.Chirality, inputAction.Value.InputType,
                            updatedPosition, Math.Max(inputAction.Value.ProgressToClick, currentMaxProgress));
                    }
                    connectionManager.SendInputActionToWebsocket(inputAction.Value);
                }
            }
        }
    }
}