namespace Ultraleap.TouchFree.Library.Connections.MessageQueues;

public class HandDataStreamStateQueueHandler : MessageQueueHandler
{
    private readonly ITrackingConnectionManager _trackingConnectionManager;
    private readonly IHandManager _handManager;

    public override ActionCode[] HandledActionCodes => new[] { ActionCode.SET_HAND_DATA_STREAM_STATE };

    protected override string WhatThisHandlerDoes => "Setting the hand data stream state";
    protected override ActionCode FailureActionCode => ActionCode.SET_HAND_DATA_STREAM_STATE_RESPONSE;

    public HandDataStreamStateQueueHandler(IUpdateBehaviour updateBehaviour, IClientConnectionManager clientMgr, ITrackingConnectionManager trackingConnectionManager, IHandManager handManager)
        : base(updateBehaviour, clientMgr)
    {
        _trackingConnectionManager = trackingConnectionManager;
        _handManager = handManager;
    }

    protected override void Handle(in IncomingRequestWithId request)
    {
        _trackingConnectionManager.SetImagesState(request.ContentRoot.GetValue("enabled").ToString() == true.ToString());
        var lens = request.ContentRoot.GetValue("lens")?.ToString()?.ToLower() == Leap.Image.CameraType.LEFT.ToString().ToLower() ? Leap.Image.CameraType.LEFT : Leap.Image.CameraType.RIGHT;
        _handManager.HandRenderLens = lens;

        SendSuccessResponse(request, ActionCode.SET_HAND_DATA_STREAM_STATE_RESPONSE);
    }
}