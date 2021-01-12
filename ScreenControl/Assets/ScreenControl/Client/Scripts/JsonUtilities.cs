using System;

using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    // Class: JsonUtilities
    // This class is used to manually convert objects to Json strings. It is used 
    // for generating custom configuration files to be sent to the ScreenControl Service.
    public static class JsonUtilities
    {
        // Group: Functions

        // Function: ConvertToJson
        // Takes a variable name (<_name>) and an object (<_obj>), determines the type of the
        // object and returns a json string to represent the object.
        public static string ConvertToJson(string _name, object _obj)
        {
            string json = "\"" + _name + "\":";

            switch (Type.GetTypeCode(_obj.GetType()))
            {
                case TypeCode.Boolean:
                    json += _obj.ToString().ToLower();
                    break;

                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    json += _obj;
                    break;

                case TypeCode.String:
                    json += "\"" + _obj + "\"";
                    break;

                case TypeCode.Object:
                    json += ConvertObjectToJson(_obj);
                    break;

                default:
                    Debug.LogError("Tried to parse an unknown type");
                    break;
            }

            return json;
        }

        // Function: ConvertObjectToJson
        // Takes an object (<_obj>), determines the type of the
        // object and returns a json string to represent the object.
        public static string ConvertObjectToJson(object _obj)
        {
            string json = "";

            switch (_obj.GetType().ToString())
            {
                case "UnityEngine.Vector3":
                    Vector3 vec = (Vector3)_obj;
                    json += "{\"x\":" + vec.x + ",\"y\":" + vec.y + ",\"z\":" + vec.z + "}";
                    break;

                default:
                    Debug.LogError("Tried to parse an unknown type");
                    break;
            }

            return json;
        }
    }
}