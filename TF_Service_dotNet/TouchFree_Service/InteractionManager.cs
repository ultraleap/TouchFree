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

            var interactionsToUse = new[]
            {
                InteractionType.VELOCITYSWIPE,
                InteractionType.PUSH
            };

            activeInteractions = interactions.Where(x => interactionsToUse.Contains(x.InteractionType));

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
                        break;
                    }

                    if (!inputAction.HasValue || currentMaxConfidence < interactionInputAction.confidence)
                    {
                        inputAction = interactionInputAction.inputAction;
                    }
                }

                if (inputAction.HasValue)
                {
                    connectionManager.SendInputActionToWebsocket(inputAction.Value);
                }
            }
        }
    }
}