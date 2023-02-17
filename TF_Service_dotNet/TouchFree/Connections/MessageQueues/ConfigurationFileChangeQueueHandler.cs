using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class ConfigurationFileChangeQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] HandledActionCodes => new[] { ActionCode.SET_CONFIGURATION_FILE };

        protected override ActionCode failureActionCode => ActionCode.CONFIGURATION_FILE_CHANGE_RESPONSE;
        protected override string whatThisHandlerDoes => "Setting configuration";

        public ConfigurationFileChangeQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr) : base(_updateBehaviour, _clientMgr) { }

        protected override Result<Empty> ValidateContent(JObject jObject, IncomingRequest request) =>
            MessageValidation.ValidateConfigJson(jObject);

        protected override void Handle(ValidatedIncomingRequest request)
        {
            // Try saving config
            // If not work, return error
            // If work, send response from above
            try
            {
                ChangeConfigFile(request.ContentRoot);
                SendSuccessResponse(request, ActionCode.CONFIGURATION_FILE_CHANGE_RESPONSE);
            }
            catch (UnauthorizedAccessException)
            {
                // Return some response indicating access authorisation issues
                SendErrorResponse(request, new Error("Did not have appropriate file access to modify the config file(s)."));
            }
        }

        // TODO: Move somewhere more general?
        private static void ChangeConfigFile(JObject contentJson)
        {
            // Get the current state of the config file(s)
            InteractionConfig intFromFile = InteractionConfigFile.LoadConfig();
            PhysicalConfig physFromFile = PhysicalConfigFile.LoadConfig();

            string physicalChanges = contentJson["physical"].ToString();
            string interactionChanges = contentJson["interaction"].ToString();

            if (physicalChanges != string.Empty)
            {
                JsonConvert.PopulateObject(physicalChanges, physFromFile);
                PhysicalConfigFile.SaveConfig(physFromFile);
            }

            if (interactionChanges != string.Empty)
            {
                JsonConvert.PopulateObject(interactionChanges, intFromFile);
                InteractionConfigFile.SaveConfig(intFromFile);
            }
        }
    }
}
