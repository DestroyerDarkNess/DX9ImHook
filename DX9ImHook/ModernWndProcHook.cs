using EasyHook;
using ImGuiNET;
using Process.NET.Native;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DX9ImHook
{
    public class ModernWndProcHook
    {

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate IntPtr WindowProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private LocalHook _windowProcHook;
        private WindowProcDelegate _windowProcDelegate;

        public bool ReturnOrigMessage = false ;

        public ModernWndProcHook(IntPtr hWnd)
        {
            IntPtr windowProcPtr = WinAPI.GetWindowLongPtr(hWnd, (int)WinAPI.GWL.GWL_WNDPROC);

            _windowProcDelegate = Marshal.GetDelegateForFunctionPointer<WindowProcDelegate>(windowProcPtr);
            _windowProcHook = LocalHook.Create(windowProcPtr, new WindowProcDelegate(WindowProcDetour), this);
            _windowProcHook.ThreadACL.SetExclusiveACL(new int[] { 0 });
        }

        public IntPtr WindowProcDetour(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr result = _windowProcDelegate(hWnd, msg, wParam, lParam);
          
            if (Values.g_Initialized == true)
            {

                //Console.SetCursorPosition(0, Console.CursorTop - 1);
                //Utils.ClearCurrentConsoleLine();
                //Console.WriteLine("[CustomWndProcHook] -> " + (WM)msg);


                if ((WM)msg == WM.KEYDOWN && (int)wParam == dllmain.KeyMenu)
                {

                    dllmain.ShowImGui_UI = !dllmain.ShowImGui_UI;
                    WinAPI.ShowCursor(dllmain.ShowImGui_UI);

                }

                if (dllmain.ShowImGui_UI == true)
                {

                    //ImGui_ImplWin32_WndProcHandler(hWnd, Msg, wParam, lParam);
                    ImplWin32.WndProcHandler(hWnd, (uint)msg, (long)wParam, (uint)lParam);
                    if (ReturnOrigMessage == true) { return result; } else { return IntPtr.Zero; }
                }

            }

            return result;
        }

        }
    }
