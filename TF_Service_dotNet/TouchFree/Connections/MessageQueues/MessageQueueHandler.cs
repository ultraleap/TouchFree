using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Linq;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public abstract class MessageQueueHandler : IMessageQueueHandler
    {
        public abstract ActionCode[] ActionCodes { get; }

        private readonly ConcurrentQueue<IncomingRequest> queue = new ConcurrentQueue<IncomingRequest>();
        private readonly UpdateBehaviour updateBehaviour;
        protected readonly IClientConnectionManager clientMgr;

        public MessageQueueHandler(UpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr)
        {
            updateBehaviour = _updateBehaviour;

            updateBehaviour.OnUpdate += CheckQueue;
            clientMgr = _clientMgr;
        }

        public void AddItemToQueue(IncomingRequest content)
        {
            if (!ActionCodes.Contains(content.action))
            {
                throw new System.Exception("Unexpected action type");
            }
            queue.Enqueue(new IncomingRequest(content.action, content.requestId, content.content));
        }

        void CheckQueue()
        {
            IncomingRequest content;
            if (queue.TryPeek(out content))
            {
                // Parse newly received messages
                queue.TryDequeue(out content);
                if (!ActionCodes.Contains(content.action))
                {
                    throw new System.Exception("Unexpected action type");
                }
                Handle(content);
            }
        }

        protected abstract void Handle(IncomingRequest _content);

        protected bool RequestIdExists(JObject _content)
        {
            if (!_content.ContainsKey("requestID") || _content.GetValue("requestID").ToString() == string.Empty)
            {
                return false;
            }

            return true;
        }
    }
}
