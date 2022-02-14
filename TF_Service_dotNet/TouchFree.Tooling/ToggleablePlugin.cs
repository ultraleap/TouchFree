namespace Ultraleap.TouchFree.Tooling
{
    // Struct: ToggleablePlugin
    // A Data structure used to toggle the use of plugins.
    [System.Serializable]
    internal struct ToggleablePlugin
    {
        public bool enabled;
        public InputActionPlugin plugin;
    }
}
