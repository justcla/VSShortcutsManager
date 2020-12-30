using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

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

            // Handle Delete key operation
            if (e.Key == Key.Delete && grid.SelectedItems?.Count > 0)
            {
                ((CommandShortcutsControlDataContext)this.DataContext).DeleteShortcuts(grid.SelectedItems.Cast<CommandShortcut>());
                e.Handled = true;
            }
        }
    }
}
