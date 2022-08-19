using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ConfigurationFileRequestQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] ActionCodes => new[] { ActionCode.REQUEST_CONFIGURATION_FILE };

        protected override string noRequestIdFailureMessage => "Config state request failed. This is due to a missing or invalid requestID";

        protected override ActionCode noRequestIdFailureActionCode => ActionCode.CONFIGURATION_RESPONSE;

        public ConfigurationFileRequestQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr) : base(_updateBehaviour, _clientMgr)
        {
        }

        protected override void Handle(IncomingRequest _request, JObject _contentObject, string requestId)
        {
            InteractionConfig interactions = InteractionConfigFile.LoadConfig();
            PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

            ConfigState currentConfig = new ConfigState(
                requestId,
                interactions,
                physical);

            clientMgr.SendResponse(currentConfig, ActionCode.CONFIGURATION_FILE_STATE);
        }
    }
}
