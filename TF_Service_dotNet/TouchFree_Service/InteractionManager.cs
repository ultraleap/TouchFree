using System.Collections.Generic;
using System.Linq;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

using Ultraleap.TouchFree.Service.Connection;

namespace Ultraleap.TouchFree.Service
{
    public class InteractionManager
    {
        private readonly IEnumerable<IInteraction> interactions;
        private readonly UpdateBehaviour updateBehaviour;
        private readonly ClientConnectionManager connectionManager;

        private IEnumerable<IInteraction> activeInteractions;
        private IInteraction interactionCurrentlyDown;
        private IInteraction locationInteraction;

        private InputAction lastLocationInputAction;
        private InputAction nonLocationRelativeInputAction;

        public InteractionManager(
            UpdateBehaviour _updateBehaviour,
            ClientConnectionManager _connectionManager,
            IEnumerable<IInteraction> _interactions,
            IConfigManager _configManager)
        {
            updateBehaviour = _updateBehaviour;
            connectionManager = _connectionManager;
            interactions = _interactions;

            _configManager.OnInteractionConfigUpdated += OnInteractionSettingsUpdated;

            OnInteractionSettingsUpdated(_configManager.InteractionConfig);
        }

        protected void OnInteractionSettingsUpdated(InteractionConfigInternal _config)
        {
            var initialisationNotStarted = activeInteractions == null;

            InteractionType[] interactionsToUse = new InteractionType[1];

            if (_config.InteractionType == InteractionType.PUSH)
            {
                interactionsToUse = new[]
                {
                    InteractionType.VELOCITYSWIPE,
                    InteractionType.PUSH
                };
            }
            else
            {
                interactionsToUse[1] = _config.InteractionType;
            }

            activeInteractions = interactions.Where(x => interactionsToUse.Contains(x.InteractionType));
            locationInteraction = interactions.Single(x => x.InteractionType == _config.InteractionType);

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
                    if (interactionCurrentlyDown != null && interactionCurrentlyDown != interaction)
                    {
                        continue;
                    }

                    var interactionInputAction = interaction.Update();
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
                        interactionCurrentlyDown = interaction;
                        nonLocationRelativeInputAction = interactionInputAction.inputAction;
                        break;
                    }

                    if (!inputAction.HasValue || currentMaxConfidence < interactionInputAction.confidence)
                    {
                        inputAction = interactionInputAction.inputAction;
                    }

                    if (interaction == locationInteraction)
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
                        var updatedPosition = new Positions(lastLocationInputAction.CursorPosition + (inputAction.Value.CursorPosition - nonLocationRelativeInputAction.CursorPosition), inputAction.Value.DistanceFromScreen);
                        inputAction = new InputAction(inputAction.Value.Timestamp, inputAction.Value.InteractionType, inputAction.Value.HandType, inputAction.Value.Chirality, inputAction.Value.InputType,
                            updatedPosition, inputAction.Value.ProgressToClick);
                    }
                    else
                    {
                        var updatedPosition = new Positions(lastLocationInputAction.CursorPosition, lastLocationInputAction.DistanceFromScreen);
                        inputAction = new InputAction(inputAction.Value.Timestamp, inputAction.Value.InteractionType, inputAction.Value.HandType, inputAction.Value.Chirality, inputAction.Value.InputType,
                            updatedPosition, inputAction.Value.ProgressToClick);
                    }
                    connectionManager.SendInputActionToWebsocket(inputAction.Value);
                }
            }
        }
    }
}