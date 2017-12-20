using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using System.Diagnostics;

namespace VSShortcutsManager
{
    public class ShortcutsScanner
    {
        private const string MSG_CAPTION_IMPORT = "Import Keyboard Shortcuts";
        private const string MSG_CAPTION_IMPORT_VSK = "Import Keyboard Mapping Scheme";
        private readonly int UPDATE_NEVER = 0;

        private UserShortcutsManager userShortcutsManager;

        private static ShortcutsScanner instance;

        private ShortcutsScanner()
        {
            userShortcutsManager = UserShortcutsManager.Instance;
        }
        public static ShortcutsScanner Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShortcutsScanner();
                }
                return instance;
            }
        }

        #region
        private string _AllUsersExtensionsPath;
        private string AllUsersExtensionsPath
        {
            get
            {
                if (_AllUsersExtensionsPath == null)
                {
                    _AllUsersExtensionsPath = GetAllUsersExtensionsPath();
                }
                return _AllUsersExtensionsPath;
            }
        }

        private string _LocalUserExtensionsPath;

        private string LocalUserExtensionsPath
        {
            get
            {
                if (_LocalUserExtensionsPath == null)
                {
                    // TODO: Replace with SettingsManager.GetLocalApplicationData
                    _LocalUserExtensionsPath = GetExtensionsPath(Environment.SpecialFolder.LocalApplicationData);
                }
                return _LocalUserExtensionsPath;
            }
        }

        public string _RoamingAppDataVSPath;

        private string RoamingAppDataVSPath
        {
            get
            {
                if (_RoamingAppDataVSPath == null)
                {
                    _RoamingAppDataVSPath = GetExtensionsPath(Environment.SpecialFolder.ApplicationData);
                }
                return _RoamingAppDataVSPath;
            }
        }
        #endregion

        public bool ExtensionsNeedRescan()
        {
            // TODO: Work out if there's been an update to anything in the user extensions dir or in the All-users extension dir
            // TODO: Include check for User setting to Scan/NotScan at startup.
            return true;
        }

        public bool ScanForAllExtensionShortcuts()
        {
            // Tip: Best to scan for VSK files first, because then they are available if a VSSetting file wants it.

            // Scan for new VSK files
            bool foundShortcuts = ScanForMappingSchemes();

            // Scan for new VSSettings files
            foundShortcuts |= ScanForNewShortcutsDefs();

            return foundShortcuts;
        }

        public bool ScanForNewShortcutsDefs()
        {
            // Process VSSettings files

            // Scan All-Users and local-user extension directories for VSSettings files
            List<string> vsSettingsFilesInExtDirs = GetFilesFromFolder(AllUsersExtensionsPath, "*.vssettings");
            vsSettingsFilesInExtDirs.AddRange(GetFilesFromFolder(LocalUserExtensionsPath, "*.vssettings"));

            List<ShortcutFileInfo> userShortcutsRegistry = userShortcutsManager.GetUserShortcutsRegistry();

            // For each VSSettings found, check VSSettings registry
            List<string> newVsSettings = new List<string>();
            List<string> updatedVsSettings = new List<string>();
            foreach (string vsSettingsFile in vsSettingsFilesInExtDirs)
            {
                ShortcutFileInfo shortcutFileInfo = userShortcutsRegistry.Find(x => x.Filepath.Equals(vsSettingsFile));
                if (shortcutFileInfo == null)
                {
                    // - New VSSettings file
                    // Add to VSSettings registry (update: prompt)
                    shortcutFileInfo = new ShortcutFileInfo(vsSettingsFile);
                    // Add to NewVSSettingsList (to alert users)
                    newVsSettings.Add(vsSettingsFile);
                    // Update the VSSettingsRegsitry
                    userShortcutsManager.AddUserShortcutsDef(shortcutFileInfo);
                }
                else
                {
                    // We already know about this file. Check update flag.
                    if (shortcutFileInfo.NotifyFlag == UPDATE_NEVER) continue;

                    FileInfo vsSettingsFileInfo = new FileInfo(vsSettingsFile);
                    if (shortcutFileInfo.LastWriteTimeEquals(vsSettingsFileInfo.LastWriteTime)) continue;
                    // This entry has been updated since it was added to the registry. Update the entry.
                    shortcutFileInfo.LastWriteTime = vsSettingsFileInfo.LastWriteTime;
                    // Add to UpdatedVSSettingsList (to alert users)
                    updatedVsSettings.Add(vsSettingsFile);
                    // Update the SettingsStore
                    userShortcutsManager.UpdateShortcutsDefInSettingsStore(shortcutFileInfo);
                }
            }

            // Alert user of new and updated shortcut defs
            if (newVsSettings.Count == 1)
            {
                // Prompt to load the new VSSettings
                if (MessageBox.Show($"One new user shortcut definition was found.\n\n{PrintList(newVsSettings)}\n\nWould you like to load these shortcuts now?", MSG_CAPTION_IMPORT, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Load the settings
                    VSShortcutsManager.LoadKeyboardShortcutsFromVSSettingsFile(newVsSettings.First());
                }
            }
            else if (newVsSettings.Count > 1)
            {
                MessageBox.Show($"There were {newVsSettings.Count} new user shortcut files found.\n\n{PrintList(newVsSettings)}");
            }
            // Updated settings files
            if (updatedVsSettings.Count > 0)
            {
                MessageBox.Show($"There were {updatedVsSettings.Count} updated user shortcut files found.\n\n{PrintList(updatedVsSettings)}\n\nYou might want to reapply these shortcuts.\nTool->Keyboard Shortcuts");
            }

            return newVsSettings.Count > 0 || updatedVsSettings.Count > 0;
        }

        public bool ScanForMappingSchemes()
        {
            List<ShortcutFileInfo> vskCopyList = new List<ShortcutFileInfo>();
            // Scan All-Users and local-user extension directories for VSK files
            List<string> vskFilesInExtDirs = GetFilesFromFolder(AllUsersExtensionsPath, "*.vsk");
            vskFilesInExtDirs.AddRange(GetFilesFromFolder(LocalUserExtensionsPath, "*.vsk"));

            List<ShortcutFileInfo> vskImportsRegistry = userShortcutsManager.GetVskImportsRegistry();

            // Check each VSK against VSK registry to see if it's new or updated.
            foreach (string vskFilePath in vskFilesInExtDirs)
            {
                FileInfo fileInfo = new FileInfo(vskFilePath);

                // Check existing VSK registry
                ShortcutFileInfo vskMappingInfo = vskImportsRegistry.FirstOrDefault(x => x.Filepath.Equals(vskFilePath));
                if (vskMappingInfo != null)
                {
                    // Compare date/time to existing datetime of VSK. If dates same, skip.
                    if (vskMappingInfo.LastWriteTimeEquals(fileInfo.LastWriteTime))
                    {
                        // This entry is already registered and has not changed.
                        continue;
                    }
                    else
                    {
                        // This entry has been updated.
                        // Update the LastWriteTime
                        vskMappingInfo.LastWriteTime = fileInfo.LastWriteTime;
                        // Update the Registry
                        userShortcutsManager.UpdateVskImportInfoInSettingsStore(vskMappingInfo);
                    }
                }
                else
                {
                    // Create new VskImports entry
                    vskMappingInfo = new ShortcutFileInfo(vskFilePath);
                    // Add it to the registry
                    userShortcutsManager.AddVskImportFile(vskMappingInfo);
                }
                // Add to VSK copy list (consider name)
                vskCopyList.Add(vskMappingInfo);
            }

            // Copy VSK files if VSKCopyList is not empty
            if (vskCopyList.Count > 0)
            {
                MessageBox.Show($"There are {vskCopyList.Count} new VSKs to copy.");
                ConfirmAndCopyVSKs(vskCopyList);
            }

            return vskCopyList.Count > 0;
        }

        private object PrintList(List<string> items)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in items)
            {
                if (sb.Length > 0) sb.Append("\n");
                sb.Append(item);
            }
            return sb.ToString();
        }

        private void ConfirmAndCopyVSKs(List<ShortcutFileInfo> vskCopyList)
        {
            foreach (ShortcutFileInfo vskMappingInfo in vskCopyList)
            {
                // Confirm and Copy single VSK
                if (MessageBox.Show($"Import mapping scheme file?\n{vskMappingInfo.Filepath}", MSG_CAPTION_IMPORT_VSK, MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    continue;
                }
                string name = vskMappingInfo.DisplayName;  // TODO: Prompt user for name
                CopyVSKToIDEDir(vskMappingInfo.Filepath, name);
            }
        }

        private void CopyVSKToIDEDir(string filepath, string name)
        {
            CopyVskUsingXCopy(filepath);
        }

        private void CopyVskUsingXCopy(string installPath)
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = @"xcopy.exe";
            process.StartInfo.Arguments = string.Format(@"""{0}"" ""{1}""", installPath, VSPathUtils.GetVsInstallPath());
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major > 5)
            {
                process.StartInfo.Verb = "runas";
            }
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
        }

        private List<string> GetFilesFromFolder(string folder, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // PERFORMS FILE IO! We want to minimize how often this occurs, plus delay this call as long as possible.
            List<string> allFiles = new List<string>();

            DirectoryInfo[] directories = new DirectoryInfo(folder).GetDirectories();
            foreach (DirectoryInfo extensionDir in directories)
            {
                //const SearchOption topDirectoryOnly = SearchOption.TopDirectoryOnly;
                List<string> matchingFiles = Directory.EnumerateFiles(extensionDir.FullName, searchPattern, searchOption).ToList();
                allFiles.AddRange(matchingFiles);
            }

            return allFiles;
        }

        private string GetAllUsersExtensionsPath()
        {
            return Path.Combine(VSPathUtils.GetVsInstallPath(), "Extensions");
        }

        private object GetVirtualRegistryRoot()
        {
            // Note: A different way of getting the registry root
            IVsShell shell = (IVsShell)Package.GetGlobalService(typeof(SVsShell));
            shell.GetProperty((int)__VSSPROPID.VSSPROPID_VirtualRegistryRoot, out object root);
            return root;
        }

        private string GetVSInstanceId()
        {
            return Path.GetFileName(GetVirtualRegistryRoot().ToString());
        }

        private void InitializePathVariables()
        {
            // Gets the version number with the /rootsuffix. Example: "15.0_6bb4f128Exp"
            string vsInstanceId = GetVSInstanceId();

            _LocalUserExtensionsPath = GetVisualStudioVersionPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), vsInstanceId);
            _RoamingAppDataVSPath = GetVisualStudioVersionPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), vsInstanceId);
        }

        private static string GetVisualStudioVersionPath(string appData, string version)
        {
            return Path.Combine(appData, "Microsoft\\VisualStudio", version);
        }

        private string GetExtensionsPath(Environment.SpecialFolder folder)
        {
            return Path.Combine(GetVisualStudioVersionPath(Environment.GetFolderPath(folder), GetVSInstanceId()), "Extensions");
        }

    }
}
