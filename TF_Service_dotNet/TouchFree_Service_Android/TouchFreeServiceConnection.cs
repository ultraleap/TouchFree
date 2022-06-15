using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ultraleap.TouchFree.Library;

namespace Ultraleap.TouchFree.Service_Android
{
    public class TouchFreeServiceConnection : Java.Lang.Object, IServiceConnection
    {
        MainActivity mainActivity;
        public TouchFreeServiceConnection(MainActivity activity)
        {
            IsConnected = false;
            Binder = null;
            mainActivity = activity;
        }

        public bool IsConnected { get; private set; }
        public TouchFreeServiceBinder Binder { get; private set; }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as TouchFreeServiceBinder;
            IsConnected = this.Binder != null;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            IsConnected = false;
            Binder = null;
        }
    }
}