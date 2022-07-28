namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public interface IMessageQueueHandler
    {
        ActionCode[] ActionCodes { get; }
        void AddItemToQueue(IncomingRequest content);
    }
}
