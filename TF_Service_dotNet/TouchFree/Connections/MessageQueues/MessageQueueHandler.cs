using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public abstract class MessageQueueHandler : IMessageQueueHandler
    {
        public abstract ActionCode[] ActionCodes { get; }

        private readonly ConcurrentQueue<IncomingRequest> queue = new();
        private readonly IUpdateBehaviour updateBehaviour;
        protected readonly IClientConnectionManager clientMgr;

        protected abstract string noRequestIdFailureMessage { get; }
        protected abstract ActionCode noRequestIdFailureActionCode { get; }

        public MessageQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr)
        {
            updateBehaviour = _updateBehaviour;

            updateBehaviour.OnUpdate += OnUpdate;
            clientMgr = _clientMgr;
        }

        public void AddItemToQueue(IncomingRequest content)
        {
            if (!ActionCodes.Contains(content.action))
            {
                throw new System.ArgumentException("Unexpected action type", nameof(content));
            }
            queue.Enqueue(new IncomingRequest(content.action, content.requestId, content.content));
        }

        protected virtual void OnUpdate()
        {
            CheckQueue();
        }

        private void CheckQueue()
        {
            IncomingRequest content;
            if (queue.TryPeek(out content))
            {
                // Parse newly received messages
                queue.TryDequeue(out content);
                if (ActionCodes.Contains(content.action))
                {
                    ValidateRequestIdAndHandleRequest(content);
                }
                else
                {
                    TouchFreeLog.ErrorWriteLine($"Unexpected ActionType of {content.action} in {GetType().Name}");
                }
            }
        }

        private void ValidateRequestIdAndHandleRequest(IncomingRequest _request)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_request.content);

            if (!RequestIdExists(contentObj))
            {
                CreateAndSendNoRequestIdError(_request);
            }
            else
            {
                Handle(_request, contentObj, contentObj.GetValue("requestID").ToString());
            }
        }

        protected virtual void CreateAndSendNoRequestIdError(IncomingRequest _request)
        {
            ResponseToClient failureResponse = new ResponseToClient(string.Empty, "Failure", noRequestIdFailureMessage, _request.content);

            // This is a failed request, do not continue with sending the status,
            // the Client will have no way to handle the config state
            clientMgr.SendResponse(failureResponse, noRequestIdFailureActionCode);
        }

        protected abstract void Handle(IncomingRequest _request, JObject _contentObject, string requestId);

        public static bool RequestIdExists(JObject _content)
        {
            if (_content?.ContainsKey("requestID") != true || _content.GetValue("requestID").ToString() == string.Empty)
            {
                return false;
            }

            return true;
        }
    }
}
