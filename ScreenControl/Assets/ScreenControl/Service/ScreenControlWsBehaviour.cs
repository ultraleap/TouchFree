using System;

using UnityEngine;

using WebSocketSharp;
using WebSocketSharp.Server;

using Ultraleap.ScreenControl.Core.ScreenControlTypes;
using Ultraleap.ScreenControl.Service.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Service
{
    internal class ScreenControlWsBehaviour : WebSocketSharp.Server.WebSocketBehavior
    {
        private const string API_HEADER_NAME = "ScApiVersion";

        public bool isConnected = false;

        public void SendInputAction(InputActionData _data)
        {
            WebsocketInputActionData converted = new WebsocketInputActionData(_data);

            CommunicationWrapper<WebsocketInputActionData> message =
                new CommunicationWrapper<WebsocketInputActionData>(ActionCodes.INPUT_ACTION.ToString(), converted);

            string jsonMessage = JsonUtility.ToJson(message);

            Send(jsonMessage);
        }

        protected override void OnOpen()
        {
            string clientApiVersion = Headers[API_HEADER_NAME];

            if (clientApiVersion != null &&
                GetVersionCompability(clientApiVersion, VersionInfo.ApiVersion) == Compatibility.COMPATIBLE)
            {
                Debug.Log("Websocket Connection opened successfully");
                isConnected = true;
            }
            else
            {
                if (clientApiVersion == null)
                {
                    Debug.LogError("No API version header was provided on connect!");
                }
                else
                {
                    string errorMsg = $"Client API version of {clientApiVersion} was incompatible with the Service's Core API Version of {VersionInfo.ApiVersion}";
                    Debug.LogError(errorMsg);
                    Close(CloseStatusCode.PolicyViolation, errorMsg);
                }
            }
        }

        protected override void OnClose(CloseEventArgs eventArgs)
        {
            Debug.Log("Websocket Connection closed");
            isConnected = false;
        }

        private Compatibility GetVersionCompability(string _clientVersion, Version _coreVersion)
        {
            Version clientVersionParsed = new Version(_clientVersion);

            if (clientVersionParsed.Major < _coreVersion.Major ||
                clientVersionParsed.Minor < _coreVersion.Minor)
            {
                return Compatibility.CLIENT_OUTDATED;
            }
            else if (clientVersionParsed.Major > _coreVersion.Major ||
                     clientVersionParsed.Minor > _coreVersion.Minor)
            {
                return Compatibility.CORE_OUTDATED;
            }

            if (clientVersionParsed.Build > _coreVersion.Build)
            {
                return Compatibility.CORE_OUTDATED;
            }

            return Compatibility.COMPATIBLE;
        }
    }
}