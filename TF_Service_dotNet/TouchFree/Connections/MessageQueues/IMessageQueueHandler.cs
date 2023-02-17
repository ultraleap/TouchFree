namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public interface IMessageQueueHandler
    {
        ActionCode[] HandledActionCodes { get; }
        void AddItemToQueue(IncomingRequest request);
    }
}
