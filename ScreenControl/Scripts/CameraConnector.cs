using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[DefaultExecutionOrder(-100)]
public class CameraConnector : MonoBehaviour
{
    [Tooltip("(Optional) This will be auto-detected if not referenced.\n\nThe camera that is used to render the canvasses. This camera should be oethographic.")]
    public Camera orthographicCamera;

    public Canvas[] targetCanvasses;
    public VideoPlayer targetVideoPlayer;

    private void Awake()
    {
        if(orthographicCamera == null)
        {
            var cameras = FindObjectsOfType<Camera>();

            foreach(var cam in cameras)
            {
                if(cam.orthographic)
                {
                    orthographicCamera = cam;
                    break;
                }
            }

            if (orthographicCamera == null)
            {
                Debug.LogError("No orthographic cameras found. This is required for the canvasses to render properly.");
                return;
            }
        }

        foreach(var canvas in targetCanvasses)
        {
            canvas.worldCamera = orthographicCamera;
        }

        targetVideoPlayer.targetCamera = orthographicCamera;
    }
}