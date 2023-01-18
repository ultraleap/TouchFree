using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ServiceStatusQueueHandler : MessageQueueHandler
    {
        private readonly IConfigManager configManager;
        private readonly IHandManager handManager;
        private readonly ITrackingDiagnosticApi trackingApi;

        public override ActionCode[] ActionCodes => new[] { ActionCode.REQUEST_SERVICE_STATUS };

        protected override string noRequestIdFailureMessage => "Service state request failed. This is due to a missing or invalid requestID";

        protected override ActionCode noRequestIdFailureActionCode => ActionCode.SERVICE_STATUS_RESPONSE;

        public ServiceStatusQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IConfigManager _configManager, IHandManager _handManager, ITrackingDiagnosticApi _trackingApi) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
            handManager = _handManager;
            trackingApi = _trackingApi;
        }

        protected override void Handle(IncomingRequest _request, JObject _contentObject, string requestId)
        {
            void handleDeviceInfoResponse()
            {
                var currentConfig = new ServiceStatus(
                    string.Empty, // No request id as this event is not a response to a request
                    handManager.ConnectionManager.TrackingServiceState,
                    configManager.ErrorLoadingConfigFiles ? ConfigurationState.ERRORED : ConfigurationState.LOADED,
                    VersionManager.ApiVersion.ToString(),
                    trackingApi.trackingServiceVersion != null ? trackingApi.trackingServiceVersion : "Tracking not connected",
                    trackingApi.connectedDeviceSerial != null ? trackingApi.connectedDeviceSerial : "Device not connected",
                    trackingApi.connectedDeviceFirmware != null ? trackingApi.connectedDeviceFirmware : "Device not connected");

                clientMgr.SendResponse(currentConfig, ActionCode.SERVICE_STATUS);

                trackingApi.OnTrackingDeviceInfoResponse -= handleDeviceInfoResponse;
            };

            trackingApi.OnTrackingDeviceInfoResponse += handleDeviceInfoResponse;
            trackingApi.RequestGetDeviceInfo();
        }
    }
}
