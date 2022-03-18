using UnityEngine;

public class DiagnosticAPIManager : MonoBehaviour
{
    public static DiagnosticAPI diagnosticAPI;

    private void Awake()
    {
        if (diagnosticAPI == null)
        {
            diagnosticAPI = new DiagnosticAPI(this);
        }

        diagnosticAPI.GetDevices(); // get the connected device ID
        diagnosticAPI.GetVersion();
    }
}