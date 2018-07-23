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
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        private void btnAddShortcut_Click(object sender, RoutedEventArgs e)
        {
            string command = txtCommand.Text;
            string scope = txtScope.Text;
            string shortcut = txtShortcut.Text;

            const string succcessMess = "Keyboard shortcut added successfully";
            MessageBox.Show(command + ":" + scope + ":" + shortcut + succcessMess, "Success");
        }

        private void txtCommand_TextChanged(object sender, TextChangedEventArgs e) => reloadConflicts();

        private void txtScope_TextChanged(object sender, TextChangedEventArgs e) => reloadConflicts();

        private void txtShortcut_TextChanged(object sender, TextChangedEventArgs e) => reloadConflicts();
        private void reloadConflicts()
        {
            //Temporary.. ToDo - Replace with actual functionality
            string command = txtCommand.Text;
            string scope = txtScope.Text;
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

        //on key down in command textbox display intellisense
        private void txtCommand_KeyUp(object sender, KeyEventArgs e)
        {
            bool found = false;
            var border = (resultStack.Parent as ScrollViewer).Parent as Border;
            var data = IntellisenseModel.GetData();

            string query = (sender as TextBox).Text;

            if (query.Length == 0)
            {
                // Clear
                resultStack.Children.Clear();
                border.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                border.Visibility = System.Windows.Visibility.Visible;
            }

            
            resultStack.Children.Clear();

            // Add the result
            foreach (var obj in data)
            {
                if (obj.ToLower().Contains(query.ToLower()))
                {
                    // The key contains this... Autocomplete must work
                    addItemToList(obj);
                    found = true;
                }
            }

            if (!found)
            {
                resultStack.Children.Add(new TextBlock() { Text = "No results found." });
            }
        }

        //Event to add the Item to the list
        private void addItemToList(string text)
        {
            TextBlock block = new TextBlock();

            
            block.Text = text;

           
            block.Margin = new Thickness(2, 3, 2, 3);
            block.Cursor = Cursors.Hand;

            
            block.MouseLeftButtonUp += (sender, e) =>
            {
                txtCommand.Text = (sender as TextBlock).Text;
            };

            block.MouseEnter += (sender, e) =>
            {
                TextBlock b = sender as TextBlock;
                b.Background = Brushes.AliceBlue;
            };

            block.MouseLeave += (sender, e) =>
            {
                TextBlock b = sender as TextBlock;
                b.Background = Brushes.Transparent;
            };

            
            resultStack.Children.Add(block);
        }

        //Hide the intelisense box when text box loose focus
        private void txtCommand_LostFocus(object sender, RoutedEventArgs e)
        {
            var border = (resultStack.Parent as ScrollViewer).Parent as Border;
            border.Visibility = System.Windows.Visibility.Hidden;

        }

        //Hide Border container when mouse clicked outside
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = (resultStack.Parent as ScrollViewer).Parent as Border;
            border.Visibility = System.Windows.Visibility.Hidden;
        }
    }

    class IntellisenseModel
    {
        static public List<string> GetData()
        {
            List<string> data = new List<string>();

            data.Add("Help.View Help");
            data.Add("Window.Move Navigation Bar");
            data.Add("Edit.Find Next Selected");

            return data;
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
