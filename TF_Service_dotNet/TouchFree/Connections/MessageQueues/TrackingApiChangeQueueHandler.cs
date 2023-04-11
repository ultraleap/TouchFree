using System;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues;

public class TrackingApiChangeQueueHandler : MessageQueueHandler
{
    public override ActionCode[] HandledActionCodes => new[] { ActionCode.GET_TRACKING_STATE, ActionCode.SET_TRACKING_STATE };

    protected override string WhatThisHandlerDoes => "Tracking State change request";

    protected override ActionCode FailureActionCode => ActionCode.TRACKING_STATE;

    private DateTime? _requestOriginTime = null;
    private readonly TimeSpan _requestTimeout = TimeSpan.FromSeconds(3d);
    private readonly IConfigManager _configManager;
    private readonly ITrackingDiagnosticApi _diagnosticApi;
    private readonly object _requestLock = new(); 
    private IncomingRequestWithId? _currentRequest;

    public TrackingApiChangeQueueHandler(IUpdateBehaviour updateBehaviour, IConfigManager configManager, IClientConnectionManager clientMgr, ITrackingDiagnosticApi diagnosticApi)
        : base(updateBehaviour, clientMgr)
    {
        _configManager = configManager;
        _diagnosticApi = diagnosticApi;
    }

    protected override void OnUpdate()
    {
        lock (_requestLock)
        {
            switch (_currentRequest)
            {
                case { } req when _requestOriginTime.HasValue &&
                                  DateTime.Now - _requestOriginTime.Value > _requestTimeout:
                    TimeoutCurrentRequest(req);
                    break;
                case null:
                    // Don't process any more queued messages when we're already processing one
                    base.OnUpdate();
                    break;
            }
        }
    }

    protected override void HandleValidationError(in IncomingRequestWithId request, in Error error)
    {
        // TODO: Put error message somewhere in response
        var maskResponse = new SuccessWrapper<MaskingData?>(false, WhatThisHandlerDoes, null);
        var boolResponse = new SuccessWrapper<bool?>(false, WhatThisHandlerDoes, null);

        TrackingApiState state = new TrackingApiState()
        {
            requestID = request.RequestId,
            mask = maskResponse,
            allowImages = boolResponse,
            cameraReversed = boolResponse,
            analyticsEnabled = boolResponse
        };

        // This is a failed request, do not continue with processing the request,
        // the Client will have no way to handle the config state
        clientMgr.SendResponse(state, FailureActionCode);
    }

    protected override Result<Empty> ValidateContent(in IncomingRequestWithId request)
    {
        if (request.ActionCode == ActionCode.SET_TRACKING_STATE)
        {
            var atLeastOneProperty = false;
            atLeastOneProperty |= request.ContentRoot.ContainsKey("mask")
                                  || request.ContentRoot.ContainsKey("allowImages")
                                  || request.ContentRoot.ContainsKey("cameraReversed")
                                  || request.ContentRoot.ContainsKey("analyticsEnabled");
            if (!atLeastOneProperty) return new Error("Json contained no properties when attempting to Set Tracking State");
        }
            
        return Result.Success;
    }

    protected override void Handle(in IncomingRequestWithId request)
    {
        lock (_requestLock)
        {
            switch (request.ActionCode)
            {
                case ActionCode.GET_TRACKING_STATE:
                    _currentRequest = request;
                    _requestOriginTime = DateTime.Now;
                    HandleGetTrackingStateRequest(request);
                    break;
                case ActionCode.SET_TRACKING_STATE:
                    _currentRequest = request;
                    _requestOriginTime = DateTime.Now;
                    HandleSetTrackingStateRequest(request);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.ActionCode),
                        $"{GetType().Name} does not handle '{request.ActionCode}' messages");
            }
        }
    }

    private void TimeoutCurrentRequest(in IncomingRequestWithId request)
    {
        // TODO: Almost a repeat of HandleValidationError but the error message goes in a different place. Review this!
        string message = "Tracking State change request failed; no response from Tracking within timeout period";

        var maskResponse = new SuccessWrapper<MaskingData?>(false, message, null);
        var boolResponse = new SuccessWrapper<bool?>(false, message, null);

        var response = new TrackingApiState
        {
            requestID = request.RequestId,
            mask = maskResponse,
            allowImages = boolResponse,
            analyticsEnabled = boolResponse,
            cameraReversed = boolResponse,
        };

        clientMgr.SendResponse(response, ActionCode.TRACKING_STATE);
        _currentRequest = null;
        _requestOriginTime = null;
    }

    private void SendTrackingDataResponse(string requestId, in DiagnosticData data)
    {
        lock (_requestLock)
        {
            var response = new TrackingApiState
            {
                requestID = requestId,
                mask = data.Masking.HasValue
                    ? new SuccessWrapper<MaskingData?>(true, "Image Mask State", data.Masking.Value)
                    : new SuccessWrapper<MaskingData?>(false, string.Empty, null),
                allowImages = data.AllowImages.HasValue
                    ? new SuccessWrapper<bool?>(true, "AllowImages State", data.AllowImages.Value)
                    : new SuccessWrapper<bool?>(false, string.Empty, null),
                analyticsEnabled = data.Analytics.HasValue
                    ? new SuccessWrapper<bool?>(true, "Analytics State", data.Analytics.Value)
                    : new SuccessWrapper<bool?>(false, string.Empty, null),
                cameraReversed = data.CameraOrientation.HasValue
                    ? new SuccessWrapper<bool?>(true, "CameraOrientation State", data.CameraOrientation.Value)
                    : new SuccessWrapper<bool?>(false, string.Empty, null)
            };

            clientMgr.SendResponse(response, ActionCode.TRACKING_STATE);
            _currentRequest = null;
            _requestOriginTime = null;
        }
    }

    private async void HandleGetTrackingStateRequest(IncomingRequestWithId request)
    {
        var data = await _diagnosticApi.RequestGet();
        SendTrackingDataResponse(request.RequestId, data);
    }

    private async void HandleSetTrackingStateRequest(IncomingRequestWithId request)
    {
        var contentObj = request.ContentRoot;

        bool needsMask = contentObj.TryGetValue("mask", out var maskToken);
        bool needsImages = contentObj.TryGetValue("allowImages", out var allowImagesToken);
        bool needsOrientation = contentObj.TryGetValue("cameraReversed", out var cameraReversedToken);
        bool needsAnalytics = contentObj.TryGetValue("analyticsEnabled", out var analyticsEnabledToken);
            
        var data = new DiagnosticData
        {
            Analytics = needsAnalytics ? analyticsEnabledToken.ToObject<bool>() : null,
            Masking = needsMask ? maskToken.ToObject<MaskingData>() : null,
            AllowImages = needsImages ? allowImagesToken.ToObject<bool>() : null,
            CameraOrientation = needsOrientation ? cameraReversedToken.ToObject<bool>() : null
        };

        var trackingFromFile = _configManager.TrackingConfig;

        if (trackingFromFile != null && (needsAnalytics || needsImages || needsMask || needsOrientation))
        {
            trackingFromFile = trackingFromFile with
            {
                Mask = needsMask ? (Configuration.MaskingData)data.Masking.Value : trackingFromFile.Mask,
                AllowImages = needsImages ? data.AllowImages.Value : trackingFromFile.AllowImages,
                CameraReversed = needsOrientation ? data.CameraOrientation.Value : trackingFromFile.CameraReversed,
                AnalyticsEnabled = needsAnalytics ? data.Analytics.Value : trackingFromFile.AnalyticsEnabled,
            };

            TrackingConfigFile.SaveConfig(trackingFromFile);
        }
            
        await _diagnosticApi.RequestSet(data);

        SendTrackingDataResponse(request.RequestId, data);
    }
}