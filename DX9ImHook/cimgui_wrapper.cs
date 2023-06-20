using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DX9ImHook
{
    internal class cimgui_wrapper
    {

        // From: https://www.unknowncheats.me/forum/c-/353706-imgui-net.html
        // Added on Imgui.NET Assembly  <<<<--------------------------------------------

        //[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX9_Init")]
        //public static extern bool Init(IntPtr device);

        //[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX9_Shutdown")]
        //public static extern void Shutdown();

        //[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX9_NewFrame")]
        //public static extern void NewFrame();

        //[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX9_RenderDrawData")]
        //public static extern void RenderDrawData(ImDrawDataPtr drawData);

        //[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX9_CreateDeviceObjects")]
        //public static extern bool CreateDeviceObjects();

        //[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX9_InvalidateDeviceObjects")]
        //public static extern void InvalidateDeviceObjects();

        // From: https://github.com/shalzuth/DxImSharp/blob/main/DxImSharp/ImGuiHook.cs

        [DllImport("user32")] public static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("cimgui")] public static extern bool ImGui_ImplWin32_Init(IntPtr hwnd);
        [DllImport("cimgui")] public static extern void ImGui_ImplWin32_NewFrame();
        [DllImport("cimgui")] public static extern void igNewFrame(); // NewFrame
        [DllImport("cimgui")] public static extern void igEndFrame(); // EndFrame
        [DllImport("cimgui")] public static extern void igShowDemoWindow(); // ShowDemoWindow
        [DllImport("cimgui")] public static extern IntPtr igCreateContext(IntPtr fontAtlas); // CreateContext
        [DllImport("cimgui")] public static extern void igStyleColorsDark(IntPtr style); // StyleColorsDark
        [DllImport("cimgui")] public static extern void igRender(); // Render
        [DllImport("cimgui")] public static extern IntPtr igGetDrawData(); // GetDrawData
        [DllImport("cimgui")] public static extern ImGuiIO igGetIO(); // GetDrawData
    }
}
