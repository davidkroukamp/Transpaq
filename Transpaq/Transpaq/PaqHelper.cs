using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Transpaq
{
    class PaqHelper
    {
        private RegistryKey appRegistry;
        private string appDataRegistryKey;

        private Dictionary<string, Thread> paqThreads { get; set; } = new Dictionary<string, Thread>();

        public PaqHelper(RegistryKey appRegistry, string appDataRegistryKey)
        {
            this.appRegistry = appRegistry;
            this.appDataRegistryKey = appDataRegistryKey;
        }

        internal void Initialise()
        {
            var paqsRegistryKey = this.appRegistry.OpenSubKey(this.appDataRegistryKey, false);
            var paqs = paqsRegistryKey.GetValueNames();
            foreach(var paqName in paqs)
            {
                var paqData = paqsRegistryKey.GetValue(paqName).ToString();
                this.LoadPaq(paqName, paqData);
            }
        }

        internal void Install(string name, string data)
        {
            this.Update(name, data);
        }
        
        internal void Update(string name, string data)
        {
            this.Revoke(name);
            RegistryKey newPaqRegistryKey = this.appRegistry.OpenSubKey(this.appDataRegistryKey, true);
            newPaqRegistryKey.SetValue(name, data);
            newPaqRegistryKey.Close();
            this.LoadPaq(name, data);
        }

        internal void Revoke(string name)
        {
            if (paqThreads.ContainsKey(name))
            {
                paqThreads[name]?.Abort();
                paqThreads.Remove(name);
            }

            RegistryKey revokePaqRegistryKey = this.appRegistry.OpenSubKey(this.appDataRegistryKey, true);
            revokePaqRegistryKey.DeleteValue(name, false);
            revokePaqRegistryKey.Close();
        }

        internal void RevokeAll()
        {
            var paqsRegistryKey = this.appRegistry.OpenSubKey(this.appDataRegistryKey, false);
            var paqs = paqsRegistryKey.GetValueNames();
            foreach (var paq in paqs)
            {
                if (paqThreads.ContainsKey(paq))
                {
                    paqThreads[paq]?.Abort();
                    paqThreads.Remove(paq);
                }

                RegistryKey revokePaqRegistryKey = this.appRegistry.OpenSubKey(this.appDataRegistryKey, true);
                revokePaqRegistryKey.DeleteValue(paq, false);
                revokePaqRegistryKey.Close();
            }
        }

        private void LoadPaq(string name, string data)
        {
            var paqBytes = Convert.FromBase64String(data);
            Thread paqThread = new Thread(new ParameterizedThreadStart(ExecutePaq));
            paqThreads.Add(name, paqThread);
            paqThread.Start(paqBytes);
        }

        private void ExecutePaq(object data)
        {
            var paqBytes = (byte[])data;
            var paqAssembly = Assembly.Load(paqBytes);
            foreach (var type in paqAssembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static))
                {
                    if (method.Name == "Execute")
                    {
                        // TODO it seems like the process wont quit until all the threads die (need to confirm)
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}