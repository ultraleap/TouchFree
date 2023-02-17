using Newtonsoft.Json;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Configuration.QuickSetup;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class QuickSetupQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] HandledActionCodes => new[] { ActionCode.QUICK_SETUP };

        protected override string whatThisHandlerDoes => "Config state request";

        protected override ActionCode failureActionCode => ActionCode.CONFIGURATION_RESPONSE;

        private readonly IQuickSetupHandler quickSetupHandler;

        public QuickSetupQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IQuickSetupHandler _quickSetupHandler) : base(_updateBehaviour, _clientMgr)
        {
            quickSetupHandler = _quickSetupHandler;
        }

        protected override void Handle(ValidatedIncomingRequest request)
        {
            QuickSetupRequest? quickSetupRequest = null;

            try
            {
                quickSetupRequest = JsonConvert.DeserializeObject<QuickSetupRequest>(request.OriginalContent);
            }
            catch { }

            var quickSetupResponse = quickSetupHandler.HandlePositionRecording(quickSetupRequest.Value.Position);

            if (quickSetupResponse?.ConfigurationUpdated == true)
            {
                InteractionConfig interactions = InteractionConfigFile.LoadConfig();
                PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

                ConfigState currentConfig = new ConfigState(
                    request.RequestId,
                    interactions,
                    physical);

                clientMgr.SendResponse(currentConfig, ActionCode.QUICK_SETUP_CONFIG);
            }
            else if (quickSetupResponse?.PositionRecorded == true)
            {
                SendSuccessResponse(request, ActionCode.QUICK_SETUP_RESPONSE);
            }
            else
            {
                SendErrorResponse(request, new Error(quickSetupResponse?.QuickSetupError ?? string.Empty), ActionCode.QUICK_SETUP_RESPONSE);
            }
        }
    }
}
