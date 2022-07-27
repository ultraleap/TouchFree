using System.Collections.Concurrent;

namespace Ultraleap.TouchFree.Library.Connection
{
    public interface IMessageQueueHandler
    {
        ActionCode[] ActionCodes { get; }
        ConcurrentQueue<IncomingRequest> Queue { get; }
    }
}
