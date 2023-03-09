using Newtonsoft.Json.Linq;
using System;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Connections.DiagnosticApi;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class TrackingApiChangeQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] HandledActionCodes => new[] { ActionCode.GET_TRACKING_STATE, ActionCode.SET_TRACKING_STATE };

        protected override string whatThisHandlerDoes => "Tracking State change request";

        protected override ActionCode failureActionCode => ActionCode.TRACKING_STATE;

        private DateTime? requestOriginTime = null;
        private readonly TimeSpan requestTimeout = TimeSpan.FromSeconds(3d);
        private readonly IConfigManager configManager;
        private readonly ITrackingDiagnosticApi diagnosticApi;
        private readonly object requestLock = new(); 
        private IncomingRequestWithId? currentRequest;

        public TrackingApiChangeQueueHandler(IUpdateBehaviour _updateBehaviour, IConfigManager _configManager, IClientConnectionManager _clientMgr, ITrackingDiagnosticApi _diagnosticApi) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
            diagnosticApi = _diagnosticApi;
        }

        protected override void OnUpdate()
        {
            lock (requestLock)
            {
                switch (currentRequest)
                {
                    case { } req when requestOriginTime.HasValue &&
                                      DateTime.Now - requestOriginTime.Value > requestTimeout:
                        TimeoutCurrentRequest(req);
                        break;
                    case null:
                        // Don't process any more queued messages when we're already processing one
                        base.OnUpdate();
                        break;
                }
            }
        }

        protected override void HandleValidationError(IncomingRequestWithId request, Error error)
        {
            // TODO: Put error message somewhere in response
            var maskResponse = new SuccessWrapper<MaskingData?>(false, whatThisHandlerDoes, null);
            var boolResponse = new SuccessWrapper<bool?>(false, whatThisHandlerDoes, null);

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
            clientMgr.SendResponse(state, failureActionCode);
        }

        protected override Result<Empty> ValidateContent(IncomingRequestWithId request)
        {
            if (request.ActionCode == ActionCode.SET_TRACKING_STATE)
            {
                var atLeastOneProperty = false;
                // TODO: Check types of properties are correct here too, then remove checks from HandleSetTrackingStateRequest below 
                atLeastOneProperty |= request.ContentRoot.ContainsKey("mask")
                                      || request.ContentRoot.ContainsKey("allowImages")
                                      || request.ContentRoot.ContainsKey("cameraReversed")
                                      || request.ContentRoot.ContainsKey("analyticsEnabled");
                if (!atLeastOneProperty) return new Error("Json contained no properties when attempting to Set Tracking State");
            }
            
            return Result.Success;
        }

        protected override void Handle(IncomingRequestWithId request)
        {
            lock (requestLock)
            {
                switch (request.ActionCode)
                {
                    case ActionCode.GET_TRACKING_STATE:
                        currentRequest = request;
                        HandleGetTrackingStateRequest(request);
                        break;
                    case ActionCode.SET_TRACKING_STATE:
                        currentRequest = request;
                        HandleSetTrackingStateRequest(request);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(request.ActionCode),
                            $"{GetType().Name} does not handle '{request.ActionCode}' messages");
                }
            }
        }

        private void TimeoutCurrentRequest(IncomingRequestWithId request)
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
            currentRequest = null;
            requestOriginTime = null;
        }

        private void SendTrackingDataResponse(string requestId, DiagnosticData data)
        {
            lock (requestLock)
            {
                var response = new TrackingApiState
                {
                    requestID = requestId,
                    // TODO: Errors for cases with no value when caused by an error. Should be refactored to use Result
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
                currentRequest = null;
                requestOriginTime = null;
            }
        }

        private async void HandleGetTrackingStateRequest(IncomingRequestWithId request)
        {
            var data = await diagnosticApi.RequestGetAll(new GetAllInfo(true, true, true, true));
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

            var trackingFromFile = configManager.TrackingConfig;

            if (trackingFromFile != null && (needsAnalytics || needsImages || needsMask || needsOrientation))
            {
                if (needsMask)
                {
                    trackingFromFile.Mask.Left = data.Masking.Value.left;
                    trackingFromFile.Mask.Right = data.Masking.Value.right;
                    trackingFromFile.Mask.Upper = data.Masking.Value.upper;
                    trackingFromFile.Mask.Lower = data.Masking.Value.lower;
                }

                if (needsImages)
                {
                    trackingFromFile.AllowImages = data.AllowImages.Value;
                }

                if (needsOrientation)
                {
                    trackingFromFile.CameraReversed = data.CameraOrientation.Value;
                }

                if (needsAnalytics)
                {
                    trackingFromFile.AnalyticsEnabled = data.Analytics.Value;
                }

                await diagnosticApi.RequestSetAll(data);

                TrackingConfigFile.SaveConfig(trackingFromFile);
            }

            // TODO: Handle setting config that hasn't been initialised yet
            SendTrackingDataResponse(request.RequestId, data);
        }
    }
}
