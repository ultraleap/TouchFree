using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.Service
{
    public class ServiceConfigHandler : MonoBehaviour
    {
        void Start()
        {
            ClientConnectionManager.Instance.LostAllConnections += OnLostAllConnections;
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
    }
}