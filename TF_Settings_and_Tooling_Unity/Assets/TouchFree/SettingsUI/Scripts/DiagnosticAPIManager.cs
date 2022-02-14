using UnityEngine;

public class DiagnosticAPIManager : MonoBehaviour
{
    public static DiagnosticAPI diagnosticAPI;
    public static bool maskngAvailable = false;

    private void Awake()
    {
        if (diagnosticAPI == null)
        {
            diagnosticAPI = new DiagnosticAPI(this);
        }

        DiagnosticAPI.OnMaskingVersionCheck += HandleMaskingVersionCheck;

        diagnosticAPI.Request("GetDevices"); // get the connected device ID
        diagnosticAPI.Request("GetVersion");
    }

    void HandleMaskingVersionCheck(bool _maskingAvailable)
    {
        maskngAvailable = _maskingAvailable;
    }
}