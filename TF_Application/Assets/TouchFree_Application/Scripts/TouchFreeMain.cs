using UnityEngine;

namespace Ultraleap.TouchFree
{
    [DefaultExecutionOrder(-1)]
    public class TouchFreeMain : MonoBehaviour
    {
        public static int CursorWindowSize = 200;
        public static Vector3 CursorWindowMiddle = new Vector2(100, 100);

        static int minCursorWindowSize = 50;
        static int maxCursorWindowSize = 500;

        void Awake()
        {
            Application.targetFrameRate = 60;
        }

        void OnEnable()
        {
            ConfigManager.Config.OnConfigUpdated += ConfigUpdated;
            ConfigUpdated();
        }

        void OnDisable()
        {
            ConfigManager.Config.OnConfigUpdated -= ConfigUpdated;
        }

        void ConfigUpdated()
        {
            if(ConfigManager.Config.cursorEnabled)
            {
                UpdateCursorWindowSize(ConfigManager.Config.cursorSizeCm);
            }
            else
            {
                // If we do not need to render a cursor, make it very small so reduce GPU load
                UpdateCursorWindowSize(2);
            }
        }

        public static void UpdateCursorWindowSize(float _cursorSize)
        {
            int newWindowSize = (int)Tooling.Utilities.MapRangeToRange(
                _cursorSize, 0.1f, 1f, minCursorWindowSize, maxCursorWindowSize);

            CursorWindowSize = newWindowSize;
            CursorWindowMiddle = Vector2.one * (CursorWindowSize / 2);
        }
    }
}