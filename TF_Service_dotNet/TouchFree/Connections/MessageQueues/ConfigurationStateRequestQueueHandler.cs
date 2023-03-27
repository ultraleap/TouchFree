using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues;

public class ConfigurationStateRequestQueueHandler : MessageQueueHandler
{
    private readonly IConfigManager _configManager;

    public override ActionCode[] HandledActionCodes => new[] { ActionCode.REQUEST_CONFIGURATION_STATE };

    protected override string WhatThisHandlerDoes => "Config state request";

    protected override ActionCode FailureActionCode => ActionCode.CONFIGURATION_RESPONSE;

    public ConfigurationStateRequestQueueHandler(IUpdateBehaviour updateBehaviour, IClientConnectionManager clientMgr, IConfigManager configManager)
        : base(updateBehaviour, clientMgr)
        => _configManager = configManager;

    protected override void Handle(in IncomingRequestWithId request)
    {
        ConfigState currentConfig = new ConfigState(
            request.RequestId,
            _configManager.InteractionConfig.ForApi(),
            _configManager.PhysicalConfig.ForApi());


        clientMgr.SendResponse(currentConfig, ActionCode.CONFIGURATION_STATE);
    }
}