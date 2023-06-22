using EasyHook;
using Process.NET.Native.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DX9ImHook.dllmain;

namespace DX9ImHook
{
   

    public enum MouseCursorHookType
    {
        SetCursorPos = 0,
        GetCursorPos = 1,
        Both = 2,
        None = 3
    }

    public class MouseCursorHook
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate bool SetCursorPosDelegate(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT point);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate bool GetCursorPosDelegate(POINT point);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 X;
            public Int32 Y;
        }

        private MouseCursorHookType HookType;
        private IntPtr Handle;

        private LocalHook SetCursorPos_Hook;
        private LocalHook GetCursorPos_Hook;

        public MouseCursorHook(IntPtr handle, MouseCursorHookType HookTypeEx) {
            Handle = handle;
            HookType = HookTypeEx;
        }

        public void FindAndHook()
        {

            switch (HookType)
            {
                case MouseCursorHookType.SetCursorPos:
                    // SetCursorPos Hook

                    SetCursorPos_Hook = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "SetCursorPos"),
                      new SetCursorPosDelegate(SetCursorPosHook),  this);

                    SetCursorPos_Hook.ThreadACL.SetExclusiveACL(new[] { 0 });

                    break;
                case MouseCursorHookType.GetCursorPos:
                    // GetCursorPos Hook

                    GetCursorPos_Hook = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "GetCursorPos"),
                     new GetCursorPosDelegate(GetCursorPosHook), this);

                    GetCursorPos_Hook.ThreadACL.SetExclusiveACL(new[] { 0 });

                    break;
                case MouseCursorHookType.Both:
                    // SetCursorPos And GetCursorPos Hook

                    SetCursorPos_Hook = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "SetCursorPos"),
                     new SetCursorPosDelegate(SetCursorPosHook), this);

                    SetCursorPos_Hook.ThreadACL.SetExclusiveACL(new[] { 0 });

                    GetCursorPos_Hook = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "GetCursorPos"),
                   new GetCursorPosDelegate(GetCursorPosHook), this);

                    GetCursorPos_Hook.ThreadACL.SetExclusiveACL(new[] { 0 });

                    break;
                default:
                    break;
            }

        }

        private bool SetCursorPosHook(int x, int y)
        {
            if (Values.g_Initialized == true && dllmain.ShowImGui_UI == true)
            {
                return false;
            }

            return SetCursorPos(x, y);
        }

        private bool GetCursorPosHook(POINT point)
        {
            if (Values.g_Initialized == true   &&  dllmain.ShowImGui_UI == true)
            {
                return false;
            }

            return GetCursorPos(out point);
        }


    }
}
