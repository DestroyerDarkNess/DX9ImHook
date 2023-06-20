using EasyHook;
using ImGuiNET;
using Process.NET.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DX9ImHook.dllmain;

namespace DX9ImHook
{
    public class UniversalWindowMessageHook
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);


        private delegate IntPtr WindowProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private LocalHook _hook;

        private IntPtr origAddr;
        private IntPtr Handle;

        public UniversalWindowMessageHook(IntPtr handle)
        {
            Handle = handle;
        }

        public void FindAndHook()
        {

            origAddr = LocalHook.GetProcAddress("user32.dll", "DefWindowProcW");

            _hook = LocalHook.Create(
             origAddr,
             new WindowProcDelegate(WindowProcHook),
             this);

            _hook.ThreadACL.SetExclusiveACL(new int[] { 0 });
        }

        public IntPtr WindowProcHook( IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
           
                //WinAPI.PostMessage(Form.Handle, message.Msg, message.WParam, message.LParam); ' Send to Form, External Hook

                if (Dx_Hooks.g_Initialized == true)
                {


                    if ((WM)msg == WM.KEYDOWN && (int)wParam == dllmain.KeyMenu)
                    {

                        dllmain.ShowImGui_UI = !dllmain.ShowImGui_UI;
                        WinAPI.ShowCursor(dllmain.ShowImGui_UI);

                    }

                    if (dllmain.ShowImGui_UI == true)
                    {

                        //ImGui_ImplWin32_WndProcHandler(hWnd, Msg, wParam, lParam);
                        ImplWin32.WndProcHandler(hWnd, (uint)msg, (long)wParam, (uint)lParam);
                        
                    }

                }
            

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

    }
}
