using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    public static class ClientSettings
    {
        //TODO: set this to the monitor fullscreen resolution on start
        public static int ScreenWidth_px = 1920;
        public static int ScreenHeight_px = 1200;

        static ClientConstantSettings curClientConstants;
        public static ClientConstantSettings clientConstants {
            get
            {
                if (curClientConstants == null)
                {
                    curClientConstants = new ClientConstantSettings();
                }

                return curClientConstants;
            }
        }
    }

    public class ClientConstantSettings
    {
        public string CursorRingColor = "#000000";
        public float CursorRingOpacity = 1;
        public string CursorDotFillColor = "#000000";
        public float CursorDotFillOpacity = 1;
        public string CursorDotBorderColor = "#FFFFFF";
        public float CursorDotBorderOpacity = 0.8f;

        public float CursorDotSizePixels = 50.0f;
        public float CursorRingMaxScale = 2.0f;
        public float CursorMaxRingScaleAtDistanceM = 0.1f;
        public bool UseScrollingOrDragging = false;
        public bool SendHoverEvents = false;

        public float HoverCursorStartTimeS = 0.5f;
        public float HoverCursorCompleteTimeS = 0.6f;
    }
}