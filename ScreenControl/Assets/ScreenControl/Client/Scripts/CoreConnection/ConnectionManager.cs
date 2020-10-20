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
            currentCoreConnection = new DirectCoreConnection();
            currentCoreConnection.Initialise();
        }
    }
}