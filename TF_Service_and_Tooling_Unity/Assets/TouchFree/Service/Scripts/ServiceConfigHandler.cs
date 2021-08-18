using UnityEngine;
using System.IO;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
{
    public class ServiceConfigHandler : MonoBehaviour
    {
        const string TOUCHFREE_APP_CONFIG_NAME = "TouchFreeConfig.json";

        void Start()
        {
            ClientConnectionManager.Instance.LostAllConnections += OnLostAllConnections;

            HandleTFAppConfigSetup();
        }

        private void OnDestroy()
        {
            ClientConnectionManager.Instance.LostAllConnections -= OnLostAllConnections;
        }

        void OnLostAllConnections()
        {
            ConfigManager.LoadConfigsFromFiles();
            ConfigManager.InteractionConfig.ConfigWasUpdated();
            ConfigManager.PhysicalConfig.ConfigWasUpdated();
        }

        void HandleTFAppConfigSetup()
        {
            if(!Directory.Exists(ConfigFileUtils.ConfigFileDirectory))
            {
                Directory.CreateDirectory(ConfigFileUtils.ConfigFileDirectory);
            }

            string filePath = Path.Combine(ConfigFileUtils.ConfigFileDirectory, TOUCHFREE_APP_CONFIG_NAME);

            if(!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "{}");
            }
        }
    }
}