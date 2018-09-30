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

        private void DataGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if (!(sender is DataGrid grid))
            {
                return;
            }

            if (e.Key == Key.Delete && grid.SelectedItems?.Count > 0)
            {
                ((CommandShortcutsControlDataContext)this.DataContext).DeleteShortcuts(grid.SelectedItems.Cast<CommandShortcut>());
                e.Handled = true;
            }
        }
    }
}
