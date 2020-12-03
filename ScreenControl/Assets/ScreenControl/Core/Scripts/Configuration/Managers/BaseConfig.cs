using System;

namespace Ultraleap.ScreenControl.Core
{
    public abstract class BaseSettings
    {
        public static event Action OnConfigUpdated;
        public void ConfigWasUpdated()
        {
            OnConfigUpdated.Invoke();
        }

        public abstract void SetAllValuesToDefault();
    }
}