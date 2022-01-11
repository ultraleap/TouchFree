using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    [DllImport("user32.dll")]
    static extern void mouse_event(int dwFlags, int dx, int dy,
                          int dwData, int dwExtraInfo);

    //[DllImport("user32.dll")]
    //static extern IntPtr CreateCursor(IntPtr hInst, int xHotSpot, int yHotSpot,
    //                                        int nWidth, int nHeight, byte[] pvANDPlane, byte[] pvXORPlane);

    //[DllImport("user32.dll")]
    //static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    //[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    //static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType,
    //                                    int cxDesired, int cyDesired, uint fuLoad);

    //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    //private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, LoadLibraryFlags dwFlags);

    //[DllImport("user32.dll")]
    //public static extern IntPtr SetCursor(IntPtr? handle);

    //[DllImport("user32.dll")]
    //static extern int ShowCursor(bool bShow);

    //private enum LoadLibraryFlags : uint
    //{
    //    DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
    //    LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
    //    LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
    //    LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
    //    LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
    //    LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
    //}

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


    public static void SendEvent(MouseEventFlags _event, int x, int y)
    {
        x = (int)Ultraleap.TouchFree.Tooling.Utilities.MapRangeToRange(x, 0, Display.main.systemWidth, 0, 65535);
        y = (int)Ultraleap.TouchFree.Tooling.Utilities.MapRangeToRange(y, 0, Display.main.systemHeight, 0, 65535);

        mouse_event((int)(_event | MouseEventFlags.ABSOLUTE), x, y, 0, 0);
    }
}