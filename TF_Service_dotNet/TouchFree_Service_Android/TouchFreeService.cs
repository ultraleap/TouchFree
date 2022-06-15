using System.Net;
using Android.Content;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using Ultraleap.TouchFree.Library.Connection;

namespace Ultraleap.TouchFree.Service_Android
{
    public class TouchFreeService : Android.App.Service
    {
        private IWebSocketHandler webSocketHandler = null;

        public TouchFreeServiceBinder Binder { get; private set; }

        public override void OnCreate()
        {
            base.OnCreate();
            if (webSocketHandler == null)
            {
                webSocketHandler = MainActivity.ServiceProvider.GetService<IWebSocketHandler>();
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            if (webSocketHandler == null)
            {
                webSocketHandler = MainActivity.ServiceProvider.GetService<IWebSocketHandler>();
            }

            var httpListener = new HttpListener();
            httpListener.Prefixes.Clear();
            httpListener.Prefixes.Add("http://localhost:9739/");
            httpListener.Prefixes.Add("ws://localhost:9739/");

            httpListener.Start();

            var context = httpListener.GetContext();
            HandleWebSocket(context);

            Binder = new TouchFreeServiceBinder();

            return Binder;
        }

        public override void OnDestroy()
        {
            Binder = null;
            webSocketHandler = null;
            base.OnDestroy();
        }

        public async void HandleWebSocket(HttpListenerContext context)
        {
            var webSocketContext = await context.AcceptWebSocketAsync("");
            var webSocket = webSocketContext.WebSocket;

            await webSocketHandler.HandleWebSocket(webSocket);
        }
    }
}