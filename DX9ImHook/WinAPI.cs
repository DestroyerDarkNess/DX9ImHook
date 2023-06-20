using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DX9ImHook
{
    public static class WinAPI
    {
        [DllImport("kernel32.dll", SetLastError = true)]   [return: MarshalAs(UnmanagedType.Bool)] public static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll")][return: MarshalAs(UnmanagedType.Bool)] public static extern bool AllocConsole();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]  public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

        [DllImport("kernel32")] public static extern IntPtr LoadLibrary(String lpFileName);

        [DllImport("user32.dll")] public static extern int ShowCursor(bool bShow);

        [DllImport("user32.dll")] public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        [DllImport("user32.dll")]  public static extern int CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, int wParam, int lParam);

       
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")] public static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")] public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr newValue);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr newValue);
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newValue)
        {
            if (IntPtr.Size != 4)
            {
                return SetWindowLongPtr64(hWnd, nIndex, newValue);
            }

            return SetWindowLong32(hWnd, nIndex, newValue);
        }

        public  enum GWL
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }
        public static IntPtr GetID(IntPtr winhandle)
        {
            return GetWindowLongPtr(winhandle, (int)GWL.GWL_ID);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")] public  static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [Flags]
        public enum GuiThreadInfoFlags
        {
            GUI_CARETBLINKING = 0x00000001,
            GUI_INMENUMODE = 0x00000004,
            GUI_INMOVESIZE = 0x00000002,
            GUI_POPUPMENUMODE = 0x00000010,
            GUI_SYSTEMMENUMODE = 0x00000008
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public int cbSize;
            public GuiThreadInfoFlags flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public System.Drawing.Rectangle rcCaret;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }

            public override string ToString()
            {
                return $"X: {X}, Y: {Y}";
            }
        }
        [DllImport("user32.dll")] public   static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
        public static IntPtr getThreadWindowHandle(uint dwThreadId)
        {
            IntPtr hWnd = IntPtr.Zero;

            // Get Window Handle and title from Thread
            GUITHREADINFO guiThreadInfo = new GUITHREADINFO();
            guiThreadInfo.cbSize = Marshal.SizeOf(guiThreadInfo);

           GetGUIThreadInfo(dwThreadId, ref guiThreadInfo);

            hWnd = guiThreadInfo.hwndActive;
            return hWnd;
        }

        [DllImport("user32.dll")]  [return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetCursorPos(out POINT point);
        [DllImport("user32.dll")] public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);
        [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]  [return: MarshalAs(UnmanagedType.Bool)] public static extern bool SetCursorPos(int x, int y);

    }
}
