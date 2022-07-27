using Newtonsoft.Json;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ConfigurationChangeQueueHandler : BaseConfigurationChangeQueueHandler
    {
        private readonly IConfigManager configManager;

        public ConfigurationChangeQueueHandler(UpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IConfigManager _configManager) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
        }

        public override ActionCode[] ActionCodes => new[] { ActionCode.SET_CONFIGURATION_STATE };

        protected override void Handle(IncomingRequest _request)
        {
            ResponseToClient response = ValidateConfigChange(_request.content);

            if (response.status == "Success")
            {
                ChangeConfig(_request.content);
            }

            clientMgr.SendConfigChangeResponse(response);
        }

        void ChangeConfig(string _content)
        {
            ConfigState combinedData = new ConfigState(string.Empty, configManager.InteractionConfig.ForApi(), configManager.PhysicalConfig.ForApi());

            JsonConvert.PopulateObject(_content, combinedData);

            configManager.InteractionConfigFromApi = combinedData.interaction;
            configManager.PhysicalConfigFromApi = combinedData.physical;

            configManager.PhysicalConfigWasUpdated();
            configManager.InteractionConfigWasUpdated();
        }
    }
}
