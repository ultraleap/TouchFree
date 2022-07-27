using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace Ultraleap.TouchFree.Library.Connection
{
    public abstract class MessageQueueHandler : IMessageQueueHandler
    {
        public ConcurrentQueue<string> Queue { get; private set; } = new ConcurrentQueue<string>();
        public abstract ActionCode ActionCode { get; }

        private readonly UpdateBehaviour updateBehaviour;
        protected readonly IClientConnectionManager clientMgr;

        public MessageQueueHandler(UpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr)
        {
            updateBehaviour = _updateBehaviour;

            updateBehaviour.OnUpdate += CheckQueue;
            clientMgr = _clientMgr;
        }

        void CheckQueue()
        {
            string content;
            if (Queue.TryPeek(out content))
            {
                // Parse newly received messages
                Queue.TryDequeue(out content);
                Handle(content);
            }
        }

        protected abstract void Handle(string _content);

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
