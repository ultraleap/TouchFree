namespace Ultraleap.TouchFree.Library.Configuration
{
    public interface IQuickSetupHandler
    {
        QuickSetupResponse HandleQuickSetupCall(QuickSetupPosition position);
    }
}
