using System;

namespace Ultraleap.TouchFree.ServiceShared
{
    public abstract class BaseSettings
    {
        public static event Action OnConfigUpdated;
        public void ConfigWasUpdated()
        {
            OnConfigUpdated?.Invoke();
        }

        public abstract void SetAllValuesToDefault();
    }
}