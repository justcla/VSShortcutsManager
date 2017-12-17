using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;

namespace VSShortcutsManager
{
    public class UserShortcutsManager
    {
        private ShellSettingsManager ShellSettingsManager;
        private WritableSettingsStore UserSettingsStore;

        // UserSettingsStore keys
        private static readonly string USER_SHORTCUTS_DEFS = "UserShortcutsDefs";
        private static readonly string NAME = "Name";
        private static readonly string FILEPATH = "Filepath";
        private static readonly string EXTENSION_NAME = "ExtensionName";
        private static readonly string LAST_WRITE_TIME = "LastWriteTime";
        private static readonly string FLAGS = "Flags";
        private static readonly string DATETIME_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

        public UserShortcutsManager(IServiceProvider package) {
            Initialize(package);
        }

        public void Initialize(IServiceProvider package)
        {
            ShellSettingsManager = new ShellSettingsManager(package);
            UserSettingsStore = ShellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        public List<UserShortcutsDef> FetchUserShortcutsRegistry()
        {
            List<UserShortcutsDef> userShortcutsRegistry = new List<UserShortcutsDef>();
            if (UserSettingsStore.CollectionExists(USER_SHORTCUTS_DEFS))
            {
                foreach (var shortcutDef in UserSettingsStore.GetSubCollectionNames(USER_SHORTCUTS_DEFS))
                {
                    userShortcutsRegistry.Add(ExtractShortcutsDefFromSettingsStore(shortcutDef));
                }
            }
            return userShortcutsRegistry;
        }

        public UserShortcutsDef ExtractShortcutsDefFromSettingsStore(string shortcutDef)
        {
            string collectionPath = $"{USER_SHORTCUTS_DEFS}\\{shortcutDef}";
            // Extract values from UserSettingsStore
            string filepath = UserSettingsStore.GetString(collectionPath, FILEPATH);
            string name = UserSettingsStore.GetString(collectionPath, NAME);
            string extensionName = UserSettingsStore.GetString(collectionPath, EXTENSION_NAME);
            DateTime lastWriteTime = DateTime.Parse(UserSettingsStore.GetString(collectionPath, LAST_WRITE_TIME));
            int flags = UserSettingsStore.GetInt32(collectionPath, FLAGS, 0);

            return new UserShortcutsDef()
            {
                Filepath = filepath,
                Name = name,
                ExtensionName = extensionName,
                LastWriteTime = lastWriteTime,
                NotifyFlag = flags
            };
        }

        public void UpdateShortcutsDefInSettingsStore(UserShortcutsDef userShortcutsDef)
        {
            // Store values in UserSettingsStore. Use the "Name" property as the Collection key
            string collectionPath = $"{USER_SHORTCUTS_DEFS}\\{userShortcutsDef.Name}";
            UserSettingsStore.CreateCollection(collectionPath);
            UserSettingsStore.SetString(collectionPath, NAME, userShortcutsDef.Name);
            UserSettingsStore.SetString(collectionPath, FILEPATH, userShortcutsDef.Filepath);
            UserSettingsStore.SetString(collectionPath, EXTENSION_NAME, userShortcutsDef.ExtensionName);
            UserSettingsStore.SetString(collectionPath, LAST_WRITE_TIME, userShortcutsDef.LastWriteTime.ToString(DATETIME_FORMAT));
            UserSettingsStore.SetInt32(collectionPath, FLAGS, userShortcutsDef.NotifyFlag);
        }

        public void ResetUserShortcutsRegistry()
        {
            UserSettingsStore.DeleteCollection(USER_SHORTCUTS_DEFS);
        }

        public static bool DateTimesAreEqual(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1.ToString(DATETIME_FORMAT).Equals(dateTime2.ToString(DATETIME_FORMAT));
        }

    }

}