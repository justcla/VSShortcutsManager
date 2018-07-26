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
        //Variable to identify if in Chord input
        private bool _isChordInput = false;
        public AddKeyboardShortcut(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            //To Replace this with All Commands.
            cmbCommandList.ItemsSource = new ScopeListViewModel(serviceProvider).DataSource;
            cmbScopeList.ItemsSource = new ScopeListViewModel(serviceProvider).DataSource;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        private void btnAddShortcut_Click(object sender, RoutedEventArgs e)
        {
            string command = "";//txtCommand.Text;
            string scope = cmbScopeList.SelectedValue.ToString();
            string shortcut = txtShortcut.Text;

            const string succcessMess = "Keyboard shortcut added successfully";
            MessageBox.Show(command + ":" + scope + ":" + shortcut + succcessMess, "Success");

        }

        private void txtCommand_TextChanged(object sender, TextChangedEventArgs e) => reloadConflicts();


        private void txtShortcut_TextChanged(object sender, TextChangedEventArgs e) => reloadConflicts();
        private void reloadConflicts()
        {
            //Temporary.. ToDo - Replace with actual functionality
            string command = "";// txtCommand.Text;
            string scope = "";//cmbScopeList.SelectedValue.ToString();
            string shortcut = txtShortcut.Text;

            string conflict = "";
            if (!String.IsNullOrEmpty(command))
            {
                conflict += "Command:" + command;
            }
            if (!String.IsNullOrEmpty(scope))
            {
                conflict += ";Scope:" + scope;
            }
            if (!String.IsNullOrEmpty(shortcut))
            {
                conflict += ";shortcut:" + shortcut;
            }
            txtConflicts.Text = conflict;
        }
       
    }
}
