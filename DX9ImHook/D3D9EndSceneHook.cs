using DX9ImHook;
using EasyHook;
using ImGuiNET;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Multimedia;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DX9ImHook.WinAPI;

namespace DX9ImHook
{
    public class D3D9EndSceneHook
    {
        [UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int EndSceneDelegate(IntPtr device);

        public List<IntPtr> devicefunctionaddresses = new List<IntPtr>();
        public  LocalHook EndSceneHooker;
        public Device deviceGlobal;

        public void FindAndHook()
        {
           
        devicefunctionaddresses = new List<IntPtr>();

            using (Direct3D d3d = new Direct3D()) {
                using (var renderform = new System.Windows.Forms.Form()) {
                    using (deviceGlobal = new Device(d3d, 0, DeviceType.NullReference, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, new PresentParameters() { BackBufferWidth = 1, BackBufferHeight = 1, DeviceWindowHandle = renderform.Handle })) {
                        devicefunctionaddresses.AddRange(GetVTblAddresses(deviceGlobal.NativePointer, 119));
                       
                    }
}
               
            }

            EndSceneHooker = LocalHook.Create(devicefunctionaddresses[(int)d3d9_indexes.Direct3DDevice9FunctionOrdinals.EndScene],
                new EndSceneDelegate(EndSceneHook), this);

            EndSceneHooker.ThreadACL.SetExclusiveACL(new Int32[1]);
        }


        static IntPtr[] GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            List<IntPtr> vtblAddresses = new List<IntPtr>();
            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = 0; i < numberOfMethods; i++)
                vtblAddresses.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));
            return vtblAddresses.ToArray();
        }

        // End Scene

        public  int EndSceneHook(IntPtr device)
        {
            
            Device dev = (Device)device;
           
            //Text(dev, "Hello Wolrd!!!");

            try
            {
                if (Values.g_Initialized == false)
                {
                    var context = cimgui_wrapper.igCreateContext(IntPtr.Zero);
                   
                    ImplWin32.Init(dllmain.GameHandle); //  cimgui_wrapper.ImGui_ImplWin32_Init(dllmain.GameHandle);
                    ImplDX9.Init(device);

                    Console.WriteLine("EndScene Hooked!");

                    Values.g_Initialized = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Imgui Init Failed!!");
                Console.WriteLine(e.Message);
            }

           
            try
            {
               
                if (Values.g_Initialized == true && dllmain.ShowImGui_UI == true)     {

                    ImplDX9.NewFrame();
                    ImplWin32.NewFrame();  //cimgui_wrapper.ImGui_ImplWin32_NewFrame();
                    ImGui.NewFrame();     //cimgui_wrapper.igNewFrame();

                    ImGuiIOPtr IO = ImGui.GetIO();
                   
                    IO.MouseDrawCursor = dllmain.ShowImGui_UI;

                    //ImGui_ImplWin32_UpdateMousePos();

                     dllmain.UI();

                    
                    ImGui.EndFrame(); //cimgui_wrapper.igEndFrame();
                    ImGui.Render(); //cimgui_wrapper.igRender();
                   
                    ImplDX9.RenderDrawData(ImGui.GetDrawData());  //ImplDX9.RenderDrawData(cimgui_wrapper.igGetDrawData());

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error On Render Frame!!");
                Console.WriteLine(e.Message);
            }

            dev.EndScene();
            return SharpDX.Result.Ok.Code;
        }




        //https://github.com/ocornut/imgui/issues/2087
        const float FLT_MAX = float.MaxValue;
     private  static void ImGui_ImplWin32_UpdateMousePos()
        {
            ImGuiIOPtr IO = ImGui.GetIO();

            // Set OS mouse position if requested (rarely used, only when ImGuiConfigFlags_NavEnableSetMousePos is enabled by user)
            if (IO.WantSetMousePos)
            {
               WinAPI.POINT MousePos = new WinAPI.POINT((int)IO.MousePos.X, (int)IO.MousePos.Y); // { (int)io.MousePos.x, (int)io.MousePos.y };

                WinAPI.ClientToScreen(dllmain.GameHandle, ref MousePos);
                WinAPI.SetCursorPos(MousePos.X, MousePos.Y);
            }

            // Set mouse position
            IO.MousePos = new System.Numerics.Vector2(FLT_MAX, FLT_MAX); //ImVec2(-FLT_MAX, -FLT_MAX);
            WinAPI.POINT pos;

            // Getting active window handle from the window threads processID # Bacause ::GetActiveWindow() fails in some cases.
            GUITHREADINFO guiThreadInfo = new GUITHREADINFO();
            guiThreadInfo.cbSize = Marshal.SizeOf(guiThreadInfo);
            WinAPI.GetGUIThreadInfo(GetWindowThreadProcessId(dllmain.GameHandle, IntPtr.Zero), ref guiThreadInfo);

            if (guiThreadInfo.hwndActive == dllmain.GameHandle && WinAPI.GetCursorPos(out pos))
                if (WinAPI.ScreenToClient(dllmain.GameHandle, ref pos))
                    ImGui.GetIO().MousePos = new System.Numerics.Vector2(pos.X, pos.Y); //ImVec2((float)pos.x, (float)pos.y);
        }
    

        //public void Text(Device device, string hook)
        //{
        //    try
        //    {
        //        using (Font font = new Font(device, new FontDescription()
        //        {
        //            Height = 20,
        //            FaceName = "Arial",
        //            Italic = false,
        //            Width = 0,
        //            MipLevels = 1,
        //            CharacterSet = FontCharacterSet.Default,
        //            OutputPrecision = FontPrecision.Default,
        //            Quality = FontQuality.ClearTypeNatural,
        //            PitchAndFamily = FontPitchAndFamily.Default | FontPitchAndFamily.DontCare,
        //            Weight = FontWeight.Bold
        //        }))
        //        {
        //            font.DrawText(null, hook, 0, 0, new ColorBGRA(50, 23,33, 255));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //       Console.WriteLine("Error: " + ex.Message);
        //    }
        //}


    }
}
