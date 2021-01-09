using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSShortcutsManager
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("124118e3-7c75-490e-8ace-742c96f001da")]
    public class CommandShortcutsToolWindow : ToolWindowPane
    {
        public const string guidVSShortcutsManagerCmdSet = "cca0811b-addf-4d7b-9dd6-fdb412c44d8a";
        public const int CommandShortcutsToolWinToolbar = 0x2004;

        public static readonly Guid VSShortcutsManagerCmdSetGuid = new Guid("cca0811b-addf-4d7b-9dd6-fdb412c44d8a");
        public const int ShowTreeViewCmdId = 0x1815;
        public const int ShowListViewCmdId = 0x1825;

        private CommandTreeView.CommandShortcutsTree treeControl;
        public CommandTreeView.CommandShortcutsTree TreeControl
        {
            get
            {
                if (treeControl == null)
                {
                    treeControl = new CommandTreeView.CommandShortcutsTree();
                }
                return treeControl;
            }
            set => treeControl = value;
        }

        private VSShortcutQueryEngine QueryEngine;
        EnvDTE.Commands DTECommands;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandShortcutsToolWindow"/> class.
        /// </summary>
        public CommandShortcutsToolWindow() : base(null)
        {
            this.Caption = "Command Shortcuts";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new CommandShortcutsControl();
        }

        protected override void Initialize()
        {
            base.Initialize();

            this.QueryEngine = VSShortcutsManager.Instance.queryEngine;
            this.DTECommands = QueryEngine.DTECommands;

            this.ToolBar = new CommandID(new Guid(guidVSShortcutsManagerCmdSet), CommandShortcutsToolWinToolbar);

            ((CommandShortcutsControl)this.Content).DataContext = new CommandShortcutsControlDataContext(this);

            RegisterCommandHandlers();

            // Set the default initial opening layout and data
            ((CommandShortcutsControl)this.Content).Content = TreeControl;  // Lazy-loading tree control property

            // Get list of commands and shortcuts (from DTE?)
            // Convert to the correct objects for the CommandShortcutsTree
            IEnumerable commands = GetCommandsFromDTE();
            // Push the data onto the TreeControl
            TreeControl.Source = commands;
        }

        public IEnumerable<object> GetCommandsFromDTE()
        {
            IEnumerable<object> result = new ObservableCollection<object>();

            // Fetch all the commands from DTE
            foreach (EnvDTE.Command dteCommand in DTECommands)
            {
                // Check for a valid command name
                string commandName = dteCommand.Name;
                if (string.IsNullOrWhiteSpace(commandName))
                {
                    continue;
                }

                // Create the CommandItem for this command
                var commandItem = new CommandTreeView.CommandItem();
                commandItem.CommandName = commandName;

                // Parse the bindings (if there are any bound to the command)
                // Note: Binding is a combination of scope and key-combo
                if (dteCommand.Bindings != null && dteCommand.Bindings is object[] bindingsObj && bindingsObj.Length > 0)
                {
                    // Build a map of [Scope => (List of shortcuts)]
                    commandItem.ShortcutGroup = GetShortcutMap(bindingsObj);
                }

                // Handle the Group name(s)
                string[] commandNameParts = commandName.Split('.');
                CommandTreeView.CommandGroup groupParent = null;
                // Handle case where there is no group (no prefix before '.')
                if (commandNameParts.Length < 2)
                {
                    // No group element to this name. Add it to "Ungrouped" group.
                    groupParent = GetCommandGroup("Ungrouped", (Collection<object>)result);
                }
                else
                {
                    // Loop over the group parts  (not the last part - that's the command name)
                    for (int i = 0; i < commandNameParts.Length-1; i++)
                    {
                        // Find the group part in the current groupParent's list of groups
                        string groupName = commandNameParts[i];

                        // Top level group. Find it in the results object
                        // Other groups: Find the item in the Items collection of the previous group
                        Collection<object> subGroups = (i == 0) ? (Collection<object>)result : groupParent.Items;

                        groupParent = GetCommandGroup(groupName, subGroups);
                    }
                }

                // groupParent is now the parent group the command item should be added to
                groupParent.Items.Add(commandItem);
            }

            return result;
        }

        private static CommandTreeView.CommandGroup GetCommandGroup(string groupName, Collection<object> groups)
        {
            CommandTreeView.CommandGroup groupParent;
            var thisGroup = groups?.SingleOrDefault(item => item is CommandTreeView.CommandGroup groupItem && groupItem.GroupName.Equals(groupName));
            // Create the CommandGroup if it doesn't exist
            if (thisGroup == null)
            {
                thisGroup = new CommandTreeView.CommandGroup { GroupName = groupName };
                groups.Add(thisGroup);
            }
            // store this item for the next round
            groupParent = (CommandTreeView.CommandGroup)thisGroup;
            return groupParent;
        }

        private Dictionary<string, List<string>> GetShortcutMap(object[] bindingsObj)
        {
            var shortcutGroup = new Dictionary<string, List<string>>();

            // Process each binding string (Scope and keyCombo)
            foreach (object bindingObj in bindingsObj)
            {
                string bindingString = (string)bindingObj;

                // bindingString looks like: "Text Editor::Ctrl+R,Ctrl+M"  (Scope::Shortcut)
                const string separator = "::";
                if (bindingString.Contains("::"))
                {
                    string scopeName = bindingString.Substring(0, bindingString.IndexOf(separator));
                    string keySequence = bindingString.Substring(bindingString.IndexOf(separator) + separator.Length);

                    // Fetch the list of shortcuts for the given scope (may not exist)
                    bool success = shortcutGroup.TryGetValue(scopeName, out List<string> shortcutKeys);
                    if (!success)
                    {
                        shortcutKeys = new List<string>();
                    }
                    shortcutKeys.Add(keySequence);

                    // Update the map with the new shortcut keys (create or update)
                    shortcutGroup[scopeName] = shortcutKeys;
                }
            }

            return shortcutGroup;
        }

        private void RegisterCommandHandlers()
        {
            //IVsActivityLog log = Package.GetGlobalService(typeof(IMenuCommandService)) as IVsActivityLog;
            //if (log == null) return;
            if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                // Switch to Tree View
                commandService.AddCommand(CreateMenuItem(ShowTreeViewCmdId, this.ShowTreeViewEventHandler));
                // Switch to List View
                commandService.AddCommand(CreateMenuItem(ShowListViewCmdId, this.ShowListViewEventHandler));
            }
        }

        private void ShowTreeViewEventHandler(object sender, EventArgs e)
        {
            ((CommandShortcutsControl)Content).contentControl.Content = TreeControl;
        }

        private void ShowListViewEventHandler(object sender, EventArgs e)
        {
            ((CommandShortcutsControl)Content).contentControl.Content = new CommandShortcutsList();
        }

        private OleMenuCommand CreateMenuItem(int cmdId, EventHandler menuItemCallback)
        {
            return new OleMenuCommand(menuItemCallback, new CommandID(VSShortcutsManagerCmdSetGuid, cmdId));
        }

        private CommandShortcutsControlDataContext GetDataContext()
        {
            var cmdShortcutsControl = (CommandShortcutsControl)Content;
            var cmdShortcutsDataContext = (CommandShortcutsControlDataContext)cmdShortcutsControl.DataContext;
            return cmdShortcutsDataContext;
        }


        #region Search

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            if (pSearchQuery == null || pSearchCallback == null)
            {
                return null;
            }

            return new CommandShortcutsSearchTask(dwCookie, pSearchQuery, pSearchCallback, this);
        }

        public override void ClearSearch()
        {
            var control = (CommandShortcutsControl)this.Content;
            var controlDataContext = (CommandShortcutsControlDataContext)control.DataContext;
            controlDataContext.ClearSearch();
        }

        private IVsEnumWindowSearchOptions m_optionsEnum;
        public override IVsEnumWindowSearchOptions SearchOptionsEnum
        {
            get
            {
                if (m_optionsEnum == null)
                {
                    List<IVsWindowSearchOption> list = new List<IVsWindowSearchOption>();

                    list.Add(this.MatchCaseOption);

                    m_optionsEnum = new WindowSearchOptionEnumerator(list) as IVsEnumWindowSearchOptions;
                }

                return m_optionsEnum;
            }
        }

        private WindowSearchBooleanOption m_matchCaseOption;
        public WindowSearchBooleanOption MatchCaseOption
        {
            get
            {
                if (m_matchCaseOption == null)
                {
                    m_matchCaseOption = new WindowSearchBooleanOption("Match case", "Match case", false);
                }

                return m_matchCaseOption;
            }
        }

        public override bool SearchEnabled => true;

        #endregion // Search
    }
}
