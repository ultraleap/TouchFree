namespace Ultraleap.TouchFree.Service.Connection
{
    public class TrackingResponse
    {
        private bool needsMask;
        private bool needsImages;
        private bool needsOrientation;
        private bool needsAnalytics;

        public string requestId;
        public string originalRequest;
        public bool isGetRequest;
        public TrackingState state;

        public TrackingResponse(string _requestId,
                                string _originalRequest,
                                bool _isGetRequest,
                                bool _needsMask,
                                bool _needsImages,
                                bool _needsOrientation,
                                bool _needsAnalytics,
                                IDiagnosticAPI diagnosticAPI)
        {
            requestId = _requestId;
            originalRequest = _originalRequest;
            isGetRequest = _isGetRequest;
            needsMask = _needsMask;
            needsImages = _needsImages;
            needsOrientation = _needsOrientation;
            needsAnalytics = _needsAnalytics;

            if (_needsMask) {
                diagnosticApi.OnMaskingResponse += this.OnMasking;
            }

            if (_needsImages) {
                diagnosticApi.OnAllowImagesResponse += this.OnAllowImages;
            }

            if (_needsOrientation) {
                diagnosticApi.OnCameraOrientationResponse += response.OnCameraOrientation;
            }

            if (_needsAnalytics) {
                diagnosticApi.OnAnalyticsResponse += response.OnAnalytics;
            }

            state = new TrackingState();
        }

        public bool Ready() {
            return (!needsMask && !needsImages && !needsOrientation && !needsAnalytics);
        }

        public void OnMasking(ImageMaskData _mask) {
            state.mask = _mask;
            needsMask = false;
        }

        public void OnAllowImages(bool _allowImages) {
            state.allowImages = _allowImages;
            needsImages = false;
        }

        public void OnCameraOrientation(bool _cameraReversed) {
            state.cameraReversed = _cameraReversed;
            needsOrientation = false;
        }

        public void OnAnalytics(bool _analytics) {
            state.analyticsEnabled = _analytics;
            needsAnalytics = false;
        }
    }
}