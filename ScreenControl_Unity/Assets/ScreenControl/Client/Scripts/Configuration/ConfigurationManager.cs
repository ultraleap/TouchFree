using System;
using System.Collections.Generic;
using UnityEngine;

using Ultraleap.ScreenControl.Client.Connection;

namespace Ultraleap.ScreenControl.Client.Configuration
{
    // Class: ConfigurationManager
    // This static class provides async methods for changing the configuration of the ScreenControl
    // service. Makes use of the static <ConnectionManager> for communication with the Service.
    public static class ConfigurationManager
    {
        // Function: SetConfigState
        // Takes in an <InteractionConfig> and a <PhysicalConfig>, transforms them both into the
        // appropriate form to go over the websocket, before sending it through the <ConnectionManager>
        public static void SetConfigState(InteractionConfig _interaction, PhysicalConfig _physical, Action<WebSocketResponse> _callback = null)
        {
            string action = ActionCodes.SET_CONFIGURATION_STATE.ToString();
            Guid requestGUID = Guid.NewGuid();
            string requestID = requestGUID.ToString();

            string jsonContent = "";
            jsonContent += "{\"action\":\"";
            jsonContent += action + "\",\"content\":{\"requestID\":\"";
            jsonContent += requestID + "\",";

            if(_interaction != null)
            {
                jsonContent += SerializeInteractionConfig(_interaction);
            }

            if (_physical != null)
            {
                jsonContent += SerializePhysicalConfig(_physical);
            }

            // last element added was final so remove the comma
            jsonContent = jsonContent.Remove(jsonContent.Length - 1);
            jsonContent += "}}";

            ConnectionManager.serviceConnection.SendMessage(jsonContent, requestID, _callback);
        }

        // Function: SetConfigState
        // A variant of the above function used to pass only a <PhysicalConfig> to the Service.
        public static void SetConfigState(PhysicalConfig _physical, Action<WebSocketResponse> _callback = null)
        {
            SetConfigState(null, _physical, _callback);
        }

        // Function: SetConfigState
        // A variant of the above function used to pass only an <InteractionConfig> to the Service.
        public static void SetConfigState(InteractionConfig _interaction, Action<WebSocketResponse> _callback = null)
        {
            SetConfigState(_interaction, null, _callback);
        }

        // Group: Private Serialization Functions
        // These functions are used to serialize the configuration objects into a format suitable
        // for websocket transmission.

        private static string SerializeInteractionConfig(InteractionConfig _interaction)
        {
            string newContent = "";

            if (_interaction.configValues.Count > 0 || _interaction.hoverAndHold.configValues.Count > 0)
            {
                newContent += "\"interaction\":{";

                foreach (KeyValuePair<string, object> value in _interaction.configValues)
                {
                    newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                    newContent += ",";
                }

                if (_interaction.hoverAndHold.configValues.Count > 0)
                {
                    newContent += "\"HoverAndHold\":{";

                    foreach (KeyValuePair<string, object> value in _interaction.hoverAndHold.configValues)
                    {
                        newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                        newContent += ",";
                    }

                    // last element added was last in the list so remove the comma
                    newContent = newContent.Remove(newContent.Length - 1);
                    newContent += "},";
                }

                // last element added was last in the list so remove the comma
                newContent = newContent.Remove(newContent.Length - 1);
                newContent += "},";
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
                    newContent += ",\"physical\":{";

                    foreach (KeyValuePair<string, object> value in _physical.configValues)
                    {
                        newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                        newContent += ",";
                    }

                    // last element added was last in the list so remove the comma
                    newContent = newContent.Remove(newContent.Length - 1);
                    newContent += "},";
                }
            }

            return newContent;
        }
    }
}