using System.IO;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.ServiceShared;
using Ultraleap.TouchFree.Tooling.Connection;
using UnityEngine;

[DisallowMultipleComponent, DefaultExecutionOrder(-10)]
public class ReadServiceConfig : MonoBehaviour
{
    private void Awake()
    {
        // Read IP and port from config before we attempt to connect
        string configPath = ConfigFileUtils.ConfigFileDirectory + "ServiceConfig.json";
        Debug.Log($"Checking for service config in {configPath}");
        if (File.Exists(configPath))
        {
            JObject obj = JObject.Parse(File.ReadAllText(configPath));
            if (obj.TryGetValue("ServiceIP", out var ipToken)) ConnectionManager.Ip = ipToken.ToString();
            if (obj.TryGetValue("ServicePort", out var portToken)) ConnectionManager.Port = portToken.ToString();
            Debug.Log("Loaded service config");
        }
    }
}