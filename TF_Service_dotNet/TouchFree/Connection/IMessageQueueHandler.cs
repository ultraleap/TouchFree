using System.Collections.Concurrent;

namespace Ultraleap.TouchFree.Library.Connection
{
    public interface IMessageQueueHandler
    {
        ActionCode ActionCode { get; }
        ConcurrentQueue<string> Queue { get; }
    }
}
