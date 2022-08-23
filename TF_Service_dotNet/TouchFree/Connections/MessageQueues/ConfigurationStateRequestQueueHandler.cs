using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ConfigurationStateRequestQueueHandler : MessageQueueHandler
    {
        private readonly IConfigManager configManager;

        public override ActionCode[] ActionCodes => new[] { ActionCode.REQUEST_CONFIGURATION_STATE };

        protected override string noRequestIdFailureMessage => "Config state request failed. This is due to a missing or invalid requestID";

        protected override ActionCode noRequestIdFailureActionCode => ActionCode.CONFIGURATION_RESPONSE;

        public ConfigurationStateRequestQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IConfigManager _configManager) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
        }

        protected override void Handle(IncomingRequest _request, JObject _contentObject, string requestId)
        {
            ConfigState currentConfig = new ConfigState(
                requestId,
                configManager.InteractionConfig.ForApi(),
                configManager.PhysicalConfig.ForApi());


            clientMgr.SendResponse(currentConfig, ActionCode.CONFIGURATION_STATE);
        }
    }
}
