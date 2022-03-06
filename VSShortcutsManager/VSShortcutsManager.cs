using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Linq;
using EnvDTE;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Settings;
using System.Xml.Linq;

namespace VSShortcutsManager
{
    /// <summary>
    /// Command handler
    /// </summary>
    public sealed class VSShortcutsManager
    {
        /// <summary>
        /// Match with symbols in VSCT file.
        /// </summary>
        public static readonly Guid VSShortcutsManagerCmdSetGuid = new Guid("cca0811b-addf-4d7b-9dd6-fdb412c44d8a");
        public const int BackupShortcutsCmdId = 0x1200;
        public const int ResetShortcutsCmdId = 0x1400;
        public const int ImportMappingSchemeCmdId = 0x1500;
        public const int ShortcutSchemesMenu = 0x2002;
        public const int DynamicThemeStartCmdId = 0x2A00;
        public const int UserShortcutsMenu = 0x1080;
        public const int ImportUserShortcutsCmdId = 0x1130;
        public const int ManageUserShortcutsCmdId = 0x1140;
        public const int DynamicUserShortcutsStartCmdId = 0x3A00;
        public const int ClearUserShortcutsCmdId = 0x1210;
        public const int ScanExtensionsCmdId = 0x1300;
        public const int AddNewShortcutCmdId = 0x1410;
        public const int LiveShortcutsViewCmdId = 0x1420;
        public const int RefreshCommandShortcutsViewCmdId = 0x1760;
        public const int CommandShortcutsToolWinCmdId = 0x1610;
        public const int FilterAllShortcutsCmdId = 0x1710;
        public const int FilterPopularShortcutsCmdId = 0x1720;
        public const int FilterUserShortcutsCmdId = 0x1730;

        private const string BACKUP_FILE_PATH = "BackupFilePath";
        private const string MSG_CAPTION_IMPORT = "Import Keyboard Shortcuts";
        private const string MSG_CAPTION_SAVE_SHORTCUTS = "Save Keyboard Shortcuts";
        private const string MSG_CAPTION_RESET = "Reset Keyboard Shortcuts";
        private const string MSG_CAPTION_IMPORT_VSK = "Import Keyboard Mapping Scheme";
        private const string DEFAULT_MAPPING_SCHEME_NAME = "(Default)";

        // UserSettingsStore constants
        private const string VSK_IMPORTS_REGISTRY_KEY = "VskImportsRegistry";

        private readonly Dictionary<string, string> ShortcutOperations = new Dictionary<string, string> { { "Shortcut", "Add" }, { "RemoveShortcut", "Remove" } };

        //private const string USER_SHORTCUTS_DEFS = "UserShortcutsDefs";
        //private const string DATETIME_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;
        public readonly VSShortcutQueryEngine queryEngine;

        private readonly int UPDATE_NEVER = 0;
        private readonly int UPDATE_PROMPT = 1;
        private readonly int UPDATE_ALWAYS = 2;
        private List<string> MappingSchemes;

        private ShellSettingsManager ShellSettingsManager;
        private UserShortcutsManager userShortcutsManager;
        //private List<ShortcutFileInfo> VskImportsRegistry;
        //private List<ShortcutFileInfo> UserShortcutsRegistry;

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static VSShortcutsManager Instance
        {
            get;
            private set;
        }

        IEnumerable<Command> _allCommandsCache;
        public IEnumerable<Command> AllCommandsCache
        {
            get
            {
                if (_allCommandsCache == null)
                {
                    ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        _allCommandsCache = await queryEngine.GetAllCommandsAsync();
                    });
                }
                return _allCommandsCache;
            }
            private set { }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new VSShortcutsManager(package);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSShortcutsManager"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private VSShortcutsManager(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            // Intialize the VSShortcutQueryEngine
            this.queryEngine = VSShortcutQueryEngine.GetInstance(ServiceProvider);

            // Register all the command handlers with the Global Command Service
            RegisterCommandHandlers();

            //ShellSettingsManager = new ShellSettingsManager(package);
            userShortcutsManager = UserShortcutsManager.Instance;

            //// Initialise path for AppDataRoaming and AppDataLocal (Optional - alternative method)
            //_RoamingAppDataVSPath = Path.Combine(ShellSettingsManager.GetApplicationDataFolder(ApplicationDataFolder.ApplicationExtensions), "Extensions");
            //_LocalUserExtensionsPath = Path.Combine(ShellSettingsManager.GetApplicationDataFolder(ApplicationDataFolder.LocalSettings), "Extensions");

            // Load user shortcut registries
            //userShortcutsManager.DeleteUserShortcutsDef("WindowHideShortcuts");
            //UserShortcutsRegistry = userShortcutsManager.GetUserShortcutsRegistry();
            // Load imported VSKs registry
            //VskImportsRegistry = userShortcutsManager.GetVskImportsRegistry();

            if (ShortcutsScanner.Instance.ExtensionsNeedRescan())
            {
                ShortcutsScanner.Instance.ScanForAllExtensionShortcuts();
            }

        }

        private void RegisterCommandHandlers()
        {
            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                commandService.AddCommand(CreateMenuItem(BackupShortcutsCmdId, this.BackupShortcuts));
                commandService.AddCommand(CreateMenuItem(ResetShortcutsCmdId, this.ResetShortcuts));
                commandService.AddCommand(CreateMenuItem(ScanExtensionsCmdId, this.ScanUserShortcuts));
                OleMenuCommand clearUserShortcutsCmd = CreateMenuItem(ClearUserShortcutsCmdId, this.ClearUserShortcuts);
                clearUserShortcutsCmd.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatusClearUserShortcuts);
                commandService.AddCommand(clearUserShortcutsCmd);

                // Import User Shortcuts from file
                commandService.AddCommand(CreateMenuItem(ImportUserShortcutsCmdId, this.ImportShortcuts));
                // Load User Shortcuts (from known vssettings files)
                // Add a dummy entry for the Load User Shortcuts menu
                commandService.AddCommand(CreateMenuItem(UserShortcutsMenu, null));
                // Add an entry for the dyanmic/expandable menu item for Load User Shortcuts
                commandService.AddCommand(new DynamicItemMenuCommand(new CommandID(VSShortcutsManagerCmdSetGuid, DynamicUserShortcutsStartCmdId),
                    IsValidUserShortcutsItem,
                    ExecuteUserShortcutsCommand,
                    OnBeforeQueryStatusUserShortcutsDynamicItem));

                // Mapping Scheme
                commandService.AddCommand(CreateMenuItem(ImportMappingSchemeCmdId, this.ImportMappingScheme));
                // Add a dummy entry for the mapping scheme menu (you can't execute a "menu")
                commandService.AddCommand(CreateMenuItem(ShortcutSchemesMenu, null));
                // Add an entry for the dyanmic/expandable menu item for mapping schemes
                CommandID dynamicItemRootId = new CommandID(VSShortcutsManagerCmdSetGuid, DynamicThemeStartCmdId);
                commandService.AddCommand(new DynamicItemMenuCommand(dynamicItemRootId,
                    IsValidMappingSchemeItem,
                    ExecuteMappingSchemeCommand,
                    OnBeforeQueryStatusMappingSchemeDynamicItem));

                // Command Shortcuts Tool Window
                commandService.AddCommand(CreateMenuItem(CommandShortcutsToolWinCmdId, this.OpenCommandShortcutsToolWin));

                // Add New Keyboard Shortcut
                commandService.AddCommand(CreateMenuItem(AddNewShortcutCmdId, this.OpenAddKeyboardShortcutDialog));
                // Open Live Shortcuts View
                commandService.AddCommand(CreateMenuItem(LiveShortcutsViewCmdId, this.OpenLiveShortcutsView));
                // Refresh Command Shortcuts View
                commandService.AddCommand(CreateMenuItem(RefreshCommandShortcutsViewCmdId, this.RefreshViewEventHandler));

                // Switch to "All" shortcuts filter
                commandService.AddCommand(CreateMenuItem(FilterAllShortcutsCmdId, this.FilterAllShortcutsEventHandler));
                // Switch to "Popular" shortcuts filter
                commandService.AddCommand(CreateMenuItem(FilterPopularShortcutsCmdId, this.FilterPopularShortcutsEventHandler));
                // Switch to "Mine" shortcuts filter
                commandService.AddCommand(CreateMenuItem(FilterUserShortcutsCmdId, this.FilterUserShortcutsEventHandler));

            }
        }

        private OleMenuCommand CreateMenuItem(int cmdId, EventHandler menuItemCallback)
        {
            return new OleMenuCommand(menuItemCallback, new CommandID(VSShortcutsManagerCmdSetGuid, cmdId));
        }

        //----------------  Command entry points -------------

        private void FilterAllShortcutsEventHandler(object sender, EventArgs e)
        {
            //MessageBox.Show("Filter: All shortcuts");
            ApplyAllShortcutsFilter();
        }

        private void FilterPopularShortcutsEventHandler(object sender, EventArgs e)
        {
            //MessageBox.Show("Filter: Popular shortcuts");
            ApplyPopularShortcutsFilter();
        }

        private void FilterUserShortcutsEventHandler(object sender, EventArgs e)
        {
            //MessageBox.Show("Filter: My shortcuts");
            ApplyUserShortcutsFilter();
        }

        private void ApplyAllShortcutsFilter()
        {
            CommandShortcutsControlDataContext cmdShortcutsDataContext = GetCommandShortcutsDataContext();
            cmdShortcutsDataContext.ApplyAllShortcutsFilter();
        }

        private void ApplyPopularShortcutsFilter()
        {
            CommandShortcutsControlDataContext cmdShortcutsDataContext = GetCommandShortcutsDataContext();
            cmdShortcutsDataContext.ApplyPopularShortcutsFilter();
        }

        private void ApplyUserShortcutsFilter()
        {
            // Fetch the list of user shortcuts - always fetches a fresh copy by exporting current settings
            List<VSShortcut> userShortcuts = GetUserShortcuts();

            CommandShortcutsControlDataContext cmdShortcutsDataContext = GetCommandShortcutsDataContext();
            cmdShortcutsDataContext.ApplyUserShortcutsFilter(userShortcuts);
        }

        private void BackupShortcuts(object sender, EventArgs e)
        {
            ExecuteSaveShortcuts();
        }

        private void ImportShortcuts(object sender, EventArgs e)
        {
            ExecuteImportShortcuts();
        }

        private void ResetShortcuts(object sender, EventArgs e)
        {
            // Confirm Reset operation
            if (MessageBox.Show("Reset keyboard shortcuts to default settings?", MSG_CAPTION_RESET, MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                return;
            }

            ResetShortcutsViaProfileManager();
            // Tools.ImportandExportSettings [/export:filename | /import:filename | /reset]   //https://msdn.microsoft.com/en-us/library/ms241277.aspx
        }

        private void ImportMappingScheme(object sender, EventArgs e)
        {
            ExecuteImportMappingScheme();
        }

        private void ScanUserShortcuts(object sender, EventArgs e)
        {
            bool foundShortcuts = ShortcutsScanner.Instance.ScanForAllExtensionShortcuts();
            if (!foundShortcuts)
            {
                MessageBox.Show("Scan complete.\n\nNo new shortcut definitions were found in the extensions directories.");
            }
        }

        private void OnBeforeQueryStatusClearUserShortcuts(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            bool hasShortcutDefs = userShortcutsManager.GetUserShortcutsRegistry().Count > 0;
            command.Visible = hasShortcutDefs;
            command.Enabled = hasShortcutDefs;
        }

        private void ClearUserShortcuts(object sender, EventArgs e)
        {
            userShortcutsManager.ResetUserShortcutsRegistry();
            MessageBox.Show("User shortcuts list has been reset.");
        }

        //-------- Apply Shortcuts Filter --------

        private List<VSShortcut> GetUserShortcuts()
        {
            IVsProfileDataManager vsProfileDataManager = GetProfileDataManager();

            // Export current settings (temporary export file)

            // Generate unique path for saving current user shortcuts
            string tempExportPath = GetUniqueExportFilePath(vsProfileDataManager);

            // About to write a temp file to disc. Make sure we always attempt to delete it.
            try
            {
                // Export the current settings to a temporary file
                int result = ExportKeyboardSettingsToFile(vsProfileDataManager, tempExportPath);
                if (result != VSConstants.S_OK)
                {
                    // Something went wrong. Throw an exception, which can be swallowed for now.
                    MessageBox.Show($"Oops.... Something went wrong trying to export settings to the following file:\n\n{tempExportPath}", MSG_CAPTION_SAVE_SHORTCUTS);
                    throw new Exception("Could not export current user keyboard settings to temporary file.");
                }

                // Now parse the UserShortcuts section of the settings file that was just created.
                // First, wait for file to finish being written (or we get an access error)
                FileUtils.WaitForFileAccess(tempExportPath, 100, 3);
                XDocument vsSettingsXDoc = XDocument.Load(tempExportPath);
                List<VSShortcut> shortcutList = ParseVSSettingsFile(vsSettingsXDoc);

                return shortcutList;
            }
            finally
            {
                // Cleanup - Delete the temporary file.
                DeleteFileSafely(tempExportPath);
            }
        }

        private static int ExportKeyboardSettingsToFile(IVsProfileDataManager vsProfileDataManager, string tempExportPath)
        {
            return vsProfileDataManager.ExportSettings(tempExportPath, GetShortcutsSettingsTreeForExport(vsProfileDataManager), out IVsSettingsErrorInformation errorInfo);
        }

        //-------- Reset Shortcuts --------

        private bool ResetShortcutsViaProfileManager()
        {
            IVsProfileDataManager vsProfileDataManager = GetProfileDataManager();
            IVsProfileSettingsFileInfo profileSettingsFileInfo = GetDefaultProfileSettingsFileInfo(vsProfileDataManager);
            if (profileSettingsFileInfo == null)
            {
                MessageBox.Show("Unable to find Default Shortcuts file.");
                return false;
            }

            // Apply the Reset
            int result = vsProfileDataManager.ResetSettings(profileSettingsFileInfo, out IVsSettingsErrorInformation errorInfo);
            if (ErrorHandler.Failed(result))
            {
                // Something went wrong. TODO: Handle error.
                MessageBox.Show("Error occurred attempting to reset keyboard shortcuts.");
                return false;
            }

            MessageBox.Show("Success! Keyboard shortcuts have been reset.", MSG_CAPTION_RESET, MessageBoxButtons.OK);
            return true;
        }

        private IVsProfileSettingsFileInfo GetDefaultProfileSettingsFileInfo(IVsProfileDataManager manager)
        {
            const string DEFAULT_SHORTCUTS_FILENAME = "DefaultShortcuts.vssettings";
            string extensionDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string resetFilePath = Path.Combine(extensionDir, "Resources", DEFAULT_SHORTCUTS_FILENAME);
            return GetProfileSettingsFileInfo(resetFilePath);
        }

        public static void ResetSettingsViaPostExecCmd()
        {
            IVsUIShell shell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
            if (shell == null)
            {
                return;
            }

            var group = VSConstants.CMDSETID.StandardCommandSet2K_guid;
            object arguments = "-reset";
            // NOTE: Call to PostExecCommand could fail. Callers should consider catching the exception. Otherwise, UI will show the error in a messagebox.
            shell.PostExecCommand(ref group, (uint)VSConstants.VSStd2KCmdID.ManageUserSettings, 0, ref arguments);
            MessageBox.Show($"Keyboard shortcuts Reset", MSG_CAPTION_RESET);
        }

        //-------- Backup Shortcuts --------

        public void ExecuteSaveShortcuts()
        {
            // Confirm Save operation
            //if (MessageBox.Show("Save current keyboard shortcuts?", MSG_CAPTION_BACKUP, MessageBoxButtons.OKCancel) != DialogResult.OK)
            //{
            //    return;
            //}

            IVsProfileDataManager vsProfileDataManager = GetProfileDataManager();

            // Generate default path for saving shortcuts
            // e.g. c:\users\justcla\appdata\local\microsoft\visualstudio\15.0_cf83efb8exp\Settings\Exp\CurrentSettings-2017-12-27-1.vssettings
            string uniqueExportPath = GetUniqueExportFilePath(vsProfileDataManager);
            // Extract the components of the export path
            string userExportDir = Path.GetDirectoryName(uniqueExportPath);
            string uniqueFilename = Path.GetFileNameWithoutExtension(uniqueExportPath);
            string fileExtension = Path.GetExtension(uniqueExportPath);

            // Open UI to let user name the file
            string shortcutsName = uniqueFilename;
            if (!SaveShortcuts.GetShortcutsName(ref shortcutsName))
            {
                // Cancel or ESC pressed
                return;
            }

            // Construct the filename where the vssettings file will be saved
            string saveFilePath = Path.Combine(userExportDir, $"{shortcutsName}{fileExtension}");

            // Check if file already exists
            if (File.Exists(saveFilePath))
            {
                // Prompt to overwrite
                if (MessageBox.Show($"The settings file already exists {saveFilePath}\n\nDo you want to replace this file?", MSG_CAPTION_SAVE_SHORTCUTS, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    // Duplicate file. User does not want to overwrite. Exit out.
                    // TODO: Consider returning the user to the file name dialog.
                    return;
                }
            }

            // Check if name already used
            if (userShortcutsManager.HasUserShortcuts(shortcutsName))
            {
                // Prompt to overwrite
                if (MessageBox.Show($"Settings already exist with the name: {saveFilePath}\n\nDo you want to replace these settings?", MSG_CAPTION_SAVE_SHORTCUTS, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    // Duplicate file. User does not want to overwrite. Exit out.
                    // TODO: Consider returning the user to the file name dialog.
                    return;
                }
            }

            // Do the export
            IVsProfileSettingsTree keyboardOnlyExportSettings = GetShortcutsSettingsTreeForExport(vsProfileDataManager);
            int result = vsProfileDataManager.ExportSettings(saveFilePath, keyboardOnlyExportSettings, out IVsSettingsErrorInformation errorInfo);
            if (result != VSConstants.S_OK)
            {
                // Something went wrong. TODO: Handle error.
                MessageBox.Show($"Oops.... Something went wrong trying to export settings to the following file:\n\n{saveFilePath}", MSG_CAPTION_SAVE_SHORTCUTS);
                return;
            }

            // Save Backup file path to SettingsManager and to UserShortcutsRegistry
            SaveBackupFilePath(saveFilePath);
            AddUserShortcutsFileToRegistry(saveFilePath);

            // Report success
            string Text = $"Your keyboard shortcuts have been saved to the following file:\n\n{saveFilePath}";
            MessageBox.Show(Text, MSG_CAPTION_SAVE_SHORTCUTS, MessageBoxButtons.OK);
        }

        private IVsProfileDataManager GetProfileDataManager()
        {
            return (IVsProfileDataManager)ServiceProvider.GetService(typeof(SVsProfileDataManager));
        }

        private static string GetUniqueExportFilePath(IVsProfileDataManager vsProfileDataManager)
        {
            vsProfileDataManager.GetUniqueExportFileName((uint)__VSPROFILEGETFILENAME.PGFN_SAVECURRENT, out string exportFilePath);
            return exportFilePath;
        }

        private static IVsProfileSettingsTree GetShortcutsSettingsTreeForExport(IVsProfileDataManager vsProfileDataManager)
        {
            vsProfileDataManager.GetSettingsForExport(out IVsProfileSettingsTree profileSettingsTree);
            EnableKeyboardOnlyInProfileSettingsTree(profileSettingsTree);
            return profileSettingsTree;
        }

        private static void EnableKeyboardOnlyInProfileSettingsTree(IVsProfileSettingsTree profileSettingsTree)
        {
            // Disable all settings
            profileSettingsTree.SetEnabled(0, 1);
            // Enable Keyboard settings
            profileSettingsTree.FindChildTree("Environment_Group\\Environment_KeyBindings", out IVsProfileSettingsTree keyboardSettingsTree);
            if (keyboardSettingsTree != null)
            {
                int enabledValue = 1;  // true
                int setChildren = 0;  // true  (redundant)
                keyboardSettingsTree.SetEnabled(enabledValue, setChildren);
            }
            return;
        }

        //------------ Load User Shortcuts --------------

        public void ExecuteImportShortcuts()
        {
            // Let user browse for a .vssettings file to import. Default to the last file saved.
            string chosenFile = BrowseForVsSettingsFile();
            if (chosenFile == null || !File.Exists(chosenFile))
            {
                // No file chosen. Possibly Cancel or ESC pressed.
                return;
            }
            if (!(Path.GetExtension(chosenFile).Equals(".vssettings", StringComparison.InvariantCultureIgnoreCase) ||
                  Path.GetExtension(chosenFile).Equals(".xml", StringComparison.InvariantCultureIgnoreCase)))
            {
                MessageBox.Show($"The chosen file is not a valid Visual Studio settings file.\n\nPlease chose a 'vssettings' file or 'xml' file.", MSG_CAPTION_IMPORT);
                return;
            }

            OpenImportShortcutsWindow(chosenFile);
        }

        public void OpenImportShortcutsWindow(string chosenFile)
        {
            // Parse the XML of the VSSettings file
            XDocument vsSettingsXDoc = XDocument.Load(chosenFile);
            List<VSShortcut> shortcutList = ParseVSSettingsFile(vsSettingsXDoc);
            // Add the conflicts to each shortcut
            ApplyShortcutConflicts(shortcutList);

            // Launch the Import Shortcuts window
            ImportShortcuts window = new ImportShortcuts(chosenFile, vsSettingsXDoc, shortcutList);
            window.ShowModal();
        }

        private List<VSShortcut> ParseVSSettingsFile(XDocument vsSettingsXDoc)
        {
            // Initialize the list of parsed shortcuts to return
            var shortcutList = new List<VSShortcut>();

            // Grab the only child in the "UserShortcuts" node
            var userShortcuts = vsSettingsXDoc.Descendants("UserShortcuts");
            foreach (var userShortcut in userShortcuts)
            {
                // Parse each Shortcut entry
                foreach (var shortcut in userShortcut.Descendants())
                {
                    // Convert the XML of one row of Shortcut entry into a parsed VSShortcut object
                    VSShortcut parsedShortcutItem = ParseShortcutElement(shortcut);
                    if (parsedShortcutItem == null)
                    {
                        Debug.WriteLine("Skipping entry: " + shortcut.Value);
                        continue;
                    }

                    // Add successfully parsed VSShortcut to the list
                    shortcutList.Add(parsedShortcutItem);
                }
            }
            return shortcutList;
        }

        /// <summary>
        /// Parse the XML elemenent into a valid VSShortcuts object.
        /// 
        /// Note: Deliberately NOT parsing the Command text. We will display the command text
        /// unparsed and allow it to fail at the time of import. Not all commands will always
        /// be available on a user's instance of VS. But it's good to show that the VSSettings
        /// file did include the shortcut for it.
        /// Also, not parsing the scope text. Seems pointless.
        /// </summary>
        private VSShortcut ParseShortcutElement(XElement shortcut)
        {
            // Read values from XML definitions
            string operationType = shortcut.Name.LocalName;
            string scopeText = shortcut.Attribute("Scope").Value;
            string commandText = shortcut.Attribute("Command").Value;
            string shortcutText = shortcut.Value;

            // Parse the Operation type (Add or Remove shortcut)
            if (!ShortcutOperations.ContainsKey(operationType))
            {
                Debug.WriteLine("Ignoring UserShortcut element: " + operationType + " with value: " + shortcut.Value);
                return null;
            }
            string operationDisplayName = ShortcutOperations[operationType];  // ie. convert to "Add" or "Remove"

            return new VSShortcut
            {
                Operation = operationDisplayName,
                Command = commandText,
                Scope = scopeText,
                Shortcut = shortcutText,
            };
        }

        private void ApplyShortcutConflicts(List<VSShortcut> shortcutList)
        {
            foreach (VSShortcut shortcut in shortcutList)
            {
                // Parse the shortcut key combinations
                string shortcutText = shortcut.Shortcut;
                IEnumerable<BindingSequence> sequences = queryEngine.GetBindingSequencesFromBindingString(shortcutText);
                if (sequences == null)
                {
                    Debug.WriteLine("Unable to parse shortcutText: " + shortcutText);
                    continue;
                }

                // Prepare the conflict list
                List<string> conflictList = GetConflictListText(shortcut.Scope, sequences);

                shortcut.Conflicts = conflictList;
            }
        }

        public bool PerformImportUserShortcuts(string chosenFile, XDocument vsSettingsXDoc, ImportShortcuts window)
        {
            // Create a temporary file to import, without shortcuts that the user unchecked
            List<VSShortcut> listOfShortcutsToRemove = window.GetUncheckedShortcuts();
            var tempImportFile = CreateSettingsFile(vsSettingsXDoc, listOfShortcutsToRemove);

            try
            {
                // Perform the import
                if (!LoadKeyboardShortcutsFromVSSettingsFile(tempImportFile)) return false;

                // Save the file in the "Load" list so it can be imported again.
                AddUserShortcutsFileToRegistry(chosenFile);
                return true;
            }
            finally
            {
                // Cleanup - Delete the temporary file.
                DeleteFileSafely(tempImportFile);
            }
        }

        /// <summary>
        /// Creates a new .vssettings file that matches the input vsSettingsXDoc without the items in the removeList
        /// </summary>
        /// <returns>string name of the file that was created</returns>
        private string CreateSettingsFile(XDocument vsSettingsXDoc, List<VSShortcut> removeList)
        {
            // Remove specific elements from the XML Doc, then save XML Doc to a temp file.
            // Note: It can only remove elements that appeared in the UI and were unchecked.
            // By default, everything else in the VSSettings file will go through untouched.
            // All this method does is carefully pluck/remove the XML elements that were
            // UNCHECKED by the user in the Import Shortcuts screen.

            // List of pointers to XML elements in the original parsed import file XDocument
            List<XElement> shortcutElementsToDelete = new List<XElement>();

            // Identify the XML elements of the shortcuts that should be removed.
            foreach (XElement userShortcut in vsSettingsXDoc.Descendants("UserShortcuts"))
            {
                foreach (XElement shortcutElement in userShortcut.Descendants())
                {
                    // Add or Remove shortcut?
                    string elementName = shortcutElement.Name.LocalName;  // ie. "Shortcut" or "RemoveShortcut"
                    string operationName = ShortcutOperations[elementName];  // ie. convert to "Add" or "Remove"
                    if (operationName == null) continue;

                    // Do we need to remove this operation shortcut from the import list?
                    VSShortcut shortcutDef = new VSShortcut
                    {
                        Operation = operationName,
                        Command = shortcutElement.Attribute("Command").Value,
                        Scope = shortcutElement.Attribute("Scope").Value,
                        Shortcut = shortcutElement.Value
                    };
                    // TODO: Check the time complexity of this algorithm.
                    if (removeList.Contains(shortcutDef))
                    {
                        shortcutElementsToDelete.Add(shortcutElement);
                    }
                }
            }

            // Now extract the XML elements of the shortcuts to be removed
            foreach (XElement shortcutElement in shortcutElementsToDelete)
            {
                shortcutElement.Remove();
            }

            // Create a new VSSettings file without the removed shortcuts
            //string saveFilePath = WriteToUniqueFilePath(vsSettingsXDoc);

            // Get a unique filename for the temporary file
            IVsProfileDataManager vsProfileDataManager = GetProfileDataManager();
            string saveFilePath = GetUniqueExportFilePath(vsProfileDataManager);

            // Write the XmlDoc to file using the new unique filename
            vsSettingsXDoc.Save(saveFilePath);

            return saveFilePath;
        }

        private static void DeleteFileSafely(string tempImportFile)
        {
            try
            {
                File.Delete(tempImportFile);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to delete temporary import settings file: {0}", e.ToString());
            }
        }

        private List<string> GetConflictListText(string scopeName, IEnumerable<BindingSequence> sequences)
        {
            if (scopeName == null)
            {
                List<string> list = new List<string>();
                list.Add("! Scope not valid");
                return list;
            }

            // Get all conflicts for the given Scope/Shortcut (as objects)
            IEnumerable<BindingConflict> conflicts = GetAllConflictObjects(scopeName, sequences);

            // Add the localized text for each conflict to a list and return it.
            return ConvertToLocalizedTextList(conflicts);
        }

        private IEnumerable<BindingConflict> GetAllConflictObjects(string scopeName, IEnumerable<BindingSequence> sequences)
        {
            IEnumerable<BindingConflict> conflicts = null;  // Must we really assign this to run to compile?
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                conflicts = await queryEngine.GetConflictsAsync(AllCommandsCache, scopeName, sequences);

            });
            return conflicts;
        }

        private static List<string> ConvertToLocalizedTextList(IEnumerable<BindingConflict> conflicts)
        {
            List<string> conflictTextList = new List<string>();
            // Work through each binding conflict type (ie. Block, Blocked in, Replaces)
            foreach (BindingConflict conflictsOfType in conflicts)
            {
                // Add the text description of each binding to the conflictText list
                foreach (Tuple<CommandBinding, Command> binding in conflictsOfType.AffectedBindings)
                {
                    conflictTextList.Add(GetConflictText(binding));
                }
            }

            return conflictTextList;
        }

        private static string GetConflictText(Tuple<CommandBinding, Command> binding)
        {
            // Prepare conflict item text
            string scopeText = binding.Item1.Scope.Name;
            string localizedShortcutText = binding.Item1.OriginalDTEString;
            string commandNameText = binding.Item2.CanonicalName;

            // Combine into string: "[Text Editor] Edit.Duplicate (Ctrl+D)"
            return $"[{scopeText}] {commandNameText} ({localizedShortcutText})";
        }

        private string BrowseForVsSettingsFile()
        {
            const string vsSettingsFilter = "VS settings files (*.vssettings)|*.vssettings|XML files (*.xml)|*.xml|All files (*.*)|*.*";
            string lastSavedFile = GetSavedBackupFilePath();
            return FileUtils.BrowseForFile(vsSettingsFilter, initialFileOrFolder: lastSavedFile);
        }

        public bool LoadKeyboardShortcutsFromVSSettingsFile(string importFilePath)
        {
            if (!File.Exists(importFilePath))
            {
                MessageBox.Show($"File does not exist: {importFilePath}", MSG_CAPTION_IMPORT);
                return false;
            }

            // Handle the change in mapping scheme, if there is one.
            string preloadMappingScheme = GetMappingScheme();

            // Import the User Shortcuts from the .vssettings file
            IVsProfileSettingsTree importShortcutsSettingsTree = GetShortcutsSettingsTreeForImport(importFilePath);
            if (!ImportSettingsFromSettingsTree(importShortcutsSettingsTree)) return false;

            MessageBox.Show($"Keyboard shortcuts successfully imported!", MSG_CAPTION_IMPORT);

            // Now check if the mapping scheme changed. If so, alert the user and offer to revert it.
            RevertMappingSchemeIfRequired(preloadMappingScheme);

            return true;
        }


        private void RevertMappingSchemeIfRequired(string preloadMappingScheme)
        {
            // Check if the mapping scheme changed.If so, alert the user.
            string postLoadMappingScheme = GetMappingScheme();
            if (postLoadMappingScheme != preloadMappingScheme)
            {
                // Alert the user that the mapping scheme changed. Ask if they want to revert it.
                DialogResult revertMappingScheme = MessageBox.Show($"Importing the user shortcuts caused the mapping scheme to changed.\n" +
                    $"It used to be: {preloadMappingScheme}\n" +
                    $"but now it is: {postLoadMappingScheme}\n\n" +
                    $"Revert to previous mapping scheme?", "Mapping scheme changed", MessageBoxButtons.YesNo);
                if (revertMappingScheme == DialogResult.Yes)
                {
                    // Switch back to the mapping scheme as it was before loading the user shortcuts.
                    SetMappingScheme(preloadMappingScheme);
                }
            }
        }

        private static IVsProfileSettingsTree GetShortcutsSettingsTreeForImport(string importFilePath)
        {
            IVsProfileSettingsFileInfo profileSettingsFileInfo = GetProfileSettingsFileInfo(importFilePath);
            profileSettingsFileInfo.GetSettingsForImport(out IVsProfileSettingsTree profileSettingsTree);
            EnableKeyboardOnlyInProfileSettingsTree(profileSettingsTree);

            return profileSettingsTree;
        }

        private static IVsProfileSettingsFileInfo GetProfileSettingsFileInfo(string importFilePath)
        {
            IVsProfileDataManager vsProfileDataManager = (IVsProfileDataManager)Package.GetGlobalService(typeof(SVsProfileDataManager));
            vsProfileDataManager.GetSettingsFiles(uint.MaxValue, out IVsProfileSettingsFileCollection vsProfileSettingsFileCollection);
            vsProfileSettingsFileCollection.AddBrowseFile(importFilePath, out IVsProfileSettingsFileInfo profileSettingsFileInfo);
            return profileSettingsFileInfo;
        }

        private static bool ImportSettingsFromSettingsTree(IVsProfileSettingsTree profileSettingsTree)
        {
            //EnableKeyboardOnlyInProfileSettingsTree(profileSettingsTree);
            IVsProfileDataManager vsProfileDataManager = (IVsProfileDataManager)Package.GetGlobalService(typeof(SVsProfileDataManager));
            int result = vsProfileDataManager.ImportSettings(profileSettingsTree, out IVsSettingsErrorInformation errorInfo);
            if (ErrorHandler.Failed(result))
            {
                // Something went wrong. TODO: Handle error.
                MessageBox.Show("Error occurred attempting to import settings.");
                return false;
            }
            return true;
        }

        private void AddUserShortcutsFileToRegistry(string importFilePath)
        {
            ShortcutFileInfo userShortcutsDef = new ShortcutFileInfo(importFilePath);
            userShortcutsManager.AddUserShortcutsDef(userShortcutsDef);
        }

        //---------- User Shortcuts ----------------

        private bool IsValidUserShortcutsItem(int commandId)
        {
            //It is a valid match if the command id is less than the total number of items the user has requested appear on our menu.
            int itemRange = (commandId - DynamicUserShortcutsStartCmdId);
            return itemRange >= 0 && itemRange < userShortcutsManager.GetUserShortcutsRegistry().Count;
        }

        private void OnBeforeQueryStatusUserShortcutsDynamicItem(object sender, EventArgs args)
        {
            List<ShortcutFileInfo> userShortcutsRegistry = userShortcutsManager.GetUserShortcutsRegistry();

            bool userShortcutsExist = userShortcutsRegistry.Count > 0;

            DynamicItemMenuCommand matchedCommand = (DynamicItemMenuCommand)sender;
            matchedCommand.Enabled = userShortcutsExist;
            matchedCommand.Visible = userShortcutsExist;

            if (userShortcutsExist)
            {
                //The root item in the expansion won't flow through IsValidDynamicItem as it will match against the actual DynamicItemMenuCommand based on the
                //'root' id given to that object on construction, only if that match fails will it try and call the dynamic id check, since it won't fail for
                //the root item we need to 'special case' it here as MatchedCommandId will be 0 in that case.
                bool isRootItem = (matchedCommand.MatchedCommandId == 0);
                int menuItemIndex = isRootItem ? 0 : (matchedCommand.MatchedCommandId - DynamicUserShortcutsStartCmdId);

                // Add an & to the front of the menu text so that the first letter becomes the accellerator key.
                ShortcutFileInfo shortcutFileInfo = userShortcutsRegistry[menuItemIndex];
                matchedCommand.Text = GetMenuTextWithAccelerator(shortcutFileInfo.DisplayName);

                //// Apply Tooltip text
                //System.Collections.IDictionary properties = matchedCommand.Properties;
                //properties.Add("filepath", shortcutFileInfo.Filepath);
            }

            //Clear this out here as we are done with it for this item.
            matchedCommand.MatchedCommandId = 0;
        }

        /// <summary>
        /// Handler for the Load User Shortcuts menu items.
        /// </summary>
        private void ExecuteUserShortcutsCommand(object sender, EventArgs args)
        {
            //// Get the name of shortcuts file from the invoked menu item (Dynamic menu - can't know at compile time)
            DynamicItemMenuCommand invokedCommand = (DynamicItemMenuCommand)sender;
            string shortcutDefName = invokedCommand.Text.Replace("&", "");  // Remove the & (keyboard accelerator) from the menu text

            //// Lookup the cache of known keyoard import files and get the full filepath
            //ShortcutFileInfo userShortcutsDef = userShortcutsManager.GetUserShortcutsInfo(shortcutDefName);
            //string importFilePath = userShortcutsDef.Filepath;

            // Get the FilePath from the ShortcutInfo - based on the index in the menu from the invoked menuCommand
            // First, find the order of the item in the menu
            int cmdId = invokedCommand.MatchedCommandId;    // This is the selected Item
            int rootMenuId = invokedCommand.CommandID.ID;   // This is the root of the DynamicRootCommand
            int menuItemIndex = cmdId - rootMenuId;     // Get the index in the menu
            // Second, find the matching item in the registry (based on the order)
            List<ShortcutFileInfo> userShortcutsRegistry = userShortcutsManager.GetUserShortcutsRegistry();
            ShortcutFileInfo shortcutFileInfo = userShortcutsRegistry[menuItemIndex];
            // Now pull out the file path
            string importFilePath = shortcutFileInfo.Filepath;

            // If file is not available on the drive, abort and offer to remove it from the list.
            if (!File.Exists(importFilePath))
            {
                if (MessageBox.Show($"File does not exist: {importFilePath}\nRemove from shortcuts registry?", MSG_CAPTION_IMPORT, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    userShortcutsManager.DeleteUserShortcutsDef(shortcutDefName);
                }
                return;
            }

            // Load the user shortcuts into the Import Shortcuts UI
            OpenImportShortcutsWindow(importFilePath);
        }

        //---------- Mapping Schemes ----------------

        private static void ExecuteImportMappingScheme()
        {
            // Open Browse dialog - search for VSK files.
            const string vskFileFilter = "VS keyboard files (*.vsk)|*.vsk|All files (*.*)|*.*";
            string fileToImport = FileUtils.BrowseForFile(vskFileFilter, VSPathUtils.GetVsInstallPath());
            if (!File.Exists(fileToImport))
            {
                // No file to copy. Abort operation.
                return;
            }

            // Check that it's a VSK file.
            if (!Path.GetExtension(fileToImport).Equals(".vsk", StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show($"The chosen file is not a valid Visual Studio keyboard mapping scheme. Please chose a VSK file.", MSG_CAPTION_IMPORT_VSK);
                return;
            }

            // Copy VSK file to IDE directory
            FileUtils.CopyVSKToIDEDir(fileToImport);
        }

        private bool IsValidMappingSchemeItem(int commandId)
        {
            //It is a valid match if the command id is less than the total number of items the user has requested appear on our menu.
            List<string> mappingSchemes = GetMappingSchemes();
            // Returning an extra one to account for the "(Default)" mapping scheme - hence <= rather than <)
            int itemRange = (commandId - (int)DynamicThemeStartCmdId);
            return itemRange >= 0 && itemRange <= mappingSchemes.Count;
        }

        private List<string> GetMappingSchemes()
        {
            if (MappingSchemes == null)
            {
                MappingSchemes = new List<string>();
                PopulateMappingSchemes();
            }
            return MappingSchemes;
        }

        private void PopulateMappingSchemes()
        {
            MappingSchemes.AddRange(FetchListOfMappingSchemes());
        }

        private List<string> FetchListOfMappingSchemes()
        {
            // PERFORMS FILE IO! We want to minimize how often this occurs, plus delay this call as long as possible.
            return Directory.EnumerateFiles(VSPathUtils.GetVsInstallPath(), "*.vsk").Select(fn => Path.GetFileNameWithoutExtension(fn)).ToList();
        }

        private string GetMappingSchemeName(int itemIndex)
        {
            if (itemIndex >= 0 && itemIndex < GetMappingSchemes().Count)
            {
                return GetMappingSchemes()[itemIndex];
            }
            else
            {
                // It's the "(Default)" mapping scheme
                return DEFAULT_MAPPING_SCHEME_NAME;
            }
        }

        private void ExecuteMappingSchemeCommand(object sender, EventArgs args)
        {
            DynamicItemMenuCommand invokedCommand = (DynamicItemMenuCommand)sender;
            ApplyMappingScheme(invokedCommand.Text.Replace("&", ""));
        }

        private void ApplyMappingScheme(string mappingSchemeName)
        {
            SetMappingScheme(mappingSchemeName);
            MessageBox.Show(string.Format("Mapping scheme changed to {0}", mappingSchemeName));
        }

        private bool IsSelected(string mappingSchemeName)
        {
            using (var vsKey = Registry.CurrentUser.OpenSubKey(VSPathUtils.GetRegistryRoot()))
            {
                if (vsKey != null)
                {
                    using (var keyboardKey = vsKey.OpenSubKey("Keyboard"))
                    {
                        if (keyboardKey != null)
                        {
                            var schemeName = keyboardKey.GetValue("SchemeName") as string;
                            if (string.IsNullOrEmpty(schemeName))
                            {
                                return mappingSchemeName == DEFAULT_MAPPING_SCHEME_NAME;
                            }

                            return string.Equals(mappingSchemeName + ".vsk", Path.GetFileName(schemeName), StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                }
            }
            return false;
        }

        private void SetMappingScheme(string mappingSchemeName)
        {
            DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            Properties props = dte.Properties["Environment", "Keyboard"];
            Property prop = props.Item("SchemeName");
            prop.Value = mappingSchemeName == DEFAULT_MAPPING_SCHEME_NAME ? "" : mappingSchemeName + ".vsk";
        }

        private string GetMappingScheme()
        {
            DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            Properties props = dte.Properties["Environment", "Keyboard"];
            Property prop = props.Item("SchemeName");
            return (string)prop.Value;
        }

        private void OnBeforeQueryStatusMappingSchemeDynamicItem(object sender, EventArgs args)
        {
            DynamicItemMenuCommand matchedCommand = (DynamicItemMenuCommand)sender;

            matchedCommand.Enabled = true;
            matchedCommand.Visible = true;

            //The root item in the expansion won't flow through IsValidDynamicItem as it will match against the actual DynamicItemMenuCommand based on the
            //'root' id given to that object on construction, only if that match fails will it try and call the dynamic id check, since it won't fail for
            //the root item we need to 'special case' it here as MatchedCommandId will be 0 in that case.
            bool isRootItem = (matchedCommand.MatchedCommandId == 0);
            int menuItemIndex = isRootItem ? 0 : (matchedCommand.MatchedCommandId - DynamicThemeStartCmdId);

            string mappingSchemeName = GetMappingSchemeName(menuItemIndex);
            matchedCommand.Text = GetMenuTextWithAccelerator(mappingSchemeName);
            matchedCommand.Checked = IsSelected(mappingSchemeName);

            //Clear this out here as we are done with it for this item.
            matchedCommand.MatchedCommandId = 0;
        }

        private static string GetMenuTextWithAccelerator(string mappingSchemeName)
        {
            // Add an "&" to the front of the menu text so that the first letter becomes the accelerator key.
            return $"&{mappingSchemeName}";
        }

        //---------- Helper methods -------------------

        private static void SaveBackupFilePath(string exportFilePath)
        {
            try
            {
                VSShortcutsManagerPackage.SettingsManager.SetValueAsync(BACKUP_FILE_PATH, exportFilePath, isMachineLocal: true);
            }
            catch (Exception e)
            {
                // Unable to save backup file location. TODO: Handle error.
            }
        }

        private string GetSavedBackupFilePath()
        {
            VSShortcutsManagerPackage.SettingsManager.TryGetValue(BACKUP_FILE_PATH, out string backupFilePath);
            return backupFilePath;
        }

        // --------- Windows --------

        /// <summary>
        /// Command Shortcuts Tool Window
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void OpenCommandShortcutsToolWin(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(CommandShortcutsToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void OpenLiveShortcutsView(object sender, EventArgs e)
        {
            LiveShortcutsView dialog = new LiveShortcutsView(ServiceProvider);
            dialog.ShowDialog();
        }

        private void RefreshViewEventHandler(object sender, EventArgs e)
        {
            RefreshCommandShortcutsView();
        }

        private void RefreshCommandShortcutsView()
        {
            CommandShortcutsControlDataContext cmdShortcutsDataContext = GetCommandShortcutsDataContext();
            cmdShortcutsDataContext.RefreshView();
        }

        private CommandShortcutsControlDataContext GetCommandShortcutsDataContext()
        {
            // Get a handle on the CommandShortcuts Toolwindow
            ThreadHelper.ThrowIfNotOnUIThread();

            CommandShortcutsToolWindow window = (CommandShortcutsToolWindow)this.package.FindToolWindow(typeof(CommandShortcutsToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot find CommandShortcuts tool window");
            }

            CommandShortcutsControl cmdShortcutsControl = (CommandShortcutsControl)window.Content;
            CommandShortcutsControlDataContext cmdShortcutsDataContext = (CommandShortcutsControlDataContext)cmdShortcutsControl.DataContext;
            return cmdShortcutsDataContext;
        }

        private void OpenAddKeyboardShortcutDialog(object sender, EventArgs e)
        {
            AddNewShortcut.AddKeyboardShortcut addShortcutWindow = new AddNewShortcut.AddKeyboardShortcut(ServiceProvider);
            addShortcutWindow.ShowDialog();
        }

    }
}