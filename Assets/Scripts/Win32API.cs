using System.Runtime.InteropServices;
using System;
using AOT;

public static class Win32API
{
    public const uint RIDEV_INPUTSINK = 0x00000100;
    public const uint RID_INPUT = 0x10000003;
    public const uint RIM_TYPEMOUSE = 0;

    public struct Margin
    {
        public int cx_left;
        public int cx_right;
        public int cy_top;
        public int cy_bottom;
    }

    public struct POINT
    {
        public int x;
        public int y;
    }

    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public int mouseData;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }


    [DllImport("kernal32.dll")]
    public static extern IntPtr GetCurrentProcess();

    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();
    [DllImport("Dwmapi.dll")]
    public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margin margins);

    [DllImport("user32.dll")]
    public static extern uint SetWindowLongA(IntPtr hwnd, int index, long attr);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);


    [DllImport("user32.dll")]
    public static extern bool UnhookWindowsHookEx(IntPtr hhook);

    [DllImport("user32.dll")]
    public static extern int CallNextHookEx(IntPtr hhook, int nCode, IntPtr wParam, IntPtr lParam);


    [DllImport("user32.dll")]
    public static extern bool SetActiveWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool AllowSetForegroundWindow(int procId);

    [DllImport("user32.dll")]
    public static extern bool SetFocus(IntPtr hWnd);

    [DllImport("Kernel32.dll")]
    public static extern uint GetCurrentThreadId();
}
