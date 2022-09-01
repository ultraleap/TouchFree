using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ServiceStatusQueueHandler : MessageQueueHandler
    {
        private readonly IConfigManager configManager;
        private readonly IHandManager handManager;

        public override ActionCode[] ActionCodes => new[] { ActionCode.REQUEST_SERVICE_STATUS };

        protected override string noRequestIdFailureMessage => "Service state request failed. This is due to a missing or invalid requestID";

        protected override ActionCode noRequestIdFailureActionCode => ActionCode.SERVICE_STATUS_RESPONSE;

        public ServiceStatusQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IConfigManager _configManager, IHandManager _handManager) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
            handManager = _handManager;
        }

        protected override void Handle(IncomingRequest _request, JObject _contentObject, string requestId)
        {
            var currentConfig = new ServiceStatus(
                requestId,
                handManager.ConnectionManager.TrackingServiceState,
                configManager.ErrorLoadingConfigFiles ? ConfigurationState.ERRORED : ConfigurationState.LOADED);


            clientMgr.SendResponse(currentConfig, ActionCode.SERVICE_STATUS);
        }
    }
}
