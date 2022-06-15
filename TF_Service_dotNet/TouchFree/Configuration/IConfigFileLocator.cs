namespace Ultraleap.TouchFree.Library.Configuration
{
    public interface IConfigFileLocator
    {
        string ConfigFileDirectory { get; }
        void ReloadConfigFileDirectoryFromRegistry();
    }
}
