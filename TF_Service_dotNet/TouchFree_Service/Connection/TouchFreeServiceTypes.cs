namespace Ultraleap.TouchFree.Service.ConnectionTypes
{
    internal enum Compatibility
    {
        COMPATIBLE,
        SERVICE_OUTDATED,
        CLIENT_OUTDATED,
        SERVICE_OUTDATED_WARNING,
        CLIENT_OUTDATED_WARNING
    }

    internal struct CommunicationWrapper<T>
    {
        public string action;
        public T content;

        public CommunicationWrapper(string _actionCode, T _content)
        {
            action = _actionCode;
            content = _content;
        }
    }
}