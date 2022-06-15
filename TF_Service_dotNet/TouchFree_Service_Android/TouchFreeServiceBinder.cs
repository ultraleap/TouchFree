

using Android.OS;
using Ultraleap.TouchFree.Library;

namespace Ultraleap.TouchFree.Service_Android
{
    public class TouchFreeServiceBinder : Binder
    {
        public InputAction? GetInputAction()
        {
            return null;
        }
    }
}