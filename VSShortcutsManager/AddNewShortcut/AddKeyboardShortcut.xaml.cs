using Microsoft.VisualStudio.Shell;
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
        private VSShortcutQueryEngine _queryEngine;
        public AddKeyboardShortcut(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _queryEngine = new VSShortcutQueryEngine(_serviceProvider);
            cmbCommandList.ItemsSource = new CommandListViewModel(_serviceProvider).DataSource;
            cmbScopeList.ItemsSource = new ScopeListViewModel(_serviceProvider).DataSource;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        private void btnAddShortcut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string shortcutcommand = cmbCommandList.SelectedValue.ToString();
                string shortcutScope = cmbScopeList.SelectedValue.ToString();
                string shortcutKeys = txtShortcut.Text;
                string shortcutBinding = shortcutScope + "::" + shortcutKeys;
                //Check if potential conflicts available
                if (listConflicts.Items.Count > 0)
                {
                    if (seekConfirm(shortcutScope, shortcutKeys) == MessageBoxResult.Yes)
                    {
                        _queryEngine.BindShortcut(shortcutcommand, shortcutBinding);
                        MessageBox.Show("Shortcut added succesfully");
                    }
                }
                else
                {
                    _queryEngine.BindShortcut(shortcutcommand, shortcutBinding);
                    MessageBox.Show("Shortcut added succesfully");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private MessageBoxResult seekConfirm (string shortcutScope, string shortcutKeys)
        {
            var conflictList = listConflicts.Items;
            string messageBoxText = "";
            switch(shortcutScope.ToLower())
            {
                case "global":
                    messageBoxText = "This 'Global' shortcut will overide in all local scope. Proceed?";
                    break;
                default:
                    messageBoxText = "Please confirm to add this shortcut";
                    break;
            }
            const string messageBoxTittle = "Confirm Add Shortcut";
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(messageBoxText, messageBoxTittle, System.Windows.MessageBoxButton.YesNo);
            return messageBoxResult;
        }
        private void fetchAndDisplayConflicts()
        {
            try
            {
                // Pull the values from the UI (Command, Scope, Shortcut)
                string shortcutcommand = cmbCommandList.SelectedValue?.ToString();
                string shortcutScope = cmbScopeList.SelectedValue?.ToString();
                string shortcutKeys = txtShortcut.Text;

                string shortcutBinding = shortcutScope + "::" + shortcutKeys;

                if (!string.IsNullOrEmpty(shortcutcommand) && !string.IsNullOrEmpty(shortcutScope) && !string.IsNullOrEmpty(shortcutKeys))
                {
                    // Get all the conflict data into a list ready for display
                    List<ConflictTableData> conflictList = PrepareConflictsTableData(shortcutcommand, shortcutScope, shortcutKeys);

                    // Bind the data to the view model
                    listConflicts.ItemsSource = conflictList;
                    btnAddShortcut.IsEnabled = true;
                }
                else
                {
                    btnAddShortcut.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<ConflictTableData> PrepareConflictsTableData(string shortcutcommand, string shortcutScopeText, string shortcutKeysText)
        {
            // Convert text like "Text Editor" and "Ctrl+R, Ctrl+O" into objects representing the Scope and key bindings
            KeybindingScope scope = _queryEngine.GetScopeByName(shortcutScopeText);
            IEnumerable<BindingSequence> shortcutChords = _queryEngine.GetBindingSequencesFromBindingString(shortcutKeysText);

            List<ConflictTableData> conflictList = new List<ConflictTableData>();
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                // Fetch all the conflict data for the give nScope/Shortcut combination
                IEnumerable<BindingConflict> conflicts = await _queryEngine.GetConflictsAsync(scope, shortcutChords);

                // Put each conflict into the backing object to use on the UI display
                foreach (BindingConflict conflict in conflicts)
                {
                    // Handle all the conflict for this conflict type
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

        private void cmbCommandList_LostFocus(object sender, RoutedEventArgs e) => fetchAndDisplayConflicts();
        private void txtShortcut_LostFocus(object sender, RoutedEventArgs e) => fetchAndDisplayConflicts();
    }
}
