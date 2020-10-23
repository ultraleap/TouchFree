using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    public static class ConnectionManager
    {
        static CoreConnection currentCoreConnection;
        public static event Action OnConnected;
        public static CoreConnection coreConnection
        {
            get
            {
                if (currentCoreConnection == null)
                {
                    // here we would make a new connection
                    Connect();
                }

                return currentCoreConnection;
            }
        }

        public static void AddConnectionListener(Action onConnectFunc)
        {
            // TODO: CHECK THIS
            // Add it to the listener where all members get pinged once connected
            OnConnected += onConnectFunc;

            if (currentCoreConnection != null)
            {
                onConnectFunc();
            }
        }

        public static void Connect()
        {
#if SCREENCONTROL_CORE
            currentCoreConnection = new DirectCoreConnection();

            // TODO: Invoke OnConnected event when connect successfully completes
#else
            var errorMsg = @"Could not initialise a Direct connection to Screen Control Core as it wasn't available!
If you wish to use ScreenControl in this manner, please import the Screen Control Core module and add
""SCREENCONTROL_CORE"" to the ""Scripting Define Symbols"" in your Player settings.";
            Debug.Log(errorMsg);
#endif
        }


        public static void Disconnect()
        {
            currentCoreConnection = null;
        }
    }
}