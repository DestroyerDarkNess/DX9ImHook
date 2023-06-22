using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using EasyHook;
using ImGuiNET;
using SharpDX.Direct3D9;

namespace DX9ImHook
{

    public class D3D9ResetHook
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int ResetDelegate(IntPtr device, ref PresentParameters presentParameters);

        public List<IntPtr> devicefunctionaddresses = new List<IntPtr>();
        public LocalHook ResetHooker;
        public Device deviceGlobal;

        private static IntPtr _originalReset = IntPtr.Zero;

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
            _originalReset = devicefunctionaddresses[(int)d3d9_indexes.Direct3DDevice9FunctionOrdinals.Reset];
            ResetHooker = LocalHook.Create(_originalReset,
                new ResetDelegate(ResetHook), this);

            ResetHooker.ThreadACL.SetExclusiveACL(new int[] { 0 });

            Console.WriteLine("Reset Hooked!");

        }


        static IntPtr[] GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            List<IntPtr> vtblAddresses = new List<IntPtr>();
            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = 0; i < numberOfMethods; i++)
                vtblAddresses.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));
            return vtblAddresses.ToArray();
        }

        public int ResetHook(IntPtr device, ref PresentParameters presentParameters)
        {
            //Device dev = (Device)device;
          
            if (Values.g_Initialized == true)
            {
                  ImplDX9.InvalidateDeviceObjects();
            }

            int result = Marshal.GetDelegateForFunctionPointer<ResetDelegate>(_originalReset)(device, ref presentParameters);
            //dev.Reset();

            if (Values.g_Initialized == true)
            {
                  ImplDX9.CreateDeviceObjects();
            }

            return result;
        }
    }

}
