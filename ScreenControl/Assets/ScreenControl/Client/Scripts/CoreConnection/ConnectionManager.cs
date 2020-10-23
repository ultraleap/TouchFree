using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    public static class ConnectionManager
    {
        static CoreConnection currentCoreConnection;
        public static CoreConnection coreConnection
        {
            get
            {
                if(currentCoreConnection == null)
                {
                    // here we would make a new connection
                    MakeNewConnection();
                }

                return currentCoreConnection;
            }
        }

        public static void MakeNewConnection()
        {
#if SCREENCONTROL_CORE_AVAIL
            currentCoreConnection = new DirectCoreConnection();
            currentCoreConnection.Initialise();
#else
                var errorMsg = @"Could not initialise a Direct connection to Screen Control Core as it wasn't available!
If you wish to use ScreenControl in this manner, please import the Screen Control Core module and add
""SCREENCONTROL_CORE_AVAIL"" to the ""Scripting Define Symbols"" in your Player settings.";
                Debug.Log(errorMsg);
#endif
        }
    }
}