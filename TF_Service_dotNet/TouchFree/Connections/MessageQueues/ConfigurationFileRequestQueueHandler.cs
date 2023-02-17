using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ConfigurationFileRequestQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] HandledActionCodes => new[] { ActionCode.REQUEST_CONFIGURATION_FILE };

        protected override string whatThisHandlerDoes => "Config state request";

        protected override ActionCode failureActionCode => ActionCode.CONFIGURATION_RESPONSE;

        public ConfigurationFileRequestQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr) : base(_updateBehaviour, _clientMgr)
        {
        }

        protected override void Handle(ValidatedIncomingRequest request)
        {
            InteractionConfig interactions = InteractionConfigFile.LoadConfig();
            PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

            ConfigState currentConfig = new ConfigState(
                request.RequestId,
                interactions,
                physical);

            clientMgr.SendResponse(currentConfig, ActionCode.CONFIGURATION_FILE_STATE);
        }
    }
}
