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
    }
}