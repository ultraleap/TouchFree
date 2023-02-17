using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ConfigurationChangeQueueHandler : MessageQueueHandler
    {
        private readonly IConfigManager configManager;

        public ConfigurationChangeQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IConfigManager _configManager) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
        }

        public override ActionCode[] HandledActionCodes => new[] { ActionCode.SET_CONFIGURATION_STATE };
        protected override ActionCode failureActionCode => ActionCode.CONFIGURATION_RESPONSE;
        protected override string whatThisHandlerDoes => "Setting configuration";

        protected override Result<Empty> ValidateContent(JObject jObject, IncomingRequest request) =>
            MessageValidation.ValidateConfigJson(jObject);

        protected override void Handle(ValidatedIncomingRequest request)
        {
            ChangeConfig(request.OriginalContent);
            SendSuccessResponse(request, ActionCode.CONFIGURATION_RESPONSE);
        }

        void ChangeConfig(string content)
        {
            ConfigState combinedData = new ConfigState(string.Empty, configManager.InteractionConfig.ForApi(), configManager.PhysicalConfig.ForApi());

            JsonConvert.PopulateObject(content, combinedData);

            configManager.InteractionConfigFromApi = combinedData.interaction;
            configManager.PhysicalConfigFromApi = combinedData.physical;

            configManager.PhysicalConfigWasUpdated();
            configManager.InteractionConfigWasUpdated();
        }
    }
}
