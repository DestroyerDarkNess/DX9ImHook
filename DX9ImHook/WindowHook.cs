using System;
using Process.NET.Native;
using Process.NET.Windows;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Linq;
using ImGuiNET;

namespace DX9ImHook
{
    public class WindowHook
    {

        private const int GWL_WNDPROC = -4;

        private WindowProc _newCallback;

        private IntPtr _oldCallback;

        protected IntPtr Handle { get; set; }

        public string Identifier { get; }

        public bool IsDisposed { get; protected set; }

        public bool IsEnabled { get; protected set; }

        public bool MustBeDisposed { get; set; } = true;


        public WindowHook(IntPtr handle, string identifier = "")
        {
            if (string.IsNullOrEmpty(identifier))
            {
                identifier = "WindowProcHook - " + handle.ToString("X");
            }

            Identifier = identifier;
            Handle = handle;
        }


        public void Enable()
        {
          
            _newCallback = OnWndProc;
            _oldCallback = Kernel32.SetWindowLongPtr(Handle, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newCallback));
            if (_oldCallback == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            IsEnabled = true;
        }

     

        public void Disable()
        {
            if (_newCallback != null)
            {
                Kernel32.SetWindowLongPtr(Handle, -4, _oldCallback);
                _newCallback = null;
                IsEnabled = false;
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (IsEnabled)
                {
                    Disable();
                }

                GC.SuppressFinalize(this);
            }
        }

        protected virtual IntPtr OnWndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (Dx_Hooks.g_Initialized == true)
            {
                //Console.WriteLine((WM)msg);

                if ((WM)msg == WM.KEYDOWN && (int)wParam == dllmain.KeyMenu)
                {

                    dllmain.ShowImGui_UI = !dllmain.ShowImGui_UI;
                    WinAPI.ShowCursor(dllmain.ShowImGui_UI);

                }

                if (dllmain.ShowImGui_UI == true)
                {

                    //ImGui_ImplWin32_WndProcHandler(hWnd, Msg, wParam, lParam);
                    ImplWin32.WndProcHandler(hWnd, (uint)msg, (long)wParam, (uint)lParam);

                    return IntPtr.Zero;

                }

            }
            return Kernel32.CallWindowProc(_oldCallback, hWnd, msg, wParam, lParam);
        }

        ~WindowHook()
        {
            if (MustBeDisposed)
            {
                Dispose();
            }
        }

        public override string ToString()
        {
            return Identifier;
        }
    }
}

