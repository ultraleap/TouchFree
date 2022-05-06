using System.Collections.Generic;
using System.Linq;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Interactions;

namespace Ultraleap.TouchFree.Library
{
    public class InteractionManager
    {
        private readonly IEnumerable<IInteraction> interactions;
        private readonly UpdateBehaviour updateBehaviour;
        private readonly IClientConnectionManager connectionManager;

        private Dictionary<IInteraction, float> activeInteractions;
        private IInteraction interactionCurrentlyDown;
        private IInteraction locationInteraction;

        private InputAction lastLocationInputAction;
        private InputAction nonLocationRelativeInputAction;

        public InteractionManager(
            UpdateBehaviour _updateBehaviour,
            IClientConnectionManager _connectionManager,
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
                    InteractionType.TOUCHPLANE,
                    InteractionType.VELOCITYSWIPE,
                    //InteractionType.PUSH
                };
            }
            else
            {
                interactionsToUse[1] = _config.InteractionType;
            }

            activeInteractions = interactions.Where(x => interactionsToUse.Contains(x.InteractionType)).ToDictionary(x => x, x => 1f);
            locationInteraction = interactions.SingleOrDefault(x => x.InteractionType == interactionsToUse[0]);

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

                    var interactionInputAction = interaction.Key.Update();
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
                        break;
                    }

                    if (!inputAction.HasValue || currentMaxConfidence < interactionInputAction.confidence * interaction.Value || interactionInputAction.confidence == 1)
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