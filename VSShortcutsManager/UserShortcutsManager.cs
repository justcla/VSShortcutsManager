using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VSShortcutsManager
{
    public class UserShortcutsManager
    {
        private WritableSettingsStore UserSettingsStore;

        //private List<ShortcutFileInfo> UserShortcutsRegistry;
        private List<ShortcutFileInfo> VskImportsRegistry;

        // UserSettingsStore keys
        private static readonly string GENERAL_SETTINGS = "GeneralSettings";
        private static readonly string USER_SHORTCUTS_DEFS = "UserShortcutsDefs";
        private static readonly string IMPORTED_MAPPING_SCHEMES = "ImportedMappingSchemes";
        private static readonly string NAME = "Name";
        private static readonly string FILEPATH = "Filepath";
        private static readonly string EXTENSION_NAME = "ExtensionName";
        private static readonly string LAST_WRITE_TIME = "LastWriteTime";
        private static readonly string FLAGS = "Flags";
        private static readonly string LAST_EXTENSION_SCAN_TIME = "LastExtensionScanTime";

        private static UserShortcutsManager instance;

        private UserShortcutsManager()
        {
            SettingsManager shellSettingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            UserSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        public static UserShortcutsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserShortcutsManager();
                }
                return instance;
            }
        }

        private List<ShortcutFileInfo> userShortcutsReg;
        public List<ShortcutFileInfo> UserShortcutsRegistry {
            get
            {
                if (userShortcutsReg == null)
                {
                    userShortcutsReg = new List<ShortcutFileInfo>();
                }
                return userShortcutsReg;
            }
            set
            {
                userShortcutsReg = value;
            }
        }

        //-------- Generic methods for handling ShortcutFileInfo in UserSettingsStore

        public List<ShortcutFileInfo> FetchShortcutFileInfo(string collectionName)
        {
            List<ShortcutFileInfo> shortcutFiles = new List<ShortcutFileInfo>();
            if (UserSettingsStore.CollectionExists(collectionName))
            {
                // shortcutDefId will be a Hash of the filepath
                foreach (string shortcutDefId in UserSettingsStore.GetSubCollectionNames(collectionName))
                {
                    shortcutFiles.Add(ExtractShortcutsInfoFromSettingsStore(collectionName, shortcutDefId));
                }
            }
            return shortcutFiles;
        }

        private ShortcutFileInfo ExtractShortcutsInfoFromSettingsStore(string collectionName, string shortcutDef)
        {
            string collectionPath = $"{collectionName}\\{shortcutDef}";
            // Extract values from UserSettingsStore
            string filepath = UserSettingsStore.GetString(collectionPath, FILEPATH);
            string name = UserSettingsStore.GetString(collectionPath, NAME);
            string extensionName = UserSettingsStore.GetString(collectionPath, EXTENSION_NAME);
            DateTime lastWriteTime = DateTime.Parse(UserSettingsStore.GetString(collectionPath, LAST_WRITE_TIME));
            int flags = UserSettingsStore.GetInt32(collectionPath, FLAGS, 0);

            return new ShortcutFileInfo()
            {
                Filepath = filepath,
                DisplayName = name,
                ExtensionName = extensionName,
                LastWriteTime = lastWriteTime,
                NotifyFlag = flags
            };
        }

        private void SaveShortcutFileInfoToSettingsStore(string collectionPrefix, ShortcutFileInfo shortcutFileInfo)
        {
            // Store values in UserSettingsStore as a hash of the filepath as the Collection key
            string collectionKey = GetRegistryKey(shortcutFileInfo);
            string collectionPath = $"{collectionPrefix}\\{collectionKey}";
            UserSettingsStore.CreateCollection(collectionPath);
            UserSettingsStore.SetString(collectionPath, NAME, shortcutFileInfo.DisplayName);
            UserSettingsStore.SetString(collectionPath, FILEPATH, shortcutFileInfo.Filepath);
            UserSettingsStore.SetString(collectionPath, EXTENSION_NAME, shortcutFileInfo.ExtensionName);
            UserSettingsStore.SetString(collectionPath, LAST_WRITE_TIME, shortcutFileInfo.LastWriteTime.ToString(ShortcutFileInfo.DATETIME_FORMAT));
            UserSettingsStore.SetInt32(collectionPath, FLAGS, shortcutFileInfo.NotifyFlag);
        }

        //-------- User shortcut definitions -------

        public List<ShortcutFileInfo> GetUserShortcutsRegistry()
        {
            // Always fetch it fresh from the registry
            return FetchShortcutFileInfo(USER_SHORTCUTS_DEFS);
        }

        internal void AddUserShortcutsDef(ShortcutFileInfo shortcutFileInfo)
        {
            // Remove any item with the same filepath
            if (HasUserShortcuts(shortcutFileInfo.Filepath))
            {
                DeleteUserShortcutsDef(shortcutFileInfo);
            }
            UserShortcutsRegistry.Add(shortcutFileInfo);
            UpdateShortcutsDefInSettingsStore(shortcutFileInfo);
        }

        public void UpdateShortcutsDefInSettingsStore(ShortcutFileInfo userShortcutsDef)
        {
            SaveShortcutFileInfoToSettingsStore(USER_SHORTCUTS_DEFS, userShortcutsDef);
        }

        public void ResetUserShortcutsRegistry()
        {
            UserShortcutsRegistry = new List<ShortcutFileInfo>();
            UserSettingsStore.DeleteCollection(USER_SHORTCUTS_DEFS);
        }

        public void DeleteUserShortcutsDef(ShortcutFileInfo shortcutFileInfo)
        {
            // Remove the shortcuts definition from the in-memory registry
            if (HasUserShortcuts(shortcutFileInfo.Filepath))
            {
                UserShortcutsRegistry.Remove(GetUserShortcutsInfo(shortcutFileInfo.Filepath));
            }
            // Update the settings store
            string shortcutDefHashStr = GetRegistryKey(shortcutFileInfo);
            string collectionPath = $"{USER_SHORTCUTS_DEFS}\\{shortcutDefHashStr}";
            UserSettingsStore.DeleteCollection(collectionPath);
        }

        private static string GetRegistryKey(ShortcutFileInfo shortcutFileInfo)
        {
            // Use the Hash of the filepath as the CollectionKey for the registry entries
            return shortcutFileInfo.Filepath.GetHashCode().ToString();
        }

        public bool HasUserShortcuts(string shortcutsFilepath)
        {
            return UserShortcutsRegistry.Exists(x => x.Filepath == shortcutsFilepath);
        }

        public ShortcutFileInfo GetUserShortcutsInfo(string shortcutFilepath)
        {
            ShortcutFileInfo userShortcutsDef = UserShortcutsRegistry.First(x => x.Filepath.Equals(shortcutFilepath));
            return userShortcutsDef;
        }

        //-------- VskImports --------

        public List<ShortcutFileInfo> GetVskImportsRegistry()
        {
            if (VskImportsRegistry == null)
            {
                // Load from SettingsStore
                VskImportsRegistry = FetchShortcutFileInfo(IMPORTED_MAPPING_SCHEMES);
            }
            return VskImportsRegistry;
        }

        internal void AddVskImportFile(ShortcutFileInfo shortcutFileInfo)
        {
            GetVskImportsRegistry().Add(shortcutFileInfo);
            UpdateVskImportInfoInSettingsStore(shortcutFileInfo);
        }

        public void UpdateVskImportInfoInSettingsStore(ShortcutFileInfo shortcutFileInfo)
        {
            SaveShortcutFileInfoToSettingsStore(IMPORTED_MAPPING_SCHEMES, shortcutFileInfo);
        }

        //------ Last Extension Scan Time ------

        public long GetLastExtensionScanTime()
        {
            return UserSettingsStore.GetInt64(GENERAL_SETTINGS, LAST_EXTENSION_SCAN_TIME, 0L);
        }
        public void SetLastExtensionScanTime(long updateTimestamp)
        {
            string collectionPath = GENERAL_SETTINGS;
            UserSettingsStore.CreateCollection(collectionPath);
            UserSettingsStore.SetInt64(collectionPath, LAST_EXTENSION_SCAN_TIME, updateTimestamp);
        }

    }

}