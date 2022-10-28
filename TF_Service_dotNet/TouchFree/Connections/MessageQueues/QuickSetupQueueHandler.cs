using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Configuration.QuickSetup;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class QuickSetupQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] ActionCodes => new[] { ActionCode.QUICK_SETUP };

        protected override string noRequestIdFailureMessage => "Config state request failed. This is due to a missing or invalid requestID";

        protected override ActionCode noRequestIdFailureActionCode => ActionCode.CONFIGURATION_RESPONSE;

        private readonly IQuickSetupHandler quickSetupHandler;

        public QuickSetupQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IQuickSetupHandler _quickSetupHandler) : base(_updateBehaviour, _clientMgr)
        {
            quickSetupHandler = _quickSetupHandler;
        }

        protected override void Handle(IncomingRequest _request, JObject _contentObject, string requestId)
        {
            QuickSetupRequest? quickSetupRequest = null;

            try
            {
                quickSetupRequest = JsonConvert.DeserializeObject<QuickSetupRequest>(_request.content);
            }
            catch { }

            var quickSetupResponse = quickSetupHandler.HandlePositionRecording(quickSetupRequest.Value.Position);

            if (quickSetupResponse?.ConfigurationUpdated == true)
            {
                InteractionConfig interactions = InteractionConfigFile.LoadConfig();
                PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

                ConfigState currentConfig = new ConfigState(
                    requestId,
                    interactions,
                    physical);

                clientMgr.SendResponse(currentConfig, ActionCode.QUICK_SETUP_CONFIG);
            }
            else if (quickSetupResponse?.PositionRecorded == true)
            {
                ResponseToClient response = new ResponseToClient(requestId, "Success", string.Empty, _request.content);
                clientMgr.SendResponse(response, ActionCode.QUICK_SETUP_RESPONSE);
            }
            else
            {
                ResponseToClient response = new ResponseToClient(requestId, "Failure", quickSetupResponse?.QuickSetupError ?? string.Empty, _request.content);
                clientMgr.SendResponse(response, ActionCode.QUICK_SETUP_RESPONSE);
            }
        }
    }
}
