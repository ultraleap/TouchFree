using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues;

public class ServiceStatusQueueHandler : MessageQueueHandler
{
    private readonly IConfigManager _configManager;
    private readonly IHandManager _handManager;
    private readonly ITrackingDiagnosticApi _trackingApi;

    public override ActionCode[] HandledActionCodes => new[] { ActionCode.REQUEST_SERVICE_STATUS };

    protected override string WhatThisHandlerDoes => "Service state request";

    protected override ActionCode FailureActionCode => ActionCode.SERVICE_STATUS_RESPONSE;

    public ServiceStatusQueueHandler(IUpdateBehaviour updateBehaviour, IClientConnectionManager clientMgr,
        IConfigManager configManager, IHandManager handManager, ITrackingDiagnosticApi trackingApi)
        : base(updateBehaviour, clientMgr)
    {
        _configManager = configManager;
        _handManager = handManager;
        _trackingApi = trackingApi;
    }

    protected override void Handle(in IncomingRequestWithId request)
    {
        var requestWithId = request; // Copy as 'in' parameter cannot be used in local function
        
        // Implementation moved to local async function because Handle cannot be made 'async' while having 'in' parameter
        HandleAsync();
        async void HandleAsync()
        {
            var deviceInfo = await _trackingApi.RequestDeviceInfo();

            var currentStatus = ServiceStatus.FromDApiTypes(requestWithId.RequestId,
                _handManager.ConnectionManager.TrackingServiceState,
                _configManager.ErrorLoadingConfigFiles ? ConfigurationState.ERRORED : ConfigurationState.LOADED,
                VersionManager.Version,
                _trackingApi.ApiInfo,
                deviceInfo);

            clientMgr.SendResponse(currentStatus, ActionCode.SERVICE_STATUS);
        }
    }
}