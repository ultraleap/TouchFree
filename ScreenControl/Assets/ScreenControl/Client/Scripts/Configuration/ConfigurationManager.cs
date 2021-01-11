using System;
using System.Collections.Generic;
using UnityEngine;

using Ultraleap.ScreenControl.Client.Connection;

namespace Ultraleap.ScreenControl.Client.Configuration
{
    public static class ConfigurationManager
    {
        public static void SetConfigState(PhysicalConfig _physical, Action<WebSocketResponse> _callback = null)
        {
            SetConfigState(null, _physical, _callback);
        }

        public static void SetConfigState(InteractionConfig _interaction, Action<WebSocketResponse> _callback = null)
        {
            SetConfigState(_interaction, null, _callback);
        }

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

        private static string SerializeInteractionConfig(InteractionConfig _interaction)
        {
            string newContent = "";

            if (_interaction.configValues.Count > 0 || _interaction.HoverAndHold.configValues.Count > 0)
            {
                newContent += "\"interaction\":{";

                foreach (KeyValuePair<string, object> value in _interaction.configValues)
                {
                    newContent += JsonUtilities.ConvertToJson(value.Key, value.Value);
                    newContent += ",";
                }

                if (_interaction.HoverAndHold.configValues.Count > 0)
                {
                    newContent += "\"HoverAndHold\":{";

                    foreach (KeyValuePair<string, object> value in _interaction.HoverAndHold.configValues)
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