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

namespace VSShortcutsManager
{
    /// <summary>
    /// Interaction logic for CommandShortcutsControl.xaml
    /// </summary>
    public partial class CommandShortcutsControl : UserControl
    {
        public CommandShortcutsControl()
        {
            InitializeComponent();
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var items = new List<VSShortcut>();
            items.Add(new VSShortcut { Command = "Best Action", Shortcut = "Ctrl+A", Scope = "Global" });
            items.Add(new VSShortcut { Command = "Second Best Action", Shortcut = "Ctrl+A", Scope = "Some Scope" });
            items.Add(new VSShortcut { Command = "Another Best Action", Shortcut = "Ctrl+Z", Scope = "Global" });
            items.Add(new VSShortcut { Command = "Great Action", Shortcut = "Ctrl+Shift+A", Scope = "Global" });
            items.Add(new VSShortcut { Command = "Another Great Action", Shortcut = "Ctrl+Shift+Z", Scope = "Global" });

            var grid = sender as DataGrid;
            grid.ItemsSource = items;
        }
    }

    class VSShortcut
    {
        public string Command { get; set; }
        public string Shortcut { get; set; }
        public string Scope { get; set; }
    }
}
