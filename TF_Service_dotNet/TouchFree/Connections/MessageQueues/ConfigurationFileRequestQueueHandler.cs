using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues;

public class ConfigurationFileRequestQueueHandler : MessageQueueHandler
{
    public override ActionCode[] HandledActionCodes => new[] { ActionCode.REQUEST_CONFIGURATION_FILE };

    protected override string WhatThisHandlerDoes => "Config state request";

    protected override ActionCode FailureActionCode => ActionCode.CONFIGURATION_RESPONSE;

    public ConfigurationFileRequestQueueHandler(IUpdateBehaviour updateBehaviour, IClientConnectionManager clientMgr)
        : base(updateBehaviour, clientMgr) { }

    protected override void Handle(in IncomingRequestWithId request)
    {
        InteractionConfig interactions = InteractionConfigFile.LoadConfig();
        PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

        ConfigState currentConfig = new ConfigState(
            request.RequestId,
            interactions,
            physical);

        clientMgr.SendResponse(currentConfig, ActionCode.CONFIGURATION_FILE_STATE);
    }
}