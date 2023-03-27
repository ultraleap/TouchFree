using Newtonsoft.Json;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Configuration.QuickSetup;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues;

public class QuickSetupQueueHandler : MessageQueueHandler
{
    public override ActionCode[] HandledActionCodes => new[] { ActionCode.QUICK_SETUP };

    protected override string WhatThisHandlerDoes => "Config state request";

    protected override ActionCode FailureActionCode => ActionCode.CONFIGURATION_RESPONSE;

    private readonly IQuickSetupHandler _quickSetupHandler;

    public QuickSetupQueueHandler(IUpdateBehaviour updateBehaviour, IClientConnectionManager clientMgr, IQuickSetupHandler quickSetupHandler)
        : base(updateBehaviour, clientMgr)
        => _quickSetupHandler = quickSetupHandler;

    protected override void Handle(in IncomingRequestWithId request)
    {
        var quickSetupRequest = JsonConvert.DeserializeObject<QuickSetupRequest>(request.OriginalContent);

        var quickSetupResponse = _quickSetupHandler.HandlePositionRecording(quickSetupRequest.Position);

        if (quickSetupResponse.ConfigurationUpdated)
        {
            InteractionConfig interactions = InteractionConfigFile.LoadConfig();
            PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

            ConfigState currentConfig = new ConfigState(
                request.RequestId,
                interactions,
                physical);

            clientMgr.SendResponse(currentConfig, ActionCode.QUICK_SETUP_CONFIG);
        }
        else if (quickSetupResponse.PositionRecorded)
        {
            SendSuccessResponse(request, ActionCode.QUICK_SETUP_RESPONSE);
        }
        else
        {
            SendErrorResponse(request, quickSetupResponse.QuickSetupError, ActionCode.QUICK_SETUP_RESPONSE);
        }
    }
}