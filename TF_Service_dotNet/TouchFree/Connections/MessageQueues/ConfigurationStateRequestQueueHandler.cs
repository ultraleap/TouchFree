﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ConfigurationStateRequestQueueHandler : MessageQueueHandler
    {
        private readonly IConfigManager configManager;

        public override ActionCode[] ActionCodes => new[] { ActionCode.REQUEST_CONFIGURATION_STATE };

        public ConfigurationStateRequestQueueHandler(UpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, IConfigManager _configManager) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
        }

        protected override void Handle(IncomingRequest _request)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_request.content);

            // Explicitly check for requestID because it is the only required key
            if (!RequestIdExists(contentObj))
            {
                ResponseToClient response = new ResponseToClient(string.Empty, "Failure", string.Empty, _request.content);
                response.message = "Config state request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sendingthe configuration,
                // the Client will have no way to handle the config state
                clientMgr.SendConfigChangeResponse(response);
                return;
            }

            ConfigState currentConfig = new ConfigState(
                contentObj.GetValue("requestID").ToString(),
                configManager.InteractionConfig.ForApi(),
                configManager.PhysicalConfig.ForApi());


            clientMgr.SendConfigState(currentConfig);
        }
    }
}