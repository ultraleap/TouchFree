using UnityEngine;

public class DiagnosticAPIManager : MonoBehaviour
{
    public static DiagnosticAPI diagnosticAPI;
    public static bool maskingAvailable = false;

    private void Awake()
    {
        if (diagnosticAPI == null)
        {
            diagnosticAPI = new DiagnosticAPI(this);
        }

        DiagnosticAPI.OnMaskingVersionCheck += HandleMaskingVersionCheck;

        diagnosticAPI.GetDevices(); // get the connected device ID
        diagnosticAPI.GetVersion();
    }

    void HandleMaskingVersionCheck(bool _maskingAvailable)
    {
        maskingAvailable = _maskingAvailable;
    }
}