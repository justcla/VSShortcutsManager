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

namespace AddKeyboardShortcut
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new CommandListViewModel();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        private void btnAddShortcut_Click(object sender, RoutedEventArgs e)
        {
            string command = txtCommand.Text;
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
            string command = txtCommand.Text;
            string scope = cmbScopeList.SelectedValue.ToString();
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

    internal class AddShortcutViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private string _command;
        public string Command
        {
            get
            {
                return _command;
            }
            set
            {
                _command = value;
                
                OnPropertyChanged("Command");
            }
        }
        private string _scope;
        public string Scope
        {
            get
            {
                return _scope;
            }
            set
            {
                _scope = value;
                OnPropertyChanged("Scope");
            }
        }
        private string _shortcut;
        public string Shortcut
        {
            get
            {
                return _shortcut;
            }
            set
            {
                _shortcut = value;
                OnPropertyChanged("Shortcut");
            }
        }
        private string _conflicts;
        public string Conflicts
        {
            get
            {
                return _conflicts;
            }
            set
            {
                _conflicts = value;
                OnPropertyChanged("Shortcut");
            }
        }

        
        public AddShortcutViewModel()
        {
           
        }
        #region INotifyPropertyChanged  
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
                if (PropertyChanged != null)
                PropertyChanged(this,
                    new System.ComponentModel.PropertyChangedEventArgs(propertyName));

        }

        #endregion
    }
}
