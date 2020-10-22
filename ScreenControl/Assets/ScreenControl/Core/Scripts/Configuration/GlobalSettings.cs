namespace Ultraleap.ScreenControl.Core
{
    public static class GlobalSettings
    {
        public static int CursorWindowSize = 256;

        public static int ScreenWidth;
        public static int ScreenHeight;

        public static VirtualScreen virtualScreen;

        public static readonly float ConfigToDisplayMeasurementMultiplier = 100; // Store in M, display in CM
    }
}