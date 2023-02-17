namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class HandDataStreamStateQueueHandler : MessageQueueHandler
    {
        private readonly ITrackingConnectionManager trackingConnectionManager;
        private readonly IHandManager handManager;

        public override ActionCode[] HandledActionCodes => new[] { ActionCode.SET_HAND_DATA_STREAM_STATE };

        protected override string whatThisHandlerDoes => "Setting the hand data stream state";
        protected override ActionCode failureActionCode => ActionCode.SET_HAND_DATA_STREAM_STATE_RESPONSE;

        public HandDataStreamStateQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, ITrackingConnectionManager _trackingConnectionManager, IHandManager _handManager) : base(_updateBehaviour, _clientMgr)
        {
            trackingConnectionManager = _trackingConnectionManager;
            handManager = _handManager;
        }

        protected override void Handle(ValidatedIncomingRequest request)
        {
            trackingConnectionManager.SetImagesState(request.ContentRoot.GetValue("enabled").ToString() == true.ToString());
            var lens = request.ContentRoot.GetValue("lens")?.ToString()?.ToLower() == Leap.Image.CameraType.LEFT.ToString().ToLower() ? Leap.Image.CameraType.LEFT : Leap.Image.CameraType.RIGHT;
            handManager.HandRenderLens = lens;

            SendSuccessResponse(request, ActionCode.SET_HAND_DATA_STREAM_STATE_RESPONSE);
        }
    }
}
