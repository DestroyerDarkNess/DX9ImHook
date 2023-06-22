using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EasyHook;
using ImGuiNET;
using SharpDX.Direct3D9;

namespace DX9ImHook
{
  
    public class D3D9PresentHook 
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int PresentDelegate(IntPtr device, IntPtr sourceRect, IntPtr destRect, IntPtr hDestWindowOverride, IntPtr dirtyRegion);

        public List<IntPtr> devicefunctionaddresses = new List<IntPtr>();
        public LocalHook PresentHooker;
        public Device deviceGlobal;

        public D3D9PresentHook()
        {
           
        }

        public void FindAndHook()
        {

            devicefunctionaddresses = new List<IntPtr>();

            using (Direct3D d3d = new Direct3D())
            {
                using (var renderform = new System.Windows.Forms.Form())
                {
                    using (deviceGlobal = new Device(d3d, 0, DeviceType.NullReference, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, new PresentParameters() { BackBufferWidth = 1, BackBufferHeight = 1, DeviceWindowHandle = renderform.Handle }))
                    {
                        devicefunctionaddresses.AddRange(GetVTblAddresses(deviceGlobal.NativePointer, 119));
                    }
                }

            }

            PresentHooker = LocalHook.Create(devicefunctionaddresses[(int)d3d9_indexes.Direct3DDevice9FunctionOrdinals.Present],
                new PresentDelegate(PresentHook), this);

            PresentHooker.ThreadACL.SetExclusiveACL(new Int32[1]);

        }


        static IntPtr[] GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            List<IntPtr> vtblAddresses = new List<IntPtr>();
            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = 0; i < numberOfMethods; i++)
                vtblAddresses.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));
            return vtblAddresses.ToArray();
        }



        public int PresentHook(IntPtr device, IntPtr sourceRect, IntPtr destRect, IntPtr hDestWindowOverride, IntPtr dirtyRegion)
        {
            Device dev = (Device)device;

            try
            {
                if (Values.g_Initialized == false)
                {
                    var context = cimgui_wrapper.igCreateContext(IntPtr.Zero);

                    ImplWin32.Init(dllmain.GameHandle); //  cimgui_wrapper.ImGui_ImplWin32_Init(dllmain.GameHandle);
                    ImplDX9.Init(device);

                    Console.WriteLine("Present Hooked!");

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

                if (Values.g_Initialized == true && dllmain.ShowImGui_UI == true)
                {

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

            dev.Present();
            return SharpDX.Result.Ok.Code;
        }
    }

}
