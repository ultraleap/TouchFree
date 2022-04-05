using System;
using System.Collections.Generic;

using Ultraleap.TouchFree.Tooling.Connection;

namespace Ultraleap.TouchFree.Tooling.Configuration
{
    // Class: ConfigurationManager
    // This static class provides async methods for changing the configuration of the TouchFree
    // service. Makes use of the static <ConnectionManager> for communication with the Service.
    public static class ConfigurationManager
    {
        // Function: RequestConfigState
        // Used to request a <ConfigState> representing the config currently in use by the Service
        // via the <webSocket>.
        // Provides a <ConfigState> asynchronously via the _callback parameter.
        public static void RequestConfigState(Action<ConfigState> _callback)
        {
            ConnectionManager.serviceConnection.RequestConfigState(_callback);
        }

        // Function: RequestConfigState
        // Used to request a <ConfigState> representing the current state of the Service's config
        // files from the Service via the <webSocket>.
        // Provides a <ConfigState> asynchronously via the _callback parameter.
        public static void RequestConfigFileState(Action<ConfigState> _callback)
        {
            ConnectionManager.serviceConnection.RequestConfigFile(_callback);
        }

        #region Request Config Change

        // Function: RequestConfigChange
        // Takes in an <InteractionConfig> and a <PhysicalConfig>, transforms them both into the
        // appropriate form to go over the websocket, before sending it through the <ConnectionManager>
        //
        // WARNING!
        // If a user changes ANY values via the TouchFree Service Settings UI,
        // values set from a client via this function will be discarded.
        public static void RequestConfigChange(InteractionConfig _interaction, PhysicalConfig _physical, Action<WebSocketResponse> _callback = null)
        {
            string action = ActionCode.SET_CONFIGURATION_STATE.ToString();
            Guid requestGUID = Guid.NewGuid();
            string requestID = requestGUID.ToString();

            string jsonContent = "";
            jsonContent += "{\"action\":\"";
            jsonContent += action + "\",\"content\":{\"requestID\":\"";
            jsonContent += requestID + "\"";

            if (_interaction != null)
            {
                jsonContent += SerializeInteractionConfig(_interaction);
            }

            if (_physical != null)
            {
                jsonContent += SerializePhysicalConfig(_physical);
            }

            jsonContent += "}}";

            ConnectionManager.serviceConnection.SendMessage(jsonContent, requestID, _callback);
        }

        // Function: RequestConfigFileChange
        // Requests a modification to the configuration **files** used by the Service. Takes in an
        // <InteractionConfig> and/or a <PhysicalConfig> representing the desired changes & sends
        // them through the <ConnectionManager>
        //
        // Provide a _callback if you require confirmation that your settings were used correctly.
        //
        // WARNING!
        // Any changes that have been made using <RequestConfigChange> by *any* connected client will be
        // lost when changing these files. The change will be applied **to the current config files directly,**
        // disregarding current active confiag state, and the config will be loaded from files.
        public static void RequestConfigFileChange(InteractionConfig _interaction, PhysicalConfig _physical, Action<WebSocketResponse> _callback = null)
        {
            string action = ActionCode.SET_CONFIGURATION_FILE.ToString();
            Guid requestGUID = Guid.NewGuid();
            string requestID = requestGUID.ToString();

            string jsonContent = "";
            jsonContent += "{\"action\":\"";
            jsonContent += action + "\",\"content\":{\"requestID\":\"";
            jsonContent += requestID + "\"";

            if (_interaction != null)
            {
                jsonContent += SerializeInteractionConfig(_interaction);
            }

            if (_physical != null)
            {
                jsonContent += SerializePhysicalConfig(_physical);
            }

            jsonContent += "}}";

            ConnectionManager.serviceConnection.SendMessage(jsonContent, requestID, _callback);
        }

        // Group: Private Serialization Functions
        // These functions are used to serialize the configuration objects into a format suitable
        // for websocket transmission.

        private static string SerializeInteractionConfig(InteractionConfig _interaction)
        {
            string newContent = "";

            if (_interaction.configValues.Count > 0 || _interaction.HoverAndHold.configValues.Count > 0)
            {
                newContent += ",";
                newContent += "\"interaction\":{";

                foreach (KeyValuePair<string, object> value in _interaction.configValues)
                {
                    newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                    newContent += ",";
                }

                // last element added was last in the list so remove the comma
                newContent = newContent.Remove(newContent.Length - 1);

                newContent += SerializeInteractionSpecificConfigs(_interaction);

                newContent += "}";
            }

            return newContent;
        }

        static string SerializeInteractionSpecificConfigs(InteractionConfig _interaction)
        {
            string newContent = "";

            if (_interaction.HoverAndHold.configValues.Count > 0)
            {
                newContent += ",";
                newContent += "\"HoverAndHold\":{";

                foreach (KeyValuePair<string, object> value in _interaction.HoverAndHold.configValues)
                {
                    newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                    newContent += ",";
                }

                // last element added was last in the list so remove the comma
                newContent = newContent.Remove(newContent.Length - 1);
                newContent += "}";
            }

            if (_interaction.TouchPlane.configValues.Count > 0)
            {
                newContent += ",";
                newContent += "\"TouchPlane\":{";

                foreach (KeyValuePair<string, object> value in _interaction.TouchPlane.configValues)
                {
                    newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                    newContent += ",";
                }

                // last element added was last in the list so remove the comma
                newContent = newContent.Remove(newContent.Length - 1);
                newContent += "}";
            }

            return newContent;
        }

        private static string SerializePhysicalConfig(PhysicalConfig _physical)
        {
            string newContent = "";

            if (_physical.configValues.Count > 0)
            {
                if (_physical.configValues.Count > 0)
                {
                    newContent += ",";
                    newContent += "\"physical\":{";

                    foreach (KeyValuePair<string, object> value in _physical.configValues)
                    {
                        newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                        newContent += ",";
                    }

                    // last element added was last in the list so remove the comma
                    newContent = newContent.Remove(newContent.Length - 1);
                    newContent += "}";
                }
            }

            return newContent;
        }

        #endregion
    }
}