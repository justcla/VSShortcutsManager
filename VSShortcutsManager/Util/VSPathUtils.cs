using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSShortcutsManager
{
    public class VSPathUtils
    {

        //// Initialise path for AppDataRoaming and AppDataLocal (Optional - alternative method)
        //_RoamingAppDataVSPath = Path.Combine(ShellSettingsManager.GetApplicationDataFolder(ApplicationDataFolder.ApplicationExtensions), "Extensions");
        //_LocalUserExtensionsPath = Path.Combine(ShellSettingsManager.GetApplicationDataFolder(ApplicationDataFolder.LocalSettings), "Extensions");

        public static string GetVsInstallPath()
        {
            string root = GetRegistryRoot();
            using (var key = Registry.LocalMachine.OpenSubKey(root))
            {
                var installDir = key.GetValue("InstallDir") as string;
                return Path.GetDirectoryName(installDir);
            }
        }

        public static string GetRegistryRoot()
        {
            var reg = Package.GetGlobalService(typeof(SLocalRegistry)) as ILocalRegistry2;
            reg.GetLocalRegistryRoot(out string root);
            return root;
        }

    }
}
