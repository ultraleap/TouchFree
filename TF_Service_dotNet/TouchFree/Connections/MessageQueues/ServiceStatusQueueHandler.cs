using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ServiceStatusQueueHandler : MessageQueueHandler
    {
        private readonly IConfigManager _configManager;
        private readonly IHandManager _handManager;
        private readonly ITrackingDiagnosticApi _trackingApi;

        public override ActionCode[] HandledActionCodes => new[] { ActionCode.REQUEST_SERVICE_STATUS };

        protected override string whatThisHandlerDoes => "Service state request";

        protected override ActionCode failureActionCode => ActionCode.SERVICE_STATUS_RESPONSE;

        public ServiceStatusQueueHandler(IUpdateBehaviour updateBehaviour, IClientConnectionManager clientMgr,
            IConfigManager configManager, IHandManager handManager, ITrackingDiagnosticApi trackingApi)
            : base(updateBehaviour, clientMgr)
        {
            _configManager = configManager;
            _handManager = handManager;
            _trackingApi = trackingApi;
        }

        protected override void Handle(IncomingRequestWithId request)
        {
            var currentStatus = new ServiceStatus(
                request.RequestId,
                _handManager.ConnectionManager.TrackingServiceState,
                _configManager.ErrorLoadingConfigFiles ? ConfigurationState.ERRORED : ConfigurationState.LOADED,
                VersionManager.Version,
                _trackingApi.ApiInfo.GetValueOrDefault().ServiceVersion,
                _trackingApi.ConnectedDevice.GetValueOrDefault().Serial,
                _trackingApi.ConnectedDevice.GetValueOrDefault().Firmware);

            clientMgr.SendResponse(currentStatus, ActionCode.SERVICE_STATUS);
        }
    }
}
