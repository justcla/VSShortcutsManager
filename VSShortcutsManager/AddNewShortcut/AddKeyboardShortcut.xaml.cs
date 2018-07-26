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
                string shortcutcommand = cmbCommandList.SelectedValue?.ToString();
                string shortcutScope = cmbScopeList.SelectedValue?.ToString();
                string shortcutKeys = txtShortcut.Text;
                string shortcutBinding = shortcutScope + "::" + shortcutKeys;
                if (!string.IsNullOrEmpty(shortcutcommand) && !string.IsNullOrEmpty(shortcutScope) && !string.IsNullOrEmpty(shortcutKeys))
                {
                    List<VSShortcut> conflictList = getConflictsText(shortcutcommand, shortcutScope, shortcutKeys);
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

        private List<VSShortcut> getConflictsText(string shortcutcommand, string shortcutScope, string shortcutKeys)
        {
            var conflictList = new List<VSShortcut>();
            var scope = _queryEngine.GetScopeByName(shortcutScope);
            var sequences = _queryEngine.GetBindingSequencesFromBindingString(shortcutKeys);
            var conflictTexts = new List<string>();
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var conflicts = await _queryEngine.GetConflictsAsync(scope, sequences);
                foreach (var conflict in conflicts)
                {
                    foreach (var binding in conflict.AffectedBindings)
                    {
                        conflictList.Add(new VSShortcut { Command = binding.Item2.CanonicalName, Scope = binding.Item1.Scope.Name, Shortcut = binding.Item1.OriginalDTEString});
                    }
                }
            });
            return conflictList;

        }

        private void cmbCommandList_LostFocus(object sender, RoutedEventArgs e) => fetchAndDisplayConflicts();
        private void txtShortcut_LostFocus(object sender, RoutedEventArgs e) => fetchAndDisplayConflicts();
    }
}
