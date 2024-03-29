﻿using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VSShortcutsManager.AddNewShortcut
{
    /// <summary>
    /// Interaction logic for AddKeyboardShortcut.xaml
    /// </summary>
    public partial class AddKeyboardShortcut : Window
    {

        private IServiceProvider _serviceProvider;
        private VSShortcutQueryEngine ShortcutQueryEngine { get; set; }

        private IEnumerable<string> _commandNames;
        public IEnumerable<string> CommandNames { get {
                if (_commandNames == null)
                {
                    _commandNames = ExtractCommandNames(AllCommandsCache);
                }
                return _commandNames;
            }
            private set { }
        }

        private IEnumerable<string> ExtractCommandNames(IEnumerable<Command> allCommands)
        {
            var commandNamesList = new List<string>();
            foreach(Command command in allCommands)
            {
                commandNamesList.Add(command.CanonicalName);
            }
            return commandNamesList;
        }

        //internal VSShortcutQueryEngine ShortcutQueryEngine { get; private set; }
        IEnumerable<Command> _allCommandsCache;

        public IEnumerable<Command> AllCommandsCache
        {
            get
            {
                if (_allCommandsCache == null)
                {
                    ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        _allCommandsCache = await ShortcutQueryEngine.GetAllCommandsAsync();
                    });
                }
                return _allCommandsCache;
            }
            private set { }
        }

        private IEnumerable<KeybindingScope> GetAllKeybindingScopes()
        {
            IEnumerable<KeybindingScope> result = null;
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                result = await ShortcutQueryEngine.GetAllBindingScopesAsync();
            });
            return result;
        }

        private void InitializeShortcutEngine(IServiceProvider serviceProvider)
        {
            if (ShortcutQueryEngine == null)
            {
                ShortcutQueryEngine = VSShortcutQueryEngine.GetInstance(serviceProvider);
            }
        }

        public AddKeyboardShortcut(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            InitializeShortcutEngine(_serviceProvider);

            InitializeUIFields();
        }

        private void InitializeUIFields()
        {
            // Initialize backing objects for combo boxes on the UI
            // Command list combo box
            cmbCommandList.ItemsSource = GetCommandNamesList();
            // Scope list combo box
            cmbScopeList.ItemsSource = GetScopeDisplayData();
            cmbScopeList.SelectedIndex = 0; // Make sure it defaults to a value

            // Set focus on the Shortcut field
            txtShortcut.Focus();
        }

        private IEnumerable<string> GetCommandNamesList()
        {
            var displayData = new List<string>();
            foreach (Command eachCommand in AllCommandsCache)
            {
                // Filter out commands with no name
                string canonicalName = eachCommand.CanonicalName;
                if (string.IsNullOrEmpty(canonicalName))
                {
                    continue;
                }

                // Use the proper command name as the display name
                displayData.Add(canonicalName);
            }

            return displayData.OrderBy(o => o.ToString());
        }

        private IEnumerable<string> GetScopeDisplayData()
        {
            var displayData = new List<string>();

            // Special scopes at the top.
            string[] specialScopes =
            {
                "Text Editor",
                "Global"
            };
            displayData.AddRange(specialScopes);
            displayData.Add("-----------------------"); // This will need to be checked for later

            // Now add all the other scopes in the system
            foreach (KeybindingScope keybindingScope in GetAllKeybindingScopes())
            {
                // Filter out scopes with no name or already included (special scopes)
                string scopeName = keybindingScope.Name;
                if (scopeName == null || specialScopes.Contains(scopeName))
                {
                    continue;
                }

                displayData.Add(scopeName);
            }

            return displayData;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        private void btnAddShortcut_Click(object sender, RoutedEventArgs e)
        {
            string shortcutcommand = cmbCommandList.SelectedValue.ToString();
            string shortcutScope = cmbScopeList.SelectedValue.ToString();
            string shortcutKeys = txtShortcut.Text;

            // Validate form - Maybe unecessary, since validation occurs elsewhere
            if (string.IsNullOrEmpty(shortcutScope.Trim('-')))
            {
                MessageBox.Show("Please select a scope.");
                return;
            }

            // If conflicts exist, confirm the action with the user
            if (listConflicts.Items.Count > 0 && SeekConfirm(shortcutScope) != MessageBoxResult.Yes)
            {
                // User said no. Abort!
                return;
            }

            // Attempt to add the shortcut
            AddShortcutBinding(shortcutcommand, shortcutScope, shortcutKeys);
        }

        private MessageBoxResult SeekConfirm (string shortcutScope)
        {
            string messageBoxText;
            switch (shortcutScope.ToLower())
            {
                case "global":
                    messageBoxText = "This 'Global' shortcut will overide in all local scope. Proceed?";
                    break;
                default:
                    messageBoxText = "Please confirm to add this shortcut";
                    break;
            }
            const string messageBoxTittle = "Confirm Add Shortcut";
            MessageBoxResult messageBoxResult = MessageBox.Show(messageBoxText, messageBoxTittle, MessageBoxButton.YesNo);
            return messageBoxResult;
        }
        private void AddShortcutBinding(string shortcutcommand, string shortcutScope, string shortcutKeys)
        {
            try
            {
                string shortcutBinding = shortcutScope + "::" + shortcutKeys;
                ShortcutQueryEngine.BindShortcut(shortcutcommand, shortcutBinding);
                MessageBox.Show("Shortcut added succesfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FetchAndDisplayConflicts()
        {
            try
            {
                // Pull the values from the UI (Command, Scope, Shortcut)
                string shortcutScope = cmbScopeList.SelectedValue?.ToString().Trim('-'); // Remove any dashes
                string shortcutKeys = txtShortcut.Text;

                // Display conflicts if Scope and Shortcut fields have values
                if (!string.IsNullOrWhiteSpace(shortcutScope) && !string.IsNullOrWhiteSpace(shortcutKeys))
                {
                    // Get all the conflict data into a list ready for display
                    List<ConflictTableData> conflictList = PrepareConflictsTableData(shortcutScope, shortcutKeys);
                    // Bind the data to the view model
                    listConflicts.ItemsSource = conflictList;
                }

                UpdateAddShortcutEnabledState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<ConflictTableData> PrepareConflictsTableData(string shortcutScopeText, string shortcutKeysText)
        {
            // Convert text like "Text Editor" and "Ctrl+R, Ctrl+O" into objects representing the Scope and key bindings
            //KeybindingScope scope = ShortcutQueryEngine.GetScopeByName(shortcutScopeText);
            IEnumerable<BindingSequence> shortcutChords = ShortcutQueryEngine.GetBindingSequencesFromBindingString(shortcutKeysText);

            List<ConflictTableData> conflictList = new List<ConflictTableData>();
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                // Fetch all the conflict data for the give nScope/Shortcut combination
                IEnumerable<BindingConflict> conflicts = await ShortcutQueryEngine.GetConflictsAsync(AllCommandsCache, shortcutScopeText, shortcutChords);

                // Put each conflict into the backing object to use on the UI display
                foreach (BindingConflict conflict in conflicts)
                {
                    // Handle all the conflicts for this conflict type
                    ConflictType conflictType = conflict.Type;

                    foreach (Tuple<CommandBinding, Command> binding in conflict.AffectedBindings)
                    {
                        conflictList.Add(new ConflictTableData
                        {
                            ConflictType = GetConflictTypeText(conflictType),
                            Command = binding.Item2.CanonicalName,
                            Scope = binding.Item1.Scope.Name,
                            Shortcut = binding.Item1.OriginalDTEString
                        });
                    }
                }
            });
            return conflictList;

        }

        private void UpdateAddShortcutEnabledState()
        {
            // Enable the Add Shortcut button if there are values in all input fields.
            string shortcutScope = cmbScopeList.SelectedValue?.ToString().Trim('-'); // Remove any dashes
            string shortcutKeys = txtShortcut.Text;
            string commandName = cmbCommandList.SelectedValue?.ToString();

            btnAddShortcut.IsEnabled = !string.IsNullOrWhiteSpace(commandName)
                                    && !string.IsNullOrWhiteSpace(shortcutScope)
                                    && !string.IsNullOrWhiteSpace(shortcutKeys);
        }

        private static string GetConflictTypeText(ConflictType conflictType)
        {
            switch (conflictType)
            {
                case ConflictType.HiddenInSomeScopes:
                    return "Blocked in";
                case ConflictType.HidesGlobalBindings:
                    return "Blocks";
                case ConflictType.ReplacesBindings:
                default:
                    return "Replaces";
            }
        }

        class ConflictTableData
        {
            public string ConflictType { get; set; }
            public string Scope { get; set; }
            public string Command { get; set; }
            public string Shortcut { get; set; }

        }

        private void cmbCommandList_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateAddShortcutEnabledState();
        private void cmbScopeList_SelectionChanged(object sender, SelectionChangedEventArgs e) => FetchAndDisplayConflicts();
        private void cmbScopeList_LostFocus(object sender, RoutedEventArgs e) => FetchAndDisplayConflicts();
        private void txtShortcut_TextChanged(object sender, TextChangedEventArgs e) => FetchAndDisplayConflicts();
        private void txtShortcut_LostFocus(object sender, RoutedEventArgs e) => FetchAndDisplayConflicts();
    }
}
