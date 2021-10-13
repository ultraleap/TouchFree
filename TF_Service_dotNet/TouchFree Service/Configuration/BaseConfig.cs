using System;

namespace Ultraleap.TouchFree.Service.Configuration
{
    public abstract class BaseConfig
    {
        public event Action OnConfigUpdated;
        public void ConfigWasUpdated()
        {
            OnConfigUpdated?.Invoke();
        }
    }
}