using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using WebSocketSharp;

public class DiagnosticAPI : IDisposable
{
    private static string uri = "ws://127.0.0.1:1024/";

    public enum Status { Closed, Connecting, Connected, Expired }
    private Status status = Status.Expired;
    private WebSocket webSocket = null;

    public delegate void GetMaskingResponseEvent(ImageMaskData _data);
    public static event GetMaskingResponseEvent OnGetMaskingResponse;

    public DiagnosticAPI()
    {
        Debug.Log("DiagnosticAPI constructor... ");
        Connect();
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
            try
            {
                GetImageMaskResponse maskResponse = JsonUtility.FromJson<GetImageMaskResponse>(e.Data);

                if (maskResponse.request.Contains("GetImageMask"))
                {
                    OnGetMaskingResponse?.Invoke(maskResponse.value);
                }
            }
            catch
            {
                Debug.Log("Received a non-masking message from the DiagnosticAPI");
            }
        }
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
        public uint device_id;
        public double lower;
        public double upper;
        public double left;
        public double right;
    }

    public struct GetImageMaskResponse
    {
        public string request;
        public int status;
        public ImageMaskData value;
    }
}