namespace Ultraleap.TouchFree.Library
{
    public interface IClientConnectionManager
    {
        void SendInputActionToWebsocket(InputAction _data);
    }
}
