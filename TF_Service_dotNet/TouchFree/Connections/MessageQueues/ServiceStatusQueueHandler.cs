using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ServiceStatusQueueHandler : MessageQueueHandler
    {
        private readonly IConfigManager configManager;
        private readonly IHandManager handManager;

        public override ActionCode[] ActionCodes => new[] { ActionCode.REQUEST_SERVICE_STATUS };

        public ServiceStatusQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IConfigManager _configManager, IHandManager _handManager) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
            handManager = _handManager;
        }

        protected override void Handle(IncomingRequest _request)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_request.content);

            // Explicitly check for requestID because it is the only required key
            if (!RequestIdExists(contentObj))
            {
                ResponseToClient response = new ResponseToClient(string.Empty, "Failure", string.Empty, _request.content);
                response.message = "Config state request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sending the status,
                // the Client will have no way to handle the config state
                clientMgr.SendStatusResponse(response);
                return;
            }

            TrackingServiceState trackingServiceState = TrackingServiceState.UNAVAILABLE;
            if (handManager.TrackingServiceConnected())
            {
                trackingServiceState = handManager.CameraConnected() ? TrackingServiceState.CONNECTED : TrackingServiceState.NO_CAMERA;
            }

            ServiceStatus currentConfig = new ServiceStatus(
                contentObj.GetValue("requestID").ToString(),
                trackingServiceState,
                configManager.ErrorLoadingConfigFiles ? ConfigurationState.ERRORED : ConfigurationState.LOADED);


            clientMgr.SendStatus(currentConfig);
        }
    }
}
