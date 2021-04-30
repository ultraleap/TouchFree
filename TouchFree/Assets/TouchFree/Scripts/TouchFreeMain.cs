using UnityEngine;

[DefaultExecutionOrder(-1)]
public class TouchFreeMain : MonoBehaviour
{
    public static int CursorWindowSize = 256;

    void Awake()
    {
        Application.targetFrameRate = 60;
    }
}