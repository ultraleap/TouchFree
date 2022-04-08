using System;
using System.Runtime.InteropServices;

public static class InputInjector
{
    // Touch
    public static bool Initialize(uint maxContacts, TouchFeedback touchFeedbackMode)
    {
        return NativeMethods.InitializeTouchInjection(maxContacts, touchFeedbackMode);
    }

    public static bool SendTouchEvent(PointerTouchInfo[] pointerTouchInfos)
    {
        return NativeMethods.InjectTouchInput(pointerTouchInfos.Length, pointerTouchInfos);
    }

    internal static class NativeMethods
    {
        [DllImport("User32.dll")]
        internal static extern bool InitializeTouchInjection(uint maxCount = 256, TouchFeedback feedbackMode = TouchFeedback.DEFAULT);

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern bool InjectTouchInput(int count, [MarshalAs(UnmanagedType.LPArray), In] PointerTouchInfo[] contacts);
    }

    // Mouse
    [DllImport("user32.dll")]
    static extern void mouse_event(int dwFlags, int dx, int dy,
                      int dwData, int dwExtraInfo);

    public static void SendMouseEvent(MouseEventFlags _event, int x, int y, int screenWidth, int screenHeight)
    {
        x = (int)Ultraleap.TouchFree.Library.Utilities.MapRangeToRange(x, 0, screenWidth, 0, 65535);
        y = (int)Ultraleap.TouchFree.Library.Utilities.MapRangeToRange(y, 0, screenHeight, 0, 65535);

        mouse_event((int)(_event | MouseEventFlags.ABSOLUTE), x, y, 0, 0);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct PointerTouchInfo
{
    public PointerInfo PointerInfo;
    public TouchFlags TouchFlags;
    public TouchMask TouchMasks;
    public ContactArea ContactArea;
    public ContactArea ContactAreaRaw;
    public uint Orientation;
    public uint Pressure;
}

[StructLayout(LayoutKind.Sequential)]
public struct PointerInfo
{
    public PointerInputType PointerInputType;
    public uint PointerId;
    public uint FrameId;
    public PointerFlags PointerFlags;
    internal IntPtr SourceDevice;
    internal IntPtr WindowTarget;
    public PointerTouchPoint PtPixelLocation;
    public PointerTouchPoint PtPixelLocationRaw;
    public PointerTouchPoint PtHimetricLocation;
    public PointerTouchPoint PtHimetricLocationRaw;
    public uint Time;
    public uint HistoryCount;
    public uint InputData;
    public uint KeyStates;
    public ulong PerformanceCount;
    public PointerButtonChangeType ButtonChangeType;
}

[StructLayout(LayoutKind.Explicit)]
public struct ContactArea
{
    [FieldOffset(0)]
    public int Left;
    [FieldOffset(4)]
    public int Top;
    [FieldOffset(8)]
    public int Right;
    [FieldOffset(12)]
    public int Bottom;
}

[StructLayout(LayoutKind.Sequential)]
public struct PointerTouchPoint
{
    public int X;
    public int Y;
}

public enum PointerInputType
{
    POINTER = 0x00000001,
    TOUCH = 0x00000002,
    PEN = 0x00000003,
    MOUSE = 0x00000004
};

public enum PointerFlags
{
    NONE = 0x00000000,
    NEW = 0x00000001,
    INRANGE = 0x00000002,
    INCONTACT = 0x00000004,
    FIRSTBUTTON = 0x00000010,
    SECONDBUTTON = 0x00000020,
    THIRDBUTTON = 0x00000040,
    FOURTHBUTTON = 0x00000080,
    FIFTHBUTTON = 0x00000100,
    PRIMARY = 0x00002000,
    CONFIDENCE = 0x000004000,
    CANCELLED = 0x000008000,
    DOWN = 0x00010000,
    UPDATE = 0x00020000,
    UP = 0x00040000,
    WHEEL = 0x00080000,
    HWHEEL = 0x00100000,
    CAPTURECHANGED = 0x00200000,
    HASTRANSFORM = 0x00400000
}

public enum PointerButtonChangeType
{
    NONE,
    FIRSTBUTTON_DOWN,
    FIRSTBUTTON_UP,
    SECONDBUTTON_DOWN,
    SECONDBUTTON_UP,
    THIRDBUTTON_DOWN,
    THIRDBUTTON_UP,
    FOURTHBUTTON_DOWN,
    FOURTHBUTTON_UP,
    FIFTHBUTTON_DOWN,
    FIFTHBUTTON_UP
}

public enum TouchFeedback
{
    DEFAULT = 0x1,
    INDIRECT = 0x2,
    NONE = 0x3
}

public enum TouchFlags
{
    NONE = 0x00000000
}

public enum TouchMask
{
    NONE = 0x00000000,
    CONTACTAREA = 0x00000001,
    ORIENTATION = 0x00000002,
    PRESSURE = 0x00000004
}

[Flags]
public enum MouseEventFlags
{
    LEFTDOWN = 0x00000002,
    LEFTUP = 0x00000004,
    MIDDLEDOWN = 0x00000020,
    MIDDLEUP = 0x00000040,
    MOVE = 0x00000001,
    ABSOLUTE = 0x00008000,
    RIGHTDOWN = 0x00000008,
    RIGHTUP = 0x00000010
}