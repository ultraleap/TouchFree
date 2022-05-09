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

        private Vector2? lastPositionModification;

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
                float currentMaxConfidence = 0;
                InputAction? lastLocationActionToUpdate = null;
                foreach(var interaction in activeInteractions)
                {
                    if (interactionCurrentlyDown != null && interactionCurrentlyDown != interaction.Key)
                    {
                        continue;
                    }

                    var interactionInputAction = interaction.Key.Update(interaction.Value);
                    if (interactionCurrentlyDown != null)
                    {
                        inputAction = interactionInputAction.inputAction;
                        if (!inputAction.HasValue || inputAction.Value.InputType == InputType.UP || inputAction.Value.InputType == InputType.CANCEL)
                        {
                            interactionCurrentlyDown = null;
                        }
                        break;
                    }

                    if (interactionCurrentlyDown == null && interactionInputAction != null && interactionInputAction.actionDetected && interactionInputAction.inputAction.InputType == InputType.DOWN)
                    {
                        inputAction = interactionInputAction.inputAction;
                        interactionCurrentlyDown = interaction.Key;
                        nonLocationRelativeInputAction = interactionInputAction.inputAction;

                        if (interactionTuning?.EnableInteractionConfidence == true)
                        {
                            activeInteractions[interaction.Key] = (float)Math.Min(1, interaction.Value + 0.05);
                            foreach(var key in activeInteractions.Keys)
                            {
                                if (key != interaction.Key)
                                {
                                    activeInteractions[key] = (float)Math.Max(0.25, activeInteractions[key] - 0.05);
                                }
                            }
                        }

                        break;
                    }

                    if (!inputAction.HasValue || currentMaxConfidence < interactionInputAction.confidence * interaction.Value)
                    {
                        inputAction = interactionInputAction.inputAction;
                        currentMaxConfidence = interactionInputAction.confidence * interaction.Value;
                    }

                    if (interaction.Key == locationInteraction)
                    {
                        lastLocationActionToUpdate = inputAction;
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
                        lastPositionModification = inputAction.Value.CursorPosition - nonLocationRelativeInputAction.CursorPosition;
                        var updatedPosition = new Positions(lastLocationInputAction.CursorPosition + lastPositionModification.Value, inputAction.Value.DistanceFromScreen);
                        inputAction = new InputAction(inputAction.Value.Timestamp, inputAction.Value.InteractionType, inputAction.Value.HandType, inputAction.Value.Chirality, inputAction.Value.InputType,
                            updatedPosition, inputAction.Value.ProgressToClick);
                    }
                    else
                    {
                        if (lastPositionModification.HasValue)
                        {
                            // Soften moving back to the location cursor position (this should be changed to use time so that it is consistent when we have lower frame rate)
                            lastPositionModification = lastPositionModification.Value.Length() > 20 ? lastPositionModification.Value / 1.5f : null;
                        }

                        var updatedPosition = new Positions(lastLocationInputAction.CursorPosition + (lastPositionModification ?? new Vector2(0, 0)), lastLocationInputAction.DistanceFromScreen);
                        inputAction = new InputAction(inputAction.Value.Timestamp, inputAction.Value.InteractionType, inputAction.Value.HandType, inputAction.Value.Chirality, inputAction.Value.InputType,
                            updatedPosition, inputAction.Value.ProgressToClick);
                    }
                    connectionManager.SendInputActionToWebsocket(inputAction.Value);
                }
            }
        }
    }
}