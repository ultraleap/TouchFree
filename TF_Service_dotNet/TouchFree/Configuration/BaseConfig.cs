using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public abstract class BaseConfig
    {
        public static event Action OnConfigUpdated;
        public void ConfigWasUpdated()
        {
            OnConfigUpdated?.Invoke();
        }
    }
}