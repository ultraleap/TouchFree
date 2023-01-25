using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class OpenFolderQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] ActionCodes => new[] { ActionCode.REQUEST_OPEN_FOLDER };

        protected override string noRequestIdFailureMessage => "Opening folder failed. This is due to a missing or invalid requestID";

        protected override ActionCode noRequestIdFailureActionCode => ActionCode.OPEN_FOLDER_RESPONSE;


        public OpenFolderQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr) : base(_updateBehaviour, _clientMgr) { }

        protected override void Handle(IncomingRequest _request, JObject _contentObject, string requestId)
        {
            ResponseToClient response = new ResponseToClient(requestId, "Failure", string.Empty, _request.content);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {

                OpenFolderRequest? openFolderRequest = null;

                try
                {
                    openFolderRequest = JsonConvert.DeserializeObject<OpenFolderRequest>(_request.content);
                }
                catch { }

                if (openFolderRequest != null)
                {

                    string path = "";
                    var baseFolder = ConfigFileUtils.ConfigFileDirectory;
                    switch (openFolderRequest?.Type)
                    {
                        case FolderType.TRACKING_LOGS:
                            path = Path.Combine(baseFolder, "..\\..\\HandTracker\\Logs\\");
                            break;
                        case FolderType.TOUCHFREE_LOGS:
                            path = Path.Combine(baseFolder, "..\\Logs\\");
                            break;
                    }

                    TouchFreeLog.WriteLine("XYZ");

                    if (Directory.Exists(path))
                    {
                    TouchFreeLog.WriteLine("JB-EXISTS: " + path);

                        try
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = path,
                                UseShellExecute = false,
                                Verb = "open",
                            };
                            Process.Start(startInfo);

                            TouchFreeLog.WriteLine("JB-Success");
                            response = new ResponseToClient(requestId, "Success", string.Empty, _request.content);
                        }
                        catch (System.Exception e)
                        {
                            TouchFreeLog.WriteLine("JB-CAUGHT: " + e);

                            response = new ResponseToClient(requestId, "Failure", e.ToString(), _request.content);
                            clientMgr.SendResponse(response, ActionCode.OPEN_FOLDER_RESPONSE);
                            return;
                        }
                    }
                }
            }
            clientMgr.SendResponse(response, ActionCode.OPEN_FOLDER_RESPONSE);
        }

    }
}
