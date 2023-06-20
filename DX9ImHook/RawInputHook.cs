using EasyHook;
using ImGuiNET;
using Process.NET.Native.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DX9ImHook
{
    public class RawInputHook
    {

     
        /// <summary>
        /// Function to retrieve raw input data.
        /// </summary>
        /// <param name="hRawInput">Handle to the raw input.</param>
        /// <param name="uiCommand">Command to issue when retrieving data.</param>
        /// <param name="pData">Raw input data.</param>
        /// <param name="pcbSize">Number of bytes in the array.</param>
        /// <param name="cbSizeHeader">Size of the header.</param>
        /// <returns>0 if successful if pData is null, otherwise number of bytes if pData is not null.</returns>
        [DllImport("user32.dll")]
        public static extern int GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        private LocalHook _rawInputHook;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int GetRawInputDataDelegate(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        private IntPtr Handle;

        public RawInputHook(IntPtr handle) { 
            Handle = handle;
        }

        public void FindAndHook()
        {
            _rawInputHook = LocalHook.Create(   LocalHook.GetProcAddress("user32.dll", "GetRawInputData"),
           new GetRawInputDataDelegate(GetRawInputDataHook),
           this);

            _rawInputHook.ThreadACL.SetExclusiveACL(new[] { 0 });

        }

        private int GetRawInputDataHook(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader)
        {
            System.Windows.Forms.Message message;
            WinAPI.GetMessage(out message, Handle, 0, 0);

            //Console.WriteLine("Translated Message -> " + (WM)message.Msg);

            //WinAPI.PostMessage(Form.Handle, message.Msg, message.WParam, message.LParam); ' Send to Form, External Hook

            if (Dx_Hooks.g_Initialized == true)
            {


                if ((WM)message.Msg == WM.KEYDOWN && (int)message.WParam == dllmain.KeyMenu)
                {

                    dllmain.ShowImGui_UI = !dllmain.ShowImGui_UI;
                    WinAPI.ShowCursor(dllmain.ShowImGui_UI);

                }

                if (dllmain.ShowImGui_UI == true)
                {

                    //ImGui_ImplWin32_WndProcHandler(hWnd, Msg, wParam, lParam);
                    ImplWin32.WndProcHandler(message.HWnd, (uint)message.Msg, (long)message.WParam, (uint)message.LParam);

                }

            }

            return GetRawInputData(hRawInput, uiCommand, pData, ref pcbSize, cbSizeHeader);
        }


    }

    }
