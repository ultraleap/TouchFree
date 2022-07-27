using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library.Connection
{
    public class HandDataStreamStateQueueHandler : MessageQueueHandler
    {
        private readonly ITrackingConnectionManager trackingConnectionManager;

        public override ActionCode[] ActionCodes => new[] { ActionCode.SET_HAND_DATA_STREAM_STATE };

        public HandDataStreamStateQueueHandler(UpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, ITrackingConnectionManager _trackingConnectionManager) : base(_updateBehaviour, _clientMgr)
        {
            trackingConnectionManager = _trackingConnectionManager;
        }

        protected override void Handle(IncomingRequest _request)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_request.content);

            // Explicitly check for requestID because it is the only required key
            if (!RequestIdExists(contentObj))
            {
                ResponseToClient failureResponse = new ResponseToClient(string.Empty, "Failure", string.Empty, _request.content);
                failureResponse.message = "Setting the hand data stream state failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sending the status,
                // the Client will have no way to handle the config state
                clientMgr.SendHandDataStreamStateResponse(failureResponse);
                return;
            }


            trackingConnectionManager.SetImagesState(contentObj.GetValue("enabled").ToString() == true.ToString());

            ResponseToClient response = new ResponseToClient(contentObj.GetValue("requestID").ToString(), "Success", string.Empty, _request.content);
            clientMgr.SendHandDataStreamStateResponse(response);
        }
    }
}
