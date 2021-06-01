using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    /// <summary>
    /// CameraConnector is used to ensure all canvasses used by ScreenControl Client reference the correct camera
    /// for rendering.
    /// e.g. The sprites used for the ring in the RingCursor prefab will not render if the camera is not directly referenced
    /// on the CursorCanvas
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class CameraConnector : MonoBehaviour
    {
        [Tooltip("(Optional) This will be auto-detected if not referenced.\n\nThe camera that is used to render the canvasses.")]
        public Camera sceneCamera;

        public Canvas[] targetCanvasses;

        private void Awake()
        {
            if (sceneCamera == null)
            {
                var cameras = FindObjectsOfType<Camera>();

                foreach (var cam in cameras)
                {
                    if (cam.orthographic)
                    {
                        sceneCamera = cam;
                        break;
                    }
                }

                if (sceneCamera == null)
                {
                    Debug.LogError("No cameras found. This is required for the canvasses to render properly.");
                    return;
                }
            }

            foreach (var canvas in targetCanvasses)
            {
                canvas.worldCamera = sceneCamera;
            }
        }
    }
}