using ImGuiNET;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Devices;
using Process.NET.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DX9ImHook
{
    public static class dllmain
    {

      public enum InputType
        {
            OldWndProc = 0,
            RawInput = 1,
            UniversalUnsafe = 2,
            ModernWndProc = 3
      }

        public enum D3d9HookType
        {
            EndScene = 0,
            Present = 1
        }

        public static int KeyMenu = 0x2D; // VK_INSERT
        public static IntPtr GameHandle = IntPtr.Zero;
        public static void EntryPoint()
        {
            // Some games like GtaSA have a certain delay in displaying the window,
            // so the MainWindowHandle cannot be obtained, To fix this we will place a timeout.
            // System.Threading.Thread.Sleep(5000);

            // Another solution to this, is to make a small loop looking for the game window,
            // until you get the handle, using the WinAPI FindWindow.

            //You can also call 'Process.GetCurrentProcess().MainWindowHandle' in a loop, until the result is different from 0.
            while (GameHandle.ToInt32() == 0)   {
                GameHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;  // Get Main Game Window Handle
            }


            /* WinAPI.AllocConsole();*/ // It Change GetCurrentProcess().MainWindowHandle , USE : GameHandle = WinAPI.FindWindow(null, "Halo");
            WinAPI.AllocConsole();

            Console.WriteLine("Game WindowHandle  -->> " + GameHandle.ToString());

            MouseCursorHookType HookCursorType = MouseCursorHookType.None;
            InputType HookInputType = InputType.UniversalUnsafe;
            D3d9HookType D3dHook = D3d9HookType.EndScene;

            try
            {
                // Extract important and native resources.
                System.IO.File.WriteAllBytes("cimgui.dll", Properties.Resources.cimgui);
                System.IO.File.WriteAllBytes("EasyLoad32.dll", Properties.Resources.EasyLoad32);
                System.IO.File.WriteAllBytes("EasyLoad64.dll", Properties.Resources.EasyLoad64);
                System.IO.File.WriteAllBytes("EasyHook64Svc.exe", Properties.Resources.EasyHook64Svc);
                System.IO.File.WriteAllBytes("EasyHook32Svc.exe", Properties.Resources.EasyHook32Svc);
                System.IO.File.WriteAllBytes("EasyHook32.dll", Properties.Resources.EasyHook32);
                System.IO.File.WriteAllBytes("EasyHook64.dll", Properties.Resources.EasyHook64);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All Resources were successfully extracted.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to Extract Resources: " + ex.Message);
            }

            try
            {
                // Fix RawImput
                // Fixed Menu on RawImput Games, Type HALO 

                Console.ForegroundColor = ConsoleColor.White;
                IntPtr handle = WinAPI.GetModuleHandle("dinput8.dll");
                if (handle != IntPtr.Zero)
                {
                    Console.WriteLine("DirectImput8 Detected!");

                    WinAPI.FreeLibrary(handle);

                    if (System.IO.File.Exists("dinput8.dll") == false)
                    {
                        try
                        {
                            // By : https://github.com/geeky/dinput8wrapper

                            System.IO.File.WriteAllBytes("dinput8.dll", Properties.Resources.dinput8); // <<<----  On windows it sometimes fails with UnauthorizedAccessException, such as when the game is on a drive other than the OS drive.

                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("------->>>>  DirectInput8 is Patched, YOU MUST RESTART THE GAME.  <<<<--------");
                            Console.ReadKey();
                            System.Diagnostics.Process.GetCurrentProcess().Kill();
                            return;

                        }
                        catch (UnauthorizedAccessException Ex) { Console.WriteLine("dinput8 patch Error: " + Ex.Message); }

                    }
                    else
                    {

                        WinAPI.LoadLibrary("dinput8.dll");
                        HookInputType = InputType.ModernWndProc;
                    }


                    //    //DirectInput8Create_t OriginalFunction = (DirectInput8Create_t)GetProcAddress("dinput8.dll", "DirectInput8Create");
                    //    // Convert c++ code : https://github.com/pampersrocker/DInput8HookingExample to C#

                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to Patched RawImpu!: " + ex.Message);
            }

            try {
                Console.ForegroundColor = ConsoleColor.White;
                //Call Costura.AssemblyLoader.Attach(); Via Reflection.

                string nspace = "Costura";

                var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsClass && t.Namespace == nspace
                        select t;

                Type AssemblyLoader = null;

                foreach (Type t in q.ToList())
                {
                    if (t.Name == "AssemblyLoader") {
                        AssemblyLoader = t;
                    }
                }

                if (AssemblyLoader == null)
                {
                    throw new Exception("AssemblyLoader Error");
                } else
                {
                    MethodInfo theMethod = AssemblyLoader.GetMethod("Attach");
                    theMethod.Invoke(null, null);
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("All Embed Libs Loaded!");
                }

              
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Costura Error: " + ex.Message);
                Console.ReadKey();
            }
            
            bool Runtine = true;
            bool Deprecated_KeyStateAsync = false;


            //For Custom Input Hook

            bool RetrieveWndProcMessages = false;

            try
            {
                // Identify the Game.

                string ProcName = Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().ProcessName);

                Console.WriteLine($"Game: {ProcName}");

                switch (ProcName.ToLower())
                {
                    case "gta_sa":
                        // Set Specifig Hook

                        HookInputType = InputType.ModernWndProc;
                        D3dHook = D3d9HookType.Present;
                        HookCursorType = MouseCursorHookType.SetCursorPos;
                        RetrieveWndProcMessages = true;

                        break;
                    case "haloce":

                        break;
                    default:

                        break;
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Identify Game Error: " + ex.Message);
            }

            try
            {
                // Hook Mouse Cursor From Games Type GTA SA.
                MouseCursorHook NewHookCursor = new MouseCursorHook(GameHandle, HookCursorType);
                NewHookCursor.FindAndHook();
                Console.WriteLine("Mouse Cursor Hooked!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to Hook Mouse Cursor: " + ex.Message);
            }


            try
            {
                // Hook DX9 EndScene Or Present

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Hooking " + D3dHook.ToString());

                switch (D3dHook)
                {
                    case D3d9HookType.EndScene:
                        // EndScene Hook

                        D3D9EndSceneHook NewEndSceneHook = new D3D9EndSceneHook();
                        NewEndSceneHook.FindAndHook();

                        break;
                    case D3d9HookType.Present:
                        // Present Hook

                        D3D9PresentHook NewD3D9PresentHook = new D3D9PresentHook();
                        NewD3D9PresentHook.FindAndHook();

                        break;
                    default:
                     
                        break;
                }

                D3D9ResetHook NewD3D9ResetHook = new D3D9ResetHook();
                NewD3D9ResetHook.FindAndHook();

            }
            catch (Exception ex)
            {
                Console.ForegroundColor= ConsoleColor.Red;
                Console.WriteLine(D3dHook.ToString() + " Error: " + ex.Message);
                Runtine = false;
            }

            try
            {
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine("Hooking " + HookInputType.ToString());

                switch (HookInputType)
                {
                    case InputType.OldWndProc:
                        // WndProc Hook

                        OldWindowHook WndProc_Hook = new OldWindowHook(GameHandle, "ImHookWndProc");
                        WndProc_Hook.Enable();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("WndProc_Hook Attach!");
                        break;
                    case  InputType.RawInput:
                        // RawInput Hook

                        RawInputHook RawInput_Hook = new RawInputHook(GameHandle);
                        RawInput_Hook.FindAndHook();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("RawInput_Hook Attach!");
                        break;
                    case InputType.UniversalUnsafe:
                        // DefWindowProcW Hook

                        UniversalWindowMessageHook DefWindowProcW_Hook = new UniversalWindowMessageHook(GameHandle);
                        DefWindowProcW_Hook.FindAndHook();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("DefWindowProcW_Hook Attach!");
                        break;
                    case InputType.ModernWndProc:
                        
                        ModernWndProcHook CustomWndProc_Hook = new ModernWndProcHook(GameHandle);
                        CustomWndProc_Hook.ReturnOrigMessage = RetrieveWndProcMessages;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Modern WndProc Hook Attach!");
                        break;
                    default:
                        Deprecated_KeyStateAsync = true;
                        break;
                }

              
            }
            catch (Exception ex)
            {
                // Input Hook Failed!
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(HookInputType.ToString() + " Error: " + ex.Message);
                Deprecated_KeyStateAsync = true;
            }

            Console.ForegroundColor = ConsoleColor.White;

            while (Runtine)  {
                try
                {
                    Thread.Sleep(10);

                    // Deprecated, replaced by WndProc Hook, See WindowHook.cs class <<<<<-------------


                    if (Deprecated_KeyStateAsync == true)
                    {

                        int keyState = WinAPI.GetAsyncKeyState(Keys.Insert);

                        if (keyState == 1 || keyState == -32767)
                        {
                            ShowImGui_UI = !ShowImGui_UI;
                            WinAPI.ShowCursor(ShowImGui_UI);
                        }

                    }

                    //Application.DoEvents(); // UnFreezing Forms Opens

                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Main bucle Error: " + ex.Message);
                }
            }

            ImplDX9.Shutdown();
            ImplWin32.Shutdown();


        }

        public static bool ShowImGui_UI = false;


        private  static bool ShowImguiDemo = false;
        public static void UI()
        {

            if (ShowImGui_UI)
            {
               
                ImGui.Begin("Another Window in C#", ref ShowImGui_UI);
                ImGui.Text("Hello from another window!");
              
                if (ImGui.Button("Show ImguiDemo"))
                    ShowImguiDemo = !ShowImguiDemo;

                if (ImGui.Button("Close Me"))
                    ShowImGui_UI = false;
              
                if (ShowImguiDemo)
                    ImGui.ShowDemoWindow();

                ImGui.End();
            }


        }



    }
}
