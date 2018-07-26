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
        public AddKeyboardShortcut(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            cmbCommandList.ItemsSource = new CommandListViewModel(_serviceProvider).DataSource;
            cmbScopeList.ItemsSource = new ScopeListViewModel(_serviceProvider).DataSource;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        private void btnAddShortcut_Click(object sender, RoutedEventArgs e)
        {
            string shortcutcommand = cmbCommandList.SelectedValue.ToString();
            string shortcutScope = cmbScopeList.SelectedValue.ToString();
            string shortcutKeys = txtShortcut.Text;
            string shortcutBinding  = shortcutScope + "::" + shortcutKeys;
           
            VSShortcutQueryEngine queryEngine = new VSShortcutQueryEngine(_serviceProvider);
           

            queryEngine.BindShortcut(shortcutcommand, shortcutBinding);
        }
    }
}
