using Newtonsoft.Json;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Configuration.QuickSetup;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class QuickSetupQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] ActionCodes => new[] { ActionCode.QUICK_SETUP };

        private readonly IQuickSetupHandler quickSetupHandler;

        public QuickSetupQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IQuickSetupHandler _quickSetupHandler) : base(_updateBehaviour, _clientMgr)
        {
            quickSetupHandler = _quickSetupHandler;
        }

        protected override void Handle(IncomingRequest _request)
        {
            QuickSetupRequest? quickSetupRequest = null;

            try
            {
                quickSetupRequest = JsonConvert.DeserializeObject<QuickSetupRequest>(_request.content);
            }
            catch { }

            // Explicitly check for requestID because it is the only required key
            if (string.IsNullOrWhiteSpace(quickSetupRequest?.requestID))
            {
                ResponseToClient response = new ResponseToClient(string.Empty, "Failure", string.Empty, _request.content);
                response.message = "Config state request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sending the configuration,
                // the Client will have no way to handle the config state
                clientMgr.SendConfigChangeResponse(response);
                return;
            }

            var quickSetupResponse = quickSetupHandler.HandlePositionRecording(quickSetupRequest.Value.Position);

            if (quickSetupResponse?.ConfigurationUpdated == true)
            {
                InteractionConfig interactions = InteractionConfigFile.LoadConfig();
                PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

                ConfigState currentConfig = new ConfigState(
                    quickSetupRequest.Value.requestID,
                    interactions,
                    physical);

                clientMgr.SendQuickSetupConfigFile(currentConfig);
            }
            else if (quickSetupResponse?.PositionRecorded == true)
            {
                ResponseToClient response = new ResponseToClient(quickSetupRequest.Value.requestID, "Success", string.Empty, _request.content);
                clientMgr.SendQuickSetupResponse(response);
            }
            else
            {
                ResponseToClient response = new ResponseToClient(quickSetupRequest.Value.requestID, "Failure", quickSetupResponse?.QuickSetupError ?? string.Empty, _request.content);
                clientMgr.SendQuickSetupResponse(response);
            }
        }
    }
}
