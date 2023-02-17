using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ConfigurationStateRequestQueueHandler : MessageQueueHandler
    {
        private readonly IConfigManager configManager;

        public override ActionCode[] HandledActionCodes => new[] { ActionCode.REQUEST_CONFIGURATION_STATE };

        protected override string whatThisHandlerDoes => "Config state request";

        protected override ActionCode failureActionCode => ActionCode.CONFIGURATION_RESPONSE;

        public ConfigurationStateRequestQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IConfigManager _configManager) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
        }

        protected override void Handle(ValidatedIncomingRequest request)
        {
            ConfigState currentConfig = new ConfigState(
                request.RequestId,
                configManager.InteractionConfig.ForApi(),
                configManager.PhysicalConfig.ForApi());


            clientMgr.SendResponse(currentConfig, ActionCode.CONFIGURATION_STATE);
        }
    }
}
