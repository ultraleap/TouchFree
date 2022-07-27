using System.Collections.Concurrent;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public interface IMessageQueueHandler
    {
        ActionCode[] ActionCodes { get; }
        ConcurrentQueue<IncomingRequest> Queue { get; }
    }
}
