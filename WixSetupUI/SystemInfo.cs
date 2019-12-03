using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Reflection;

namespace WixSetupUI
{
    public class SystemInfo : IDisposable
    {
        private bool CopyConfigFile(string path, string fileName)
        {
            try
            {
                if (!(Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "//LIFECODES//InstallTemp")))
                {
                    CreateFolderAndSetSecurity(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "//LIFECODES//InstallTemp");
                }
                if (File.Exists($"{path}\\{fileName}"))
                {
                    File.Copy($"{path}\\{fileName}", $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\LIFECODES\\InstallTemp\\{fileName}", true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ReturnConfigFile(string path, string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string tempPath = $"{appData}\\LIFECODES\\InstallTemp";
            try
            {
                if (File.Exists($"{path}\\{fileName}"))
                {
                    File.Copy($"{tempPath}\\{fileName}", $"{path}\\{fileName}", true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void BackupConfigFiles()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            CopyConfigFile($@"{appData}\Lifecodes\", "fileName.config");
        }

        public void RestoreConfigFiles()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            ReturnConfigFile($@"{appData}\Lifecodes\", "fileName.config");
        }

        private void CreateFolderAndSetSecurity(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                DirectorySecurity sec = Directory.GetAccessControl(path);
                // Using this instead of the "Everyone" string means we work on non-English systems.
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                sec.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                Directory.SetAccessControl(path, sec);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // This is only needed if app can't be removed normally with MSI.
        public void UninstallApplication()
        {
            ProcessStartInfo info = new ProcessStartInfo("MsiExec.exe", "/x {C0D8BB7F-C179-4E4C-AF29-A65980B741F3} /quiet");
            Process appRemove = Process.Start(info);

            do
            {
                appRemove.Refresh();
            }
            while (!appRemove.WaitForExit(500));
        }

        public bool CheckIfAppExistsAlready(string appName)
        {
            string keyName;
            // 32 bit
            keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            if (ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", appName) == true)
            {
                return true;
            }

            keyName = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            if (ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", appName) == true)
            {
                return true;
            }

            return false;
        }

        public bool CheckIfAppExistsAlready(string appName, string versionNumber)
        {
            string keyName;
            // 32 bit
            keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            if (ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", appName) == true)
            {
                if (ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayVersion", versionNumber))
                {
                    return true;
                }
            }

            keyName = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            if (ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", appName) == true)
            {
                if (ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayVersion", versionNumber))
                {
                    return true;
                }
            }

            return false;
        }


        private static bool ExistsInSubKey(RegistryKey p_root, string p_subKeyName, string p_attributeName, string appName)
        {
            RegistryKey subkey;
            string keyName;

            using (RegistryKey key = p_root.OpenSubKey(p_subKeyName))
            {
                if (key != null)
                {
                    foreach (string kn in key.GetSubKeyNames())
                    {
                        using (subkey = key.OpenSubKey(kn))
                        {
                            keyName = subkey.GetValue(p_attributeName) as string;
                            if (appName.Equals(keyName, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private bool ServicesInstalled;
        private bool Services13Installed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
