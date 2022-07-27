using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connection
{
    public class ConfigurationFileRequestQueueHandler : MessageQueueHandler
    {
        public override ActionCode ActionCode => ActionCode.REQUEST_CONFIGURATION_FILE;

        public ConfigurationFileRequestQueueHandler(UpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr) : base(_updateBehaviour, _clientMgr)
        {
        }

        protected override void Handle(string _content)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if (!RequestIdExists(contentObj))
            {
                ResponseToClient response = new ResponseToClient(string.Empty, "Failure", string.Empty, _content);
                response.message = "Config state request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sending the configuration,
                // the Client will have no way to handle the config state
                clientMgr.SendConfigChangeResponse(response);
                return;
            }

            InteractionConfig interactions = InteractionConfigFile.LoadConfig();
            PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

            ConfigState currentConfig = new ConfigState(
                contentObj.GetValue("requestID").ToString(),
                interactions,
                physical);

            clientMgr.SendConfigFile(currentConfig);
        }
    }
}
