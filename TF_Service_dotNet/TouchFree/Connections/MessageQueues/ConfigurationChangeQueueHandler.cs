using Newtonsoft.Json;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues;

public class ConfigurationChangeQueueHandler : MessageQueueHandler
{
    private readonly IConfigManager _configManager;

    public ConfigurationChangeQueueHandler(IUpdateBehaviour updateBehaviour, IClientConnectionManager clientMgr, IConfigManager configManager)
        : base(updateBehaviour, clientMgr)
        => _configManager = configManager;

    public override ActionCode[] HandledActionCodes => new[] { ActionCode.SET_CONFIGURATION_STATE };
    protected override ActionCode FailureActionCode => ActionCode.CONFIGURATION_RESPONSE;
    protected override string WhatThisHandlerDoes => "Setting configuration";

    protected override Result<Empty> ValidateContent(in IncomingRequestWithId request) =>
        MessageValidation.ValidateConfigJson(request.ContentRoot);

    protected override void Handle(in IncomingRequestWithId request)
    {
        ChangeConfig(request.OriginalContent);
        SendSuccessResponse(request, ActionCode.CONFIGURATION_RESPONSE);
    }

    void ChangeConfig(string content)
    {
        ConfigState combinedData = new ConfigState(string.Empty, _configManager.InteractionConfig.ForApi(), _configManager.PhysicalConfig.ForApi());

        JsonConvert.PopulateObject(content, combinedData);

        _configManager.InteractionConfigFromApi = combinedData.interaction;
        _configManager.PhysicalConfigFromApi = combinedData.physical;

        _configManager.PhysicalConfigWasUpdated();
        _configManager.InteractionConfigWasUpdated();
    }
}