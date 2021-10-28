using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public abstract class BaseConfig
    {
        public delegate void ConfigUpdated(BaseConfig config = null);
        public event ConfigUpdated OnConfigUpdated;

        public void ConfigWasUpdated()
        {
            OnConfigUpdated?.Invoke(this);
        }
    }
}