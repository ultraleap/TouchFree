using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class DiagnosticAPI : IDisposable
{
    private static string uri = "ws://127.0.0.1:1024/";

    public enum Status { Closed, Connecting, Connected, Expired }
    private Status status = Status.Expired;
    private WebSocket webSocket = null;

    public static event Action<ImageMaskData> OnGetMaskingResponse;
    public static event Action<bool> OnMaskingVersionCheck;

    public uint connectedDeviceID;
    public bool maskingAllowed = false;

    const string minimumMaskingAPIVerison = "2.0.0";

    ConcurrentQueue<string> newMessages = new ConcurrentQueue<string>();

    public DiagnosticAPI(MonoBehaviour _creatorMonobehaviour)
    {
        Debug.Log("DiagnosticAPI constructor... ");
        Connect();

        _creatorMonobehaviour.StartCoroutine(MessageQueueReader());
    }

    IEnumerator MessageQueueReader()
    {
        while(true)
        {
            if (newMessages.TryDequeue(out var message))
            {
                HandleMessage(message);
            }

            yield return null;
        }
    }

    private void Connect()
    {
        if (status == Status.Connecting || status == Status.Connected)
        {
            return;
        }

        bool requireSetup = status == Status.Expired;
        status = Status.Connecting;

        if (requireSetup)
        {
            Debug.Log("DiagnosticAPI setup.");

            if (webSocket != null)
            {
                Debug.Log("DiagnosticAPI close previous.");
                webSocket.Close();
            }

            webSocket = new WebSocket(uri);
            webSocket.OnMessage += onMessage;
            webSocket.OnOpen += (sender, e) => {
                Debug.Log("DiagnosticAPI open... ");
                status = Status.Connected;
            };
            webSocket.OnError += (sender, e) => {
                Debug.Log("DiagnosticAPI error! " + e.Message + "\n" + e.Exception.ToString() );
                status = Status.Expired;
            };
            webSocket.OnClose += (sender, e) => {
                Debug.Log("DiagnosticAPI closed. " + e.Reason);
                status = Status.Closed;
            };
        }

        Debug.Log("DiagnosticAPI connecting... ");
        try
        {
            webSocket.Connect();
        }
        catch (System.Exception ex)
        { 
            Debug.Log("DiagnosticAPI connection exception... " + "\n" + ex.ToString() );
            status = Status.Expired;
        }
    }

    private void onMessage (object sender, MessageEventArgs e)
    {
        if (e.IsText)
        {
            newMessages.Enqueue(e.Data);
        }
    }

    void HandleMessage(string _message)
    {
        if (_message.Contains("GetImageMask"))
        {
            try
            {
                GetImageMaskResponse maskResponse = JsonUtility.FromJson<GetImageMaskResponse>(_message);
                OnGetMaskingResponse?.Invoke(maskResponse.value);
            }
            catch
            {
                Debug.Log("DiagnosticAPI - Could not parse GetImageMask data: " + _message);
            }
        }
        else if(_message.Contains("GetDevices"))
        {
            try
            {
                GetDevicesResponse devicesResponse = JsonUtility.FromJson<GetDevicesResponse>(_message);
                connectedDeviceID = devicesResponse.value[0].id;
                Request("GetImageMask:" + connectedDeviceID);
            }
            catch
            {
                Debug.Log("DiagnosticAPI - Could not parse GetDevices data: " + _message);
            }
        }
        else if(_message.Contains("Version"))
        {
            try
            {
                string version = _message.Split('=')[1]; // Split the string (version=X.X.X) and get the 2nd half (X.X.X)
                // GetVersionResponse versionResponse = JsonUtility.FromJson<GetVersionResponse>(_message); TODO: Change to this for next API version?
                HandleDiagnosticAPIVersion(version);
            }
            catch
            {
                Debug.Log("DiagnosticAPI - Could not parse Version data: " + _message);
            }
        }
    }

    public void HandleDiagnosticAPIVersion(string _version)
    {
        Version curVersion = new Version(_version);
        Version minVersion = new Version(minimumMaskingAPIVerison);

        if (curVersion.CompareTo(minVersion) >= 0)
        {
            // Version allows masking
            maskingAllowed = true;
        }
        else
        {
            // Version does not allow masking
            maskingAllowed = false;
        }

        OnMaskingVersionCheck?.Invoke(maskingAllowed);
    }

    public void Request(string key)
    {
        if (status == Status.Connected)
        {
            webSocket.Send(key);
        }
        else
        {
            Connect();
        }
    }

    void IDisposable.Dispose ()
    {
        status = Status.Expired;
        webSocket.Close();
    }

    [Serializable]
    public struct ImageMaskData
    {
        public double lower;
        public double upper;
        public double right;
        public double left;
        public uint device_id;
    }

    public struct GetImageMaskResponse
    {
        public string request;
        public int status;
        public ImageMaskData value;
    }

    public struct GetDevicesResponse
    {
        public int status;
        public DiagnosticDevice[] value;
        public string request;
    }

    [Serializable]
    public struct DiagnosticDevice
    {
        public uint id;
        public string type;
        public uint clients;
        public bool streaming;
    }

    public struct GetVersionResponse
    {
        public string request;
        public int status;
        public VersionContainer value;
    }

    [Serializable]
    public struct VersionContainer
    {
        public string version;
    }
}